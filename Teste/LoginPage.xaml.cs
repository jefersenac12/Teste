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

                    int id = root.GetProperty("id").GetInt32();
                    string nome = root.TryGetProperty("nome", out var n) ? n.GetString() ?? "" : "";
                    string email = root.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";

                    Preferences.Set("UsuarioId", id);
                    Preferences.Set("UsuarioNome", nome);
                    Preferences.Set("UsuarioEmail", email);
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
