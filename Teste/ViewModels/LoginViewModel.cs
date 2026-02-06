using System.ComponentModel;
using System.Windows.Input;
using Teste.Models;
using Teste.Services;

namespace Teste.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private string _tipo = "Familia";
        private bool _isLoading;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        public string Tipo
        {
            get => _tipo;
            set => SetProperty(ref _tipo, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand VoltarCommand { get; }
        public ICommand CadastroCommand { get; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
            Title = "Login";
            LoginCommand = new Command(async () => await OnLoginAsync());
            VoltarCommand = new Command(async () => await OnVoltar());
            CadastroCommand = new Command(async () => await OnCadastro());
        }

        private async Task OnLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", "Preencha todos os campos", "OK")!;
                return;
            }

            IsLoading = true;
            IsBusy = true;

            try
            {
                var usuario = await _apiService.LoginAsync<Usuario>(Email, Senha, Tipo);
                
                if (usuario != null)
                {
                    Preferences.Default.Set("usuario_id", usuario.Id);
                    Preferences.Default.Set("usuario_nome", usuario.Nome);
                    Preferences.Default.Set("usuario_tipo", usuario.Tipo);

                    await Application.Current?.MainPage?.DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK")!;
                    await ShellNavigationService.ClearAndNavigateToPageAsync("Agendamento");
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Erro", "Email ou senha incorretos", "OK")!;
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

        private async Task OnCadastro()
        {
            if (Tipo == "Familia")
                await ShellNavigationService.NavigateToCadastroFamiliaAsync();
            else
                await ShellNavigationService.NavigateToCadastroAgenciaAsync();
        }
    }
}
