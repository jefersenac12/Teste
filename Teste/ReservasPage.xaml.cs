using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using System.Linq;

namespace Teste
{
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
        public string Status { get; set; }
    }

    public class ReservaExibicao
    {
        public Reserva Reserva { get; set; }
        public string AtividadeNome { get; set; }
        public string DataTexto { get; set; }
        public string StatusUpper { get; set; }
    }

    public partial class ReservasAtivasPage : ContentPage
    {
        private readonly string apiUrlReserva = "http://tiijeferson.runasp.net/api/Reserva";
        private List<ReservaExibicao> _reservasExibicao = new();

        public ReservasAtivasPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarReservasAtivasAsync();
        }

        // ============================================================
        // Carrega reservas do servidor ou gera reservas fictícias
        // ============================================================
        private async Task CarregarReservasAtivasAsync()
        {
            if (ListaReservasAtivas == null)
            {
                await DisplayAlert("Erro", "Componente da lista não foi inicializado.", "OK");
                return;
            }

            ListaReservasAtivas.Children.Clear();

            try
            {
                int usuarioId = Preferences.Get("UsuarioId", Preferences.Get("ClienteId", 1));
                using var client = new HttpClient();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                HttpResponseMessage resp = await client.GetAsync($"{apiUrlReserva}/usuario/{usuarioId}");
                if (!resp.IsSuccessStatusCode)
                    resp = await client.GetAsync($"{apiUrlReserva}?usuarioId={usuarioId}");

                List<Reserva> reservas = new();
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    reservas = JsonSerializer.Deserialize<List<Reserva>>(json, opts) ?? new List<Reserva>();
                }

                // 🔹 Se não houver reservas no servidor, cria reservas fictícias
                if (reservas == null || reservas.Count == 0)
                {
                    reservas = GerarReservasFicticias(usuarioId);
                    await DisplayAlert("Aviso", "Nenhuma reserva encontrada. Exibindo reservas fictícias.", "OK");
                }

                _reservasExibicao.Clear();
                foreach (var reserva in reservas)
                {
                    string statusTexto = reserva.Status?.ToUpperInvariant() ?? "PENDENTE";
                    if (statusTexto != "PENDENTE" && statusTexto != "PAGO" && statusTexto != "CANCELADO")
                        continue;

                    string atividadeNome = $"Reserva #{reserva.Id}";
                    string dataTexto = reserva.DataReserva.ToString("dd/MM/yyyy - HH:mm");

                    _reservasExibicao.Add(new ReservaExibicao
                    {
                        Reserva = reserva,
                        AtividadeNome = atividadeNome,
                        DataTexto = dataTexto,
                        StatusUpper = statusTexto
                    });
                }

                RenderizarReservas(_reservasExibicao);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar reservas: {ex.Message}", "OK");
            }
        }

        // ============================================================
        // Gera reservas fictícias
        // ============================================================
        private List<Reserva> GerarReservasFicticias(int usuarioId)
        {
            return new List<Reserva>
            {
                new Reserva
                {
                    Id = 1,
                    AgendaId = 101,
                    UsuarioId = usuarioId,
                    DataReserva = DateTime.Now.AddDays(2),
                    InteiraEntrada = 2,
                    MeiaEntrada = 1,
                    ValorTotal = 150.00m,
                    Status = "PAGO"
                },
                new Reserva
                {
                    Id = 2,
                    AgendaId = 102,
                    UsuarioId = usuarioId,
                    DataReserva = DateTime.Now.AddDays(5),
                    InteiraEntrada = 1,
                    MeiaEntrada = 2,
                    ValorTotal = 90.00m,
                    Status = "PENDENTE"
                },
                new Reserva
                {
                    Id = 3,
                    AgendaId = 103,
                    UsuarioId = usuarioId,
                    DataReserva = DateTime.Now.AddDays(7),
                    InteiraEntrada = 3,
                    MeiaEntrada = 0,
                    ValorTotal = 180.00m,
                    Status = "CANCELADO"
                }
            };
        }

        // ============================================================
        // Renderização visual
        // ============================================================
        private void RenderizarReservas(IEnumerable<ReservaExibicao> itens)
        {
            ListaReservasAtivas.Children.Clear();
            var lista = itens?.ToList() ?? new List<ReservaExibicao>();

            if (lista.Count == 0)
            {
                ListaReservasAtivas.Children.Add(new Label
                {
                    Text = "Nenhuma reserva ativa encontrada.",
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20)
                });
                return;
            }

            foreach (var item in lista)
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
        }

        private View MontarCardReserva(string atividade, string dataTexto,
            int inteira, int meia, int npen, decimal valorTotal, string status)
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
                StrokeShape = new RoundRectangle { CornerRadius = 12 }
            };

            var stack = new VerticalStackLayout { Spacing = 8 };
            stack.Children.Add(new Label { Text = atividade, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#A42D45") });
            stack.Children.Add(new Label { Text = $"Data: {dataTexto}", FontSize = 13, TextColor = Color.FromArgb("#A42D45") });
            stack.Children.Add(new Label { Text = $"Participantes: {inteira}x adultos, {meia}x crianças", FontSize = 13, TextColor = Color.FromArgb("#A42D45") });

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

            var corStatus = status == "PAGO" ? "#F68F55" :
                            status == "CANCELADO" ? "#F2686F" : "#FCBC71";

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

            if (status == "PENDENTE" || status == "CANCELADO")
            {
                var btnExcluir = new Button
                {
                    Text = "🗑 Excluir",
                    TextColor = Color.FromArgb("#666"),
                    FontSize = 13,
                    BackgroundColor = Color.FromArgb("#F5F5F5"),
                    CornerRadius = 12,
                    Padding = new Thickness(12, 6)
                };
                btnExcluir.Clicked += OnExcluirClicked;
                grid.Add(btnExcluir, 2, 0);
            }

            stack.Children.Add(grid);
            border.Content = stack;
            return border;
        }

        // ============================================================
        // Eventos
        // ============================================================
        private async void OnVoltarClicked(object sender, EventArgs e) => await Navigation.PopAsync();

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Menu", "Cancelar", null, "Configurações", "Ajuda", "Sobre");
            if (action == "Configurações")
                await DisplayAlert("Configurações", "Abrindo configurações...", "OK");
            else if (action == "Ajuda")
                await DisplayAlert("Ajuda", "Abrindo central de ajuda...", "OK");
            else if (action == "Sobre")
                await DisplayAlert("Sobre", "App de Reservas - Versão 1.0", "OK");
        }

        private async void OnExcluirClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Confirmar Exclusão", "Deseja realmente excluir esta reserva?", "Sim", "Não");
            if (answer)
                await DisplayAlert("Sucesso", "Reserva excluída com sucesso!", "OK");
        }

        private async void OnSairClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Confirmar Saída", "Deseja realmente sair?", "Sim", "Não");
            if (answer)
                await Navigation.PopToRootAsync();
        }

        private async void OnVerTodasClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Todas as Reservas", "Carregando todas as reservas...", "OK");
        }

        private async void OnCardClicked(object sender, EventArgs e)
        {
            if (sender is Border b && b.BindingContext is ReservaExibicao re)
            {
                await DisplayAlert("Reserva",
                    $"Atividade: {re.AtividadeNome}\nData: {re.DataTexto}\nStatus: {re.StatusUpper}\nValor: R$ {re.Reserva.ValorTotal:N2}",
                    "OK");
            }
        }
    }
}
