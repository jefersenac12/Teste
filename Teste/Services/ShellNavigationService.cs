using Microsoft.Maui.Controls;

namespace Teste.Services
{
    public class ShellNavigationService
    {
        public static async Task NavigateToPageAsync(string route)
        {
            await Shell.Current.GoToAsync(route);
        }

        public static async Task NavigateToPageAsync(string route, Dictionary<string, object> parameters)
        {
            await Shell.Current.GoToAsync(route, parameters);
        }

        public static async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public static async Task ClearAndNavigateToPageAsync(string route)
        {
            await Shell.Current.GoToAsync($"//{route}");
        }

        public static async Task ClearAndNavigateToPageAsync(string route, Dictionary<string, object> parameters)
        {
            await Shell.Current.GoToAsync($"//{route}", parameters);
        }

        // Métodos específicos para as rotas do aplicativo
        public static async Task NavigateToMainPageAsync()
        {
            await ClearAndNavigateToPageAsync("MainPage");
        }

        public static async Task NavigateToCadastroFamiliaAsync()
        {
            await NavigateToPageAsync("CadastroFamilia");
        }

        public static async Task NavigateToCadastroAgenciaAsync()
        {
            await NavigateToPageAsync("CadastroAgencia");
        }

        public static async Task NavigateToLoginAsync()
        {
            await NavigateToPageAsync("Login");
        }

        public static async Task NavigateToAgendamentoAsync()
        {
            await NavigateToPageAsync("Agendamento");
        }

        public static async Task NavigateToAtividadesAsync()
        {
            await NavigateToPageAsync("Atividades");
        }

        public static async Task NavigateToPagamentoAsync()
        {
            await NavigateToPageAsync("Pagamento");
        }

        public static async Task NavigateToReservasAsync()
        {
            await NavigateToPageAsync("Reservas");
        }

        public static async Task NavigateToPerfilAsync()
        {
            await NavigateToPageAsync("Perfil");
        }

        // Método para logout (limpa a navegação e volta para o início)
        public static async Task LogoutAsync()
        {
            Preferences.Default.Clear();
            await ClearAndNavigateToPageAsync("MainPage");
        }
    }
}
