using Microsoft.Maui.Storage;
using System;

namespace Teste
{
    public partial class AtividadesPage : ContentPage
    {
        private int adultos = 0;
        private int criancas0a5 = 0;
        private int criancas6a12 = 0;

        private readonly double precoBasico = 25.00;
        private readonly double precoCompleto = 65.00;
        private readonly double precoTrezinho = 15.00;
        private readonly double precoCrianca6a12 = 12.50;

        private DateTime dataSelecionada;
        private TimeSpan? horarioSelecionado = null;

        public AtividadesPage()
        {
            InitializeComponent();

            // Recupera data salva
            if (Preferences.ContainsKey("DataAgendamento"))
            {
                string dataStr = Preferences.Get("DataAgendamento", "");
                if (DateTime.TryParse(dataStr, out DateTime data))
                    dataSelecionada = data;
                else
                    dataSelecionada = DateTime.Now;
            }
            else
            {
                dataSelecionada = DateTime.Now;
            }

            AtualizarDataNaTela();

            // Eventos para atualizar total quando atividades mudam
            CheckBasico.CheckedChanged += OnAtividadeChanged;
            CheckCompleto.CheckedChanged += OnAtividadeChanged;
            CheckTrezinho.CheckedChanged += OnAtividadeChanged;

            AtualizarTotal();
        }

        private void AtualizarDataNaTela()
        {
            var dataLabel = (this.Content as ScrollView)?
                .Content.FindByName<Label>("LblDataSelecionada");

            if (dataLabel != null)
                dataLabel.Text = dataSelecionada.ToString("dd/MM/yyyy");
        }

        private void BtnVoltar_Clicked(object? sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void OnAtividadeChanged(object? sender, CheckedChangedEventArgs e)
        {
            AtualizarTotal();
        }

        private void TimePickerHorario_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimePicker.Time))
            {
                horarioSelecionado = TimePickerHorario.Time;
                DisplayAlert("Horário selecionado", $"{horarioSelecionado:hh\\:mm}", "OK");
            }
        }

        // =============== CONTADORES ===============
        private void BtnAdultoMenos_Clicked(object? sender, EventArgs e)
        {
            if (adultos > 0) adultos--;
            LblAdultos.Text = adultos.ToString();
            AtualizarTotal();
        }

        private void BtnAdultoMais_Clicked(object? sender, EventArgs e)
        {
            adultos++;
            LblAdultos.Text = adultos.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca0a5Menos_Clicked(object? sender, EventArgs e)
        {
            if (criancas0a5 > 0) criancas0a5--;
            LblCrianca0a5.Text = criancas0a5.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca0a5Mais_Clicked(object? sender, EventArgs e)
        {
            criancas0a5++;
            LblCrianca0a5.Text = criancas0a5.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca6a12Menos_Clicked(object? sender, EventArgs e)
        {
            if (criancas6a12 > 0) criancas6a12--;
            LblCrianca6a12.Text = criancas6a12.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca6a12Mais_Clicked(object? sender, EventArgs e)
        {
            criancas6a12++;
            LblCrianca6a12.Text = criancas6a12.ToString();
            AtualizarTotal();
        }

        // =============== CÁLCULO TOTAL ===============
        private void AtualizarTotal()
        {
            double total = 0;

            // Soma todas as atividades escolhidas para adultos
            if (CheckBasico.IsChecked)
                total += adultos * precoBasico;

            if (CheckCompleto.IsChecked)
                total += adultos * precoCompleto;

            if (CheckTrezinho.IsChecked)
                total += adultos * precoTrezinho;

            // Crianças 6 a 12 anos
            total += criancas6a12 * precoCrianca6a12;

            // Crianças 0 a 5 anos (grátis)
            // Se quiser cobrar algo, adicione aqui:
            // total += criancas0a5 * precoCrianca0a5;

            LblTotalEstimado.Text = $"Total estimado: R${total:F2}";
        }

        // =============== BOTÃO CONTINUAR ===============
        private async void BtnContinuar_Clicked(object? sender, EventArgs e)
        {
            // Valida atividades
            string atividades = "";
            if (CheckBasico.IsChecked) atividades += "Café da manhã Básico, ";
            if (CheckCompleto.IsChecked) atividades += "Café da manhã Completo, ";
            if (CheckTrezinho.IsChecked) atividades += "Trezinho / Colha e Pague";

            atividades = atividades.Trim().TrimEnd(',');

            if (string.IsNullOrEmpty(atividades))
            {
                await DisplayAlert("Aviso", "Selecione pelo menos uma atividade.", "OK");
                return;
            }

            // Valida horário
            if (horarioSelecionado == null)
            {
                await DisplayAlert("Aviso", "Selecione um horário.", "OK");
                return;
            }

            string horario = horarioSelecionado.Value.ToString(@"hh\:mm");

            // Salva dados
            Preferences.Set("AtividadesSelecionadas", atividades);
            Preferences.Set("QtdAdultos", adultos);
            Preferences.Set("QtdCriancas0a5", criancas0a5);
            Preferences.Set("QtdCriancas6a12", criancas6a12);
            Preferences.Set("TotalEstimado", LblTotalEstimado.Text.Replace("Total estimado: R$", "").Trim());
            Preferences.Set("DataAgendamento", dataSelecionada.ToString("yyyy-MM-dd"));
            Preferences.Set("HorarioSelecionado", horario);

            await DisplayAlert("Resumo da reserva",
                $"Atividades: {atividades}\n" +
                $"Data: {dataSelecionada:dd/MM/yyyy}\n" +
                $"Horário: {horario}\n" +
                $"Adultos: {adultos}\n" +
                $"Crianças (0–5): {criancas0a5}\n" +
                $"Crianças (6–12): {criancas6a12}\n\n" +
                $"{LblTotalEstimado.Text}",
                "OK");

            await Navigation.PushAsync(new PagamentoPage());
        }

        // =============== SET DATA ===============
        public void SetData(DateTime data)
        {
            dataSelecionada = data;
            Preferences.Set("DataAgendamento", dataSelecionada.ToString("yyyy-MM-dd"));
            AtualizarDataNaTela();
        }
    }
}