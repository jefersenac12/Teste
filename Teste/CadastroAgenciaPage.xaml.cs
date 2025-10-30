using System.Text.Json;
using System.Text;

namespace Teste;

public partial class CadastroAgenciaPage : ContentPage
{
    private static readonly HttpClient client = new HttpClient();
    private readonly string apiUrl = "http://tiijeferson.runasp.net/api/Usuario/cadastrarAgencia";
    //private readonly string apiUrl = "https://localhost:7064/api/Usuario/cadastrarAgencia";

    public CadastroAgenciaPage()
    {
        InitializeComponent();
    }

    private async void OnCadastrarAgenciaClicked(object sender, EventArgs e)
    {
        // Valida��o dos campos obrigat�rios
        if (string.IsNullOrWhiteSpace(nomeEntry.Text) ||
            string.IsNullOrWhiteSpace(telefoneEntry.Text) ||
            string.IsNullOrWhiteSpace(emailEntry.Text) ||
            string.IsNullOrWhiteSpace(senhaEntry.Text) ||
            string.IsNullOrWhiteSpace(cnpjEntry.Text))
        {
            await DisplayAlert("Erro", "Todos os campos s�o obrigat�rios.", "OK");
            return;
        }

        if (!emailEntry.Text.Contains("@") || !emailEntry.Text.Contains("."))
        {
            await DisplayAlert("Erro", "E-mail inv�lido.", "OK");
            return;
        }

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
                await DisplayAlert("Sucesso", "Ag�ncia cadastrada com sucesso!", "OK");

                // Limpa os campos ap�s sucesso
                nomeEntry.Text = string.Empty;
                telefoneEntry.Text = string.Empty;
                emailEntry.Text = string.Empty;
                senhaEntry.Text = string.Empty;
                cnpjEntry.Text = string.Empty;

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
            await DisplayAlert("Erro", $"Erro de conex�o: {ex.Message}", "OK");
        }
    }

    private void OnFamiliaClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CadastroFamiliaPage());
    }

    private void OnEntrarClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new LoginPage());
    }

    private void OnCadastrarClicked(object sender, EventArgs e)
    {
        // Navega para a p�gina de cadastro de fam�lia
    }

    private void OnAgenciaClicked(object sender, EventArgs e)
    {
        // J� est� na p�gina de cadastro de ag�ncia
    }
}
