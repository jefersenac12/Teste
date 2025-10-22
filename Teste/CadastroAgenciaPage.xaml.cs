using System.Text.Json;
using System.Text;

namespace Teste;

public partial class CadastroAgenciaPage : ContentPage
{
    private static readonly HttpClient client = new HttpClient();
    private readonly string apiUrl = "https://localhost:7064/api/Usuario/cadastrarAgencia";

    public CadastroAgenciaPage()
    {
        InitializeComponent();
    }

    private async void OnCadastrarAgenciaClicked(object sender, EventArgs e)
    {
        var novoUsuario = new
        {
            Nome = nomeEntry.Text,
            Telefone = telefoneEntry.Text,
            Email = emailEntry.Text,
            Senha = senhaEntry.Text,
            CNPJ = cnpjEntry.Text
        };

        var json = JsonSerializer.Serialize(novoUsuario);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sucesso", "Agência cadastrada com sucesso!", "OK");
                await Navigation.PushAsync(new AgendamentoPage());
            }
            else
            {
                var erro = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Erro", $"Falha ao cadastrar: {erro}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro de conexão: {ex.Message}", "OK");
        }
    }

    private void OnFamiliaClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CadastroFamiliaPage());
    }

    private void OnEntrarClicked(object sender, EventArgs e)
    {
        // Volta para a tela de Login
        Navigation.PushAsync(new LoginPage());
    }

    private void OnCadastrarClicked(object sender, EventArgs e)
    {
        // Navega para a pagina de cadastro de famalia
    }

    private void OnAgenciaClicked(object sender, EventArgs e)
    {

    }
}

