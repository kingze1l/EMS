using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EMS.ViewModels
{
    public class BirthdayViewModel : INotifyPropertyChanged
    {
        private string _employeeName;
        private bool _hasBirthdayToday;

        public string EmployeeName
        {
            get => _employeeName;
            set
            {
                _employeeName = value;
                OnPropertyChanged();
            }
        }

        public bool HasBirthdayToday
        {
            get => _hasBirthdayToday;
            set
            {
                _hasBirthdayToday = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}