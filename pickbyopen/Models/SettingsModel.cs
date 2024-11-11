using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pickbyopen.Models
{
    public partial class SettingsModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _value = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
