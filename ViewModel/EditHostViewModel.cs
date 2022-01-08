using Marvel.Model;
using Prism.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Marvel.ViewModel
{
    public class EditHostViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _ipEdited = "";
        private string _usernameEdited = "";
        private string _passwordEdited = "";
        private Host _host;

        public EditHostViewModel(Host host)
        {
            Host = host;

            IPEdited = host.IP;
            UsernameEdited = host.Username;
            PasswordEdited = host.Password;

            SaveEditCommand = new DelegateCommand(SaveEdit, CanSaveEdit);
        }

        public string IPEdited
        {
            get
            {
                return _ipEdited;
            }
            set
            {
                _ipEdited = value;
                NotifyPropertyChanged();
            }
        }

        public string UsernameEdited
        {
            get
            {
                return _usernameEdited;
            }
            set
            {
                _usernameEdited = value;
                NotifyPropertyChanged();
            }
        }

        public string PasswordEdited
        {
            get
            {
                return _passwordEdited;
            }
            set
            {
                _passwordEdited = value;
                NotifyPropertyChanged();
            }
        }

        public Host Host
        {
            get
            {
                return _host;
            }
            set
            {
                _host = value;
                NotifyPropertyChanged();
            }
        }

        public DelegateCommand SaveEditCommand { get; private set; }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanSaveEdit()
        {
            return true;
        }

        private void SaveEdit()
        {
            Host.IP = _ipEdited;
            Host.Username = _usernameEdited;
            Host.Password = _passwordEdited;
        }
    }
}
