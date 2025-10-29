

using System.Net.Http;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Teste
{
    public partial class ReservasAtivasPage : ContentPage
    {
        private readonly string apiUrlReserva = "http://tiijeferson.runasp.net/api/Reserva";
        private readonly string apiUrlAgenda = "http://tiijeferson.runasp.net/api/Agenda";

        public ReservasAtivasPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarReservasAtivasAsync();
        }

        private async Task CarregarReservasAtivasAsync()
        {
            try
            {
                if (ListaReservasAtivas == null) return;
                ListaReservasAtivas.Children.Clear();

                int usuarioId = Preferences.ContainsKey("UsuarioId")
                    ? Preferences.Get("UsuarioId", 1)
                    : Preferences.Get("ClienteId", 1);

                using var client = new HttpClient();
                // Tenta padrões de endpoint
                HttpResponseMessage resp = await client.GetAsync($"{apiUrlReserva}/usuario/{usuarioId}");
                if (!resp.IsSuccessStatusCode)
                {
                    resp = await client.GetAsync($"{apiUrlReserva}?usuarioId={usuarioId}");
                }

                if (!resp.IsSuccessStatusCode)
                {
                    await DisplayAlert("Aviso", "Não foi possível carregar reservas ativas do servidor.", "OK");
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
                    string statusTexto = "PENDENTE";

                    // Tenta buscar detalhes de Agenda
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

                    // Filtro simples para "ativas": mantemos PENDENTE/PAGO
                    var statusReserva = TryGetString(el, "Status");
                    if (!string.IsNullOrWhiteSpace(statusReserva))
                        statusTexto = statusReserva;

                    if (statusTexto != "PENDENTE" && statusTexto != "PAGO")
                        continue;

                    // Monta card
                    var card = MontarCardReserva(atividadeNome, dataTexto,
                        inteira, meia, npen, valorTotal, statusTexto);
                    ListaReservasAtivas.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar reservas: {ex.Message}", "OK");
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

            var stack = new VerticalStackLayout { Spacing = 8 };
            stack.Children.Add(new Label { Text = atividade, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#A42D45") });
            if (!string.IsNullOrWhiteSpace(dataTexto))
            {
                stack.Children.Add(new Label { Text = $"Data: {dataTexto}", FontSize = 13, TextColor = Color.FromArgb("#A42D45") });
            }

            stack.Children.Add(new Label
            {
                Text = $"Participantes: {inteira}x Adultos, {meia}x Meia, {npen}x 0–5",
                FontSize = 13,
                TextColor = Color.FromArgb("#A42D45")
            });

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                Margin = new Thickness(0, 8, 0, 0)
            };

            grid.Add(new Label
            {
                Text = $"Valor: R$ {valorTotal.ToString("N2", ptBr)}",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#A42D45"),
                VerticalOptions = LayoutOptions.Center
            }, 0, 0);

            var statusBorder = new Border
            {
                BackgroundColor = status == "PAGO" ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F68F55"),
                //StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(16, 6)
            };
            statusBorder.Content = new Label
            {
                Text = status == "PAGO" ? "Pago" : "Pendente",
                TextColor = Colors.White,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold
            };
            grid.Add(statusBorder, 1, 0);

            stack.Children.Add(grid);
            border.Content = stack;
            return border;
        }

        private static int TryGetInt(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number)
                return v.GetInt32();
            // tenta variações de nome
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

        // Navegar para Anteriores
        private async void OnAnterioresClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReservasAnterioresPage());
        }

        // Voltar
        private async void OnVoltarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Menu
        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "Menu",
                "Cancelar",
                null,
                "Configura��es",
                "Ajuda",
                "Sobre"
            );

            if (action == "Configura��es")
            {
                await DisplayAlert("Configura��es", "Abrindo configura��es...", "OK");
            }
            else if (action == "Ajuda")
            {
                await DisplayAlert("Ajuda", "Abrindo central de ajuda...", "OK");
            }
            else if (action == "Sobre")
            {
                await DisplayAlert("Sobre", "App de Reservas - Vers�o 1.0", "OK");
            }
        }

        // Anteriores (permanece na p�gina)
        //private void OnAnterioresClicked(object sender, EventArgs e)
        //{
        //    // J� est� na p�gina anteriores, n�o faz nada
        //}

        // Menu
        //private async void OnMenuClicked(object sender, EventArgs e)
        //{
        //    string action = await DisplayActionSheet(
        //        "Menu",
        //        "Cancelar",
        //        null,
        //        "Configura��es",
        //        "Ajuda",
        //        "Sobre"
        //    );

        //    if (action == "Configura��es")
        //    {
        //        await DisplayAlert("Configura��es", "Abrindo configura��es...", "OK");
        //    }
        //    else if (action == "Ajuda")
        //    {
        //        await DisplayAlert("Ajuda", "Abrindo central de ajuda...", "OK");
        //    }
        //    else if (action == "Sobre")
        //    {
        //        await DisplayAlert("Sobre", "App de Reservas - Vers�o 1.0", "OK");
        //    }
        //}

        //// Ativas (permanece na p�gina)
        //private void OnAtivasClicked(object sender, EventArgs e)
        //{
        //    // J� est� na p�gina ativa, n�o faz nada
        //}

        //// Excluir reserva
        //private async void OnExcluirClicked(object sender, EventArgs e)
        //{
        //    bool answer = await DisplayAlert(
        //        "Confirmar Exclus�o",
        //        "Deseja realmente excluir esta reserva?",
        //        "Sim",
        //        "N�o"
        //    );

        //    if (answer)
        //    {
        //        await DisplayAlert("Sucesso", "Reserva exclu�da com sucesso!", "OK");
        //        // Aqui voc� pode adicionar a l�gica para remover a reserva do backend
        //    }
        //}

        // Sair (logout ou voltar para tela inicial)
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
                // Navegar para a p�gina de login ou p�gina inicial
                await Navigation.PopToRootAsync();
                // Ou se quiser ir para uma p�gina espec�fica:
                // Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        // Ver todas as reservas
        private async void OnVerTodasClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Todas as Reservas", "Carregando todas as reservas...", "OK");
            // Aqui voc� pode carregar mais reservas ou expandir a lista
        }

        // Ver detalhes da reserva
        private async void OnVerDetalhesClicked(object sender, EventArgs e)
        {
            // Voc� pode passar o ID da reserva para a p�gina de detalhes
            await DisplayAlert("Detalhes", "Abrindo detalhes da reserva...", "OK");
           
        }

        // Clique no card da reserva
        private async void OnCardClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Reserva", "Abrindo detalhes da reserva...", "OK");
         
        }

        // Buscar reservas
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue;
           
        }

        private void OnAtivasClicked(object sender, EventArgs e)
        {

        }

        private async void OnExcluirClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Confirmar Exclus�o",
                "Deseja realmente excluir esta reserva?",
                "Sim",
                "N�o"
            );

            if (answer)
            {
                await DisplayAlert("Sucesso", "Reserva exclu�da com sucesso!", "OK");
                // Aqui voc� pode adicionar a l�gica para remover a reserva do backend
            }
        }
    }
}