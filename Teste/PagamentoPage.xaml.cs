
using System.Globalization;
using System.Text;
using System.Text.Json;


namespace Teste
{
    public partial class PagamentoPage : ContentPage
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiUrlAgenda = "http://tiijeferson.runasp.net/api/Agenda";
        private readonly string apiUrlReserva = "http://tiijeferson.runasp.net/api/Reserva";
        private readonly string apiUrlPagamento = "http://tiijeferson.runasp.net/api/Pagamento";

        private decimal totalAmount = 0;
        private string pixKey = "fazenda.villaggio@pix.com.br";
        private string qrCodeUrl = "";
        private string comprovantePath = string.Empty;
        private int safraId = 0;
        private List<int> atividadeIds = new List<int>();

        public PagamentoPage()
        {
            InitializeComponent();
            CarregarDadosReserva();
         
        }

        /// <summary>
        /// Carrega dados da reserva a partir de Preferences e define safra com base no mês selecionado.
        /// </summary>
        private void CarregarDadosReserva()
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

                // Determina safra e frutas pelo mês da data selecionada
                var safraNome = ObterSafraPorMes(dataSelecionada.Month, out List<string> frutasSafra);

                // Atualiza safraId e salva em Preferences
                Preferences.Set("SafraId", safraId);
                Preferences.Set("SafraNome", safraNome);

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

                // Preencher labels (garantir que os controles existam no XAML)
                LblDataHorario.Text = $"{dataSelecionada:dd/MM/yyyy} - {horario:hh\\:mm}";
                LblPessoas.Text = $"{adultos} Adultos, {criancas0a5} Crianças (0–5), {criancas6a12} Crianças (6–12)";
                LblAtividades.Text = string.IsNullOrWhiteSpace(atividades) ? "Nenhuma atividade selecionada" : atividades;

                // Adiciona safra e frutas
                var frutasTexto = frutasSafra.Any() ? string.Join(", ", frutasSafra) : "Nenhuma fruta definida";
                LblAtividades.Text += $"\n\nSafra: {safraNome}\nFrutas da Safra: {frutasTexto}";

                // Mostrar total
                LblTotal.Text = $"R${totalStr}";

                // Converter total para decimal usando InvariantCulture
                if (!decimal.TryParse(totalStr, NumberStyles.Any, CultureInfo.InvariantCulture, out totalAmount))
                {
                    // Tenta com cultura local como fallback
                    if (!decimal.TryParse(totalStr, NumberStyles.Any, CultureInfo.CurrentCulture, out totalAmount))
                    {
                        totalAmount = 0;
                        DisplayAlert("Aviso", "Não foi possível ler o valor total. Será usado R$0,00.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                // Não propagar exceção direta; exibir mensagem amigável
                DisplayAlert("Erro", $"Não foi possível carregar os dados da reserva: {ex.Message}", "OK");
            }
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

            bool confirmar = await DisplayAlert(
                "Confirmar Pagamento",
                $"Deseja confirmar o pagamento de R${totalAmount:F2}?",
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
                int clienteId = Preferences.Get("ClienteId", 1);

                int adultos = Preferences.Get("QtdAdultos", 0);
                int criancas0a5 = Preferences.Get("QtdCriancas0a5", 0);
                int criancas6a12 = Preferences.Get("QtdCriancas6a12", 0);

                int quantidadeTotal = adultos + criancas0a5 + criancas6a12;

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
                    await DisplayAlert("Erro", "Não foi possível criar ou encontrar a agenda.", "OK");
                    return;
                }

                // 2) Criar reserva
                var novaReserva = new
                {
                    AgendaId = agendaId,
                    ClienteId = clienteId,
                    Quantidade = quantidadeTotal,
                    InteiraEntrada = adultos,
                    MeiaEntrada = criancas6a12,
                    PrecoTotal = totalAmount,
                    DataReserva = DateTime.Now
                };

                var jsonReserva = JsonSerializer.Serialize(novaReserva);
                var contentReserva = new StringContent(jsonReserva, Encoding.UTF8, "application/json");

                var responseReserva = await client.PostAsync(apiUrlReserva, contentReserva);

                if (!responseReserva.IsSuccessStatusCode)
                {
                    var erroReserva = await responseReserva.Content.ReadAsStringAsync();
                    await DisplayAlert("Erro", $"Falha ao criar reserva: {erroReserva}", "OK");
                    return;
                }

                var jsonResponseReserva = await responseReserva.Content.ReadAsStringAsync();
                int reservaId = 0;
                try
                {
                    var reservaCriada = JsonSerializer.Deserialize<JsonElement>(jsonResponseReserva, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (reservaCriada.TryGetProperty("id", out JsonElement idEl) && idEl.ValueKind == JsonValueKind.Number)
                    {
                        reservaId = idEl.GetInt32();
                    }
                    else
                    {
                        // tenta buscar propriedade em lower/other names
                        if (reservaCriada.TryGetProperty("Id", out JsonElement idEl2))
                            reservaId = idEl2.GetInt32();
                    }
                }
                catch
                {
                    // fallback: tentar ler inteiro diretamente do corpo (quando API retorna apenas id)
                    if (int.TryParse(jsonResponseReserva, out int idFallback))
                        reservaId = idFallback;
                }

                if (reservaId == 0)
                {
                    await DisplayAlert("Erro", "Resposta inesperada ao criar reserva (id não encontrado).", "OK");
                    return;
                }

                // 3) Criar pagamento
                string statusPagamento = string.IsNullOrEmpty(comprovantePath) ? "PENDENTE" : "AGUARDANDO_CONFIRMACAO";

                var novoPagamento = new
                {
                    ReservaId = reservaId,
                    FormaPagamento = "PIX",
                    Valor = totalAmount,
                    Status = statusPagamento,
                    ChavePix = pixKey,
                    QrCode = qrCodeUrl,
                    ComprovantePath = comprovantePath, // ideal: enviar arquivo real via multipart/form-data
                    DataPagamento = DateTime.Now
                };

                var jsonPagamento = JsonSerializer.Serialize(novoPagamento);
                var contentPagamento = new StringContent(jsonPagamento, Encoding.UTF8, "application/json");

                var responsePagamento = await client.PostAsync(apiUrlPagamento, contentPagamento);

                if (responsePagamento.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sucesso", "Pagamento confirmado! Sua reserva foi registrada.", "OK");
                    Preferences.Set("UltimaReservaId", reservaId);

                    // navegar para página de reservas ativas (se existir)
                    try
                    {
                        await Navigation.PushAsync(new ReservasAtivasPage());
                    }
                    catch
                    {
                        // fallback para voltar à root
                        await Navigation.PopToRootAsync();
                    }
                }
                else
                {
                    var erroPagamento = await responsePagamento.Content.ReadAsStringAsync();
                    await DisplayAlert("Erro", $"Falha ao registrar pagamento: {erroPagamento}", "OK");
                }
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
                string dataHoraStr = dataHora.ToString("yyyy-MM-ddTHH:mm:ss");
                string urlBuscar = $"{apiUrlAgenda}/buscar?dataHora={Uri.EscapeDataString(dataHoraStr)}&safraId={safraId}&atividadeId={atividadeId}";

                var responseBuscar = await client.GetAsync(urlBuscar);

                if (responseBuscar.IsSuccessStatusCode)
                {
                    var jsonResponse = await responseBuscar.Content.ReadAsStringAsync();
                    var agenda = JsonSerializer.Deserialize<JsonElement>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (agenda.ValueKind == JsonValueKind.Object && agenda.TryGetProperty("id", out JsonElement idElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Number)
                            return idElement.GetInt32();
                    }
                }

                // Se não encontrou, criar nova agenda
                var novaAgenda = new
                {
                    SafraId = safraId,
                    AtividadeId = atividadeId,
                    DataHora = dataHora,
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
                    await DisplayAlert("Erro", $"Falha ao criar agenda: {erro}", "OK");
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
            frutas = new List<string>();

            // Ajuste os IDs conforme sua modelagem real (aqui são exemplos)
            if (mes >= 1 && mes <= 3) // Jan-Mar => Verão
            {
                safraId = 1;
                frutas = new List<string> { "🍉 Melancia", "🥭 Manga", "🍍 Abacaxi" };
                return "Safra de Verão 2025";
            }
            else if (mes >= 4 && mes <= 6) // Abr-Jun => Outono
            {
                safraId = 2;
                frutas = new List<string> { "🍊 Laranja", "🍋 Limão", "🍌 Banana" };
                return "Safra de Outono 2025";
            }
            else if (mes >= 7 && mes <= 9) // Jul-Set => Inverno
            {
                safraId = 3;
                frutas = new List<string> { "🍎 Maçã", "🍐 Pera", "🍇 Uva" };
                return "Safra de Inverno 2025";
            }
            else // Out-Dez => Primavera
            {
                safraId = 4;
                frutas = new List<string> { "🍑 Pêssego", "🍓 Morango", "🍈 Goiaba" };
                return "Safra de Primavera 2025";
            }
        }
    }
}
