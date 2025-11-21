using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Teste
{
    public partial class LoginPage : ContentPage
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiUrlAgencia = "http://tiijeferson.runasp.net/api/Usuario/logarAgencia";
        private readonly string apiUrlFamilia = "http://tiijeferson.runasp.net/api/Usuario/logarFamilia";

        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnIdentificadorChanged(object sender, TextChangedEventArgs e)
        {
            string texto = e.NewTextValue;

            if (string.IsNullOrWhiteSpace(texto))
            {
                identificadorEntry.Placeholder = "Telefone ou CNPJ";
                return;
            }

            string apenasNumeros = Regex.Replace(texto, @"[^\d]", "");

            if (apenasNumeros.Length >= 14)
                identificadorEntry.Placeholder = "CNPJ detectado";
            else if (apenasNumeros.Length >= 10)
                identificadorEntry.Placeholder = "Telefone detectado";
            else
                identificadorEntry.Placeholder = "Telefone ou CNPJ";
        }

        private async void OnEntrarClicked(object sender, EventArgs e)
        {
            string identificador = identificadorEntry.Text?.Trim();
            string senha = senhaEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(senha))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            // Tenta login como Família
            if (await TentarLogin(apiUrlFamilia, new { Telefone = identificador, Senha = senha }, "Família"))
                return;

            // Se falhar, tenta como Agência
            if (await TentarLogin(apiUrlAgencia, new { CNPJ = identificador, Senha = senha }, "Agência"))
                return;

            await DisplayAlert("Falha", "Credenciais inválidas. Tente novamente.", "OK");
        }

        private static bool TryGetInt(JsonElement el, string name, out int value)
        {
            value = 0;
            if (el.TryGetProperty(name, out var p))
            {
                if (p.ValueKind == JsonValueKind.Number) { value = p.GetInt32(); return true; }
                if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var v)) { value = v; return true; }
            }
            return false;
        }

        private static string GetString(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? (p.GetString() ?? "") : "";
        }

        private async Task<bool> TentarLogin(string url, object request, string tipoUsuario)
        {
            try
            {
                string json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                string respostaJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(respostaJson);
                    var root = doc.RootElement;

                    int usuarioId = 0;
                    string nome = "";
                    string email = "";
                    string telefone = "";

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (TryGetInt(root, "usuarioId", out var uid)) usuarioId = uid;
                        else if (TryGetInt(root, "UsuarioId", out var uid2)) usuarioId = uid2;
                        else if (root.TryGetProperty("usuario", out var usuarioEl) && usuarioEl.ValueKind == JsonValueKind.Object)
                        {
                            if (TryGetInt(usuarioEl, "id", out var nestedId)) usuarioId = nestedId;
                            nome = GetString(usuarioEl, "nome");
                            email = GetString(usuarioEl, "email");
                            telefone = GetString(usuarioEl, "telefone");
                        }
                        else
                        {
                            if (TryGetInt(root, "id", out var topId)) usuarioId = topId;
                            nome = GetString(root, "nome");
                            email = GetString(root, "email");
                            telefone = GetString(root, "telefone");
                        }
                    }

                    if (usuarioId <= 0)
                    {
                        await DisplayAlert("Falha", "Não foi possível identificar o usuário na resposta.", "OK");
                        return false;
                    }

                    Preferences.Set("UsuarioId", usuarioId);
                    Preferences.Set("ClienteId", usuarioId);
                    Preferences.Set("UsuarioNome", nome);
                    Preferences.Set("UsuarioEmail", email);
                    Preferences.Set("UsuarioTelefone", telefone);
                    Preferences.Set("UsuarioTipo", tipoUsuario);

                    await DisplayAlert("Sucesso", $"Login {tipoUsuario} realizado com sucesso!", "OK");
                    await Navigation.PushAsync(new AgendamentoPage());
                    LimparCampos();
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN ERRO] {response.StatusCode}: {respostaJson}");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao realizar login ({tipoUsuario}): {ex.Message}", "OK");
            }

            return false;
        }

        private void LimparCampos()
        {
            identificadorEntry.Text = string.Empty;
            senhaEntry.Text = string.Empty;
        }

        private async void OnCadastrarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CadastroFamiliaPage());
        }
    }
}
