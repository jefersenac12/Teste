using System.Text;
using System.Text.Json;

namespace Teste;

public partial class CadastroFamiliaPage : ContentPage
{
    private HttpClient client = new HttpClient();
    private readonly string apiUrl = "http://tiijeferson.runasp.net/api/Usuario/cadastrarFamilia";

    public CadastroFamiliaPage()
    {
        InitializeComponent();
    }

    private async void OnCadastrarFamiliaClicked(object sender, EventArgs e)
    {
        // Validação dos campos obrigatórios
        if (string.IsNullOrWhiteSpace(nomeEntry.Text) ||
            string.IsNullOrWhiteSpace(telefoneEntry.Text) ||
            string.IsNullOrWhiteSpace(senhaEntry.Text))
        {
            await DisplayAlert("Erro", "Todos os campos são obrigatórios.", "OK");
            return;
        }

        var novaFamilia = new
        {
            Nome = nomeEntry.Text,
            Telefone = telefoneEntry.Text,
            Senha = senhaEntry.Text,
        };

        var json = JsonSerializer.Serialize(novaFamilia);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sucesso", "Família cadastrada com sucesso!", "OK");

                // Limpa os campos após sucesso
                nomeEntry.Text = string.Empty;
                telefoneEntry.Text = string.Empty;
                senhaEntry.Text = string.Empty;

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

    private void OnAgenciaClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CadastroAgenciaPage());
    }

    private void OnEntrarClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new LoginPage());
    }

    private void OnCadastrarClicked(object sender, EventArgs e)
    {
        // Já está na página de cadastro de família
    }
}
