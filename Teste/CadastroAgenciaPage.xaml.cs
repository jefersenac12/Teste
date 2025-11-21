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
        // Validação dos campos obrigatórios
        if (string.IsNullOrWhiteSpace(nomeEntry.Text) ||
            string.IsNullOrWhiteSpace(telefoneEntry.Text) ||
            string.IsNullOrWhiteSpace(emailEntry.Text) ||
            string.IsNullOrWhiteSpace(senhaEntry.Text) ||
            string.IsNullOrWhiteSpace(cnpjEntry.Text))
        {
            await DisplayAlert("Erro", "Todos os campos são obrigatórios.", "OK");
            return;
        }

        if (!emailEntry.Text.Contains("@") || !emailEntry.Text.Contains("."))
        {
            await DisplayAlert("Erro", "E-mail inválido.", "OK");
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
                string respostaJson = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(respostaJson);
                    var root = doc.RootElement;
                    int usuarioId = 0;
                    string nome = "";
                    string telefone = "";
                    string email = "";

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number) usuarioId = idEl.GetInt32();
                        else if (root.TryGetProperty("UsuarioId", out var idEl2) && idEl2.ValueKind == JsonValueKind.Number) usuarioId = idEl2.GetInt32();
                        else if (root.TryGetProperty("usuario", out var usuarioEl) && usuarioEl.ValueKind == JsonValueKind.Object)
                        {
                            if (usuarioEl.TryGetProperty("id", out var nestedId) && nestedId.ValueKind == JsonValueKind.Number) usuarioId = nestedId.GetInt32();
                            nome = usuarioEl.TryGetProperty("nome", out var n) && n.ValueKind == JsonValueKind.String ? (n.GetString() ?? "") : "";
                            telefone = usuarioEl.TryGetProperty("telefone", out var t) && t.ValueKind == JsonValueKind.String ? (t.GetString() ?? "") : "";
                            email = usuarioEl.TryGetProperty("email", out var emailEl) && emailEl.ValueKind == JsonValueKind.String ? (emailEl.GetString() ?? "") : "";
                        }
                        else
                        {
                            nome = root.TryGetProperty("nome", out var n) && n.ValueKind == JsonValueKind.String ? (n.GetString() ?? "") : "";
                            telefone = root.TryGetProperty("telefone", out var t) && t.ValueKind == JsonValueKind.String ? (t.GetString() ?? "") : "";
                            email = root.TryGetProperty("email", out var emailEl2) && emailEl2.ValueKind == JsonValueKind.String ? (emailEl2.GetString() ?? "") : "";
                        }
                    }

                    if (usuarioId > 0)
                    {
                        Preferences.Set("UsuarioId", usuarioId);
                        Preferences.Set("ClienteId", usuarioId);
                        Preferences.Set("UsuarioNome", string.IsNullOrEmpty(nome) ? nomeEntry.Text : nome);
                        Preferences.Set("UsuarioTelefone", string.IsNullOrEmpty(telefone) ? telefoneEntry.Text : telefone);
                        Preferences.Set("UsuarioEmail", string.IsNullOrEmpty(email) ? emailEntry.Text : email);
                        Preferences.Set("UsuarioTipo", "Agência");
                    }
                }
                catch { }

                await DisplayAlert("Sucesso", "Agência cadastrada com sucesso!", "OK");
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
            await DisplayAlert("Erro", $"Erro de conexão: {ex.Message}", "OK");
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
        // Navega para a página de cadastro de família
    }

    private void OnAgenciaClicked(object sender, EventArgs e)
    {
        // Já está na página de cadastro de agência
    }
}
