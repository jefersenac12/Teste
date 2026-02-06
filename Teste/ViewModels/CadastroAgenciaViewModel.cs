using System.ComponentModel;
using System.Windows.Input;
using Teste.Models;
using Teste.Services;

namespace Teste.ViewModels
{
    public class CadastroAgenciaViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _nome = string.Empty;
        private string _telefone = string.Empty;
        private string _email = string.Empty;
        private string _cnpj = string.Empty;
        private string _senha = string.Empty;
        private string _confirmarSenha = string.Empty;
        private bool _isLoading;

        public string Nome
        {
            get => _nome;
            set => SetProperty(ref _nome, value);
        }

        public string Telefone
        {
            get => _telefone;
            set => SetProperty(ref _telefone, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Cnpj
        {
            get => _cnpj;
            set => SetProperty(ref _cnpj, value);
        }

        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        public string ConfirmarSenha
        {
            get => _confirmarSenha;
            set => SetProperty(ref _confirmarSenha, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand CadastrarCommand { get; }
        public ICommand VoltarCommand { get; }

        public CadastroAgenciaViewModel()
        {
            _apiService = new ApiService();
            Title = "Cadastro Agência";
            CadastrarCommand = new Command(async () => await OnCadastrarAsync());
            VoltarCommand = new Command(async () => await OnVoltar());
        }

        private async Task OnCadastrarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome) || 
                string.IsNullOrWhiteSpace(Telefone) || 
                string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Cnpj) ||
                string.IsNullOrWhiteSpace(Senha))
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", "Preencha todos os campos", "OK")!;
                return;
            }

            if (Senha != ConfirmarSenha)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", "As senhas não coincidem", "OK")!;
                return;
            }

            if (Senha.Length < 6)
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", "A senha deve ter pelo menos 6 caracteres", "OK")!;
                return;
            }

            if (Cnpj.Length != 14 || !Cnpj.All(char.IsDigit))
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", "CNPJ deve ter 14 dígitos", "OK")!;
                return;
            }

            IsLoading = true;
            IsBusy = true;

            try
            {
                var usuario = new Usuario
                {
                    Nome = Nome,
                    Telefone = Telefone,
                    Email = Email,
                    Cnpj = Cnpj,
                    Senha = Senha,
                    Tipo = "Agencia"
                };

                var resultado = await _apiService.CreateAsync<Usuario>("/Usuario", usuario);
                
                if (resultado != null)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Sucesso", "Cadastro realizado com sucesso!", "OK")!;
                    await ShellNavigationService.NavigateToLoginAsync();
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Erro", "Não foi possível realizar o cadastro", "OK")!;
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
