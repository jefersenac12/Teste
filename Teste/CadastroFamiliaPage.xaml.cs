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
                string respostaJson = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(respostaJson);
                    var root = doc.RootElement;
                    int usuarioId = 0;
                    string nome = "";
                    string telefone = "";

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number) usuarioId = idEl.GetInt32();
                        else if (root.TryGetProperty("UsuarioId", out var idEl2) && idEl2.ValueKind == JsonValueKind.Number) usuarioId = idEl2.GetInt32();
                        else if (root.TryGetProperty("usuario", out var usuarioEl) && usuarioEl.ValueKind == JsonValueKind.Object)
                        {
                            if (usuarioEl.TryGetProperty("id", out var nestedId) && nestedId.ValueKind == JsonValueKind.Number) usuarioId = nestedId.GetInt32();
                            nome = usuarioEl.TryGetProperty("nome", out var n) && n.ValueKind == JsonValueKind.String ? (n.GetString() ?? "") : "";
                            telefone = usuarioEl.TryGetProperty("telefone", out var t) && t.ValueKind == JsonValueKind.String ? (t.GetString() ?? "") : "";
                        }
                        else
                        {
                            nome = root.TryGetProperty("nome", out var n) && n.ValueKind == JsonValueKind.String ? (n.GetString() ?? "") : "";
                            telefone = root.TryGetProperty("telefone", out var t) && t.ValueKind == JsonValueKind.String ? (t.GetString() ?? "") : "";
                        }
                    }

                    if (usuarioId > 0)
                    {
                        Preferences.Set("UsuarioId", usuarioId);
                        Preferences.Set("ClienteId", usuarioId);
                        Preferences.Set("UsuarioNome", string.IsNullOrEmpty(nome) ? nomeEntry.Text : nome);
                        Preferences.Set("UsuarioTelefone", string.IsNullOrEmpty(telefone) ? telefoneEntry.Text : telefone);
                        Preferences.Set("UsuarioTipo", "Familia");
                    }
                }
                catch { }

                await DisplayAlert("Sucesso", "Família cadastrada com sucesso!", "OK");

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
