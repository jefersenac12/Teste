

namespace Teste;

public partial class PerfilPage : ContentPage
{
    private string usuarioId;

    public PerfilPage()
    {
        InitializeComponent();
        CarregarDadosPerfil();
    }

    private void CarregarDadosPerfil()
    {
        try
        {
            // TODO: Implementar busca de dados do usuário no banco de dados ou API
            // Exemplo:
            // usuarioId = Preferences.Get("user_id", "");
            // var usuario = await _apiService.ObterUsuario(usuarioId);

            // Dados mock para teste
            UserNameLabel.Text = "João Silva";
            UserPhoneLabel.Text = "(12) 99999-9999";
        }
        catch (Exception ex)
        {
            DisplayAlert("Erro", "Não foi possível carregar os dados do perfil.", "OK");
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        try
        {
            // TODO: Criar a página EditarPerfilPage ou substituir pelo nome da página correta
            // await Navigation.PushAsync(new EditarPerfilPage());

            await DisplayAlert("Editar Perfil", "Página de edição em desenvolvimento.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível abrir a página de edição.", "OK");
        }
    }

    private async void OnDeleteReservationClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirmDelete = await DisplayAlert(
                "Confirmar Exclusão",
                "Tem certeza que deseja excluir esta reserva?",
                "Sim",
                "Não"
            );

            if (confirmDelete)
            {
                // TODO: Implementar exclusão da reserva no banco de dados ou API
                // Remover item da lista
                await DisplayAlert("Sucesso", "Reserva excluída com sucesso.", "OK");
                // Recarregar a lista de reservas
                CarregarDadosPerfil();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível excluir a reserva.", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirmLogout = await DisplayAlert(
                "Confirmar Saída",
                "Tem certeza que deseja sair?",
                "Sim",
                "Não"
            );

            if (confirmLogout)
            {
                // TODO: Limpar dados de sessão
                // Preferences.Remove("user_id");
                // Preferences.Remove("user_token");

                // Voltar para página de login
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível realizar logout.", "OK");
        }
    }
}
