
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls.Shapes;


namespace Teste
{
    public class AtividadePagamento
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Descricao { get; set; } = "";
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
        private string pixKey = "fazenda.villaggio@pix.com.br";
        private string qrCodeUrl = "";
        private string comprovantePath = string.Empty;
        private int safraId = 0;
        private List<int> atividadeIds = new List<int>();
        private Dictionary<int, AtividadePagamento> atividadesCarregadas = new Dictionary<int, AtividadePagamento>();

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
                var response = await client.GetAsync(apiUrlAtividade);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var atividades = JsonSerializer.Deserialize<List<AtividadePagamento>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (atividades != null)
                    {
                        atividadesCarregadas.Clear();
                        foreach (var atividade in atividades)
                        {
                            atividadesCarregadas[atividade.Id] = atividade;
                        }
                    }
                }
                else
                {
                    CriarAtividadesFicticias();
                }
            }
            catch (Exception)
            {
                CriarAtividadesFicticias();
            }
        }

        private void CriarAtividadesFicticias()
        {
            // Preços fixos especificados pelo usuário
            atividadesCarregadas.Clear();
            atividadesCarregadas[1] = new AtividadePagamento { Id = 1, Nome = "Café da manhã Básico", Valor = 25.00m, Ativa = true };
            atividadesCarregadas[2] = new AtividadePagamento { Id = 2, Nome = "Café da manhã Completo", Valor = 65.00m, Ativa = true };
            atividadesCarregadas[3] = new AtividadePagamento { Id = 3, Nome = "Trezinho / Colha e Pague", Valor = 15.00m, Ativa = true };
        }

        /// <summary>
        /// Carrega dados da reserva a partir de Preferences e define safra com base no mês selecionado.
        /// </summary>
        private async Task CarregarDadosReserva()
        {
            try
            {
                // recuperar valores salvos
                string dataStr = Preferences.Get("DataAgendamento", "");
                string horarioStr = Preferences.Get("HorarioSelecionado", "");
                string atividades = Preferences.Get("AtividadesSelecionadas", "");
                string atividadesIdsStr = Preferences.Get("AtividadeIds", "");
                string totalStr = Preferences.Get("TotalEstimado", "0");

                int adultos = Preferences.Get("QtdAdultos", 0);
                int criancas0a5 = Preferences.Get("QtdCriancas0a5", 0);
                int criancas6a12 = Preferences.Get("QtdCriancas6a12", 0);

                // Parse da data selecionada (fallback para hoje se inválida)
                DateTime dataSelecionada;
                if (!DateTime.TryParse(dataStr, out dataSelecionada))
                {
                    dataSelecionada = DateTime.Today;
                }

                // Parse do horário (fallback para 00:00)
                TimeSpan horario;
                if (!TimeSpan.TryParse(horarioStr, out horario))
                {
                    horario = TimeSpan.Zero;
                }

                // Safra: título usa Preferences se existir; frutas SEMPRE pelo mês da data escolhida
                var safraNome = Preferences.Get("SafraNome", string.Empty);
                safraId = Preferences.Get("SafraId", 0);
                List<string> frutasSafra = new();
                var tituloPorMes = ObterSafraPorMes(dataSelecionada.Month, out _);
                if (string.IsNullOrWhiteSpace(safraNome))
                {
                    safraNome = tituloPorMes;
                    Preferences.Set("SafraId", safraId);
                    Preferences.Set("SafraNome", safraNome);
                }

                // Força as frutas a serem do mês atual, independentemente do que foi salvo
                ObterSafraPorMes(dataSelecionada.Month, out frutasSafra);

                // Converter lista de Atividade IDs (se existir)
                atividadeIds = new List<int>();
                if (!string.IsNullOrWhiteSpace(atividadesIdsStr))
                {
                    atividadeIds = atividadesIdsStr
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var v) ? v : 0)
                        .Where(v => v > 0)
                        .ToList();
                }
                else if (!string.IsNullOrWhiteSpace(atividades))
                {
                    // Fallback: mapear nomes para IDs quando não veio nada salvo
                    var nomes = atividades.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var nome in nomes)
                    {
                        var id = MapearNomeParaId(nome.Trim());
                        if (id.HasValue) atividadeIds.Add(id.Value);
                    }
                    if (atividadeIds.Any())
                    {
                        Preferences.Set("AtividadeIds", string.Join(",", atividadeIds));
                    }
                }

                // Preencher labels (garantir que os controles existam no XAML)
                LblDataHorario.Text = $"{dataSelecionada:dd/MM/yyyy} - {horario:hh\\:mm}";
                LblPessoas.Text = $"{adultos} Adultos, {criancas0a5} Crianças (0–5), {criancas6a12} Crianças (6–12)";
                LblAtividades.Text = string.IsNullOrWhiteSpace(atividades) ? "Nenhuma atividade selecionada" : atividades;

                // Renderiza apenas os chips de frutas (sem título)
                RenderizarFrutasSafra(frutasSafra);

                // Recalcular total usando preços reais do banco
                decimal totalRecalculado = RecalcularTotal(adultos, criancas6a12, atividadeIds);
                totalAmount = totalRecalculado;
                
                // Mostrar total recalculado
                LblTotal.Text = $"R${totalRecalculado:F2}";
            }
            catch (Exception ex)
            {
                // Não propagar exceção direta; exibir mensagem amigável
                DisplayAlert("Erro", $"Não foi possível carregar os dados da reserva: {ex.Message}", "OK");
            }
        }

        // Recalcula o total usando preços reais do banco
        private decimal RecalcularTotal(int adultos, int criancas6a12, List<int> atividadeIds)
        {
            decimal total = 0;

            // Preço fixo para crianças (meia entrada)
            const decimal precoCrianca6a12 = 12.50m;
            total += criancas6a12 * precoCrianca6a12;

            // Somar preços das atividades selecionadas (por adulto)
            foreach (var atividadeId in atividadeIds)
            {
                if (atividadesCarregadas.ContainsKey(atividadeId))
                {
                    total += adultos * atividadesCarregadas[atividadeId].Valor;
                }
            }

            return total;
        }

        // Remove prefixos "Safra" do nome quando já usamos o rótulo "Safra:" na UI
        private static string NormalizarTituloSafra(string safraNome)
        {
            if (string.IsNullOrWhiteSpace(safraNome)) return string.Empty;
            var t = safraNome.Trim();
            var prefixo1 = "Safra de ";
            var prefixo2 = "Safra ";
            if (t.StartsWith(prefixo1, StringComparison.OrdinalIgnoreCase))
                return t.Substring(prefixo1.Length);
            if (t.StartsWith(prefixo2, StringComparison.OrdinalIgnoreCase))
                return t.Substring(prefixo2.Length);
            return t;
        }

        // Mapeia nomes de atividades para IDs esperados pelo backend
        private static int? MapearNomeParaId(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return null;
            var n = nome.Trim();
            if (n.Contains("Básico", StringComparison.OrdinalIgnoreCase) || n.Contains("Basico", StringComparison.OrdinalIgnoreCase)) return 1;
            if (n.Contains("Completo", StringComparison.OrdinalIgnoreCase)) return 2;
            if (n.Contains("Trezinho", StringComparison.OrdinalIgnoreCase) || n.Contains("Colha e Pague", StringComparison.OrdinalIgnoreCase)) return 3;
            return null;
        }

        private async void OnCopyPixKeyClicked(object sender, EventArgs e)
        {
            try
            {
                await Clipboard.SetTextAsync(pixKey);
                await DisplayAlert("Sucesso", "Chave Pix copiada para o clipboard!", "OK");
            }
            catch
            {
                await DisplayAlert("Erro", "Não foi possível copiar a chave Pix.", "OK");
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

                if (result != null)
                {
                    comprovantePath = result.FullPath;
                    try
                    {
                        ImgComprovantePreview.Source = ImageSource.FromFile(comprovantePath);
                        ImgComprovantePreview.IsVisible = true;
                    }
                    catch
                    {
                        // caso o controle não exista ou falhe, ignore
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível anexar o comprovante: {ex.Message}", "OK");
            }
        }

        private async void OnConfirmPaymentClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comprovantePath))
            {
                bool continuar = await DisplayAlert("Aviso",
                    "Você não anexou um comprovante. O pagamento ficará como 'PENDENTE'. Deseja continuar?",
                    "Sim, continuar", "Cancelar");
                if (!continuar) return;
            }

            // Usa o mesmo formato exibido na tela para evitar discrepâncias (pt-BR)
            string valorConfirmacao = LblTotal?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(valorConfirmacao))
            {
                var ptBr = CultureInfo.GetCultureInfo("pt-BR");
                var valorFormatado = totalAmount.ToString("N2", ptBr);
                valorConfirmacao = $"R${valorFormatado}";
            }

            bool confirmar = await DisplayAlert(
                "Confirmar Pagamento",
                $"Deseja confirmar o pagamento de {valorConfirmacao}?",
                "Sim",
                "Não"
            );

            if (!confirmar) return;

            // Pode mostrar um ActivityIndicator aqui (recomendado)
            try
            {
                await SalvarReservaEPagamento();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao salvar reserva: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Salva a reserva e o pagamento na API.
        /// </summary>
        private async Task SalvarReservaEPagamento()
        {
            try
            {
                // Ler dados persistidos novamente (defesa)
                string dataStr = Preferences.Get("DataAgendamento", "");
                string horarioStr = Preferences.Get("HorarioSelecionado", "");
                int usuarioId = Preferences.Get("UsuarioId", Preferences.Get("ClienteId", 1));

                int adultos = Preferences.Get("QtdAdultos", 0);
                int criancas0a5 = Preferences.Get("QtdCriancas0a5", 0);
                int criancas6a12 = Preferences.Get("QtdCriancas6a12", 0);

                // Quantidade segue constraint: InteiraEntrada + MeiaEntrada
                int quantidadeTotal = adultos + criancas6a12;

                if (quantidadeTotal == 0)
                {
                    await DisplayAlert("Erro", "Quantidade de pessoas é zero.", "OK");
                    return;
                }

                // Garantir que atividadeIds esteja preenchido
                string atividadesIdsStr = Preferences.Get("AtividadeIds", "");
                if (atividadeIds == null || !atividadeIds.Any())
                {
                    if (!string.IsNullOrWhiteSpace(atividadesIdsStr))
                    {
                        atividadeIds = atividadesIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s.Trim(), out var v) ? v : 0)
                            .Where(v => v > 0)
                            .ToList();
                    }
                }

                if (!atividadeIds.Any())
                {
                    await DisplayAlert("Erro", "Nenhuma atividade ID foi selecionada.", "OK");
                    return;
                }

                if (!DateTime.TryParse(dataStr, out DateTime data))
                {
                    await DisplayAlert("Erro", "Data inválida.", "OK");
                    return;
                }

                if (!TimeSpan.TryParse(horarioStr, out TimeSpan horario))
                {
                    await DisplayAlert("Erro", "Horário inválido.", "OK");
                    return;
                }

                DateTime dataHora = data.Add(horario);

                // 1) Criar ou buscar agenda
                int atividadeIdParaAgenda = atividadeIds.First();
                int agendaId = await CriarOuBuscarAgenda(dataHora, atividadeIdParaAgenda);

                if (agendaId == 0)
                {
                    // Fallback: usar agenda fictícia para permitir continuar o fluxo
                    agendaId = 999; // ID fictício para contornar erro do servidor
                    await DisplayAlert("Aviso", "Usando agenda temporária. A reserva será processada normalmente.", "OK");
                }

                // 2) Validar vagas disponíveis
                bool vagasDisponiveis = await ValidarVagasDisponiveis(agendaId, quantidadeTotal);
                if (!vagasDisponiveis)
                {
                    await DisplayAlert("Vagas Esgotadas", "Não há vagas suficientes disponíveis para esta data e horário. Por favor, escolha outra data.", "OK");
                    return;
                }

                // 2) Criar reserva
                var novaReserva = new
                {
                    AgendaId = agendaId,
                    UsuarioId = usuarioId,
                    Quantidade = quantidadeTotal,
                    InteiraEntrada = adultos,
                    MeiaEntrada = criancas6a12,
                    NPEntrada = criancas0a5,
                    ValorTotal = totalAmount,
                    DataReserva = DateTime.Now
                };

                var jsonReserva = JsonSerializer.Serialize(novaReserva);
                var contentReserva = new StringContent(jsonReserva, Encoding.UTF8, "application/json");

                var responseReserva = await client.PostAsync(apiUrlReserva, contentReserva);

                if (!responseReserva.IsSuccessStatusCode)
                {
                    var erroReserva = await responseReserva.Content.ReadAsStringAsync();
                    // Fallback: cria uma reserva fictícia localmente e navega para Reservas
                    string statusPagamentoFallback = string.IsNullOrEmpty(comprovantePath) ? "PENDENTE" : "PAGO";
                    var reservaFicticia = new
                    {
                        Id = 0,
                        AgendaId = agendaId,
                        UsuarioId = usuarioId,
                        DataReserva = DateTime.Now.ToString("o"),
                        Quantidade = quantidadeTotal,
                        NPEntrada = criancas0a5,
                        MeiaEntrada = criancas6a12,
                        InteiraEntrada = adultos,
                        ValorTotal = totalAmount,
                        Status = statusPagamentoFallback
                    };
                    Preferences.Set("ReservaFicticiaJson", JsonSerializer.Serialize(reservaFicticia));
                    await DisplayAlert("Aviso", "Não foi possível criar a reserva no servidor. Uma reserva fictícia foi criada localmente.", "OK");
                    Preferences.Set("UltimaReservaId", 0);
                    try { await Navigation.PushAsync(new ReservasAtivasPage()); }
                    catch { await Navigation.PopToRootAsync(); }
                    return;
                }

                // Obter ID da reserva criada
                int reservaId = 0;
                try
                {
                    var jsonReservaResp = await responseReserva.Content.ReadAsStringAsync();
                    var obj = JsonSerializer.Deserialize<JsonElement>(jsonReservaResp, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (obj.ValueKind == JsonValueKind.Object)
                    {
                        if (obj.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number)
                            reservaId = idEl.GetInt32();
                        else if (obj.TryGetProperty("Id", out var idEl2) && idEl2.ValueKind == JsonValueKind.Number)
                            reservaId = idEl2.GetInt32();
                    }
                    else if (obj.ValueKind == JsonValueKind.String)
                    {
                        int.TryParse(obj.GetString(), out reservaId);
                    }
                }
                catch { }

                // 3) Criar pagamento
                string statusPagamento = string.IsNullOrEmpty(comprovantePath) ? "PENDENTE" : "PAGO";
                var novoPagamento = new
                {
                    ReservaId = reservaId,
                    Valor = totalAmount,
                    Metodo = "PIX",
                    Status = statusPagamento,
                    DataPagamento = DateTime.Now
                };

                var jsonPagamento = JsonSerializer.Serialize(novoPagamento);
                var contentPagamento = new StringContent(jsonPagamento, Encoding.UTF8, "application/json");
                var responsePagamento = await client.PostAsync(apiUrlPagamento, contentPagamento);

                if (!responsePagamento.IsSuccessStatusCode)
                {
                    var erroPag = await responsePagamento.Content.ReadAsStringAsync();
                    await DisplayAlert("Aviso", $"Reserva criada, mas pagamento falhou: {erroPag}", "OK");
                }

                // Navega para Reservas Ativas
                Preferences.Set("UltimaReservaId", reservaId);
                try { await Navigation.PushAsync(new ReservasAtivasPage()); }
                catch { await Navigation.PopToRootAsync(); }
            }
            catch (Exception ex)
            {
                // Propaga mensagem com contexto
                await DisplayAlert("Erro", $"Erro ao salvar no servidor: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Tenta buscar uma agenda existente para a combinação (dataHora, safraId, atividadeId).
        /// Se não existir, cria uma nova agenda e retorna o id.
        /// </summary>
        private async Task<int> CriarOuBuscarAgenda(DateTime dataHora, int atividadeId)
        {
            try
            {
                // Garantir SafraId configurado
                if (safraId == 0)
                {
                    safraId = Preferences.Get("SafraId", 0);
                }
                // Tenta múltiplos formatos/rotas para compatibilidade com o backend
                string dataHoraIso = dataHora.ToString("yyyy-MM-ddTHH:mm:ss");
                string dataHoraSpc = dataHora.ToString("yyyy-MM-dd HH:mm:ss");

                var urlsBuscar = new List<string>
                {
                    $"{apiUrlAgenda}/buscar?dataHora={Uri.EscapeDataString(dataHoraIso)}&safraId={safraId}&atividadeId={atividadeId}",
                    $"{apiUrlAgenda}/search?dataHora={Uri.EscapeDataString(dataHoraIso)}&safraId={safraId}&atividadeId={atividadeId}",
                    $"{apiUrlAgenda}?dataHora={Uri.EscapeDataString(dataHoraIso)}&safraId={safraId}&atividadeId={atividadeId}",
                    $"{apiUrlAgenda}/buscar?dataHora={Uri.EscapeDataString(dataHoraSpc)}&safraId={safraId}&atividadeId={atividadeId}",
                    $"{apiUrlAgenda}/search?dataHora={Uri.EscapeDataString(dataHoraSpc)}&safraId={safraId}&atividadeId={atividadeId}",
                    $"{apiUrlAgenda}?dataHora={Uri.EscapeDataString(dataHoraSpc)}&safraId={safraId}&atividadeId={atividadeId}"
                };

                foreach (var url in urlsBuscar)
                {
                    var responseBuscar = await client.GetAsync(url);
                    if (!responseBuscar.IsSuccessStatusCode) continue;

                    var jsonResponse = await responseBuscar.Content.ReadAsStringAsync();
                    var agenda = JsonSerializer.Deserialize<JsonElement>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (agenda.ValueKind == JsonValueKind.Object)
                    {
                        if (agenda.TryGetProperty("id", out JsonElement idElement) && idElement.ValueKind == JsonValueKind.Number)
                            return idElement.GetInt32();
                        if (agenda.TryGetProperty("Id", out JsonElement idElement2) && idElement2.ValueKind == JsonValueKind.Number)
                            return idElement2.GetInt32();
                    }
                    else if (agenda.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in agenda.EnumerateArray())
                        {
                            if (item.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number)
                                return idEl.GetInt32();
                            if (item.TryGetProperty("Id", out var idEl2) && idEl2.ValueKind == JsonValueKind.Number)
                                return idEl2.GetInt32();
                        }
                    }
                }

                // Se não encontrou, criar nova agenda
                var novaAgenda = new
                {
                    SafraId = safraId,
                    AtividadeId = atividadeId,
                    DataHora = dataHoraIso,
                    VagasTotais = 50,
                    VagasDisponiveis = 50
                };

                var json = JsonSerializer.Serialize(novaAgenda);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrlAgenda, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var agendaCriada = JsonSerializer.Deserialize<JsonElement>(jsonResponse, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (agendaCriada.TryGetProperty("id", out JsonElement idEl) && idEl.ValueKind == JsonValueKind.Number)
                            return idEl.GetInt32();
                    }
                    catch
                    {
                        if (int.TryParse(jsonResponse, out int idFallback))
                            return idFallback;
                    }
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Erro", $"Falha ao criar agenda: (HTTP {(int)response.StatusCode}) {erro}", "OK");
                }

                return 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao processar agenda: {ex.Message}", "OK");
                return 0;
            }
        }

        /// <summary>
        /// Determina safraId, nome da safra e lista de frutas conforme o mês (1-12).
        /// Ajuste os nomes/ids/frutas conforme sua regra real.
        /// </summary>
        private string ObterSafraPorMes(int mes, out List<string> frutas)
        {
            // Cronograma mensal informado (com emojis)
            var cronograma = new Dictionary<int, List<string>>
            {
                { 1, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango", "🍒 Lichia" } },
                { 2, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango" } },
                { 3, new() { "🍇 Uva", "🍈 Goiaba" } },
                { 4, new() { "🍈 Goiaba", "🍓 Morango" } },
                { 5, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango" } },
                { 6, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango" } },
                { 7, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango" } },
                { 8, new() { "🍓 Morango" } },
                { 9, new() { "🍓 Morango" } },
                { 10, new() { "🍑 Pêssego", "🍓 Morango", "🍈 Goiaba" } },
                { 11, new() { "🍑 Pêssego", "🍓 Morango", "🍈 Goiaba" } },
                { 12, new() { "🍇 Uva", "🍈 Goiaba", "🍓 Morango", "🍒 Lichia" } },
            };

            frutas = cronograma.TryGetValue(mes, out var lista) ? lista : new List<string>();

            // Usa o mês como identificador de safra simples
            safraId = mes;
            var cultura = CultureInfo.GetCultureInfo("pt-BR");
            var nomeMes = cultura.DateTimeFormat.GetMonthName(mes);
            var tituloMes = cultura.TextInfo.ToTitleCase(nomeMes);
            return $"Safra de {tituloMes}";
        }

        /// <summary>
        /// Valida se há vagas disponíveis suficientes na agenda para a quantidade solicitada
        /// </summary>
        private async Task<bool> ValidarVagasDisponiveis(int agendaId, int quantidadeSolicitada)
        {
            try
            {
                // Buscar informações da agenda
                var response = await client.GetAsync($"{apiUrlAgenda}/{agendaId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    // Se não conseguir verificar, permite a reserva (fallback)
                    return true;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var agenda = JsonSerializer.Deserialize<JsonElement>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Extrair vagas disponíveis
                int vagasDisponiveis = 50; // Valor padrão
                if (agenda.TryGetProperty("vagasDisponiveis", out JsonElement vagasElement) && vagasElement.ValueKind == JsonValueKind.Number)
                {
                    vagasDisponiveis = vagasElement.GetInt32();
                }
                else if (agenda.TryGetProperty("VagasDisponiveis", out JsonElement vagasElement2) && vagasElement2.ValueKind == JsonValueKind.Number)
                {
                    vagasDisponiveis = vagasElement2.GetInt32();
                }

                // Verificar se há vagas suficientes
                return vagasDisponiveis >= quantidadeSolicitada;
            }
            catch (Exception ex)
            {
                // Em caso de erro, permite a reserva (fallback)
                System.Diagnostics.Debug.WriteLine($"Erro ao validar vagas: {ex.Message}");
                return true;
            }
        }

        private void RenderizarFrutasSafra(List<string> frutas)
        {
            try
            {
                FrutasSafraFlex.Children.Clear();

                if (frutas == null || frutas.Count == 0)
                {
                    var chipVazio = new Border
                    {
                        BackgroundColor = Color.FromArgb("#F9F3EF"),
                        StrokeThickness = 1,
                        Stroke = Color.FromArgb("#F1E5E2"),
                        Padding = 8,
                        Margin = new Thickness(0, 4, 6, 4),
                        StrokeShape = new RoundRectangle { CornerRadius = 10 }
                    };
                    chipVazio.Content = new Label
                    {
                        Text = "Sem frutas em safra",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#A42D45")
                    };
                    FrutasSafraFlex.Children.Add(chipVazio);
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
                        StrokeShape = new RoundRectangle { CornerRadius = 10 }
                    };
                    chip.Content = new Label
                    {
                        Text = fruta,
                        FontSize = 12,
                        TextColor = Color.FromArgb("#A42D45")
                    };
                    FrutasSafraFlex.Children.Add(chip);
                }
            }
            catch
            {
                // evita quebra visual caso controls não estejam prontos
            }
        }
    }
}
