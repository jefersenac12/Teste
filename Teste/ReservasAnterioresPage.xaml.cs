

namespace Teste
{
    public partial class ReservasAnterioresPage : ContentPage
    {
        public ReservasAnterioresPage()
        {
            InitializeComponent();
        }

        // Navegar para Ativas
        private async void OnAtivasClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Voltar
        private async void OnVoltarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Ver todas as reservas
        private async void OnVerTodasClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Todas as Reservas", "Carregando todas as reservas...", "OK");
            // Aqui você pode carregar mais reservas ou navegar para uma lista completa
        }

        // Sair
        private async void OnSairClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Confirmar Saída",
                "Deseja realmente sair?",
                "Sim",
                "Não"
            );

            if (answer)
            {
                await Navigation.PopToRootAsync();
            }
        }

        // Ver detalhes da reserva
        private async void OnCardTapped(object sender, EventArgs e)
        {
            await DisplayAlert("Detalhes", "Abrindo detalhes da reserva anterior...", "OK");
            // await Navigation.PushAsync(new DetalhesReservaPage(reservaId));
        }

        // Clique no card da reserva
        private async void OnCardClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Reserva", "Abrindo detalhes da reserva anterior...", "OK");
            // await Navigation.PushAsync(new DetalhesReservaPage(reservaId));
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {

        }

        private void OnAnterioresClicked(object sender, EventArgs e)
        {

        }
    }
}

