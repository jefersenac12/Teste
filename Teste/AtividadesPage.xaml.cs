using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;

namespace Teste
{
    public partial class AtividadesPage : ContentPage
    {
        // Variáveis de estado
        private int adultos = 1;
        private int criancas0a5 = 0;
        private int criancas6a12 = 0;

        // Preços Fixos
        private readonly double precoBasico = 25.00;
        private readonly double precoCompleto = 65.00;
        private readonly double precoTrezinho = 15.00;
        private readonly double precoCrianca6a12 = 12.50; // Meia entrada

        private DateTime dataSelecionada;
        private TimeSpan? horarioSelecionado = null;

        public AtividadesPage()
        {
            InitializeComponent();

            // Inicializa Labels de contagem na tela
            LblAdultos.Text = adultos.ToString();
            LblCrianca0a5.Text = criancas0a5.ToString();
            LblCrianca6a12.Text = criancas6a12.ToString(); // Garantindo que esta Label existe no XAML

            // Carrega a data salva (ou usa a data atual como fallback)
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

            // Sincroniza eventos de CheckBox
            CheckBasico.CheckedChanged += OnAtividadeChanged;
            CheckCompleto.CheckedChanged += OnAtividadeChanged;
            CheckTrezinho.CheckedChanged += OnAtividadeChanged;

            // Anexa o PropertyChanged do TimePicker (conforme sua solicitação)
            // Se o TimePickerHorario não estiver inicializado pelo XAML, isso pode causar um erro
            // No entanto, assumindo que InitializeComponent() funciona, anexamos aqui.
            TimePickerHorario.PropertyChanged += TimePickerHorario_PropertyChanged;

            AtualizarTotal();
        }

        // ========== SET DATA ==========
        public void SetData(DateTime data)
        {
            dataSelecionada = data;
            Preferences.Set("DataAgendamento", dataSelecionada.ToString("yyyy-MM-dd"));
            AtualizarDataNaTela();
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

        // Você solicitou NÃO mexer neste método, então ele é mantido.
        // Ele rastreia a hora selecionada.
        private void TimePickerHorario_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimePicker.Time))
                horarioSelecionado = TimePickerHorario.Time;
        }

        // ========== CONTADORES DE ADULTOS ==========
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

        // ========== CONTADORES DE CRIANÇAS (0 a 5) ==========
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

        // ========== CONTADORES DE CRIANÇAS (6 a 12) ==========
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

            // Custo de Atividades Adicionais (calculado por adulto)
            if (CheckBasico.IsChecked)
                total += adultos * precoBasico;

            if (CheckCompleto.IsChecked)
                total += adultos * precoCompleto;

            if (CheckTrezinho.IsChecked)
                total += adultos * precoTrezinho;

            // Custo das crianças (6 a 12 anos)
            // Se as atividades acima forem por pessoa, a lógica do total deve ser refeita
            // Mas, seguindo a estrutura atual (somente adultos pagam o preço total da atividade):
            total += criancas6a12 * precoCrianca6a12;

            LblTotalEstimado.Text = $"Total estimado: R${total:F2}";
        }

        // ========== BOTÃO CONTINUAR ==========
        private async void BtnContinuar_Clicked(object? sender, EventArgs e)
        {
            // Valida se o horário foi selecionado (o TimePicker inicializa como 00:00:00, 
            // mas o uso de `horarioSelecionado == null` depende se a propriedade foi disparada)
            // Uma verificação mais robusta é usar a propriedade Time do controle:
            TimeSpan horarioCapturado = TimePickerHorario.Time;

            // Verifica se o usuário interagiu com o TimePicker.
            // Para garantir que não é o valor padrão, poderíamos adicionar uma validação mais complexa,
            // mas por enquanto, usaremos a verificação baseada no valor da instância (se for 00:00:00, pedimos para selecionar).
            if (horarioCapturado == new TimeSpan(0, 0, 0) && horarioSelecionado == null)
            {
                await DisplayAlert("Aviso", "Por favor, selecione um horário.", "OK");
                return;
            }

            // Se o PropertyChanged não foi disparado, o valor mais recente é o TimePickerHorario.Time
            if (horarioSelecionado == null)
                horarioSelecionado = horarioCapturado;


            if (!CheckBasico.IsChecked && !CheckCompleto.IsChecked && !CheckTrezinho.IsChecked)
            {
                await DisplayAlert("Aviso", "Selecione pelo menos uma atividade.", "OK");
                return;
            }

            // Garante que o total está atualizado
            AtualizarTotal();

            // Monta a lista de atividades escolhidas
            List<string> atividadesSelecionadas = new List<string>();
            if (CheckBasico.IsChecked) atividadesSelecionadas.Add("Café da manhã Básico");
            if (CheckCompleto.IsChecked) atividadesSelecionadas.Add("Café da manhã Completo");
            if (CheckTrezinho.IsChecked) atividadesSelecionadas.Add("Trezinho / Colha e Pague");

            // Mapeia as atividades selecionadas para seus IDs
            // Assumimos os seguintes IDs no backend:
            // 1 = Café da manhã Básico
            // 2 = Café da manhã Completo
            // 3 = Trezinho / Colha e Pague
            List<int> atividadeIds = new List<int>();
            if (CheckBasico.IsChecked) atividadeIds.Add(1);
            if (CheckCompleto.IsChecked) atividadeIds.Add(2);
            if (CheckTrezinho.IsChecked) atividadeIds.Add(3);

            string atividadesTexto = string.Join(", ", atividadesSelecionadas);
            string totalStr = LblTotalEstimado.Text.Replace("Total estimado: R$", "").Trim();

            // Salva dados no Preferences
            Preferences.Set("AtividadesSelecionadas", atividadesTexto);
            Preferences.Set("AtividadeIds", string.Join(",", atividadeIds));
            Preferences.Set("QtdAdultos", adultos);
            Preferences.Set("QtdCriancas0a5", criancas0a5);
            Preferences.Set("QtdCriancas6a12", criancas6a12);
            Preferences.Set("TotalEstimado", totalStr);
            Preferences.Set("DataAgendamento", dataSelecionada.ToString("yyyy-MM-dd"));
            Preferences.Set("HorarioSelecionado", horarioSelecionado.Value.ToString(@"hh\:mm"));

            // Exibe o resumo
            await DisplayAlert("Resumo da reserva",
                $"Atividades: {atividadesTexto}\n\n" +
                $"Data: {dataSelecionada:dd/MM/yyyy}\n" +
                $"Horário: {horarioSelecionado.Value:hh\\:mm}\n" +
                $"Adultos: {adultos}\n" +
                $"Crianças (0–5): {criancas0a5}\n" +
                $"Crianças (6–12): {criancas6a12}\n\n" +
                $"{LblTotalEstimado.Text}",
                "OK");

            // Navega para a próxima página
            // NOTA: Certifique-se de que a classe PagamentoPage existe
            await Navigation.PushAsync(new PagamentoPage());
        }
    }
}
