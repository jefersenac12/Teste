using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace Teste
{
    // Modelo do dia
    public class DiaCalendario : INotifyPropertyChanged
    {
        public int Numero { get; set; }
        public bool IsFromCurrentMonth { get; set; }
        public DateTime Data { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class AgendamentoPage : ContentPage
    {
        private DateTime _dataAtual;
        public ObservableCollection<DiaCalendario> Dias { get; set; }
        public ObservableCollection<string> Frutas { get; set; }

        public AgendamentoPage()
        {
            InitializeComponent();

            Dias = new ObservableCollection<DiaCalendario>();
            Frutas = new ObservableCollection<string>();

            _dataAtual = DateTime.Now; // Mês atual
            DiasCollectionView.ItemsSource = Dias;
            BindableLayout.SetItemsSource(FrutasList, Frutas);

            AtualizarCalendario();
            AtualizarFrutas();
        }

        private void AtualizarCalendario()
        {
            Dias.Clear();
            MesLabel.Text = _dataAtual.ToString("MMMM yyyy", new CultureInfo("pt-BR"));

            var primeiroDia = new DateTime(_dataAtual.Year, _dataAtual.Month, 1);
            var ultimoDia = primeiroDia.AddMonths(1).AddDays(-1);
            int primeiroDiaSemana = (int)primeiroDia.DayOfWeek;

            var mesAnterior = _dataAtual.AddMonths(-1);
            int diasMesAnterior = DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);

            // Dias do mês anterior para preencher o início do calendário
            for (int i = primeiroDiaSemana - 1; i >= 0; i--)
            {
                int dia = diasMesAnterior - i;
                Dias.Add(new DiaCalendario
                {
                    Numero = dia,
                    IsFromCurrentMonth = false,
                    Data = new DateTime(mesAnterior.Year, mesAnterior.Month, dia)
                });
            }

            // Dias do mês atual
            for (int i = 1; i <= ultimoDia.Day; i++)
            {
                Dias.Add(new DiaCalendario
                {
                    Numero = i,
                    IsFromCurrentMonth = true,
                    Data = new DateTime(_dataAtual.Year, _dataAtual.Month, i),
                    IsSelected = (i == DateTime.Now.Day && _dataAtual.Month == DateTime.Now.Month)
                });
            }

            // Dias do próximo mês para completar o grid
            var proximoMes = _dataAtual.AddMonths(1);
            int totalNecessario = 42; // 6 semanas visuais (6 linhas x 7 colunas)
            int totalDias = Dias.Count;

            for (int i = 1; totalDias + i <= totalNecessario; i++)
            {
                Dias.Add(new DiaCalendario
                {
                    Numero = i,
                    IsFromCurrentMonth = false,
                    Data = new DateTime(proximoMes.Year, proximoMes.Month, i)
                });
            }
        }

        private void AtualizarFrutas()
        {
            Frutas.Clear();

            // Exemplo: frutas sazonais por mês
            switch (_dataAtual.Month)
            {
                case 10:
                    Frutas.Add("🍓 Morango");
                    Frutas.Add("🍊 Laranja");
                    Frutas.Add("🍇 Uva");
                    break;
                case 11:
                    Frutas.Add("🥭 Manga");
                    Frutas.Add("🍈 Melão");
                    break;
                case 12:
                    Frutas.Add("🍉 Melancia");
                    Frutas.Add("🍍 Abacaxi");
                    break;
                default:
                    Frutas.Add("🍌 Banana");
                    Frutas.Add("🍎 Maçã");
                    break;
            }

            bool temFrutas = Frutas.Any();
            FrutasList.IsVisible = temFrutas;
            NoFruitsLabel.IsVisible = !temFrutas;
        }

        private void OnMesAnteriorClicked(object sender, EventArgs e)
        {
            _dataAtual = _dataAtual.AddMonths(-1);
            AtualizarCalendario();
            AtualizarFrutas();
        }

        private void OnProximoMesClicked(object sender, EventArgs e)
        {
            _dataAtual = _dataAtual.AddMonths(1);
            AtualizarCalendario();
            AtualizarFrutas();
        }

        private void OnDiaSelecionado(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not DiaCalendario diaSelecionado)
                return;

            foreach (var dia in Dias)
                dia.IsSelected = false;

            diaSelecionado.IsSelected = true;

            // Se o usuário clicar em um dia fora do mês atual, muda automaticamente o mês
            if (!diaSelecionado.IsFromCurrentMonth)
            {
                _dataAtual = diaSelecionado.Data;
                AtualizarCalendario();
                AtualizarFrutas();
            }
        }

        private async void OnContinuarClicked(object sender, EventArgs e)
        {
            var diaSelecionado = Dias.FirstOrDefault(d => d.IsSelected && d.IsFromCurrentMonth);

            if (diaSelecionado != null)
            {
                // Salva a data selecionada para as próximas telas
                Preferences.Set("DataAgendamento", diaSelecionado.Data.ToString("yyyy-MM-dd"));

                await DisplayAlert("Agendamento",
                    $"Data selecionada: {diaSelecionado.Data:dd/MM/yyyy}", "OK");

                // Chama a tela de atividades e passa a data
                var atividadesPage = new AtividadesPage();
                atividadesPage.SetData(diaSelecionado.Data);
                await Navigation.PushAsync(atividadesPage);
            }
            else
            {
                await DisplayAlert("Erro", "Por favor, selecione um dia válido.", "OK");
            }
        }
    }
}
