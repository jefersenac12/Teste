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
            {
                identificadorEntry.Placeholder = "CNPJ detectado";
            }
            else if (apenasNumeros.Length >= 10)
            {
                identificadorEntry.Placeholder = "Telefone detectado";
            }
            else
            {
                identificadorEntry.Placeholder = "Telefone ou CNPJ";
            }
        }

        private async void OnEntrarClicked(object sender, EventArgs e)
        {
            string identificador = identificadorEntry.Text;
            string senha = senhaEntry.Text;

            if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(senha))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            // Tenta primeiro como Família
            bool loginFamilia = await TentarLoginFamilia(identificador, senha);
            if (loginFamilia)
            {
                LimparCampos();
                return;
            }

            // Se falhar, tenta como Agência
            bool loginAgencia = await TentarLoginAgencia(identificador, senha);
            if (loginAgencia)
            {
                LimparCampos();
                return;
            }

            // Se ambos falharem
            await DisplayAlert("Falha", "Login não encontrado", "OK");
        }

        private async Task<bool> TentarLoginFamilia(string telefone, string senha)
        {
            try
            {
                var loginRequest = new
                {
                    Telefone = telefone,
                    Senha = senha
                };

                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrlFamilia, content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sucesso", "Login Família realizado!", "OK");
                    await Navigation.PushAsync(new AtividadesPage());
                    return true;
                }
            }
            catch { }

            return false;
        }

        private async Task<bool> TentarLoginAgencia(string cnpj, string senha)
        {
            try
            {
                var loginRequest = new
                {
                    CNPJ = cnpj,
                    Senha = senha
                };

                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrlAgencia, content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sucesso", "Login Agência realizado!", "OK");
                    await Navigation.PushAsync(new AtividadesPage());
                    return true;
                }
            }
            catch { }

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