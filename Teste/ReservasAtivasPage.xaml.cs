// ReservasAtivasPage.xaml.cs
using Microsoft.Maui.Controls;

namespace Teste
{
    public partial class ReservasAtivasPage : ContentPage
    {
        public ReservasAtivasPage()
        {
            InitializeComponent();
        }

        // Navegar para Anteriores
        private async void OnAnterioresClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReservasAnterioresPage());
        }

        // Voltar
        private async void OnVoltarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Menu
        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "Menu",
                "Cancelar",
                null,
                "Configura��es",
                "Ajuda",
                "Sobre"
            );

            if (action == "Configura��es")
            {
                await DisplayAlert("Configura��es", "Abrindo configura��es...", "OK");
            }
            else if (action == "Ajuda")
            {
                await DisplayAlert("Ajuda", "Abrindo central de ajuda...", "OK");
            }
            else if (action == "Sobre")
            {
                await DisplayAlert("Sobre", "App de Reservas - Vers�o 1.0", "OK");
            }
        }

        // Anteriores (permanece na p�gina)
        //private void OnAnterioresClicked(object sender, EventArgs e)
        //{
        //    // J� est� na p�gina anteriores, n�o faz nada
        //}

        // Menu
        //private async void OnMenuClicked(object sender, EventArgs e)
        //{
        //    string action = await DisplayActionSheet(
        //        "Menu",
        //        "Cancelar",
        //        null,
        //        "Configura��es",
        //        "Ajuda",
        //        "Sobre"
        //    );

        //    if (action == "Configura��es")
        //    {
        //        await DisplayAlert("Configura��es", "Abrindo configura��es...", "OK");
        //    }
        //    else if (action == "Ajuda")
        //    {
        //        await DisplayAlert("Ajuda", "Abrindo central de ajuda...", "OK");
        //    }
        //    else if (action == "Sobre")
        //    {
        //        await DisplayAlert("Sobre", "App de Reservas - Vers�o 1.0", "OK");
        //    }
        //}

        //// Ativas (permanece na p�gina)
        //private void OnAtivasClicked(object sender, EventArgs e)
        //{
        //    // J� est� na p�gina ativa, n�o faz nada
        //}

        //// Excluir reserva
        //private async void OnExcluirClicked(object sender, EventArgs e)
        //{
        //    bool answer = await DisplayAlert(
        //        "Confirmar Exclus�o",
        //        "Deseja realmente excluir esta reserva?",
        //        "Sim",
        //        "N�o"
        //    );

        //    if (answer)
        //    {
        //        await DisplayAlert("Sucesso", "Reserva exclu�da com sucesso!", "OK");
        //        // Aqui voc� pode adicionar a l�gica para remover a reserva do backend
        //    }
        //}

        // Sair (logout ou voltar para tela inicial)
        private async void OnSairClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Confirmar Sa�da",
                "Deseja realmente sair?",
                "Sim",
                "N�o"
            );

            if (answer)
            {
                // Navegar para a p�gina de login ou p�gina inicial
                await Navigation.PopToRootAsync();
                // Ou se quiser ir para uma p�gina espec�fica:
                // Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        // Ver todas as reservas
        private async void OnVerTodasClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Todas as Reservas", "Carregando todas as reservas...", "OK");
            // Aqui voc� pode carregar mais reservas ou expandir a lista
        }

        // Ver detalhes da reserva
        private async void OnVerDetalhesClicked(object sender, EventArgs e)
        {
            // Voc� pode passar o ID da reserva para a p�gina de detalhes
            await DisplayAlert("Detalhes", "Abrindo detalhes da reserva...", "OK");
           
        }

        // Clique no card da reserva
        private async void OnCardClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Reserva", "Abrindo detalhes da reserva...", "OK");
         
        }

        // Buscar reservas
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue;
           
        }

        private void OnAtivasClicked(object sender, EventArgs e)
        {

        }

        private async void OnExcluirClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Confirmar Exclus�o",
                "Deseja realmente excluir esta reserva?",
                "Sim",
                "N�o"
            );

            if (answer)
            {
                await DisplayAlert("Sucesso", "Reserva exclu�da com sucesso!", "OK");
                // Aqui voc� pode adicionar a l�gica para remover a reserva do backend
            }
        }
    }
}