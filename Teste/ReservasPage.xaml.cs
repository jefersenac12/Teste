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
        public string Status { get; set; } = "PENDENTE";
    }

    // ===============================
    // MODELO COMPLETO DA RESERVA (API)
    // ===============================
    public class ReservaCompleta
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
        public string Status { get; set; } = "PENDENTE";
        public AgendaInfo Agenda { get; set; }
        public List<PagamentoInfo> Pagamentos { get; set; } = new();
    }

    public class AgendaInfo
    {
        public int Id { get; set; }
        public DateTime DataHora { get; set; }
        public AtividadeInfo Atividade { get; set; }
    }

    public class AtividadeInfo
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }

    public class PagamentoInfo
    {
        public int Id { get; set; }
        public int ReservaId { get; set; }
        public decimal Valor { get; set; }
        public string Metodo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataPagamento { get; set; }
    }

    // ===============================
    // CLASSE DE EXIBIÇÃO (UX)
    // ===============================
    public class ReservaExibicao
    {
        public ReservaCompleta Reserva { get; set; }
        public string AtividadeNome { get; set; } = "Atividade não especificada";
        public string DataTexto { get; set; } = string.Empty;
        public string HorarioTexto { get; set; } = string.Empty;
        public string StatusUpper { get; set; } = "PENDENTE";
        public string CorStatus { get; set; } = "#FCBC71";
        public string TextoStatus { get; set; } = "Pendente";
    }

    // ===============================
    // PÁGINA PRINCIPAL
    // ===============================
    public partial class ReservasPage : ContentPage
    {
        private const string ApiBaseUrl = "http://tiijeferson.runasp.net/api";
        private readonly List<ReservaExibicao> _reservasExibicao = new();
        private bool _isLoading = false;

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
            if (_isLoading) return;

            if (ListaReservasAtivas == null)
            {
                await DisplayAlert("Erro", "A lista de reservas não foi inicializada.", "OK");
                return;
            }

            _isLoading = true;

            try
            {
                // Mostra mensagem de carregamento
                ListaReservasAtivas.Children.Clear();
                ListaReservasAtivas.Children.Add(new Label
                {
                    Text = "Carregando reservas...",
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20),
                    TextColor = Colors.Gray,
                    FontSize = 16
                });

                int usuarioId = Preferences.Get("UsuarioId", Preferences.Get("ClienteId", 0));

                if (usuarioId == 0)
                {
                    await DisplayAlert("Aviso", "Usuário não identificado. Faça login novamente.", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"🔄 Carregando reservas para usuário ID: {usuarioId}");

                List<ReservaCompleta> reservas = await CarregarReservasDaApi(usuarioId);

                // Se não veio nada, gera dados de exemplo
                if (reservas == null || reservas.Count == 0)
                {
                    reservas = GerarReservasFicticias(usuarioId);
                    System.Diagnostics.Debug.WriteLine("ℹ️ Nenhuma reserva encontrada na API, usando dados de teste");
                }

                // Converte para formato de exibição
                _reservasExibicao.Clear();
                foreach (var reserva in reservas)
                {
                    var reservaExibicao = ConverterParaExibicao(reserva);
                    _reservasExibicao.Add(reservaExibicao);
                }

                RenderizarReservas(_reservasExibicao);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Erro crítico: {ex}");
                await DisplayAlert("Erro", $"Falha ao carregar reservas: {ex.Message}", "OK");
                RenderizarReservas(new List<ReservaExibicao>());
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task<List<ReservaCompleta>> CarregarReservasDaApi(int usuarioId)
        {
            using var client = new HttpClient();
            // Define timeout para evitar espera infinita
            client.Timeout = TimeSpan.FromSeconds(30);

            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip
            };

            // Tenta múltiplos endpoints
            var endpoints = new[]
            {
                $"{ApiBaseUrl}/Usuario/{usuarioId}/reservas",
                $"{ApiBaseUrl}/Reserva/usuario/{usuarioId}",
                $"{ApiBaseUrl}/Reserva?usuarioId={usuarioId}",
                $"{ApiBaseUrl}/Usuario/Reserva/{usuarioId}"
            };

            foreach (var endpoint in endpoints)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"🔍 Tentando endpoint: {endpoint}");
                    var resp = await client.GetAsync(endpoint);

                    if (resp.IsSuccessStatusCode)
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"✅ Resposta da API: {json}");

                        // Tenta desserializar como lista de reservas completas
                        var reservas = JsonSerializer.Deserialize<List<ReservaCompleta>>(json, opts);
                        if (reservas != null && reservas.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ {reservas.Count} reservas carregadas da API");
                            return reservas;
                        }

                        // Tenta desserializar como lista simples
                        var reservasSimples = JsonSerializer.Deserialize<List<Reserva>>(json, opts);
                        if (reservasSimples != null && reservasSimples.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ {reservasSimples.Count} reservas simples carregadas da API");
                            return reservasSimples.Select(r => new ReservaCompleta
                            {
                                Id = r.Id,
                                AgendaId = r.AgendaId,
                                UsuarioId = r.UsuarioId,
                                DataReserva = r.DataReserva,
                                Quantidade = r.Quantidade,
                                NPEntrada = r.NPEntrada,
                                MeiaEntrada = r.MeiaEntrada,
                                InteiraEntrada = r.InteiraEntrada,
                                ValorTotal = r.ValorTotal,
                                Status = r.Status
                            }).ToList();
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Endpoint {endpoint} retornou: {resp.StatusCode}");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    System.Diagnostics.Debug.WriteLine($"🌐 Erro de rede no endpoint {endpoint}: {httpEx.Message}");
                }
                catch (TaskCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine($"⏰ Timeout no endpoint {endpoint}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Erro no endpoint {endpoint}: {ex.Message}");
                }
            }

            return new List<ReservaCompleta>();
        }

        private ReservaExibicao ConverterParaExibicao(ReservaCompleta reserva)
        {
            var dataReserva = reserva.DataReserva;
            var atividadeNome = reserva.Agenda?.Atividade?.Nome ?? $"Atividade {reserva.AgendaId}";

            // Determina status baseado nos pagamentos
            string status = "PENDENTE";
            if (reserva.Pagamentos != null && reserva.Pagamentos.Any())
            {
                var ultimoPagamento = reserva.Pagamentos.OrderByDescending(p => p.DataPagamento).First();
                status = ultimoPagamento.Status ?? reserva.Status ?? "PENDENTE";
            }
            else
            {
                status = reserva.Status ?? "PENDENTE";
            }

            // Define cores e textos do status
            var (corStatus, textoStatus) = status.ToUpper() switch
            {
                "PAGO" or "APROVADO" => ("#4CAF50", "Pago"),
                "CANCELADO" or "CANCELADA" => ("#F44336", "Cancelado"),
                "PENDENTE" or "AGUARDANDO" => ("#FF9800", "Pendente"),
                "EM_ANALISE" or "ANALISE" => ("#2196F3", "Em Análise"),
                _ => ("#FF9800", "Pendente")
            };

            return new ReservaExibicao
            {
                Reserva = reserva,
                AtividadeNome = atividadeNome,
                DataTexto = dataReserva.ToString("dd/MM/yyyy"),
                HorarioTexto = dataReserva.ToString("HH:mm"),
                StatusUpper = status.ToUpper(),
                CorStatus = corStatus,
                TextoStatus = textoStatus
            };
        }

        // ============================================================
        // Gera reservas fictícias (exemplo)
        // ============================================================
        private static List<ReservaCompleta> GerarReservasFicticias(int usuarioId) =>
            new()
            {
                new ReservaCompleta
                {
                    Id = 1,
                    AgendaId = 101,
                    UsuarioId = usuarioId,
                    DataReserva = DateTime.Now.AddDays(2),
                    InteiraEntrada = 2,
                    MeiaEntrada = 1,
                    NPEntrada = 0,
                    Quantidade = 3,
                    ValorTotal = 150.00m,
                    Status = "PAGO",
                    Agenda = new AgendaInfo
                    {
                        DataHora = DateTime.Now.AddDays(2),
                        Atividade = new AtividadeInfo { Nome = "Café da manhã Básico", Valor = 25.00m }
                    }
                },
                new ReservaCompleta
                {
                    Id = 2,
                    AgendaId = 102,
                    UsuarioId = usuarioId,
                    DataReserva = DateTime.Now.AddDays(5),
                    InteiraEntrada = 1,
                    MeiaEntrada = 2,
                    NPEntrada = 1,
                    Quantidade = 4,
                    ValorTotal = 90.00m,
                    Status = "PENDENTE",
                    Agenda = new AgendaInfo
                    {
                        DataHora = DateTime.Now.AddDays(5),
                        Atividade = new AtividadeInfo { Nome = "Trezinho / Colha e Pague", Valor = 15.00m }
                    }
                },
                new ReservaCompleta
                {
                    Id = 3,
                    AgendaId = 103,
                    UsuarioId = usuarioId,
                    DataReserva = DateTime.Now.AddDays(7),
                    InteiraEntrada = 3,
                    MeiaEntrada = 0,
                    NPEntrada = 2,
                    Quantidade = 5,
                    ValorTotal = 180.00m,
                    Status = "EM_ANALISE",
                    Agenda = new AgendaInfo
                    {
                        DataHora = DateTime.Now.AddDays(7),
                        Atividade = new AtividadeInfo { Nome = "Café da manhã Completo", Valor = 65.00m }
                    }
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
                    Text = "Nenhuma reserva encontrada.",
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20),
                    TextColor = Colors.Gray,
                    FontSize = 16
                });
                return;
            }

            // Ordena por data (mais recente primeiro)
            lista = lista.OrderByDescending(r => r.Reserva.DataReserva).ToList();

            foreach (var item in lista)
            {
                try
                {
                    var card = MontarCardReserva(item);
                    card.BindingContext = item;

                    var tap = new TapGestureRecognizer();
                    tap.Tapped += OnCardClicked;
                    card.GestureRecognizers.Add(tap);

                    ListaReservasAtivas.Children.Add(card);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erro ao criar card: {ex.Message}");
                }
            }
        }

        private View MontarCardReserva(ReservaExibicao item)
        {
            var ptBr = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");

            var border = new Border
            {
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                Padding = 16,
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(10, 5),
                StrokeShape = new RoundRectangle { CornerRadius = 12 }
            };

            var stack = new VerticalStackLayout { Spacing = 8 };

            // Cabeçalho com atividade e status
            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };

            headerGrid.Add(new Label
            {
                Text = item.AtividadeNome,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#A42D45"),
                VerticalOptions = LayoutOptions.Center
            }, 0, 0);

            // Status
            var statusBorder = new Border
            {
                BackgroundColor = Color.FromArgb(item.CorStatus),
                Padding = new Thickness(12, 6),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = new Label
                {
                    Text = item.TextoStatus,
                    TextColor = Colors.White,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
            headerGrid.Add(statusBorder, 1, 0);

            stack.Children.Add(headerGrid);

            // Data e horário
            stack.Children.Add(new Label
            {
                Text = $"📅 {item.DataTexto} 🕒 {item.HorarioTexto}",
                FontSize = 14,
                TextColor = Color.FromArgb("#666666")
            });

            // Participantes
            var participantesText = $"👥 {item.Reserva.InteiraEntrada}x adultos, {item.Reserva.MeiaEntrada}x crianças (6-12)";
            if (item.Reserva.NPEntrada > 0)
                participantesText += $", {item.Reserva.NPEntrada}x crianças (0-5)";

            stack.Children.Add(new Label
            {
                Text = participantesText,
                FontSize = 14,
                TextColor = Color.FromArgb("#666666")
            });

            // Valor
            stack.Children.Add(new Label
            {
                Text = $"💵 Valor: R$ {item.Reserva.ValorTotal.ToString("N2", ptBr)}",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#A42D45")
            });

            // ID da reserva (pequeno e discreto)
            stack.Children.Add(new Label
            {
                Text = $"ID: {item.Reserva.Id}",
                FontSize = 10,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.End
            });

            border.Content = stack;
            return border;
        }

        // ============================================================
        // Eventos de clique
        // ============================================================
        private async void OnCardClicked(object sender, EventArgs e)
        {
            if (sender is Border border && border.BindingContext is ReservaExibicao reserva)
            {
                var detalhes = $"🆔 ID: {reserva.Reserva.Id}\n" +
                             $"📋 Atividade: {reserva.AtividadeNome}\n" +
                             $"📅 Data: {reserva.DataTexto}\n" +
                             $"🕒 Horário: {reserva.HorarioTexto}\n" +
                             $"👥 Adultos: {reserva.Reserva.InteiraEntrada}\n" +
                             $"👶 Crianças (6-12): {reserva.Reserva.MeiaEntrada}\n" +
                             $"🍼 Crianças (0-5): {reserva.Reserva.NPEntrada}\n" +
                             $"💰 Valor Total: R$ {reserva.Reserva.ValorTotal:N2}\n" +
                             $"📊 Status: {reserva.TextoStatus}";

                await DisplayAlert("Detalhes da Reserva", detalhes, "OK");
            }
        }

        private async void OnVoltarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            // Implementar menu de opções se necessário
            await DisplayAlert("Menu", "Opções do menu em desenvolvimento", "OK");
        }

        private async void OnVerTodasClicked(object sender, EventArgs e)
        {
            // Recarregar todas as reservas
            await CarregarReservasAsync();
        }

        private async void OnSairClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Sair", "Deseja realmente sair do aplicativo?", "Sim", "Não");
            if (confirmar)
            {
                // Limpar preferências e sair
                Preferences.Clear();
                Application.Current.Quit();
            }
        }

        private async void OnAtualizarClicked(object sender, EventArgs e)
        {
            await CarregarReservasAsync();
        }
    }
}