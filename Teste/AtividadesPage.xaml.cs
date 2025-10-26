using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;

namespace Teste
{
    public partial class AtividadesPage : ContentPage
    {
        private int adultos = 1;
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
            LblAdultos.Text = adultos.ToString();

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

            CheckBasico.CheckedChanged += OnAtividadeChanged;
            CheckCompleto.CheckedChanged += OnAtividadeChanged;
            CheckTrezinho.CheckedChanged += OnAtividadeChanged;

            AtualizarTotal();
        }

        private void AtualizarDataNaTela()
        {
            if (LblDataSelecionada != null)
                LblDataSelecionada.Text = dataSelecionada.ToString("dd/MM/yyyy");
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
                horarioSelecionado = TimePickerHorario.Time;
        }

        // ========== CONTADORES ==========
        private void BtnAdultoMenos_Clicked(object? sender, EventArgs e)
        {
            if (adultos > 1) adultos--;
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

        // ========== CÁLCULO TOTAL ==========
        private void AtualizarTotal()
        {
            double total = 0;

            if (CheckBasico.IsChecked)
                total += adultos * precoBasico;

            if (CheckCompleto.IsChecked)
                total += adultos * precoCompleto;

            if (CheckTrezinho.IsChecked)
                total += adultos * precoTrezinho;

            total += criancas6a12 * precoCrianca6a12;

            LblTotalEstimado.Text = $"Total estimado: R${total:F2}";
        }

        // ========== BOTÃO CONTINUAR ==========
        private async void BtnContinuar_Clicked(object? sender, EventArgs e)
        {
            if (horarioSelecionado == null)
            {
                await DisplayAlert("Aviso", "Por favor, selecione um horário.", "OK");
                return;
            }

            if (!CheckBasico.IsChecked && !CheckCompleto.IsChecked && !CheckTrezinho.IsChecked)
            {
                await DisplayAlert("Aviso", "Selecione pelo menos uma atividade.", "OK");
                return;
            }

            AtualizarTotal();

            // Monta a lista de atividades escolhidas
            List<string> atividadesSelecionadas = new List<string>();
            if (CheckBasico.IsChecked) atividadesSelecionadas.Add("Café da manhã Básico");
            if (CheckCompleto.IsChecked) atividadesSelecionadas.Add("Café da manhã Completo");
            if (CheckTrezinho.IsChecked) atividadesSelecionadas.Add("Trezinho / Colha e Pague");

            string atividadesTexto = string.Join(", ", atividadesSelecionadas);

            string totalStr = LblTotalEstimado.Text.Replace("Total estimado: R$", "").Trim();

            Preferences.Set("AtividadesSelecionadas", atividadesTexto);
            Preferences.Set("QtdAdultos", adultos);
            Preferences.Set("QtdCriancas0a5", criancas0a5);
            Preferences.Set("QtdCriancas6a12", criancas6a12);
            Preferences.Set("TotalEstimado", totalStr);
            Preferences.Set("DataAgendamento", dataSelecionada.ToString("yyyy-MM-dd"));
            Preferences.Set("HorarioSelecionado", horarioSelecionado.Value.ToString(@"hh\:mm"));

            // Agora o resumo mostra as atividades selecionadas corretamente
            await DisplayAlert("Resumo da reserva",
                $"Atividades: {atividadesTexto}\n\n" +
                $"Data: {dataSelecionada:dd/MM/yyyy}\n" +
                $"Horário: {horarioSelecionado.Value:hh\\:mm}\n" +
                $"Adultos: {adultos}\n" +
                $"Crianças (0–5): {criancas0a5}\n" +
                $"Crianças (6–12): {criancas6a12}\n\n" +
                $"{LblTotalEstimado.Text}",
                "OK");

            await Navigation.PushAsync(new PagamentoPage());
        }

        // ========== SET DATA ==========
        public void SetData(DateTime data)
        {
            dataSelecionada = data;
            Preferences.Set("DataAgendamento", dataSelecionada.ToString("yyyy-MM-dd"));
            AtualizarDataNaTela();
        }
    }
}

