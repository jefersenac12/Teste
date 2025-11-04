using System;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls.Shapes;

namespace Teste
{

    // =========================================================================
    // CLASSES DE MODELO (MODELOS PARA DESSERIALIZAÇÃO)
    // =========================================================================

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

    public class Agenda
    {
        public int Id { get; set; }
        public int AtividadeId { get; set; }
        public string DataHora { get; set; }
        // Adicione outras propriedades da Agenda que você precisa
    }
    
    // Modelo de exibição para evitar recomputar dados em filtros/renderizações
    public class ReservaExibicao
    {
        public Reserva Reserva { get; set; }
        public string AtividadeNome { get; set; }
        public string DataTexto { get; set; }
        public string StatusUpper { get; set; }
    }
    
    // =========================================================================
    // CLASSE DA PÁGINA (CORRIGIDA)
    // * Não contém a declaração redundante de ListaReservasAtivas *
    // =========================================================================

    public partial class ReservasAtivasPage : ContentPage
    {
        // URLs das APIs
        private readonly string apiUrlReserva = "http://tiijeferson.runasp.net/api/Reserva";
        private readonly string apiUrlAgenda = "http://tiijeferson.runasp.net/api/Agenda";
        private readonly string apiUrlAtividade = "http://tiijeferson.runasp.net/api/Atividade";
        
        // Cache das reservas para busca/filtragem
        private List<ReservaExibicao> _reservasExibicao = new();
        
        // O campo 'ListaReservasAtivas' será automaticamente criado pelo InitializeComponent
        // a partir da declaração 'x:Name' no seu XAML.

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
            // O componente ListaReservasAtivas é inicializado via XAML/InitializeComponent.
            if (ListaReservasAtivas == null) 
            {
                await DisplayAlert("Erro de Componente", "O componente ListaReservasAtivas não foi inicializado. Verifique seu XAML.", "OK");
                return;
            }
            ListaReservasAtivas.Children.Clear();

            try
            {
                // Tenta obter UsuarioId, se não existir, tenta ClienteId, com default 1
                int usuarioId = Preferences.Get("UsuarioId", Preferences.Get("ClienteId", 1));

                using var client = new HttpClient();
                // Opções para desserialização (ignora case)
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 1. Tenta padrões de endpoint para buscar reservas do usuário
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
                
                // Desserialização direta para a lista de objetos Reserva
                var reservas = JsonSerializer.Deserialize<List<Reserva>>(json, opts) ?? new List<Reserva>();

                // Inclui reserva fictícia local, se existir
                string reservaFicticiaJson = Preferences.Get("ReservaFicticiaJson", "");
                if (!string.IsNullOrWhiteSpace(reservaFicticiaJson))
                {
                    try
                    {
                        var el = JsonSerializer.Deserialize<JsonElement>(reservaFicticiaJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var rf = new Reserva
                        {
                            Id = 0,
                            AgendaId = SafeGetInt(el, "AgendaId"),
                            UsuarioId = SafeGetInt(el, "UsuarioId"),
                            DataReserva = SafeGetDate(el, "DataReserva"),
                            Quantidade = SafeGetInt(el, "Quantidade"),
                            NPEntrada = SafeGetInt(el, "NPEntrada"),
                            MeiaEntrada = SafeGetInt(el, "MeiaEntrada"),
                            InteiraEntrada = SafeGetInt(el, "InteiraEntrada"),
                            ValorTotal = SafeGetDecimal(el, "ValorTotal"),
                            Status = TryGetString(el, "Status") ?? "PENDENTE"
                        };
                        reservas.Add(rf);
                        // Limpa para evitar duplicação em próximas aberturas
                        Preferences.Remove("ReservaFicticiaJson");
                    }
                    catch { /* ignora falhas de parsing */ }
                }

                _reservasExibicao.Clear();

                // 2. Itera sobre cada reserva carregada
                foreach (var reserva in reservas)
                {
                    // Determina o status e permite PENDENTE, PAGO e CANCELADO conforme a imagem
                    string statusTexto = reserva.Status?.ToUpperInvariant() ?? "PENDENTE";
                    if (statusTexto != "PENDENTE" && statusTexto != "PAGO" && statusTexto != "CANCELADO")
                        continue; // ignora outros estados não exibidos na imagem

                    string atividadeNome = $"Reserva #{reserva.Id}";
                    string dataTexto = "";

                    // 3. Tenta buscar detalhes de Agenda/Atividade (se o AgendaId for válido)
                    if (reserva.AgendaId > 0)
                    {
                        try
                        {
                            var rAgenda = await client.GetAsync($"{apiUrlAgenda}/{reserva.AgendaId}");
                            if (rAgenda.IsSuccessStatusCode)
                            {
                                var jAg = await rAgenda.Content.ReadAsStringAsync();
                                var agenda = JsonSerializer.Deserialize<Agenda>(jAg, opts);
                                
                                if (agenda != null)
                                {
                                    // Formata DataHora
                                    if (!string.IsNullOrWhiteSpace(agenda.DataHora) && DateTime.TryParse(agenda.DataHora, out var dh))
                                    {
                                        dataTexto = dh.ToString("dd/MM/yyyy - HH:mm");
                                    }
                                    
                                    if (agenda.AtividadeId > 0)
                                    {
                                        try
                                        {
                                            var rAt = await client.GetAsync($"{apiUrlAtividade}/{agenda.AtividadeId}");
                                            if (rAt.IsSuccessStatusCode)
                                            {
                                                var jAt = await rAt.Content.ReadAsStringAsync();
                                                var atv = JsonSerializer.Deserialize<JsonElement>(jAt, opts);
                                                string nomeAtv = TryGetString(atv, "Nome");
                                                if (string.IsNullOrWhiteSpace(nomeAtv)) nomeAtv = TryGetString(atv, "nome");
                                                atividadeNome = string.IsNullOrWhiteSpace(nomeAtv) ? $"Atividade #{agenda.AtividadeId}" : nomeAtv;
                                            }
                                            else
                                            {
                                                atividadeNome = $"Atividade #{agenda.AtividadeId}";
                                            }
                                        }
                                        catch
                                        {
                                            atividadeNome = $"Atividade #{agenda.AtividadeId}";
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex) 
                        { 
                            System.Diagnostics.Debug.WriteLine($"Erro ao buscar Agenda {reserva.AgendaId}: {ex.Message}");
                        }
                    }

                    _reservasExibicao.Add(new ReservaExibicao
                    {
                        Reserva = reserva,
                        AtividadeNome = atividadeNome,
                        DataTexto = dataTexto,
                        StatusUpper = statusTexto
                    });
                }

                // Renderiza a lista uma única vez com cache, ou mostra mensagem de vazio
                RenderizarReservas(_reservasExibicao);
            }
            catch (JsonException jEx)
            {
                await DisplayAlert("Erro de Dados", $"Erro ao processar dados de reservas. Verifique a estrutura do JSON. Detalhes: {jEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro geral ao carregar reservas: {ex.Message}", "OK");
            }
        }

        // =========================================================================
        // MÉTODOS AUXILIARES E EVENTOS
        // =========================================================================

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
                var card = MontarCardReserva(item.AtividadeNome,
                                             item.DataTexto,
                                             item.Reserva.InteiraEntrada,
                                             item.Reserva.MeiaEntrada,
                                             item.Reserva.NPEntrada,
                                             item.Reserva.ValorTotal,
                                             item.StatusUpper);

                // Vincula contexto para clique
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
                WidthRequest = 300
            };
            // Aplicar cantos arredondados conforme design
            border.StrokeShape = new RoundRectangle { CornerRadius = 12 };

            var stack = new VerticalStackLayout { Spacing = 8 };
            stack.Children.Add(new Label { Text = atividade, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#A42D45") });
            if (!string.IsNullOrWhiteSpace(dataTexto))
            {
                stack.Children.Add(new Label { Text = $"Data: {dataTexto}", FontSize = 13, TextColor = Color.FromArgb("#A42D45") });
            }

            stack.Children.Add(new Label
            {
                Text = $"Participantes: {inteira}x Adultos, {meia}x criança",
                FontSize = 13,
                TextColor = Color.FromArgb("#A42D45")
            });

            var statusUpper = (status ?? "PENDENTE").ToUpperInvariant();

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                    // Terceira coluna aparece somente quando pendente (botão Excluir)
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

            var statusBorder = new Border
            {
                BackgroundColor = statusUpper == "PAGO" ? Color.FromArgb("#F68F55") :
                                 statusUpper == "CANCELADO" ? Color.FromArgb("#F2686F") :
                                 Color.FromArgb("#FCBC71"),
                Padding = new Thickness(12, 6),
                StrokeShape = new RoundRectangle { CornerRadius = 12 }
            };
            statusBorder.Content = new Label
            {
                Text = statusUpper == "PAGO" ? "Pago" : statusUpper == "CANCELADO" ? "Cancelado" : "Pendente",
                TextColor = Colors.White,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold
            };
            grid.Add(statusBorder, 1, 0);

            // Botão Excluir somente quando pendente
            if (statusUpper == "PENDENTE" || statusUpper == "CANCELADO")
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

        private static string TryGetString(JsonElement element, string propName)
        {
            try
            {
                if (element.ValueKind != JsonValueKind.Object)
                    return null;

                if (!element.TryGetProperty(propName, out var prop))
                    return null;

                switch (prop.ValueKind)
                {
                    case JsonValueKind.String:
                        return prop.GetString();
                    case JsonValueKind.Number:
                        // Retorna o texto bruto (mantém formatação original do JSON)
                        return prop.GetRawText();
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return prop.GetBoolean().ToString();
                    case JsonValueKind.Object:
                    case JsonValueKind.Array:
                        // Para objetos/arrays, retorna o JSON bruto como string (opcional)
                        return prop.GetRawText();
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        // Auxiliares de parsing para reserva fictícia
        private static int SafeGetInt(JsonElement el, string name)
        {
            try
            {
                if (el.ValueKind != JsonValueKind.Object) return 0;
                if (!el.TryGetProperty(name, out var prop)) return 0;
                if (prop.ValueKind == JsonValueKind.Number)
                {
                    if (prop.TryGetInt32(out int v)) return v;
                    var raw = prop.GetRawText();
                    if (int.TryParse(raw, out v)) return v;
                }
                else if (prop.ValueKind == JsonValueKind.String)
                {
                    var s = prop.GetString();
                    if (int.TryParse(s, out int v)) return v;
                }
                return 0;
            }
            catch { return 0; }
        }

        private static decimal SafeGetDecimal(JsonElement el, string name)
        {
            try
            {
                if (el.ValueKind != JsonValueKind.Object) return 0m;
                if (!el.TryGetProperty(name, out var prop)) return 0m;
                if (prop.ValueKind == JsonValueKind.Number)
                {
                    if (prop.TryGetDecimal(out decimal v)) return v;
                    var raw = prop.GetRawText();
                    if (decimal.TryParse(raw, out v)) return v;
                }
                else if (prop.ValueKind == JsonValueKind.String)
                {
                    var s = prop.GetString();
                    if (decimal.TryParse(s, out decimal v)) return v;
                }
                return 0m;
            }
            catch { return 0m; }
        }

        private static DateTime SafeGetDate(JsonElement el, string name)
        {
                var s = TryGetString(el, name);
                if (DateTime.TryParse(s, out var dt)) return dt;
                return DateTime.Now;
        }

        private async void OnVoltarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "Menu",
                "Cancelar",
                null,
                "Configurações", 
                "Ajuda",
                "Sobre"
            );

            if (action == "Configurações")
            {
                await DisplayAlert("Configurações", "Abrindo configurações...", "OK");
            }
            else if (action == "Ajuda")
            {
                await DisplayAlert("Ajuda", "Abrindo central de ajuda...", "OK");
            }
            else if (action == "Sobre")
            {
                await DisplayAlert("Sobre", "App de Reservas - Versão 1.0", "OK");
            }
        }

        private void OnAtivasClicked(object sender, EventArgs e)
        {
            // Já está na página ativa
        }

        private async void OnExcluirClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Confirmar Exclusão", 
                "Deseja realmente excluir esta reserva?",
                "Sim",
                "Não"
            );

            if (answer)
            {
                // Adicione a lógica de DELETE/CANCELAMENTO de reserva no backend aqui
                await DisplayAlert("Sucesso", "Reserva excluída com sucesso!", "OK");
                // Após a exclusão, recarregue a lista:
                // await CarregarReservasAtivasAsync(); 
            }
        }

        private async void OnSairClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Confirmar Saída",
                "Deseja realmente sair?",
                "Sim",
                "Não"
            );

            if (answer)
            {
                await Navigation.PopToRootAsync();
            }
        }

        private async void OnVerTodasClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Todas as Reservas", "Carregando todas as reservas...", "OK");
        }

        private async void OnVerDetalhesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Detalhes", "Abrindo detalhes da reserva...", "OK");
        }

        private async void OnCardClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Border b && b.BindingContext is ReservaExibicao re)
                {
                    await DisplayAlert("Reserva",
                        $"Atividade: {re.AtividadeNome}\nData: {re.DataTexto}\nStatus: {re.StatusUpper}\nValor: {re.Reserva.ValorTotal:N2}",
                        "OK");
                }
                else
                {
                    await DisplayAlert("Reserva", "Abrindo detalhes da reserva...", "OK");
                }
            }
            catch
            {
                await DisplayAlert("Reserva", "Abrindo detalhes da reserva...", "OK");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(searchText))
            {
                RenderizarReservas(_reservasExibicao);
                return;
            }

            var q = searchText.ToLowerInvariant();
            var filtrado = _reservasExibicao.Where(r =>
                (r.AtividadeNome ?? string.Empty).ToLowerInvariant().Contains(q) ||
                (r.DataTexto ?? string.Empty).ToLowerInvariant().Contains(q) ||
                (r.StatusUpper ?? string.Empty).ToLowerInvariant().Contains(q)
            );
            RenderizarReservas(filtrado);
        }
    }
}