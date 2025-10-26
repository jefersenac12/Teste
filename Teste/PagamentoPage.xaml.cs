using Microsoft.Maui.Storage;
using System;

namespace Teste;

public partial class PagamentoPage : ContentPage
{
    private decimal totalAmount = 0;
    private string pixKey = "fazenda.villaggio@pix.com.br";
    private string comprovantePath = string.Empty;

    public PagamentoPage()
    {
        InitializeComponent();
        CarregarDadosReserva();
    }

    private void CarregarDadosReserva()
    {
        try
        {
            string data = Preferences.Get("DataAgendamento", "");
            string horario = Preferences.Get("HorarioSelecionado", "");
            string atividades = Preferences.Get("AtividadesSelecionadas", "");
            int adultos = Preferences.Get("QtdAdultos", 0);
            int criancas0a5 = Preferences.Get("QtdCriancas0a5", 0);
            int criancas6a12 = Preferences.Get("QtdCriancas6a12", 0);
            string total = Preferences.Get("TotalEstimado", "0");

            LblDataHorario.Text = $"{DateTime.Parse(data):dd/MM/yyyy} - {horario}";
            LblPessoas.Text = $"{adultos} Adultos, {criancas0a5} Crianças (0–5), {criancas6a12} Crianças (6–12)";
            LblAtividades.Text = atividades;
            LblTotal.Text = $"R${total}";

            decimal.TryParse(total, out totalAmount);
        }
        catch
        {
            DisplayAlert("Erro", "Não foi possível carregar os dados da reserva.", "OK");
        }
    }

    private async void OnCopyPixKeyClicked(object sender, EventArgs e)
    {
        try
        {
            await Clipboard.SetTextAsync(pixKey);
            await DisplayAlert("Sucesso", "Chave Pix copiada para o clipboard!", "OK");
        }
        catch
        {
            await DisplayAlert("Erro", "Não foi possível copiar a chave Pix.", "OK");
        }
    }

    private async void OnUploadProofClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione o comprovante de pagamento",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                comprovantePath = result.FullPath;
                ImgComprovantePreview.Source = ImageSource.FromFile(comprovantePath);
                ImgComprovantePreview.IsVisible = true;

                await DisplayAlert("Sucesso", "Comprovante selecionado com sucesso!", "OK");
            }
        }
        catch
        {
            await DisplayAlert("Erro", "Não foi possível anexar o comprovante.", "OK");
        }
    }

    private async void OnConfirmPaymentClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(comprovantePath))
        {
            bool continuar = await DisplayAlert("Aviso", "Você não anexou um comprovante. Deseja continuar mesmo assim?", "Sim", "Cancelar");
            if (!continuar) return;
        }

        bool confirmar = await DisplayAlert(
            "Confirmar Pagamento",
            $"Deseja confirmar o pagamento de R${totalAmount:F2}?",
            "Sim",
            "Não"
        );

        if (confirmar)
        {
            await DisplayAlert("Sucesso", "Pagamento confirmado! Sua reserva foi registrada.", "OK");
            await Navigation.PushAsync(new ReservasAtivasPage());
        }
    }
}
