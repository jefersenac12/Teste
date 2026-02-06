using System.Collections.ObjectModel;
using System.Windows.Input;
using Teste.Models;
using Teste.Services;

namespace Teste.ViewModels
{
    public class AtividadesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Atividade> _atividades;
        private ObservableCollection<Agenda> _agendas;
        private Atividade? _atividadeSelecionada;
        private Agenda? _agendaSelecionada;
        private bool _isLoading;

        public ObservableCollection<Atividade> Atividades
        {
            get => _atividades;
            set => SetProperty(ref _atividades, value);
        }

        public ObservableCollection<Agenda> Agendas
        {
            get => _agendas;
            set => SetProperty(ref _agendas, value);
        }

        public Atividade? AtividadeSelecionada
        {
            get => _atividadeSelecionada;
            set => SetProperty(ref _atividadeSelecionada, value);
        }

        public Agenda? AgendaSelecionada
        {
            get => _agendaSelecionada;
            set => SetProperty(ref _agendaSelecionada, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand CarregarAtividadesCommand { get; }
        public ICommand SelecionarAtividadeCommand { get; }
        public ICommand ContinuarCommand { get; }
        public ICommand VoltarCommand { get; }

        public AtividadesViewModel()
        {
            _apiService = new ApiService();
            Title = "Atividades";
            Atividades = new ObservableCollection<Atividade>();
            Agendas = new ObservableCollection<Agenda>();
            
            CarregarAtividadesCommand = new Command(async () => await CarregarAtividadesAsync());
            SelecionarAtividadeCommand = new Command<Atividade>(async (atividade) => await SelecionarAtividadeAsync(atividade));
            ContinuarCommand = new Command(async () => await ContinuarAsync());
            VoltarCommand = new Command(async () => await OnVoltar());

            CarregarAtividadesAsync();
        }

        private async Task CarregarAtividadesAsync()
        {
            IsLoading = true;
            IsBusy = true;

            try
            {
                var atividades = await _apiService.GetAllAsync<Atividade>("/Atividade");
                
                Atividades.Clear();
                foreach (var atividade in atividades)
                {
                    Atividades.Add(atividade);
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Não foi possível carregar as atividades: {ex.Message}", "OK")!;
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
            }
        }

        private async Task SelecionarAtividadeAsync(Atividade atividade)
        {
            AtividadeSelecionada = atividade;
            await CarregarAgendasAsync(atividade.Id);
        }

        private async Task CarregarAgendasAsync(int atividadeId)
        {
            IsLoading = true;
            IsBusy = true;

            try
            {
                var safraId = Preferences.Default.Get("safra_id", 0);
                var dataSelecionada = DateTime.Parse(Preferences.Default.Get("data_selecionada", DateTime.Today.ToString("yyyy-MM-dd")));
                
                var agendas = await _apiService.GetAllAsync<Agenda>("/agenda");
                
                Agendas.Clear();
                foreach (var agenda in agendas.Where(a => 
                    a.SafraId == safraId && 
                    a.AtividadeId == atividadeId && 
                    a.DataHora.Date == dataSelecionada.Date &&
                    a.VagasDisponiveis > 0))
                {
                    Agendas.Add(agenda);
                }

                if (Agendas.Count == 0)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Informação", "Não há horários disponíveis para esta atividade na data selecionada", "OK")!;
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Não foi possível carregar os horários: {ex.Message}", "OK")!;
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
            }
        }

        private async Task ContinuarAsync()
        {
            if (AtividadeSelecionada == null)
            {
                await Application.Current?.MainPage?.DisplayAlert("Atenção", "Selecione uma atividade", "OK")!;
                return;
            }

            if (AgendaSelecionada == null)
            {
                await Application.Current?.MainPage?.DisplayAlert("Atenção", "Selecione um horário", "OK")!;
                return;
            }

            Preferences.Default.Set("atividade_id", AtividadeSelecionada.Id);
            Preferences.Default.Set("atividade_nome", AtividadeSelecionada.Nome);
            Preferences.Default.Set("atividade_valor", AtividadeSelecionada.Valor);
            Preferences.Default.Set("agenda_id", AgendaSelecionada.Id);
            Preferences.Default.Set("agenda_data", AgendaSelecionada.DataHora.ToString("yyyy-MM-dd HH:mm"));

            await ShellNavigationService.NavigateToPagamentoAsync();
        }

        private async Task OnVoltar()
        {
            await ShellNavigationService.GoBackAsync();
        }
    }
}
