namespace Teste.Views
{
    public partial class InicioPage : ContentPage
    {
        public InicioPage()
        {
            InitializeComponent();
        }

        private async void OnFamiliaClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("CadastroFamilia");
        }

        private async void OnAgenciaClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("CadastroAgencia");
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("Login");
        }
    }
}
