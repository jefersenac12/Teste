
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace Teste
{
    public class FrutaPeriodo
    {
        public string Nome { get; set; }
        public string Emoji { get; set; }
        // Armazena Mês e Dia de Início/Fim do ciclo ANUAL
        public (int Mes, int Dia) InicioAnual { get; set; }
        public (int Mes, int Dia) FimAnual { get; set; }

        public FrutaPeriodo(string nome, string emoji, int mesInicio, int diaInicio, int mesFim, int diaFim)
        {
            Nome = nome;
            Emoji = emoji;
            InicioAnual = (mesInicio, diaInicio);
            FimAnual = (mesFim, diaFim);
        }

        // Método que verifica se uma data ESPECÍFICA está no período de safra da fruta.
        public bool EstaEmSafra(DateTime data)
        {
            int mes = data.Month;
            int dia = data.Day;

            // Cria um valor numérico que representa o dia do ano (1 a 366) para comparação
            // (Ignora o ano para tratar safras anuais, mas é menos propenso a erro do que só o mês)
            int diaDoAnoSelecionado = data.DayOfYear;

            // Safra Cruzada (Ex: Dezembro (12) a Março (3))
            if (FimAnual.Mes < InicioAnual.Mes)
            {
                // Para safras que cruzam o ano, o início e o fim estão no mesmo ano para o cálculo
                // Início do ciclo no ano (ex: 10/12)
                DateTime inicioData = new DateTime(data.Year, InicioAnual.Mes, InicioAnual.Dia);
                // Fim do ciclo no ano (ex: 31/03 do PRÓXIMO ano - ajustamos para o ano de comparação)
                DateTime fimData = new DateTime(data.Year, FimAnual.Mes, FimAnual.Dia);

                // Se a data selecionada for no início do ano (mes <= FimAnual.Mes), compara com o ano anterior
                if (mes <= FimAnual.Mes)
                {
                    inicioData = new DateTime(data.Year - 1, InicioAnual.Mes, InicioAnual.Dia);
                }
                // Se a data selecionada for no final do ano (mes >= InicioAnual.Mes), compara com o próximo ano
                else if (mes >= InicioAnual.Mes)
                {
                    fimData = new DateTime(data.Year + 1, FimAnual.Mes, FimAnual.Dia);
                }

                return data.Date >= inicioData.Date && data.Date <= fimData.Date;
            }
            else
            {
                // Safra simples (Ex: Maio a Julho)
                DateTime inicioData = new DateTime(data.Year, InicioAnual.Mes, InicioAnual.Dia);
                DateTime fimData = new DateTime(data.Year, FimAnual.Mes, FimAnual.Dia);

                return data.Date >= inicioData.Date && data.Date <= fimData.Date;
            }
        }
    }


    // Modelo do dia
    public class DiaCalendario : INotifyPropertyChanged
    {
        // ... (Corpo do DiaCalendario mantido inalterado) ...
        public int Numero { get; set; }
        public bool IsFromCurrentMonth { get; set; }
        public DateTime Data { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Modelo de Safra (da API)
    public class Safra
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativa { get; set; }
    }

    public partial class AgendamentoPage : ContentPage
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiUrlSafras = "http://tiijeferson.runasp.net/api/agenda";
        private readonly string apiUrlTodasSafras = "http://tiijeferson.runasp.net/api/Safra";

        // ===================================
        // 2. CRONOGRAMA DE SAFRA POR FRUTA (Lista de FrutaPeriodo)
        // ===================================
        private static readonly List<FrutaPeriodo> FrutaCronograma = new List<FrutaPeriodo>
        {
            // Janeiro - uva, goiaba, morango, lichia
            // Fevereiro - uva, goiaba, morango
            // Março - uva, goiaba
            new FrutaPeriodo("Uva", "🍇", 12, 1, 7, 31),      // Dezembro (01) a Julho (31)
            new FrutaPeriodo("Goiaba", "🍈", 1, 1, 7, 31),    // Janeiro (01) a Julho (31)
            new FrutaPeriodo("Morango", "🍓", 1, 1, 7, 31),   // Janeiro (01) a Julho (31)
            new FrutaPeriodo("Lichia", "🍒", 12, 1, 1, 31),   // Dezembro (01) a Janeiro (31) - CRUZA O ANO

            // Agosto - morango (Continuação)
            // Setembro - morango (Continuação)
            new FrutaPeriodo("Morango", "🍓", 8, 1, 9, 30),   // Agosto (01) a Setembro (30)

            // Outubro - pêssego, morango, goiaba
            // Novembro - pêssego, morango, goiaba
            // Dezembro - uva, goiaba, morango, lichia
            new FrutaPeriodo("Pêssego", "🍑", 10, 1, 11, 30), // Outubro (01) a Novembro (30)
            new FrutaPeriodo("Morango", "🍓", 10, 1, 12, 31), // Outubro (01) a Dezembro (31)
            new FrutaPeriodo("Goiaba", "🍈", 10, 1, 12, 31),  // Outubro (01) a Dezembro (31)
        };


        private DateTime _dataAtual;
        private Safra? _safraAtiva;
        private List<Safra> _todasSafras = new List<Safra>();
        public ObservableCollection<DiaCalendario> Dias { get; set; }
        public ObservableCollection<string> Frutas { get; set; }

        public AgendamentoPage()
        {
            InitializeComponent();

            Dias = new ObservableCollection<DiaCalendario>();
            Frutas = new ObservableCollection<string>();

            _dataAtual = DateTime.Now;
            DiasCollectionView.ItemsSource = Dias;
            BindableLayout.SetItemsSource(FrutasList, Frutas);

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await CarregarTodasSafras();
                await CarregarSafraAtiva();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro Crítico", $"Não foi possível inicializar: {ex.Message}", "OK");
            }
            finally
            {
                AtualizarCalendario();
                AtualizarFrutas(); // AGORA USA A LISTA DE FrutaCronograma
            }
        }

        // --- MÉTODOS DE CARREGAMENTO (Mantidos inalterados) ---

        private async Task CarregarTodasSafras()
        {
            try
            {
                var response = await client.GetAsync(apiUrlTodasSafras);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var safras = JsonSerializer.Deserialize<List<Safra>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _todasSafras = safras ?? new List<Safra>();
                }
                else
                {
                    _todasSafras = new List<Safra>();
                }
            }
            catch (Exception)
            {
                _todasSafras = new List<Safra>();
            }
        }

        private Safra? EncontrarSafraPorData(DateTime data)
        {
            if (_todasSafras?.Any() == true)
            {
                return _todasSafras.FirstOrDefault(s =>
                    data.Date >= s.DataInicio.Date &&
                    data.Date <= s.DataFim.Date);
            }
            return null;
        }

        private async Task CarregarSafraAtiva()
        {
            try
            {
                var response = await client.GetAsync(apiUrlSafras);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var safras = JsonSerializer.Deserialize<List<Safra>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _safraAtiva = safras?.FirstOrDefault();

                    if (_safraAtiva == null)
                    {
                        CarregarSafraFallback();
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("Aviso", "Nenhuma safra ativa encontrada na API. Usando dados locais.", "OK");
                        });
                    }
                }
                else
                {
                    CarregarSafraFallback();
                }
            }
            catch (Exception ex)
            {
                CarregarSafraFallback();
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Erro de Rede", $"Erro ao carregar safra: {ex.Message}. Usando dados locais.", "OK");
                });
            }

            if (_safraAtiva != null)
            {
                Preferences.Set("SafraId", _safraAtiva.Id);
                Preferences.Set("SafraNome", _safraAtiva.Nome);
            }
        }

        private void CarregarSafraFallback()
        {
            var dataAtual = DateTime.Now;
            int mesAtual = dataAtual.Month;
            int anoAtual = dataAtual.Year;

            // Definindo as Safras para 2025/2026 (Baseado em períodos comuns de colheita)
            // 1. Safra de Verão (Dezembro a Março - Ano Vira)
            if (mesAtual >= 12 || mesAtual <= 3)
            {
                DateTime inicio;
                DateTime fim;

                if (mesAtual >= 12) // Dezembro
                {
                    inicio = new DateTime(anoAtual, 12, 1);
                    fim = new DateTime(anoAtual + 1, 3, 31);
                }
                else // Janeiro, Fevereiro, Março
                {
                    inicio = new DateTime(anoAtual - 1, 12, 1);
                    fim = new DateTime(anoAtual, 3, 31);
                }

                _safraAtiva = new Safra
                {
                    Id = 1,
                    Nome = $"Safra de Verão {inicio.Year}/{fim.Year}",
                    DataInicio = inicio,
                    DataFim = fim,
                    Ativa = true
                };
            }
            // 2. Safra de Outono/Inverno (Abril a Setembro)
            else if (mesAtual >= 4 && mesAtual <= 9)
            {
                _safraAtiva = new Safra
                {
                    Id = 2,
                    Nome = $"Safra de Outono/Inverno {anoAtual}",
                    DataInicio = new DateTime(anoAtual, 4, 1),
                    DataFim = new DateTime(anoAtual, 9, 30),
                    Ativa = true
                };
            }
            // 3. Safra de Primavera (Outubro a Novembro)
            else // Outubro e Novembro
            {
                _safraAtiva = new Safra
                {
                    Id = 3,
                    Nome = $"Safra de Primavera {anoAtual}",
                    DataInicio = new DateTime(anoAtual, 10, 1),
                    DataFim = new DateTime(anoAtual, 11, 30),
                    Ativa = true
                };
            }
        }

        // --- ATUALIZAR CALENDÁRIO (Mantido inalterado) ---
        private void AtualizarCalendario()
        {
            Dias.Clear();
            MesLabel.Text = _dataAtual.ToString("MMMM yyyy", new CultureInfo("pt-BR"));

            var primeiroDia = new DateTime(_dataAtual.Year, _dataAtual.Month, 1);
            var ultimoDia = primeiroDia.AddMonths(1).AddDays(-1);
            int primeiroDiaSemana = (int)primeiroDia.DayOfWeek;

            var mesAnterior = _dataAtual.AddMonths(-1);
            int diasMesAnterior = DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);

            // Dias do mês anterior
            for (int i = primeiroDiaSemana - 1; i >= 0; i--)
            {
                int dia = diasMesAnterior - i;
                Dias.Add(new DiaCalendario
                {
                    Numero = dia,
                    IsFromCurrentMonth = false,
                    Data = new DateTime(mesAnterior.Year, mesAnterior.Month, dia)
                });
            }

            // Dias do mês atual
            for (int i = 1; i <= ultimoDia.Day; i++)
            {
                var dataDia = new DateTime(_dataAtual.Year, _dataAtual.Month, i);
                Dias.Add(new DiaCalendario
                {
                    Numero = i,
                    IsFromCurrentMonth = true,
                    Data = dataDia,
                    IsSelected = (dataDia.Date == DateTime.Now.Date)
                });
            }

            // Dias do próximo mês
            var proximoMes = _dataAtual.AddMonths(1);
            int totalNecessario = 42;
            int totalDias = Dias.Count;

            for (int i = 1; totalDias + i <= totalNecessario; i++)
            {
                Dias.Add(new DiaCalendario
                {
                    Numero = i,
                    IsFromCurrentMonth = false,
                    Data = new DateTime(proximoMes.Year, proximoMes.Month, i)
                });
            }
        }

        // ===================================
        // 3. MÉTODO ATUALIZAR FRUTAS (Usando FrutaCronograma)
        // ===================================
        private void AtualizarFrutas()
        {
            Frutas.Clear();

            // Usa a data atual do calendário para verificar as frutas em safra
            DateTime dataReferencia = new DateTime(_dataAtual.Year, _dataAtual.Month, 1);

            // Filtra as frutas que estão em safra no mês/ano de referência
            var frutasDoMes = FrutaCronograma
                .Where(f => f.EstaEmSafra(dataReferencia))
                .DistinctBy(f => f.Nome)
                .Select(f => $"{f.Emoji} {f.Nome}")
                .ToList();

            foreach (var fruta in frutasDoMes)
            {
                Frutas.Add(fruta);
            }

            // Garante que o rótulo do mês no painel de frutas esteja correto (Assumindo que LblMesSafra exista no XAML)
            // Para referência: O XAML usa um rótulo com o texto "Julho 2024"
            // Você pode querer mudar para: 
            // LblMesSafra.Text = _dataAtual.ToString("MMMM yyyy", new CultureInfo("pt-BR"));

            bool temFrutas = Frutas.Any();
            FrutasList.IsVisible = temFrutas;
            NoFruitsLabel.IsVisible = !temFrutas;
        }

        // --- NAVEGAÇÃO E SELEÇÃO (Mantidos inalterados) ---
        private void OnMesAnteriorClicked(object sender, EventArgs e)
        {
            _dataAtual = _dataAtual.AddMonths(-1);
            AtualizarCalendario();
            AtualizarFrutas();
        }

        private void OnProximoMesClicked(object sender, EventArgs e)
        {
            _dataAtual = _dataAtual.AddMonths(1);
            AtualizarCalendario();
            AtualizarFrutas();
        }

        private void OnDiaSelecionado(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not DiaCalendario diaSelecionado)
                return;

            foreach (var dia in Dias.Where(d => d.IsSelected && d != diaSelecionado))
            {
                dia.IsSelected = false;
            }

            if (!diaSelecionado.IsSelected)
            {
                diaSelecionado.IsSelected = true;
            }

            if (!diaSelecionado.IsFromCurrentMonth)
            {
                _dataAtual = diaSelecionado.Data;
                AtualizarCalendario();
                AtualizarFrutas();

                var diaNoNovoMes = Dias.FirstOrDefault(d => d.Data.Date == diaSelecionado.Data.Date);
                if (diaNoNovoMes != null)
                {
                    diaNoNovoMes.IsSelected = true;
                }
            }
        }

        // ===================================
        // 4. BOTÃO CONTINUAR (Com Verificação de Safra da Fruta)
        // ===================================
        private async void OnContinuarClicked(object sender, EventArgs e)
        {
            var diaSelecionado = Dias.FirstOrDefault(d => d.IsSelected);

            if (diaSelecionado == null)
            {
                await DisplayAlert("Erro", "Por favor, selecione um dia.", "OK");
                return;
            }

            // 1. Verifica se a data selecionada está dentro de QUALQUER período de safra de fruta
            bool estaEmSafraDeFruta = FrutaCronograma.Any(f => f.EstaEmSafra(diaSelecionado.Data));

            if (!estaEmSafraDeFruta)
            {
                // 2. Se a data NÃO estiver em safra por fruta, mostra o aviso.
                Safra? safraEstacional = EncontrarSafraPorData(diaSelecionado.Data) ?? _safraAtiva;

                string nomeSafra = safraEstacional?.Nome ?? "Indisponível";
                DateTime dataInicioSafra = safraEstacional?.DataInicio ?? DateTime.MinValue;
                DateTime dataFimSafra = safraEstacional?.DataFim ?? DateTime.MaxValue;

                // Exibe o alerta com o formato da imagem fornecida
                await DisplayAlert("Aviso",
                    $"A data selecionada ({diaSelecionado.Data:dd/MM/yyyy}) não está dentro de nenhum período de safra de fruta disponível.\n\n" +
                    $"Safra atual: {nomeSafra} ({dataInicioSafra:dd/MM/yyyy} a {dataFimSafra:dd/MM/yyyy})",
                    "OK");
                return;
            }

            // 3. Atualiza a Safra Ativa e salva nos Preferences (usando o Safra do banco, se disponível)
            var safraParaData = EncontrarSafraPorData(diaSelecionado.Data);
            if (safraParaData != null)
            {
                _safraAtiva = safraParaData;
                Preferences.Set("SafraId", _safraAtiva.Id);
                Preferences.Set("SafraNome", _safraAtiva.Nome);
            }

            // Salvar dados e navegar
            Preferences.Set("DataAgendamento", diaSelecionado.Data.ToString("yyyy-MM-dd"));

            var atividadesPage = new AtividadesPage();
            // Assumimos que AtividadesPage tem um método SetData(DateTime)
            atividadesPage.SetData(diaSelecionado.Data);
            await Navigation.PushAsync(atividadesPage);
        }
    }
}