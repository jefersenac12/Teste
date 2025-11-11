using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace Teste
{
    public class AtividadePagamento
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public bool Ativa { get; set; }
    }

    public partial class PagamentoPage : ContentPage
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly string apiUrlAgenda = "http://tiijeferson.runasp.net/api/Agenda";
        private readonly string apiUrlReserva = "http://tiijeferson.runasp.net/api/Reserva";
        private readonly string apiUrlPagamento = "http://tiijeferson.runasp.net/api/Pagamento";
        private readonly string apiUrlAtividade = "http://tiijeferson.runasp.net/api/Atividade";

        private decimal totalAmount = 0;
        private readonly string pixKey = "fazenda.villaggio@pix.com.br";
        private string comprovantePath = string.Empty;
        private int safraId = 0;

        private List<int> atividadeIds = new();
        private readonly Dictionary<int, AtividadePagamento> atividadesCarregadas = new();

        public string Status { get; set; } = "PENDENTE";

        public PagamentoPage()
        {
            InitializeComponent();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await CarregarAtividades();
            await CarregarDadosReserva();
        }

        private async Task CarregarAtividades()
        {
            try
            {
                var resp = await client.GetAsync(apiUrlAtividade);
                if (!resp.IsSuccessStatusCode)
                {
                    CriarAtividadesFicticias();
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var atividades = JsonSerializer.Deserialize<List<AtividadePagamento>>(json, opts);

                if (atividades == null || atividades.Count == 0)
                {
                    CriarAtividadesFicticias();
                    return;
                }

                atividadesCarregadas.Clear();
                foreach (var a in atividades.Where(a => a.Ativa))
                    atividadesCarregadas[a.Id] = a;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarAtividades erro: {ex.Message}");
                CriarAtividadesFicticias();
            }
        }

        private void CriarAtividadesFicticias()
        {
            atividadesCarregadas.Clear();
            atividadesCarregadas[1] = new() { Id = 1, Nome = "Café da manhã Básico", Valor = 25.00m, Ativa = true };
            atividadesCarregadas[2] = new() { Id = 2, Nome = "Café da manhã Completo", Valor = 65.00m, Ativa = true };
            atividadesCarregadas[3] = new() { Id = 3, Nome = "Trezinho / Colha e Pague", Valor = 15.00m, Ativa = true };
        }

        private async Task CarregarDadosReserva()
        {
            try
            {
                string dataStr = Preferences.Get("DataAgendamento", "");
                string horarioStr = Preferences.Get("HorarioSelecionado", "");
                string atividadesStr = Preferences.Get("AtividadesSelecionadas", "");
                string atividadesIdsStr = Preferences.Get("AtividadesIds", "");
                string valorTotalEstimado = Preferences.Get("ValorTotalEstimado", "0");
                string atividadesValoresStr = Preferences.Get("AtividadesValores", "");

                int adultos = Preferences.Get("QtdAdultos", 0);
                int criancas0a5 = Preferences.Get("QtdCriancas0a5", 0);
                int criancas6a12 = Preferences.Get("QtdCriancas6a12", 0);

                DateTime dataSelecionada;
                if (!TryParseDateOnly(dataStr, out dataSelecionada))
                    dataSelecionada = DateTime.Today;

                TimeSpan horario;
                if (!TimeSpan.TryParse(horarioStr, out horario))
                    horario = TimeSpan.Zero;

                safraId = Preferences.Get("SafraId", 0);
                if (safraId == 0)
                    safraId = dataSelecionada.Month;

                string safraNome = Preferences.Get("SafraNome", "");

                List<string> frutasSafra = new();
                var tituloPorMes = ObterSafraPorMes(dataSelecionada.Month, out frutasSafra);
                if (string.IsNullOrWhiteSpace(safraNome))
                {
                    safraNome = tituloPorMes;
                    Preferences.Set("SafraNome", safraNome);
                    Preferences.Set("SafraId", safraId);
                }

                atividadeIds = new();
                if (!string.IsNullOrWhiteSpace(atividadesIdsStr))
                {
                    atividadeIds = atividadesIdsStr
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var v) ? v : 0)
                        .Where(v => v > 0)
                        .ToList();
                }

                if (LblDataHorario != null)
                    LblDataHorario.Text = $"{dataSelecionada:dd/MM/yyyy} - {horario:hh\\:mm}";

                if (LblPessoas != null)
                    LblPessoas.Text = $"{adultos} Adultos, {criancas0a5} Crianças (0–5), {criancas6a12} Crianças (6–12)";

                if (LblAtividades != null)
                    LblAtividades.Text = string.IsNullOrWhiteSpace(atividadesStr) ? "Nenhuma atividade selecionada" : atividadesStr;

                RenderizarFrutasSafra(frutasSafra);

                // SEMPRE recalcula o total para garantir precisão
                totalAmount = RecalcularTotal(adultos, criancas6a12, atividadeIds, atividadesValoresStr);

                System.Diagnostics.Debug.WriteLine($"Total recalculado: {totalAmount} (Adultos: {adultos}, Crianças6-12: {criancas6a12}, Atividades: {atividadeIds.Count})");

                if (LblTotal != null)
                    LblTotal.Text = $"R$ {totalAmount:F2}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarDadosReserva erro: {ex.Message}");
                await DisplayAlert("Erro", $"Falha ao carregar dados: {ex.Message}", "OK");
            }
        }

        private static bool TryParseDateOnly(string dateStr, out DateTime date)
        {
            date = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(dateStr)) return false;

            // Tenta múltiplos formatos
            var formatos = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy-MM-ddTHH:mm:ss", "o" };
            return DateTime.TryParseExact(dateStr, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                || DateTime.TryParse(dateStr, out date);
        }

        private decimal RecalcularTotal(int adultos, int criancas6a12, List<int> atividadeIds, string atividadesValoresStr = "")
        {
            decimal total = criancas6a12 * 12.50m; // Meia entrada para crianças 6-12 anos

            // PRIMEIRO: Tenta usar os valores salvos das atividades (mais confiável)
            if (!string.IsNullOrEmpty(atividadesValoresStr))
            {
                var valores = atividadesValoresStr.Split(',')
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m)
                    .Where(v => v > 0)
                    .ToList();

                foreach (var valor in valores)
                {
                    total += adultos * valor;
                    System.Diagnostics.Debug.WriteLine($"Adicionando atividade: {adultos} x {valor} = {adultos * valor}");
                }

                if (valores.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Total calculado com valores salvos: {total}");
                    return total;
                }
            }

            // SEGUNDO: Fallback - usa as atividades carregadas da API
            foreach (var id in atividadeIds)
            {
                if (atividadesCarregadas.TryGetValue(id, out var atv))
                {
                    total += adultos * atv.Valor;
                    System.Diagnostics.Debug.WriteLine($"Adicionando atividade API: {adultos} x {atv.Valor} = {adultos * atv.Valor}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Total final: {total}");
            return total;
        }

        private async void OnCopyPixKeyClicked(object sender, EventArgs e)
        {
            try
            {
                await Clipboard.SetTextAsync(pixKey);
                await DisplayAlert("Sucesso", "Chave Pix copiada!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao copiar: {ex.Message}", "OK");
            }
        }

        private async void OnUploadProofClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o comprovante de pagamento",
                    FileTypes = FilePickerFileType.Images
                });

                if (result == null) return;

                comprovantePath = result.FullPath;
                if (ImgComprovantePreview != null)
                {
                    ImgComprovantePreview.Source = ImageSource.FromFile(comprovantePath);
                    ImgComprovantePreview.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao anexar o comprovante: {ex.Message}", "OK");
            }
        }

        private async void OnConfirmPaymentClicked(object sender, TappedEventArgs e)
        {
            if (string.IsNullOrEmpty(comprovantePath))
            {
                bool continuar = await DisplayAlert("Aviso", "Sem comprovante anexado. Deseja continuar?", "Sim", "Não");
                if (!continuar) return;
            }

            bool confirmar = await DisplayAlert("Confirmar Pagamento", $"Deseja confirmar o pagamento de R$ {totalAmount:F2}?", "Sim", "Não");
            if (!confirmar) return;

            var border = (sender as TapGestureRecognizer)?.Parent as Border;
            if (border != null)
                border.IsEnabled = false;

            try
            {
                await SalvarReservaEPagamento();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnConfirmPaymentClicked erro: {ex.Message}\n{ex.StackTrace}");
                await DisplayAlert("Erro", $"Erro ao salvar reserva: {ex.Message}", "OK");
                if (border != null)
                    border.IsEnabled = true;
            }
        }

        private async Task SalvarReservaEPagamento()
        {
            try
            {
                int usuarioId = Preferences.Get("UsuarioId", 0);
                if (usuarioId == 0) usuarioId = Preferences.Get("ClienteId", 0);

                if (usuarioId == 0)
                {
                    await DisplayAlert("Erro", "Usuário não identificado. Faça login novamente.", "OK");
                    return;
                }

                string dataStr = Preferences.Get("DataAgendamento", "");
                string horarioStr = Preferences.Get("HorarioSelecionado", "");
                string atividadesValoresStr = Preferences.Get("AtividadesValores", "");

                DateTime dataHora;
                if (!TryParseDateOnly(dataStr, out dataHora))
                {
                    await DisplayAlert("Erro", "Data inválida.", "OK");
                    return;
                }

                // Adiciona o horário à data
                if (TimeSpan.TryParse(horarioStr, out var horario))
                {
                    dataHora = dataHora.Date.Add(horario);
                }

                System.Diagnostics.Debug.WriteLine($"DataHora combinada: {dataHora:yyyy-MM-dd HH:mm:ss}");

                int adultos = Preferences.Get("QtdAdultos", 0);
                int criancas0a5 = Preferences.Get("QtdCriancas0a5", 0);
                int criancas6a12 = Preferences.Get("QtdCriancas6a12", 0);
                int quantidadeTotal = adultos + criancas0a5 + criancas6a12;

                if (quantidadeTotal <= 0)
                {
                    await DisplayAlert("Erro", "Quantidade de pessoas inválida.", "OK");
                    return;
                }

                int atividadeId = atividadeIds.FirstOrDefault();
                if (atividadeId == 0)
                {
                    await DisplayAlert("Erro", "Nenhuma atividade selecionada.", "OK");
                    return;
                }

                // Recalcula o valor final usando a mesma lógica
                decimal valorFinal = RecalcularTotal(adultos, criancas6a12, atividadeIds, atividadesValoresStr);
                if (valorFinal <= 0)
                {
                    await DisplayAlert("Erro", "Valor total inválido.", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Valor final calculado: {valorFinal}");

                // Tenta criar agenda (não bloqueia se falhar)
                int agendaId = 0;
                try
                {
                    agendaId = await CriarOuBuscarAgenda(dataHora, atividadeId);
                    System.Diagnostics.Debug.WriteLine($"AgendaId obtido: {agendaId}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao criar agenda (continuando): {ex.Message}");
                }

                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // Cria a reserva
                var novaReserva = new
                {
                    AgendaId = agendaId > 0 ? agendaId : (int?)null,
                    UsuarioId = usuarioId,
                    Quantidade = quantidadeTotal,
                    InteiraEntrada = adultos,
                    MeiaEntrada = criancas6a12,
                    NPEntrada = criancas0a5,
                    ValorTotal = Math.Round(valorFinal, 2),
                    DataReserva = dataHora.ToString("yyyy-MM-ddTHH:mm:ss")
                };

                var jsonReserva = JsonSerializer.Serialize(novaReserva, opts);
                System.Diagnostics.Debug.WriteLine($"Enviando reserva: {jsonReserva}");

                var contentReserva = new StringContent(jsonReserva, Encoding.UTF8, "application/json");
                var responseReserva = await client.PostAsync(apiUrlReserva, contentReserva);

                if (!responseReserva.IsSuccessStatusCode)
                {
                    var erro = await SafeReadContent(responseReserva);
                    System.Diagnostics.Debug.WriteLine($"Erro ao criar reserva: HTTP {(int)responseReserva.StatusCode} - {erro}");
                    await DisplayAlert("Erro", $"Falha ao criar reserva: {erro}", "OK");
                    return;
                }

                // Lê o ID da reserva
                string jsonResp = await responseReserva.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Resposta da reserva: {jsonResp}");

                int reservaId = 0;
                try
                {
                    if (int.TryParse(jsonResp, out var idSimples))
                    {
                        reservaId = idSimples;
                    }
                    else
                    {
                        var doc = JsonSerializer.Deserialize<JsonElement>(jsonResp, opts);
                        if (doc.ValueKind == JsonValueKind.Object)
                        {
                            if (doc.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number)
                                reservaId = idEl.GetInt32();
                            else if (doc.TryGetProperty("Id", out var idEl2) && idEl2.ValueKind == JsonValueKind.Number)
                                reservaId = idEl2.GetInt32();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao parsear reservaId: {ex.Message}");
                }

                if (reservaId == 0)
                {
                    await DisplayAlert("Aviso", "Reserva criada mas ID não foi retornado. Verifique suas reservas.", "OK");
                    await Navigation.PushAsync(new ReservasPage());
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Reserva criada com ID: {reservaId}");
                Preferences.Set("UltimaReservaId", reservaId);

                // Cria o pagamento
                var novoPagamento = new
                {
                    ReservaId = reservaId,
                    Valor = Math.Round(valorFinal, 2),
                    Metodo = "PIX",
                    Status = string.IsNullOrEmpty(comprovantePath) ? "PENDENTE" : "PAGO",
                    DataPagamento = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                };

                var jsonPagamento = JsonSerializer.Serialize(novoPagamento, opts);
                System.Diagnostics.Debug.WriteLine($"Enviando pagamento: {jsonPagamento}");

                var responsePagamento = await client.PostAsync(apiUrlPagamento,
                    new StringContent(jsonPagamento, Encoding.UTF8, "application/json"));

                if (responsePagamento.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Pagamento criado com sucesso");
                    await DisplayAlert("Sucesso", $"Reserva (ID: {reservaId}) e pagamento processados com sucesso!\nValor: R$ {valorFinal:F2}", "OK");
                }
                else
                {
                    var erroPag = await SafeReadContent(responsePagamento);
                    System.Diagnostics.Debug.WriteLine($"Erro no pagamento: {erroPag}");
                    await DisplayAlert("Aviso", $"Reserva criada (ID: {reservaId}), mas pagamento falhou.", "OK");
                }

                // Limpa os dados temporários
                Preferences.Remove("AtividadesValores");
                Preferences.Remove("AtividadesSelecionadas");
                Preferences.Remove("AtividadesIds");
                Preferences.Remove("ValorTotalEstimado");

                // Navega para página de reservas
                await Navigation.PushAsync(new ReservasPage());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SalvarReservaEPagamento erro: {ex.Message}\n{ex.StackTrace}");
                await DisplayAlert("Erro inesperado", $"Erro: {ex.Message}", "OK");
            }
        }

        private static async Task<string> SafeReadContent(HttpResponseMessage resp)
        {
            try
            {
                return await resp.Content.ReadAsStringAsync();
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<int> CriarOuBuscarAgenda(DateTime dataHora, int atividadeId)
        {
            try
            {
                if (atividadeId == 0) return 0;

                if (safraId == 0)
                    safraId = dataHora.Month;

                string dataHoraIso = dataHora.ToString("yyyy-MM-ddTHH:mm:ss");
                System.Diagnostics.Debug.WriteLine($"CriarOuBuscarAgenda: AtivId={atividadeId}, SafraId={safraId}, Data={dataHoraIso}");

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Tenta buscar agenda existente
                var urlsBusca = new[]
                {
                    $"{apiUrlAgenda}/buscar?dataHora={Uri.EscapeDataString(dataHoraIso)}&safraId={safraId}&atividadeId={atividadeId}",
                    $"{apiUrlAgenda}/search?atividadeId={atividadeId}&safraId={safraId}",
                    $"{apiUrlAgenda}?atividadeId={atividadeId}"
                };

                foreach (var url in urlsBusca)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Buscando em: {url}");
                        var resp = await client.GetAsync(url);

                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Resposta busca: {json}");

                            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "[]")
                                continue;

                            if (int.TryParse(json, out var idSimples))
                                return idSimples;

                            var el = JsonSerializer.Deserialize<JsonElement>(json, opts);

                            if (el.ValueKind == JsonValueKind.Object)
                            {
                                if (el.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number)
                                    return idEl.GetInt32();
                                if (el.TryGetProperty("Id", out var idEl2) && idEl2.ValueKind == JsonValueKind.Number)
                                    return idEl2.GetInt32();
                            }
                            else if (el.ValueKind == JsonValueKind.Array && el.GetArrayLength() > 0)
                            {
                                var primeiro = el.EnumerateArray().First();
                                if (primeiro.TryGetProperty("id", out var idEl))
                                    return idEl.GetInt32();
                                if (primeiro.TryGetProperty("Id", out var idEl2))
                                    return idEl2.GetInt32();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao buscar em {url}: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("Agenda não encontrada, criando nova...");

                // Cria nova agenda
                var novaAgenda = new
                {
                    SafraId = safraId,
                    AtividadeId = atividadeId,
                    DataHora = dataHoraIso,
                    VagasTotais = 50,
                    VagasDisponiveis = 50
                };

                var jsonNova = JsonSerializer.Serialize(novaAgenda, opts);
                System.Diagnostics.Debug.WriteLine($"Criando agenda: {jsonNova}");

                var post = await client.PostAsync(apiUrlAgenda,
                    new StringContent(jsonNova, Encoding.UTF8, "application/json"));

                if (!post.IsSuccessStatusCode)
                {
                    var erro = await SafeReadContent(post);
                    System.Diagnostics.Debug.WriteLine($"Erro ao criar agenda: {erro}");
                    return 0;
                }

                var jsonResp = await post.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Resposta criação agenda: {jsonResp}");

                if (int.TryParse(jsonResp, out var idNum))
                    return idNum;

                var elResp = JsonSerializer.Deserialize<JsonElement>(jsonResp, opts);
                if (elResp.ValueKind == JsonValueKind.Object)
                {
                    if (elResp.TryGetProperty("id", out var idEl))
                        return idEl.GetInt32();
                    if (elResp.TryGetProperty("Id", out var idEl2))
                        return idEl2.GetInt32();
                }
                else if (elResp.ValueKind == JsonValueKind.Number)
                {
                    return elResp.GetInt32();
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CriarOuBuscarAgenda erro: {ex.Message}");
                return 0;
            }
        }

        private string ObterSafraPorMes(int mes, out List<string> frutas)
        {
            var cronograma = new Dictionary<int, List<string>>
            {
                { 1, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango", "🍒 Lichia" } },
                { 2, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango" } },
                { 3, new() { "🍇 Uva", "🍈 Goiaba" } },
                { 4, new() { "🍈 Goiaba", "🍓 Morango" } },
                { 10, new() { "🍑 Pêssego", "🍓 Morango", "🍈 Goiaba" } },
                { 11, new() { "🍑 Pêssego", "🍓 Morango", "🍈 Goiaba" } },
                { 12, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango", "🍒 Lichia" } }
            };

            frutas = cronograma.GetValueOrDefault(mes, new List<string>());
            safraId = mes;
            var cultura = CultureInfo.GetCultureInfo("pt-BR");
            return $"Safra de {cultura.TextInfo.ToTitleCase(cultura.DateTimeFormat.GetMonthName(mes))}";
        }

        private void RenderizarFrutasSafra(List<string> frutas)
        {
            if (FrutasSafraFlex == null) return;
            FrutasSafraFlex.Children.Clear();

            if (frutas == null || frutas.Count == 0)
            {
                FrutasSafraFlex.Children.Add(new Label { Text = "Sem frutas em safra", TextColor = Colors.Gray });
                return;
            }

            foreach (var fruta in frutas)
            {
                FrutasSafraFlex.Children.Add(new Border
                {
                    BackgroundColor = Color.FromArgb("#F9F3EF"),
                    StrokeThickness = 1,
                    Stroke = Color.FromArgb("#F1E5E2"),
                    Padding = 8,
                    Margin = new Thickness(0, 4, 6, 4),
                    StrokeShape = new RoundRectangle { CornerRadius = 10 },
                    Content = new Label
                    {
                        Text = fruta,
                        FontSize = 12,
                        TextColor = Color.FromArgb("#A42D45")
                    }
                });
            }
        }
    }
}