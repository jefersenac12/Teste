using System.ComponentModel;
using System.Windows.Input;
using Teste.Models;
using Teste.Services;

namespace Teste.ViewModels
{
    public class PagamentoViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _quantidade = 1;
        private int _npEntrada = 0;
        private int _meiaEntrada = 0;
        private int _inteiraEntrada = 1;
        private decimal _valorTotal;
        private string _qrCode = string.Empty;
        private bool _isLoading;
        private bool _pagamentoGerado;

        public int Quantidade
        {
            get => _quantidade;
            set
            {
                if (SetProperty(ref _quantidade, value))
                {
                    CalcularTotal();
                }
            }
        }

        public int NpEntrada
        {
            get => _npEntrada;
            set
            {
                if (SetProperty(ref _npEntrada, value))
                {
                    CalcularTotal();
                }
            }
        }

        public int MeiaEntrada
        {
            get => _meiaEntrada;
            set
            {
                if (SetProperty(ref _meiaEntrada, value))
                {
                    CalcularTotal();
                }
            }
        }

        public int InteiraEntrada
        {
            get => _inteiraEntrada;
            set
            {
                if (SetProperty(ref _inteiraEntrada, value))
                {
                    CalcularTotal();
                }
            }
        }

        public decimal ValorTotal
        {
            get => _valorTotal;
            set => SetProperty(ref _valorTotal, value);
        }

        public string QrCode
        {
            get => _qrCode;
            set => SetProperty(ref _qrCode, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool PagamentoGerado
        {
            get => _pagamentoGerado;
            set => SetProperty(ref _pagamentoGerado, value);
        }

        public ICommand GerarPagamentoCommand { get; }
        public ICommand ConfirmarPagamentoCommand { get; }
        public ICommand VoltarCommand { get; }

        public PagamentoViewModel()
        {
            _apiService = new ApiService();
            Title = "Pagamento";
            
            GerarPagamentoCommand = new Command(async () => await GerarPagamentoAsync());
            ConfirmarPagamentoCommand = new Command(async () => await ConfirmarPagamentoAsync());
            VoltarCommand = new Command(async () => await OnVoltar());

            CalcularTotal();
        }

        private void CalcularTotal()
        {
            var valorAtividade = Preferences.Default.Get("atividade_valor", 0m);
            var total = (NpEntrada * 0) + (MeiaEntrada * valorAtividade * 0.5m) + (InteiraEntrada * valorAtividade);
            ValorTotal = total;
        }

        private async Task GerarPagamentoAsync()
        {
            if (Quantidade != NpEntrada + MeiaEntrada + InteiraEntrada)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", "A quantidade deve ser igual à soma das entradas", "OK")!;
                return;
            }

            if (Quantidade <= 0)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", "A quantidade deve ser maior que zero", "OK")!;
                return;
            }

            IsLoading = true;
            IsBusy = true;

            try
            {
                var agendaId = Preferences.Default.Get("agenda_id", 0);
                var qrCode = await _apiService.GerarPixAsync(agendaId, ValorTotal);
                
                if (!string.IsNullOrEmpty(qrCode))
                {
                    QrCode = qrCode;
                    PagamentoGerado = true;
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Erro", "Não foi possível gerar o código PIX", "OK")!;
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

        private async Task ConfirmarPagamentoAsync()
        {
            IsLoading = true;
            IsBusy = true;

            try
            {
                var usuarioId = Preferences.Default.Get("usuario_id", 0);
                var agendaId = Preferences.Default.Get("agenda_id", 0);

                var reserva = new Reserva
                {
                    AgendaId = agendaId,
                    UsuarioId = usuarioId,
                    Quantidade = Quantidade,
                    NpEntrada = NpEntrada,
                    MeiaEntrada = MeiaEntrada,
                    InteiraEntrada = InteiraEntrada,
                    DataReserva = DateTime.Now,
                    Status = "Confirmada"
                };

                var resultado = await _apiService.CreateAsync<Reserva>("/Reserva", reserva);
                
                if (resultado != null)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Sucesso", "Reserva confirmada com sucesso!", "OK")!;
                    await ShellNavigationService.NavigateToReservasAsync();
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Erro", "Não foi possível confirmar a reserva", "OK")!;
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

        private async Task OnVoltar()
        {
            await ShellNavigationService.GoBackAsync();
        }
    }
}
