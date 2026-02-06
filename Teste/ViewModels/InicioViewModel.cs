using System.Windows.Input;
using Teste.Services;

namespace Teste.ViewModels
{
    public class InicioViewModel : BaseViewModel
    {
        public ICommand FamiliaCommand { get; }
        public ICommand AgenciaCommand { get; }
        public ICommand LoginCommand { get; }

        public InicioViewModel()
        {
            Title = "Início";
            FamiliaCommand = new Command(async () => await OnFamiliaClicked());
            AgenciaCommand = new Command(async () => await OnAgenciaClicked());
            LoginCommand = new Command(async () => await OnLoginClicked());
        }

        private async Task OnFamiliaClicked()
        {
            await ShellNavigationService.NavigateToCadastroFamiliaAsync();
        }

        private async Task OnAgenciaClicked()
        {
            await ShellNavigationService.NavigateToCadastroAgenciaAsync();
        }

        private async Task OnLoginClicked()
        {
            await ShellNavigationService.NavigateToLoginAsync();
        }
    }
}
