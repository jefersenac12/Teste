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

        // Endpoints (ajuste se necessário)
        private readonly string apiUrlAgenda = "http://tiijeferson.runasp.net/api/usuario/Agenda";
        private readonly string apiUrlReserva = "http://tiijeferson.runasp.net/api/usuario/Reserva";
        private readonly string apiUrlPagamento = "http://tiijeferson.runasp.net/api/usuario/Pagamento";
        private readonly string apiUrlAtividade = "http://tiijeferson.runasp.net/api/usuario/Atividade";

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
                foreach (var a in atividades)
                    atividadesCarregadas[a.Id] = a;
            }
            catch (Exception ex)
            {
                // fallback para evitar que a página quebre
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
                string atividadesIdsStr = Preferences.Get("AtividadeIds", "");
                string totalStr = Preferences.Get("TotalEstimado", "0");

                int adultos = Preferences.Get("QtdAdultos", 0);
                int criancas0a5 = Preferences.Get("QtdCriancas0a5", 0);
                int criancas6a12 = Preferences.Get("QtdCriancas6a12", 0);

                // Data: tenta múltiplos formatos
                DateTime dataSelecionada;
                if (!TryParseDateOnly(dataStr, out dataSelecionada))
                    dataSelecionada = DateTime.Today;

                TimeSpan horario;
                if (!TimeSpan.TryParse(horarioStr, out horario))
                    horario = TimeSpan.Zero;

                safraId = Preferences.Get("SafraId", 0);
                string safraNome = Preferences.Get("SafraNome", "");

                List<string> frutasSafra = new();
                var tituloPorMes = ObterSafraPorMes(dataSelecionada.Month, out frutasSafra);
                if (string.IsNullOrWhiteSpace(safraNome))
                {
                    safraNome = tituloPorMes;
                    Preferences.Set("SafraId", safraId);
                    Preferences.Set("SafraNome", safraNome);
                }

                // IDs de atividades
                atividadeIds = new();
                if (!string.IsNullOrWhiteSpace(atividadesIdsStr))
                {
                    atividadeIds = atividadesIdsStr
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var v) ? v : 0)
                        .Where(v => v > 0)
                        .ToList();
                }
                else if (!string.IsNullOrWhiteSpace(atividadesStr))
                {
                    var nomes = atividadesStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var nome in nomes)
                    {
                        var id = MapearNomeParaId(nome.Trim());
                        if (id.HasValue) atividadeIds.Add(id.Value);
                    }
                    if (atividadeIds.Any())
                        Preferences.Set("AtividadeIds", string.Join(",", atividadeIds));
                }

                // Atualiza labels (só se existirem no XAML)
                if (LblDataHorario != null) LblDataHorario.Text = $"{dataSelecionada:dd/MM/yyyy} - {horario:hh\\:mm}";
                if (LblPessoas != null) LblPessoas.Text = $"{adultos} Adultos, {criancas0a5} Crianças (0–5), {criancas6a12} Crianças (6–12)";
                if (LblAtividades != null) LblAtividades.Text = string.IsNullOrWhiteSpace(atividadesStr) ? "Nenhuma atividade selecionada" : atividadesStr;

                RenderizarFrutasSafra(frutasSafra);

                // Total: tenta usar TotalEstimado se válido, senão recalcula
                if (!decimal.TryParse(totalStr, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out var parsedTotal))
                {
                    totalAmount = RecalcularTotal(adultos, criancas6a12, atividadeIds);
                }
                else
                {
                    totalAmount = parsedTotal;
                }

                if (LblTotal != null) LblTotal.Text = $"R${totalAmount:F2}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao carregar dados: {ex.Message}", "OK");
            }
        }

        private static bool TryParseDateOnly(string dateStr, out DateTime date)
        {
            date = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(dateStr)) return false;

            var pt = CultureInfo.GetCultureInfo("pt-BR");

            // tenta ISO primeiro
            if (DateTime.TryParse(dateStr, out var dt))
            {
                date = dt.Date;
                return true;
            }

            // tenta dd/MM/yyyy
            if (DateTime.TryParseExact(dateStr, new[] { "dd/MM/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "o" }, pt, DateTimeStyles.None, out var dt2))
            {
                date = dt2.Date;
                return true;
            }

            return false;
        }

        private decimal RecalcularTotal(int adultos, int criancas6a12, List<int> atividadeIds)
        {
            decimal total = criancas6a12 * 12.50m;

            foreach (var id in atividadeIds)
            {
                if (atividadesCarregadas.TryGetValue(id, out var atv))
                    total += adultos * atv.Valor;
            }

            return total;
        }

        private static int? MapearNomeParaId(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return null;
            var lower = nome.ToLowerInvariant();
            if (lower.Contains("básico") || lower.Contains("basico")) return 1;
            if (lower.Contains("completo")) return 2;
            if (lower.Contains("trezinho") || lower.Contains("colha")) return 3;
            return null;
        }

        private async void OnCopyPixKeyClicked(object sender, EventArgs e)
        {
            try
            {
                await Clipboard.SetTextAsync(pixKey);
                await DisplayAlert("Sucesso", "Chave Pix copiada!", "OK");
            }
            catch
            {
                await DisplayAlert("Erro", "Falha ao copiar a chave Pix.", "OK");
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

        private async void OnConfirmPaymentClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comprovantePath))
            {
                bool continuar = await DisplayAlert("Aviso", "Sem comprovante anexado. Deseja continuar?", "Sim", "Não");
                if (!continuar) return;
            }

            bool confirmar = await DisplayAlert("Confirmar Pagamento", $"Deseja confirmar o pagamento de R${totalAmount:F2}?", "Sim", "Não");
            if (!confirmar) return;

            try
            {
                await SalvarReservaEPagamento();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao salvar reserva: {ex.Message}", "OK");
            }
        }

        private async Task SalvarReservaEPagamento()
        {
            try
            {
                // Tenta recuperar UsuarioId; aceita fallback ClienteId para compatibilidade com sessões antigas
                int usuarioId = Preferences.Get("UsuarioId", 0);
                if (usuarioId == 0) usuarioId = Preferences.Get("ClienteId", 0);

                if (usuarioId == 0)
                {
                    await DisplayAlert("Erro", "Usuário não identificado. Faça login novamente.", "OK");
                    return;
                }

                // Data/hora: prefere Preferences (data iso ou dd/MM/yyyy) + HorarioSelecionado
                string dataStr = Preferences.Get("DataAgendamento", "");
                string horarioStr = Preferences.Get("HorarioSelecionado", "");

                DateTime dataHora;
                var pt = CultureInfo.GetCultureInfo("pt-BR");

                // tenta combinar data + horário
                if (!string.IsNullOrWhiteSpace(dataStr) && !string.IsNullOrWhiteSpace(horarioStr)
                    && DateTime.TryParseExact($"{dataStr} {horarioStr}",
                        new[] { "yyyy-MM-dd HH:mm", "dd/MM/yyyy HH:mm", "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "o" }, pt, DateTimeStyles.None, out var dtComb))
                {
                    dataHora = dtComb;
                }
                else if (TryParseDateOnly(dataStr, out var dOnly) && TimeSpan.TryParse(horarioStr, out var ts))
                {
                    dataHora = dOnly.Date.Add(ts);
                }
                else if (TryParseDateOnly(dataStr, out var dOnly2))
                {
                    dataHora = dOnly2.Date;
                }
                else
                {
                    await DisplayAlert("Erro", "Data/hora inválida.", "OK");
                    return;
                }

                int adultos = Preferences.Get("QtdAdultos", 0);
                int criancas0a5 = Preferences.Get("QtdCriancas0a5", 0);
                int criancas6a12 = Preferences.Get("QtdCriancas6a12", 0);
                int quantidadeTotal = adultos + criancas6a12 + criancas0a5;
                if (quantidadeTotal <= 0)
                {
                    await DisplayAlert("Erro", "Quantidade de pessoas inválida.", "OK");
                    return;
                }

                // Garante que atividadeIds esteja preenchido (tenta ler Preferences como fallback)
                if (atividadeIds == null || !atividadeIds.Any())
                {
                    var atividadesIdsStr = Preferences.Get("AtividadeIds", "");
                    if (!string.IsNullOrWhiteSpace(atividadesIdsStr))
                    {
                        atividadeIds = atividadesIdsStr
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s.Trim(), out var v) ? v : 0)
                            .Where(v => v > 0)
                            .ToList();
                    }
                }

                if (atividadeIds == null || !atividadeIds.Any())
                {
                    await DisplayAlert("Erro", "Nenhuma atividade selecionada.", "OK");
                    return;
                }

                int atividadeId = atividadeIds.First();
                if (atividadeId == 0)
                {
                    await DisplayAlert("Erro", "ID de atividade inválido.", "OK");
                    return;
                }

                // Criar ou buscar agenda
                int agendaId = await CriarOuBuscarAgenda(dataHora, atividadeId);
                if (agendaId == 0)
                {
                    await DisplayAlert("Erro", "Falha ao criar/obter agenda.", "OK");
                    return;
                }

                // Validar vagas (se endpoint suportar)
                bool vagas = await ValidarVagasDisponiveis(agendaId, quantidadeTotal);
                if (!vagas)
                {
                    await DisplayAlert("Vagas Esgotadas", "Não há vagas suficientes.", "OK");
                    return;
                }

                // Monta reserva
                var novaReserva = new
                {
                    AgendaId = agendaId,
                    UsuarioId = usuarioId,
                    Quantidade = quantidadeTotal,
                    InteiraEntrada = adultos,
                    MeiaEntrada = criancas6a12,
                    NPEntrada = criancas0a5,
                    ValorTotal = totalAmount,
                    DataReserva = dataHora.ToString("o")
                };

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var jsonReserva = JsonSerializer.Serialize(novaReserva, opts);
                var contentReserva = new StringContent(jsonReserva, Encoding.UTF8, "application/json");

                var responseReserva = await client.PostAsync(apiUrlReserva, contentReserva);

                if (!responseReserva.IsSuccessStatusCode)
                {
                    var erroText = await SafeReadContent(responseReserva);
                    await DisplayAlert("Erro", $"Falha ao criar reserva: HTTP {(int)responseReserva.StatusCode}\n{erroText}", "OK");
                    return;
                }

                // Ler ID retornado da reserva (se houver)
                int reservaId = 0;
                try
                {
                    var jsonResp = await responseReserva.Content.ReadAsStringAsync();
                    var doc = JsonSerializer.Deserialize<JsonElement>(jsonResp, opts);

                    if (doc.ValueKind == JsonValueKind.Object)
                    {
                        if (doc.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number) reservaId = idEl.GetInt32();
                        else if (doc.TryGetProperty("Id", out var idEl2) && idEl2.ValueKind == JsonValueKind.Number) reservaId = idEl2.GetInt32();
                    }
                    else if (doc.ValueKind == JsonValueKind.Number)
                    {
                        reservaId = doc.GetInt32();
                    }
                    else if (doc.ValueKind == JsonValueKind.String)
                    {
                        int.TryParse(doc.GetString(), out reservaId);
                    }
                }
                catch
                {
                    // reservaId pode ficar 0; continuamos, mas idealmente o backend retorna o id
                }

                // Monta pagamento usando o reservaId obtido (se 0, backend deve relacionar por outro campo)
                string statusPagamento = string.IsNullOrEmpty(comprovantePath) ? "PENDENTE" : "PAGO";
                var novoPagamento = new
                {
                    ReservaId = reservaId,
                    Valor = totalAmount,
                    Metodo = "PIX",
                    Status = statusPagamento,
                    DataPagamento = DateTime.Now.ToString("o")
                };

                var jsonPagamento = JsonSerializer.Serialize(novoPagamento, opts);
                var responsePagamento = await client.PostAsync(apiUrlPagamento, new StringContent(jsonPagamento, Encoding.UTF8, "application/json"));

                if (!responsePagamento.IsSuccessStatusCode)
                {
                    var erroPag = await SafeReadContent(responsePagamento);
                    await DisplayAlert("Aviso", $"Reserva criada (id {reservaId}) mas pagamento falhou: HTTP {(int)responsePagamento.StatusCode}\n{erroPag}", "OK");
                }
                else
                {
                    await DisplayAlert("Sucesso", "Reserva e pagamento processados com sucesso.", "OK");
                }

                Preferences.Set("UltimaReservaId", reservaId);
                // navega para ReservasPage
                try { await Navigation.PushAsync(new ReservasPage()); }
                catch { await Navigation.PopToRootAsync(); }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro inesperado", $"Erro ao salvar reserva/pagamento: {ex.Message}", "OK");
            }
        }

        // Lê corpo da resposta de forma segura
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

        // Valida vagas na agenda (tenta extrair propriedades comuns)
        private async Task<bool> ValidarVagasDisponiveis(int agendaId, int quantidadeSolicitada)
        {
            try
            {
                var resp = await client.GetAsync($"{apiUrlAgenda}/{agendaId}");
                if (!resp.IsSuccessStatusCode) return true; // fallback permissivo

                var json = await resp.Content.ReadAsStringAsync();
                var el = JsonSerializer.Deserialize<JsonElement>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                int vagasDisponiveis = 50;
                if (el.ValueKind == JsonValueKind.Object)
                {
                    if (el.TryGetProperty("vagasDisponiveis", out var vEl) && vEl.ValueKind == JsonValueKind.Number) vagasDisponiveis = vEl.GetInt32();
                    else if (el.TryGetProperty("VagasDisponiveis", out var vEl2) && vEl2.ValueKind == JsonValueKind.Number) vagasDisponiveis = vEl2.GetInt32();
                    else if (el.TryGetProperty("vagasTotais", out var vt) && vt.ValueKind == JsonValueKind.Number) vagasDisponiveis = vt.GetInt32();
                }

                return vagasDisponiveis >= quantidadeSolicitada;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ValidarVagasDisponiveis erro: {ex.Message}");
                return true; // fallback
            }
        }

        private async Task<int> CriarOuBuscarAgenda(DateTime dataHora, int atividadeId)
        {
            try
            {
                if (atividadeId == 0) return 0;

                safraId = safraId == 0 ? Preferences.Get("SafraId", 0) : safraId;
                string dataHoraIso = dataHora.ToString("yyyy-MM-ddTHH:mm:ss");

                var urls = new[]
                {
                    $"{apiUrlAgenda}/buscar?dataHora={Uri.EscapeDataString(dataHoraIso)}&safraId={safraId}&atividadeId={atividadeId}",
                    $"{apiUrlAgenda}/search?dataHora={Uri.EscapeDataString(dataHoraIso)}&safraId={safraId}&atividadeId={atividadeId}",
                    $"{apiUrlAgenda}?dataHora={Uri.EscapeDataString(dataHoraIso)}&safraId={safraId}&atividadeId={atividadeId}"
                };

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                foreach (var url in urls)
                {
                    try
                    {
                        var resp = await client.GetAsync(url);
                        if (!resp.IsSuccessStatusCode) continue;

                        var json = await resp.Content.ReadAsStringAsync();
                        var el = JsonSerializer.Deserialize<JsonElement>(json, opts);

                        if (el.ValueKind == JsonValueKind.Object)
                        {
                            if (el.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number) return idEl.GetInt32();
                            if (el.TryGetProperty("Id", out var idEl2) && idEl2.ValueKind == JsonValueKind.Number) return idEl2.GetInt32();
                        }
                        else if (el.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in el.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.Object)
                                {
                                    if (item.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number) return idEl.GetInt32();
                                    if (item.TryGetProperty("Id", out var idEl2) && idEl2.ValueKind == JsonValueKind.Number) return idEl2.GetInt32();
                                }
                            }
                        }
                    }
                    catch { /* tenta próxima URL */ }
                }

                // Se não encontrou, cria uma agenda
                var novaAgenda = new
                {
                    SafraId = safraId,
                    AtividadeId = atividadeId,
                    DataHora = dataHoraIso,
                    VagasTotais = 50,
                    VagasDisponiveis = 50
                };

                var jsonNova = JsonSerializer.Serialize(novaAgenda, opts);
                var content = new StringContent(jsonNova, Encoding.UTF8, "application/json");
                var post = await client.PostAsync(apiUrlAgenda, content);

                if (!post.IsSuccessStatusCode) return 0;

                var jsonResp = await post.Content.ReadAsStringAsync();
                var elResp = JsonSerializer.Deserialize<JsonElement>(jsonResp, opts);

                if (elResp.ValueKind == JsonValueKind.Object)
                {
                    if (elResp.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number) return idProp.GetInt32();
                    if (elResp.TryGetProperty("Id", out var idProp2) && idProp2.ValueKind == JsonValueKind.Number) return idProp2.GetInt32();
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
                { 12, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango", "🍒 Lichia" } },
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
                var chip = new Border
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
                };
                FrutasSafraFlex.Children.Add(chip);
            }
        }
    }
}

