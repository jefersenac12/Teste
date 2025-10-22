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
                "Configurações",
                "Ajuda",
                "Sobre"
            );

            if (action == "Configurações")
            {
                await DisplayAlert("Configurações", "Abrindo configurações...", "OK");
            }
            else if (action == "Ajuda")
            {
                await DisplayAlert("Ajuda", "Abrindo central de ajuda...", "OK");
            }
            else if (action == "Sobre")
            {
                await DisplayAlert("Sobre", "App de Reservas - Versão 1.0", "OK");
            }
        }

        // Anteriores (permanece na página)
        //private void OnAnterioresClicked(object sender, EventArgs e)
        //{
        //    // Já está na página anteriores, não faz nada
        //}

        // Menu
        //private async void OnMenuClicked(object sender, EventArgs e)
        //{
        //    string action = await DisplayActionSheet(
        //        "Menu",
        //        "Cancelar",
        //        null,
        //        "Configurações",
        //        "Ajuda",
        //        "Sobre"
        //    );

        //    if (action == "Configurações")
        //    {
        //        await DisplayAlert("Configurações", "Abrindo configurações...", "OK");
        //    }
        //    else if (action == "Ajuda")
        //    {
        //        await DisplayAlert("Ajuda", "Abrindo central de ajuda...", "OK");
        //    }
        //    else if (action == "Sobre")
        //    {
        //        await DisplayAlert("Sobre", "App de Reservas - Versão 1.0", "OK");
        //    }
        //}

        //// Ativas (permanece na página)
        //private void OnAtivasClicked(object sender, EventArgs e)
        //{
        //    // Já está na página ativa, não faz nada
        //}

        //// Excluir reserva
        //private async void OnExcluirClicked(object sender, EventArgs e)
        //{
        //    bool answer = await DisplayAlert(
        //        "Confirmar Exclusão",
        //        "Deseja realmente excluir esta reserva?",
        //        "Sim",
        //        "Não"
        //    );

        //    if (answer)
        //    {
        //        await DisplayAlert("Sucesso", "Reserva excluída com sucesso!", "OK");
        //        // Aqui você pode adicionar a lógica para remover a reserva do backend
        //    }
        //}

        // Sair (logout ou voltar para tela inicial)
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
                // Navegar para a página de login ou página inicial
                await Navigation.PopToRootAsync();
                // Ou se quiser ir para uma página específica:
                // Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        // Ver todas as reservas
        private async void OnVerTodasClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Todas as Reservas", "Carregando todas as reservas...", "OK");
            // Aqui você pode carregar mais reservas ou expandir a lista
        }

        // Ver detalhes da reserva
        private async void OnVerDetalhesClicked(object sender, EventArgs e)
        {
            // Você pode passar o ID da reserva para a página de detalhes
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
                "Confirmar Exclusão",
                "Deseja realmente excluir esta reserva?",
                "Sim",
                "Não"
            );

            if (answer)
            {
                await DisplayAlert("Sucesso", "Reserva excluída com sucesso!", "OK");
                // Aqui você pode adicionar a lógica para remover a reserva do backend
            }
        }
    }
}