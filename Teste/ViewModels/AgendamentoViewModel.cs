using System.Collections.ObjectModel;
using System.Windows.Input;
using Teste.Models;
using Teste.Services;

namespace Teste.ViewModels
{
    public class AgendamentoViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Safra> _safras;
        private Safra? _safraSelecionada;
        private bool _isLoading;
        private DateTime _dataSelecionada;

        public ObservableCollection<Safra> Safras
        {
            get => _safras;
            set => SetProperty(ref _safras, value);
        }

        public Safra? SafraSelecionada
        {
            get => _safraSelecionada;
            set => SetProperty(ref _safraSelecionada, value);
        }

        public DateTime DataSelecionada
        {
            get => _dataSelecionada;
            set => SetProperty(ref _dataSelecionada, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand CarregarSafrasCommand { get; }
        public ICommand SelecionarDataCommand { get; }
        public ICommand ContinuarCommand { get; }
        public ICommand LogoutCommand { get; }

        public AgendamentoViewModel()
        {
            _apiService = new ApiService();
            Title = "Agendamento";
            Safras = new ObservableCollection<Safra>();
            DataSelecionada = DateTime.Today;
            
            CarregarSafrasCommand = new Command(async () => await CarregarSafrasAsync());
            SelecionarDataCommand = new Command<Safra>(async (safra) => await SelecionarDataAsync(safra));
            ContinuarCommand = new Command(async () => await ContinuarAsync());
            LogoutCommand = new Command(async () => await OnLogout());

            CarregarSafrasAsync();
        }

        private async Task CarregarSafrasAsync()
        {
            IsLoading = true;
            IsBusy = true;

            try
            {
                var safras = await _apiService.GetAllAsync<Safra>("/Safra");
                
                Safras.Clear();
                foreach (var safra in safras.Where(s => s.DataFim >= DateTime.Today))
                {
                    Safras.Add(safra);
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Não foi possível carregar as safras: {ex.Message}", "OK")!;
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
            }
        }

        private async Task SelecionarDataAsync(Safra safra)
        {
            SafraSelecionada = safra;
        }

        private async Task ContinuarAsync()
        {
            if (SafraSelecionada == null)
            {
                await Application.Current?.MainPage?.DisplayAlert("Atenção", "Selecione uma safra", "OK")!;
                return;
            }

            if (DataSelecionada < SafraSelecionada.DataInicio || DataSelecionada > SafraSelecionada.DataFim)
            {
                await Application.Current?.MainPage?.DisplayAlert("Atenção", "A data selecionada está fora do período da safra", "OK")!;
                return;
            }

            Preferences.Default.Set("safra_id", SafraSelecionada.Id);
            Preferences.Default.Set("safra_nome", SafraSelecionada.Nome);
            Preferences.Default.Set("data_selecionada", DataSelecionada.ToString("yyyy-MM-dd"));

            await ShellNavigationService.NavigateToAtividadesAsync();
        }

        private async Task OnLogout()
        {
            Preferences.Default.Clear();
            await ShellNavigationService.LogoutAsync();
        }
    }
}
