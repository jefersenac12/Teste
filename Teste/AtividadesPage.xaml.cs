
namespace Teste
{
    public partial class AtividadesPage : ContentPage
    {
        private int adultos = 1;
        private int criancas0a5 = 0;
        private int criancas6a12 = 0;

        private const double precoAdulto = 25.00;
        private const double precoCrianca6a12 = 12.50;

        public AtividadesPage()
        {
            InitializeComponent();
            AtualizarTotal();
        }

        private void BtnVoltar_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void PickerAtividades_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DatePickerData_DateSelected(object sender, DateChangedEventArgs e)
        {
            DisplayAlert("Data selecionada", $"{e.NewDate:dd/MM/yyyy}", "OK");
        }

        private void TimePickerHorario_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimePicker.Time))
                DisplayAlert("Hor?rio selecionado", $"{TimePickerHorario.Time}", "OK");
        }

        private void BtnAdultoMenos_Clicked(object sender, EventArgs e)
        {
            if (adultos > 1)
                adultos--;

            LblAdultos.Text = adultos.ToString();
            AtualizarTotal();
        }

        private void BtnAdultoMais_Clicked(object sender, EventArgs e)
        {
            adultos++;
            LblAdultos.Text = adultos.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca0a5Menos_Clicked(object sender, EventArgs e)
        {
            if (criancas0a5 > 0)
                criancas0a5--;

            LblCrianca0a5.Text = criancas0a5.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca0a5Mais_Clicked(object sender, EventArgs e)
        {
            criancas0a5++;
            LblCrianca0a5.Text = criancas0a5.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca6a12Menos_Clicked(object sender, EventArgs e)
        {
            if (criancas6a12 > 0)
                criancas6a12--;

            LblCrianca6a12.Text = criancas6a12.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca6a12Mais_Clicked(object sender, EventArgs e)
        {
            criancas6a12++;
            LblCrianca6a12.Text = criancas6a12.ToString();
            AtualizarTotal();
        }

        private void AtualizarTotal()
        {
            double total = (adultos * precoAdulto) + (criancas6a12 * precoCrianca6a12);
            LblTotalEstimado.Text = $"Total estimado: R${total:F2}";
        }

        private void BtnContinuar_Clicked(object sender, EventArgs e)
        {
            DisplayAlert("Pagamento", $"Total: {LblTotalEstimado.Text}", "OK");
            Navigation.PushAsync(new PagamentoPage());
        }

        public void SetData(DateTime data)
        {
           
        }
    }
}
