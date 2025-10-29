

using System.Net.Http;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Teste
{
    public partial class ReservasAnterioresPage : ContentPage
    {
        private readonly string apiUrlReserva = "http://tiijeferson.runasp.net/api/Reserva";
        private readonly string apiUrlAgenda = "http://tiijeferson.runasp.net/api/Agenda";

        public ReservasAnterioresPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarReservasAnterioresAsync();
        }

        private async Task CarregarReservasAnterioresAsync()
        {
            try
            {
                if (ListaReservasAnteriores == null) return;
                ListaReservasAnteriores.Children.Clear();

                int usuarioId = Preferences.ContainsKey("UsuarioId")
                    ? Preferences.Get("UsuarioId", 1)
                    : Preferences.Get("ClienteId", 1);

                using var client = new HttpClient();
                HttpResponseMessage resp = await client.GetAsync($"{apiUrlReserva}/usuario/{usuarioId}");
                if (!resp.IsSuccessStatusCode)
                {
                    resp = await client.GetAsync($"{apiUrlReserva}?usuarioId={usuarioId}");
                }

                if (!resp.IsSuccessStatusCode)
                {
                    await DisplayAlert("Aviso", "Não foi possível carregar reservas anteriores do servidor.", "OK");
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var arr = JsonSerializer.Deserialize<JsonElement>(json);
                if (arr.ValueKind != JsonValueKind.Array)
                {
                    await DisplayAlert("Aviso", "Resposta inesperada ao listar reservas.", "OK");
                    return;
                }

                foreach (var el in arr.EnumerateArray())
                {
                    int id = TryGetInt(el, "Id");
                    int agendaId = TryGetInt(el, "AgendaId");
                    int inteira = TryGetInt(el, "InteiraEntrada");
                    int meia = TryGetInt(el, "MeiaEntrada");
                    int npen = TryGetInt(el, "NPEntrada");
                    decimal valorTotal = TryGetDecimal(el, "ValorTotal");

                    string atividadeNome = $"Reserva #{id}";
                    string dataTexto = "";
                    string statusTexto = "";

                    DateTime? dataAgenda = null;
                    if (agendaId > 0)
                    {
                        try
                        {
                            var rAgenda = await client.GetAsync($"{apiUrlAgenda}/{agendaId}");
                            if (rAgenda.IsSuccessStatusCode)
                            {
                                var jAg = await rAgenda.Content.ReadAsStringAsync();
                                var ag = JsonSerializer.Deserialize<JsonElement>(jAg, opts);
                                var dataHoraStr = TryGetString(ag, "DataHora");
                                if (!string.IsNullOrWhiteSpace(dataHoraStr) && DateTime.TryParse(dataHoraStr, out var dh))
                                {
                                    dataAgenda = dh;
                                    dataTexto = dh.ToString("dd/MM/yyyy - HH:mm");
                                }
                                int atividadeId = TryGetInt(ag, "AtividadeId");
                                if (atividadeId > 0)
                                {
                                    atividadeNome = $"Atividade #{atividadeId}";
                                }
                            }
                        }
                        catch { }
                    }

                    statusTexto = TryGetString(el, "Status");

                    // Filtro simples para "anteriores": data passada ou status CANCELADO
                    bool ehAnterior = false;
                    if (dataAgenda.HasValue)
                        ehAnterior = dataAgenda.Value < DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(statusTexto) && statusTexto.ToUpperInvariant().Contains("CANCEL"))
                        ehAnterior = true;

                    if (!ehAnterior)
                        continue;

                    var card = MontarCardReserva(atividadeNome, dataTexto,
                        inteira, meia, npen, valorTotal, string.IsNullOrWhiteSpace(statusTexto) ? "PAGO" : statusTexto.ToUpperInvariant());
                    ListaReservasAnteriores.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar reservas anteriores: {ex.Message}", "OK");
            }
        }

        private static View MontarCardReserva(string atividade, string dataTexto,
            int inteira, int meia, int npen, decimal valorTotal, string status)
        {
            var ptBr = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
            var border = new Border
            {
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                //StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = 16
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
                ColumnSpacing = 12,
                RowSpacing = 4
            };

            // Indicador de status
            var statusCircle = new Border
            {
                WidthRequest = 32,
                HeightRequest = 32,
                BackgroundColor = status == "PAGO" ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F68F55"),
                //StrokeShape = new Ellipse(),
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 4, 0, 0),
                Content = new Label
                {
                    Text = status == "PAGO" ? "✓" : "!",
                    TextColor = Colors.White,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
            grid.Add(statusCircle, 0, 0);

            // Conteúdo
            grid.Add(new Label { Text = atividade, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#A42D45") }, 1, 0);

            var statusBadge = new Border
            {
                BackgroundColor = status == "PAGO" ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F68F55"),
                //StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(16, 6),
                VerticalOptions = LayoutOptions.Start,
                Content = new Label { Text = status == "PAGO" ? "Pago" : status, TextColor = Colors.White, FontSize = 13, FontAttributes = FontAttributes.Bold }
            };
            grid.Add(statusBadge, 2, 0);

            if (!string.IsNullOrWhiteSpace(dataTexto))
                grid.Add(new Label { Text = dataTexto, FontSize = 13, TextColor = Color.FromArgb("#A42D45") }, 1, 1);

            grid.Add(new Label { Text = $"{inteira}x Adultos", FontSize = 13, TextColor = Color.FromArgb("#A42D45") }, 1, 2);
            grid.Add(new Label { Text = $"{meia}x Criança", FontSize = 13, TextColor = Color.FromArgb("#A42D45") }, 1, 3);

            grid.Add(new Label
            {
                Text = $"R$ {valorTotal.ToString("N2", ptBr)}",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#A42D45"),
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 8, 0, 0)
            }, 1, 4);

            border.Content = grid;
            return border;
        }

        private static int TryGetInt(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number)
                return v.GetInt32();
            if (el.TryGetProperty(name.ToLowerInvariant(), out v) && v.ValueKind == JsonValueKind.Number)
                return v.GetInt32();
            return 0;
        }

        private static decimal TryGetDecimal(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number)
                return v.GetDecimal();
            if (el.TryGetProperty(name.ToLowerInvariant(), out v) && v.ValueKind == JsonValueKind.Number)
                return v.GetDecimal();
            if (el.TryGetProperty(name, out v) && v.ValueKind == JsonValueKind.String && decimal.TryParse(v.GetString(), out var d))
                return d;
            return 0m;
        }

        private static string TryGetString(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String)
                return v.GetString() ?? string.Empty;
            if (el.TryGetProperty(name.ToLowerInvariant(), out v) && v.ValueKind == JsonValueKind.String)
                return v.GetString() ?? string.Empty;
            return string.Empty;
        }

        // Navegar para Ativas
        private async void OnAtivasClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Voltar
        private async void OnVoltarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Ver todas as reservas
        private async void OnVerTodasClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Todas as Reservas", "Carregando todas as reservas...", "OK");
            // Aqui voc� pode carregar mais reservas ou navegar para uma lista completa
        }

        // Sair
        private async void OnSairClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Confirmar Sa�da",
                "Deseja realmente sair?",
                "Sim",
                "N�o"
            );

            if (answer)
            {
                await Navigation.PopToRootAsync();
            }
        }

        // Ver detalhes da reserva
        private async void OnCardTapped(object sender, EventArgs e)
        {
            await DisplayAlert("Detalhes", "Abrindo detalhes da reserva anterior...", "OK");
            // await Navigation.PushAsync(new DetalhesReservaPage(reservaId));
        }

        // Clique no card da reserva
        private async void OnCardClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Reserva", "Abrindo detalhes da reserva anterior...", "OK");
            // await Navigation.PushAsync(new DetalhesReservaPage(reservaId));
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {

        }

        private void OnAnterioresClicked(object sender, EventArgs e)
        {

        }
    }
}

