
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text;

namespace Teste
{
    // Modelo do dia
    public class DiaCalendario : INotifyPropertyChanged
    {
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

    // Modelo de Safra
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
        private readonly string apiUrlSafras = "http://tiijeferson.runasp.net/api/Safra/ativas";
        private readonly string apiUrlTodasSafras = "http://tiijeferson.runasp.net/api/Safra"; // Nova URL para todas as safras

        private DateTime _dataAtual;
        private Safra? _safraAtiva;
        private List<Safra> _todasSafras = new List<Safra>(); // Lista de todas as safras
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
                await CarregarTodasSafras(); // Carrega todas as safras primeiro
                await CarregarSafraAtiva();
            }
            catch (Exception ex)
            {
                // Em caso de erro na inicialização, a safra de fallback já foi definida
                await DisplayAlert("Erro Crítico", $"Não foi possível inicializar: {ex.Message}", "OK");
            }
            finally
            {
                AtualizarCalendario();
                AtualizarFrutas();
            }
        }

        // Novo método para carregar todas as safras do banco
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

        // Método para encontrar safra por data
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

        // --- MÉTODO DE CARREGAMENTO (Sua versão, que é boa) ---
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
                        // API retornou sucesso, mas sem dados. Usa fallback.
                        CarregarSafraFallback();
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("Aviso", "Nenhuma safra ativa encontrada na API. Usando dados locais.", "OK");
                        });
                    }
                }
                else
                {
                    // API falhou. Usa fallback.
                    CarregarSafraFallback();
                }
            }
            catch (Exception ex)
            {
                // Erro de conexão. Usa fallback.
                CarregarSafraFallback();
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Erro de Rede", $"Erro ao carregar safra: {ex.Message}. Usando dados locais.", "OK");
                });
            }

            // Garante que a Safra (da API ou Fallback) seja salva
            if (_safraAtiva != null)
            {
                Preferences.Set("SafraId", _safraAtiva.Id);
                Preferences.Set("SafraNome", _safraAtiva.Nome);
            }
        }

        // --- MÉTODO DE FALLBACK DINÂMICO ATUALIZADO PARA 2025 ---
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

                // Lógica de ano-virado (Ex: Dez 2025 -> Mar 2026)
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

        // --- ATUALIZAR CALENDÁRIO (Sem alterações) ---
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
                    IsSelected = (dataDia.Date == DateTime.Now.Date) // Seleciona o dia de hoje
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

        // --- ATUALIZAR FRUTAS (CORRIGIDO com emojis válidos) ---
        private void AtualizarFrutas()
        {
            Frutas.Clear();

            // Define o emoji e o nome da fruta para o mês atual
      
            switch (_dataAtual.Month)
            {
                case 1: // Janeiro
                    Frutas.Add("🍇 Uva");
                    Frutas.Add("🍈 Goiaba"); 
                    Frutas.Add("🍓 Morango");
                    Frutas.Add("🍒 Lichia");
                    break;
                case 2: // Fevereiro
                    Frutas.Add("🍇 Uva");
                    Frutas.Add("🍈 Goiaba"); 
                    Frutas.Add("🍓 Morango");
                    break;
                case 3: // Março
                    Frutas.Add("🍇 Uva");
                    Frutas.Add("🍈 Goiaba"); 
                    break;
                case 4: // Abril
                    Frutas.Add("🍈 Goiaba");
                    Frutas.Add("🍓 Morango");
                    break;
                case 5: // Maio
                    Frutas.Add("🍇 Uva");
                    Frutas.Add("🍈 Goiaba"); 
                    Frutas.Add("🍓 Morango");
                    break;
                case 6: // Junho
                    Frutas.Add("🍇 Uva");
                    Frutas.Add("🍈 Goiaba");
                    Frutas.Add("🍓 Morango");
                    break;
                case 7: // Julho
                    Frutas.Add("🍇 Uva");
                    Frutas.Add("🍈 Goiaba"); 
                    Frutas.Add("🍓 Morango");
                    break;
                case 8: // Agosto
                    Frutas.Add("🍓 Morango");
                    break;
                case 9: // Setembro
                    Frutas.Add("🍓 Morango");
                    break;
                case 10: // Outubro
                    Frutas.Add("🍑 Pêssego");
                    Frutas.Add("🍓 Morango");
                    Frutas.Add("🍈 Goiaba"); 
                    break;
                case 11: // Novembro
                    Frutas.Add("🍑 Pêssego");
                    Frutas.Add("🍓 Morango");
                    Frutas.Add("🍈 Goiaba"); 
                    break;
                case 12: // Dezembro
                    Frutas.Add("🍇 Uva");
                    Frutas.Add("🍈 Goiaba");
                    Frutas.Add("🍓 Morango");
                    Frutas.Add("🍒 Lichia");
                    break;
            }

            bool temFrutas = Frutas.Any();
            FrutasList.IsVisible = temFrutas;
            NoFruitsLabel.IsVisible = !temFrutas;
        }

        // --- NAVEGAÇÃO E SELEÇÃO ---
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

        // --- LÓGICA DE SELEÇÃO (CORRIGIDA para maior eficiência) ---
        private void OnDiaSelecionado(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not DiaCalendario diaSelecionado)
                return;

            // CORREÇÃO: Lógica mais eficiente
            // Desmarca todos os *outros* dias
            foreach (var dia in Dias.Where(d => d.IsSelected && d != diaSelecionado))
            {
                dia.IsSelected = false;
            }

            // Garante que o dia clicado esteja selecionado
            if (!diaSelecionado.IsSelected)
            {
                diaSelecionado.IsSelected = true;
            }

            // Se o dia selecionado NÃO for do mês atual, navega para o mês dele
            if (!diaSelecionado.IsFromCurrentMonth)
            {
                _dataAtual = diaSelecionado.Data;
                AtualizarCalendario();
                AtualizarFrutas();

                // Encontra e seleciona o dia correspondente no novo mês carregado
                var diaNoNovoMes = Dias.FirstOrDefault(d => d.Data.Date == diaSelecionado.Data.Date);
                if (diaNoNovoMes != null)
                {
                    diaNoNovoMes.IsSelected = true;
                }
            }
        }

        // --- BOTÃO CONTINUAR (Verifique se 'AtividadesPage' e 'SetData' existem) ---
        private async void OnContinuarClicked(object sender, EventArgs e)
        {
            var diaSelecionado = Dias.FirstOrDefault(d => d.IsSelected);

            if (diaSelecionado == null)
            {
                await DisplayAlert("Erro", "Por favor, selecione um dia.", "OK");
                return;
            }

            // Se o usuário clicou tão rápido que o dia selecionado ainda é do mês anterior
            if (!diaSelecionado.IsFromCurrentMonth)
            {
                // A lógica do OnDiaSelecionado já deve ter corrigido isso,
                // mas como defesa, pegamos o dia selecionado do *novo* mês
                diaSelecionado = Dias.FirstOrDefault(d => d.IsSelected && d.IsFromCurrentMonth);
                if (diaSelecionado == null)
                {
                    await DisplayAlert("Erro", "Por favor, selecione um dia válido do mês.", "OK");
                    return;
                }
            }

            // Primeiro, tenta encontrar safra específica para a data selecionada
            var safraParaData = EncontrarSafraPorData(diaSelecionado.Data);
            
            if (safraParaData != null)
            {
                // Usa a safra específica encontrada para a data
                _safraAtiva = safraParaData;
                Preferences.Set("SafraId", _safraAtiva.Id);
                Preferences.Set("SafraNome", _safraAtiva.Nome);
            }
            else if (_safraAtiva == null)
            {
                await DisplayAlert("Erro", "Aguarde, carregando dados da safra...", "OK");
                // Tenta carregar de novo
                await CarregarSafraAtiva();
                if (_safraAtiva == null)
                {
                    await DisplayAlert("Erro", "Não foi possível carregar dados da safra. Tente novamente.", "OK");
                    return;
                }
            }

            // Verificar se a data está dentro da safra (agora dinâmica ou da API)
            if (diaSelecionado.Data.Date < _safraAtiva.DataInicio.Date || diaSelecionado.Data.Date > _safraAtiva.DataFim.Date)
            {
                // Se não há safra específica para a data, mostra aviso mais informativo
                if (safraParaData == null)
                {
                    await DisplayAlert("Aviso",
                        $"A data selecionada ({diaSelecionado.Data:dd/MM/yyyy}) não está dentro de nenhum período de safra disponível.\n\n" +
                        $"Safra atual: {_safraAtiva.Nome} ({_safraAtiva.DataInicio:dd/MM/yyyy} a {_safraAtiva.DataFim:dd/MM/yyyy})",
                        "OK");
                }
                else
                {
                    await DisplayAlert("Aviso",
                        $"A data selecionada está fora do período da safra ativa ({_safraAtiva.Nome}).",
                        "OK");
                }
                return;
            }

            // Salvar dados para próxima tela
            Preferences.Set("DataAgendamento", diaSelecionado.Data.ToString("yyyy-MM-dd"));
            // Preferences já foram setados no CarregarSafraAtiva ou acima
            // Preferences.Set("SafraId", _safraAtiva.Id);
            // Preferences.Set("SafraNome", _safraAtiva.Nome);

            // Navega para AtividadesPage
            // (Isto assume que você tem uma página 'AtividadesPage.cs' 
            // e que ela tem um método público 'SetData')
            var atividadesPage = new AtividadesPage();
            atividadesPage.SetData(diaSelecionado.Data);
            await Navigation.PushAsync(atividadesPage);
        }
    }
}