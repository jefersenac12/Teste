using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Teste.Models
{
    public class CalendarDay : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int Day { get; set; }
        public bool IsFromCurrentMonth { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}