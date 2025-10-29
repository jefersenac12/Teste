namespace Teste
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnAgenciaClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CadastroAgenciaPage());
        }

        private void OnFamiliaClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CadastroFamiliaPage());
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LoginPage());

        }




        //private void OnCounterClicked(object sender, EventArgs e)
        //{
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}

        //private void Aula1_Clicked(object sender, EventArgs e)
        //{
        //    Receber.Text = Entrada.Text;
        //}
    }

}
