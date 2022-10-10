using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Marvel.Model
{
    public class Host : INotifyPropertyChanged
    {
        private string _ip;
        private string _username;
        private string _password;
        private bool _isChecked;
        private string _details;
        private string _portsConnectivity;
        private string _systemInformation;

        public event PropertyChangedEventHandler PropertyChanged;

        public string IP
        {
            get
            {
                return _ip;
            }
            set
            {
                _ip = value;
                NotifyPropertyChanged();
            }
        }
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                NotifyPropertyChanged();
            }
        }
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                NotifyPropertyChanged();
            }
        }
        public string Details
        {
            get
            {
                return _details;
            }
            set
            {
                _details = value;
                NotifyPropertyChanged();
            }
        }
        public string PortsConnectivity
        {
            get
            {
                return _portsConnectivity;
            }
            set
            {
                _portsConnectivity = value;
                NotifyPropertyChanged();
            }
        }
        public string SystemInformation
        {
            get
            {
                return _systemInformation;
            }
            set
            {
                _systemInformation = value;
                NotifyPropertyChanged();
            }
        }

        public Host(Host newHost)
        {
            IP = newHost.IP;
            Username = newHost.Username;
            Password = newHost.Password;
            IsChecked = true;
            Details = "";
            PortsConnectivity = "";
            SystemInformation = "";
        }

        public Host(string IP, string Username, string Password)
        {
            this.IP = IP;
            this.Username = Username;
            this.Password = Password;
            IsChecked = true;
            Details = "";
            PortsConnectivity = "";
            SystemInformation = "";
        }

        public Host(string IP)
        {
            this.IP = IP;
            Details = "";
            PortsConnectivity = "";
            SystemInformation = "";
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
