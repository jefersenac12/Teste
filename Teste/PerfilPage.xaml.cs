

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
            // TODO: Implementar busca de dados do usu�rio no banco de dados ou API
            // Exemplo:
            // usuarioId = Preferences.Get("user_id", "");
            // var usuario = await _apiService.ObterUsuario(usuarioId);

            // Dados mock para teste
            UserNameLabel.Text = "Jo�o Silva";
            UserPhoneLabel.Text = "(12) 99999-9999";
        }
        catch (Exception ex)
        {
            DisplayAlert("Erro", "N�o foi poss�vel carregar os dados do perfil.", "OK");
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
            // TODO: Criar a p�gina EditarPerfilPage ou substituir pelo nome da p�gina correta
            // await Navigation.PushAsync(new EditarPerfilPage());

            await DisplayAlert("Editar Perfil", "P�gina de edi��o em desenvolvimento.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "N�o foi poss�vel abrir a p�gina de edi��o.", "OK");
        }
    }

    private async void OnDeleteReservationClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirmDelete = await DisplayAlert(
                "Confirmar Exclus�o",
                "Tem certeza que deseja excluir esta reserva?",
                "Sim",
                "N�o"
            );

            if (confirmDelete)
            {
                // TODO: Implementar exclus�o da reserva no banco de dados ou API
                // Remover item da lista
                await DisplayAlert("Sucesso", "Reserva exclu�da com sucesso.", "OK");
                // Recarregar a lista de reservas
                CarregarDadosPerfil();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "N�o foi poss�vel excluir a reserva.", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirmLogout = await DisplayAlert(
                "Confirmar Sa�da",
                "Tem certeza que deseja sair?",
                "Sim",
                "N�o"
            );

            if (confirmLogout)
            {
                // TODO: Limpar dados de sess�o
                // Preferences.Remove("user_id");
                // Preferences.Remove("user_token");

                // Voltar para p�gina de login
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "N�o foi poss�vel realizar logout.", "OK");
        }
    }
}
