
using Teste.Models;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Teste.ViewModels
{
    public class AgendamentoViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CalendarDay> Dias { get; set; }
        public ObservableCollection<string> Frutas { get; set; }

        public DateTime DataAtual { get; set; }

        public AgendamentoViewModel()
        {
            Dias = new ObservableCollection<CalendarDay>();
            Frutas = new ObservableCollection<string>();
            DataAtual = DateTime.Now;
            AtualizarCalendario();
            AtualizarFrutas();
        }

        public void AtualizarCalendario()
        {
            Dias.Clear();

            var primeiroDiaDoMes = new DateTime(DataAtual.Year, DataAtual.Month, 1);
            int offset = (int)primeiroDiaDoMes.DayOfWeek;

            // Adiciona dias do mês anterior para preencher o grid
            for (int i = 0; i < offset; i++)
            {
                Dias.Add(new CalendarDay { Day = 0, IsFromCurrentMonth = false });
            }

            int diasNoMes = DateTime.DaysInMonth(DataAtual.Year, DataAtual.Month);
            for (int i = 1; i <= diasNoMes; i++)
            {
                Dias.Add(new CalendarDay
                {
                    Day = i,
                    IsFromCurrentMonth = true,
                    IsSelected = (DataAtual.Day == i)
                });
            }

            OnPropertyChanged(nameof(Dias));
        }

        public void AtualizarFrutas()
        {
            Frutas.Clear();
            if (DataAtual.Month == 10)
            {
                Frutas.Add("🍓 Morango");
                Frutas.Add("🍊 Laranja");
                Frutas.Add("🍇 Uva");
            }
            else if (DataAtual.Month == 11)
            {
                Frutas.Add("🥭 Manga");
                Frutas.Add("🍈 Melão");
            }

            // Aqui você pode ajustar a visibilidade na página (não necessário aqui)
        }

        public void SelecionarDia(CalendarDay dia)
        {
            if (dia == null || !dia.IsFromCurrentMonth) return;

            foreach (var d in Dias)
            {
                d.IsSelected = false;
            }
            dia.IsSelected = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
