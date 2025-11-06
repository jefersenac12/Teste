using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;

namespace Teste
{
    // ===============================
    // MODELO PRINCIPAL (igual ao BD)
    // ===============================
    public class Reserva
    {
        public int Id { get; set; }
        public int AgendaId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime DataReserva { get; set; }
        public int Quantidade { get; set; }
        public int NPEntrada { get; set; }
        public int MeiaEntrada { get; set; }
        public int InteiraEntrada { get; set; }
        public decimal ValorTotal { get; set; }
    }

    // ===============================
    // CLASSE DE EXIBIÇÃO (UX)
    // ===============================
    public class ReservaExibicao
    {
        public Reserva Reserva { get; set; }
        public string AtividadeNome { get; set; }
        public string DataTexto { get; set; }
        public string StatusUpper { get; set; } = "PENDENTE"; // valor default visual
    }

    // ===============================
    // PÁGINA PRINCIPAL
    // ===============================
    public partial class ReservasPage : ContentPage
    {
        private const string ApiUrlReserva = "http://tiijeferson.runasp.net/api/Usuario/Reserva";
        private readonly List<ReservaExibicao> _reservasExibicao = new();

        public ReservasPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarReservasAsync();
        }

        // ============================================================
        // Carrega reservas do servidor ou gera reservas fictícias
        // ============================================================
        private async Task CarregarReservasAsync()
        {
            if (ListaReservasAtivas == null)
            {
                await DisplayAlert("Erro", "A lista de reservas não foi inicializada.", "OK");
                return;
            }

            ListaReservasAtivas.Children.Clear();

            try
            {
                int usuarioId = Preferences.Get("UsuarioId", Preferences.Get("ClienteId", 1));
                using var client = new HttpClient();

                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip
                };

                HttpResponseMessage resp = null;

                try
                {
                    resp = await client.GetAsync($"{ApiUrlReserva}/usuario/{usuarioId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Erro no endpoint 1: {ex.Message}");
                }

                // Tenta outro endpoint se o primeiro falhar
                if (resp == null || !resp.IsSuccessStatusCode)
                {
                    try
                    {
                        resp = await client.GetAsync($"{ApiUrlReserva}?usuarioId={usuarioId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Erro no endpoint 2: {ex.Message}");
                    }
                }

                List<Reserva> reservas = new();

                if (resp != null && resp.IsSuccessStatusCode)
                {
                    try
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        reservas = JsonSerializer.Deserialize<List<Reserva>>(json, opts) ?? new List<Reserva>();
                        Console.WriteLine($"✅ {reservas.Count} reservas carregadas da API");
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"❌ Erro ao desserializar JSON: {jsonEx.Message}");
                        await DisplayAlert("Erro", "Formato de dados inválido recebido da API.", "OK");
                    }
                }

                // Se não veio nada, gera dados de exemplo
                if (reservas == null || reservas.Count == 0)
                {
                    reservas = GerarReservasFicticias(usuarioId);
                    await DisplayAlert("Aviso", "Nenhuma reserva encontrada. Exibindo dados de teste.", "OK");
                }

                // Converte para formato de exibição
                _reservasExibicao.Clear();
                _reservasExibicao.AddRange(reservas.Select(r => new ReservaExibicao
                {
                    Reserva = r,
                    AtividadeNome = $"Atividade {r.AgendaId}",
                    DataTexto = r.DataReserva.ToString("dd/MM/yyyy"),
                    StatusUpper = "PENDENTE"
                }));

                RenderizarReservas(_reservasExibicao);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Erro crítico: {ex}");
                await DisplayAlert("Erro", $"Falha ao carregar reservas: {ex.Message}", "OK");
                RenderizarReservas(new List<ReservaExibicao>());
            }
        }

        // ============================================================
        // Gera reservas fictícias (exemplo)
        // ============================================================
        private static List<Reserva> GerarReservasFicticias(int usuarioId) =>
            new()
            {
                new Reserva
                {
                    Id = 1,
                    AgendaId = 101,
                    UsuarioId = usuarioId,
                    DataReserva = DateTime.Now.AddDays(2),
                    InteiraEntrada = 2,
                    MeiaEntrada = 1,
                    NPEntrada = 0,
                    Quantidade = 3,
                    ValorTotal = 150.00m
                },
                new Reserva
                {
                    Id = 2,
                    AgendaId = 102,
                    UsuarioId = usuarioId,
                    DataReserva = DateTime.Now.AddDays(5),
                    InteiraEntrada = 1,
                    MeiaEntrada = 2,
                    NPEntrada = 1,
                    Quantidade = 4,
                    ValorTotal = 90.00m
                },
                new Reserva
                {
                    Id = 3,
                    AgendaId = 103,
                    UsuarioId = usuarioId,
                    DataReserva = DateTime.Now.AddDays(7),
                    InteiraEntrada = 3,
                    MeiaEntrada = 0,
                    NPEntrada = 2,
                    Quantidade = 5,
                    ValorTotal = 180.00m
                }
            };

        // ============================================================
        // Renderização visual dos cards
        // ============================================================
        private void RenderizarReservas(IEnumerable<ReservaExibicao> itens)
        {
            ListaReservasAtivas.Children.Clear();
            var lista = itens?.ToList() ?? [];

            if (lista.Count == 0)
            {
                ListaReservasAtivas.Children.Add(new Label
                {
                    Text = "Nenhuma reserva ativa encontrada.",
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20),
                    TextColor = Colors.Gray
                });
                return;
            }

            foreach (var item in lista)
            {
                try
                {
                    var card = MontarCardReserva(
                        item.AtividadeNome,
                        item.DataTexto,
                        item.Reserva.InteiraEntrada,
                        item.Reserva.MeiaEntrada,
                        item.Reserva.NPEntrada,
                        item.Reserva.ValorTotal,
                        item.StatusUpper);

                    card.BindingContext = item;
                    var tap = new TapGestureRecognizer();
                    tap.Tapped += OnCardClicked;
                    card.GestureRecognizers.Add(tap);
                    ListaReservasAtivas.Children.Add(card);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao criar card: {ex.Message}");
                }
            }
        }

        private View MontarCardReserva(
            string atividade, string dataTexto,
            int inteira, int meia, int npen,
            decimal valorTotal, string status)
        {
            var ptBr = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");

            var border = new Border
            {
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                Padding = 16,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = 300,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Margin = new Thickness(0, 5)
            };

            var stack = new VerticalStackLayout { Spacing = 8 };

            stack.Children.Add(new Label
            {
                Text = atividade,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#A42D45")
            });

            stack.Children.Add(new Label
            {
                Text = $"Data: {dataTexto}",
                FontSize = 13,
                TextColor = Color.FromArgb("#A42D45")
            });

            var participantesText = $"Participantes: {inteira}x adultos, {meia}x crianças";
            if (npen > 0)
                participantesText += $", {npen}x (0-5 anos)";

            stack.Children.Add(new Label
            {
                Text = participantesText,
                FontSize = 13,
                TextColor = Color.FromArgb("#A42D45")
            });

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 8,
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

            // Define cor do status
            var corStatus = status switch
            {
                "PAGO" => "#F68F55",
                "CANCELADO" => "#F2686F",
                _ => "#FCBC71"
            };

            var statusBorder = new Border
            {
                BackgroundColor = Color.FromArgb(corStatus),
                Padding = new Thickness(12, 6),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = new Label
                {
                    Text = status == "PAGO" ? "Pago" :
                           status == "CANCELADO" ? "Cancelado" : "Pendente",
                    TextColor = Colors.White,
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold
                }
            };
            grid.Add(statusBorder, 1, 0);

            stack.Children.Add(grid);
            border.Content = stack;
            return border;
        }

        // ============================================================
        // Eventos de clique
        // ============================================================
        private async void OnCardClicked(object sender, EventArgs e)
        {
            if (sender is Border b && b.BindingContext is ReservaExibicao re)
            {
                await DisplayAlert("Detalhes da Reserva",
                    $"Atividade: {re.AtividadeNome}\n" +
                    $"Data: {re.DataTexto}\n" +
                    $"Adultos: {re.Reserva.InteiraEntrada}\n" +
                    $"Crianças (6-12): {re.Reserva.MeiaEntrada}\n" +
                    $"Crianças (0-5): {re.Reserva.NPEntrada}\n" +
                    $"Valor Total: R$ {re.Reserva.ValorTotal:N2}",
                    "OK");
            }
        }

        private void OnVoltarClicked(object sender, EventArgs e)
        {

        }

        private void OnMenuClicked(object sender, EventArgs e)
        {

        }

        private void OnVerTodasClicked(object sender, EventArgs e)
        {

        }

        private void OnSairClicked(object sender, EventArgs e)
        {

        }
    }
}
