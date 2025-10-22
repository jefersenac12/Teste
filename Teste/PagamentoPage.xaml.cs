using Teste.ViewModels;

namespace Teste;

public partial class PagamentoPage : ContentPage
{
    private decimal totalAmount = 0;
    private string pixKey = "fazenda.villaggio@pix.com.br";

    public PagamentoPage()
    {
        InitializeComponent();
        CarregarDadosReserva();
    }

    private void CarregarDadosReserva()
    {
        //try
        //{
        //    // TODO: Buscar dados da reserva do banco de dados ou API
        //    // Dados mock para teste
        //    ReservationDateLabel.Text = "15 de Julho, 2025 - 10:00";
        //    PeopleCountLabel.Text = "2 pessoas";
        //    TotalAmountLabel.Text = "R$130,00";
        //    totalAmount = 130;

        //    // Atualizar as atividades
        //    AtualizarAtividades();
        //}
        //catch (Exception ex)
        //{
        //    DisplayAlert("Erro", "Não foi possível carregar os dados da reserva.", "OK");
        //}
    }

    private void AtualizarAtividades()
    {
        try
        {
            // TODO: Implementar busca de atividades do banco de dados ou API
            // As atividades já estão no XAML como exemplo
            // Você pode adicionar dinamicamente aqui se necessário
        }
        catch (Exception ex)
        {
            DisplayAlert("Erro", "Não foi possível carregar as atividades.", "OK");
        }
    }

    private async void OnCopyPixKeyClicked(object sender, EventArgs e)
    {
        try
        {
            // Copiar a chave Pix para o clipboard
            await Clipboard.SetTextAsync(pixKey);
            await DisplayAlert("Sucesso", "Chave Pix copiada para o clipboard!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível copiar a chave Pix.", "OK");
        }
    }

    private async void OnConfirmPaymentClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirmar = await DisplayAlert(
                "Confirmar Pagamento",
                $"Tem certeza que deseja confirmar o pagamento de R${totalAmount:F2}?",
                "Sim",
                "Não"
            );

            if (confirmar)
            {
                // TODO: Implementar lógica de processamento de pagamento
                // 1. Enviar comprovante para o servidor
                // 2. Atualizar status da reserva no banco de dados
                // 3. Gerar comprovante

                await DisplayAlert("Sucesso", "Pagamento processado com sucesso! Você receberá um comprovante por SMS.", "OK");

                
                await Navigation.PushAsync(new ReservasAtivasPage());

            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível processar o pagamento. Tente novamente.", "OK");
        }
    }

    private async void OnUploadProofClicked(object sender, EventArgs e)
    {
        try
        {
            // TODO: Implementar upload de comprovante
            // Você pode usar FilePicker ou outro método

            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Selecione o comprovante"
            });

            if (result != null)
            {
                // Enviar arquivo para o servidor
                await DisplayAlert("Sucesso", "Comprovante enviado com sucesso!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível fazer upload do comprovante.", "OK");
        }
    }
}