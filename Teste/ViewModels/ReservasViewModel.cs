using System.Collections.ObjectModel;
using System.Windows.Input;
using Teste.Models;
using Teste.Services;

namespace Teste.ViewModels
{
    public class ReservasViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Reserva> _reservas;
        private Reserva? _reservaSelecionada;
        private bool _isLoading;

        public ObservableCollection<Reserva> Reservas
        {
            get => _reservas;
            set => SetProperty(ref _reservas, value);
        }

        public Reserva? ReservaSelecionada
        {
            get => _reservaSelecionada;
            set => SetProperty(ref _reservaSelecionada, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand CarregarReservasCommand { get; }
        public ICommand CancelarReservaCommand { get; }
        public ICommand DetalhesCommand { get; }
        public ICommand NovoAgendamentoCommand { get; }
        public ICommand LogoutCommand { get; }

        public ReservasViewModel()
        {
            _apiService = new ApiService();
            Title = "Minhas Reservas";
            Reservas = new ObservableCollection<Reserva>();
            
            CarregarReservasCommand = new Command(async () => await CarregarReservasAsync());
            CancelarReservaCommand = new Command<Reserva>(async (reserva) => await CancelarReservaAsync(reserva));
            DetalhesCommand = new Command<Reserva>(async (reserva) => await MostrarDetalhesAsync(reserva));
            NovoAgendamentoCommand = new Command(async () => await OnNovoAgendamento());
            LogoutCommand = new Command(async () => await OnLogout());

            CarregarReservasAsync();
        }

        private async Task CarregarReservasAsync()
        {
            IsLoading = true;
            IsBusy = true;

            try
            {
                var usuarioId = Preferences.Default.Get("usuario_id", 0);
                var reservas = await _apiService.GetAllAsync<Reserva>("/Reserva");
                
                Reservas.Clear();
                foreach (var reserva in reservas.Where(r => r.UsuarioId == usuarioId))
                {
                    Reservas.Add(reserva);
                }

                if (Reservas.Count == 0)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Informação", "Você não possui reservas", "OK")!;
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Não foi possível carregar as reservas: {ex.Message}", "OK")!;
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
            }
        }

        private async Task CancelarReservaAsync(Reserva reserva)
        {
            if (reserva == null) return;

            var result = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar Cancelamento", 
                $"Deseja cancelar a reserva {reserva.Id}?", 
                "Sim", 
                "Não")!;

            if (!result) return;

            IsLoading = true;
            IsBusy = true;

            try
            {
                var dataReserva = reserva.DataReserva;
                var horasDiferenca = (DateTime.Now - dataReserva).TotalHours;

                if (horasDiferenca < 24)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Erro", "Só é possível cancelar reservas com pelo menos 24 horas de antecedência", "OK")!;
                    return;
                }

                var sucesso = await _apiService.DeleteAsync("/Reserva", reserva.Id);
                
                if (sucesso)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Sucesso", "Reserva cancelada com sucesso", "OK")!;
                    await CarregarReservasAsync();
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Erro", "Não foi possível cancelar a reserva", "OK")!;
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK")!;
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
            }
        }

        private async Task MostrarDetalhesAsync(Reserva reserva)
        {
            if (reserva == null) return;

            try
            {
                var agenda = await _apiService.GetByIdAsync<Agenda>("/agenda", reserva.AgendaId);
                var atividade = await _apiService.GetByIdAsync<Atividade>("/Atividade", agenda?.AtividadeId ?? 0);
                var safra = await _apiService.GetByIdAsync<Safra>("/Safra", agenda?.SafraId ?? 0);

                var detalhes = $"Reserva: {reserva.Id}\n" +
                              $"Safra: {safra?.Nome ?? "N/A"}\n" +
                              $"Atividade: {atividade?.Nome ?? "N/A"}\n" +
                              $"Data/Hora: {agenda?.DataHora:dd/MM/yyyy HH:mm}\n" +
                              $"Quantidade: {reserva.Quantidade}\n" +
                              $"NP: {reserva.NPEntrada} | Meia: {reserva.MeiaEntrada} | Inteira: {reserva.InteiraEntrada}\n" +
                              $"Status: {reserva.Status}\n" +
                              $"Data Reserva: {reserva.DataReserva:dd/MM/yyyy HH:mm}";

                await Application.Current?.MainPage?.DisplayAlert("Detalhes da Reserva", detalhes, "OK")!;
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Não foi possível carregar os detalhes: {ex.Message}", "OK")!;
            }
        }

        private async Task OnNovoAgendamento()
        {
            await ShellNavigationService.NavigateToAgendamentoAsync();
        }

        private async Task OnLogout()
        {
            Preferences.Default.Clear();
            await ShellNavigationService.LogoutAsync();
        }
    }
}
