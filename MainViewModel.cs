using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Marvel.Enum;
using Marvel.ProtocolCommands;

namespace Marvel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _ip = "";
        private string _username = "";
        private string _password = "";
        private string _segment = "";
        private string _fromDirectory = "";
        private string _toDirectory = "";
        private IEnumerable<ProtocolsEnum> _protocols = null;
        private ProtocolsEnum _selectedProtocol;
        private IEnumerable<CommandsEnum> _commands = null;
        private CommandsEnum _selectedCommand;
        private ObservableCollection<Host> _hostList = new();
        private WinRMCommands _winRMCommands = new();
        
        public IEnumerable<ProtocolsEnum> Protocols
        {
            get
            {
                if (_protocols == null)
                {
                    _protocols = System.Enum.GetValues(typeof(ProtocolsEnum)).Cast<ProtocolsEnum>();
                }

                return _protocols;
            }
            set
            {
                _protocols = value;
                NotifyPropertyChanged();
            }
        }

        public ProtocolsEnum SelectedProtocol
        {
            get
            {
                return _selectedProtocol;
            }
            set
            {
                _selectedProtocol = value;
                NotifyPropertyChanged();
            }
        }

        public IEnumerable<CommandsEnum> Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = System.Enum.GetValues(typeof(CommandsEnum)).Cast<CommandsEnum>();
                }

                return _commands;
            }
            set
            {
                _commands = value;
                NotifyPropertyChanged();
            }
        }

        public CommandsEnum SelectedCommand
        {
            get
            {
                return _selectedCommand;
            }
            set
            {
                _selectedCommand = value;
                NotifyPropertyChanged();
            }
        }

        public MainViewModel()
        {
            AddHostCommand = new DelegateCommand(AddHost, CanAddHost);
            ScanSegmentCommand = new DelegateCommand(ScanSegment, CanScanSegment);
            ScanForLocalIPsCommand = new DelegateCommand(ScanForLocalIPs, CanScanForLocalIPs);
            AddHostsFromFileCommand = new DelegateCommand(AddHostsFromFile, CanAddHostsFromFile);
            CancelCommand = new DelegateCommand(Cancel, CanCancel);
            RunCommandCommand = new DelegateCommand(RunCommand, CanRunCommand);
            RemoveHostCommand = new DelegateCommand<Host>(RemoveHost, CanRemoveHost);
        }

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

        public string Segment
        {
            get
            {
                return _segment;
            }
            set
            {
                _segment = value;
                NotifyPropertyChanged();
            }
        }

        public string FromDirectory
        {
            get
            {
                return _fromDirectory;
            }
            set
            {
                _fromDirectory = value;
                NotifyPropertyChanged();
            }
        }

        public string ToDirectory
        {
            get
            {
                return _toDirectory;
            }
            set
            {
                _toDirectory = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<Host> HostList
        {
            get
            {
                return _hostList;
            }
            set
            {
                _hostList = value;
                NotifyPropertyChanged();
            }
        }

        public DelegateCommand AddHostCommand { get; private set; }
        public DelegateCommand ScanSegmentCommand { get; private set; }
        public DelegateCommand ScanForLocalIPsCommand { get; private set; }
        public DelegateCommand AddHostsFromFileCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand RunCommandCommand { get; private set; }
        public DelegateCommand<Host> RemoveHostCommand { get; private set; }
        //public DelegateCommand ScanCommand { get; private set; }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanAddHost()
        {
            return true;
        }

        private void AddHost()
        {
            Host newHost = new(_ip, _username, _password);
            HostList.Add(newHost);

            IP = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
        }

        private bool CanScanSegment()
        {
            return true;
        }

        private void ScanSegment()
        {
            
        }

        private bool CanScanForLocalIPs()
        {
            return true;
        }

        // With the command arp -a in CMD, we extract with the use of regex dynamic IPs.
        private void ScanForLocalIPs()
        {
            string cmdOutput = "";
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C arp -a",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                cmdOutput += process.StandardOutput.ReadLine();
            }

            process.WaitForExit();

            Regex ipRegex = new Regex(@"\b(\d{1,3}\.){3}\d{1,3}\b");
            Regex typeRegex = new Regex(@"Type|static|dynamic");
            MatchCollection resultIP = ipRegex.Matches(cmdOutput);
            MatchCollection resultType = typeRegex.Matches(cmdOutput);
            List<string> ipList = new();

            for (int i = 0; i < resultType.Count; i++)
            {
                if (resultType[i].ToString() == "dynamic")
                {
                    ipList.Add(resultIP[i].ToString());
                }
            }


            foreach (string ip in ipList)
            {
                Host newHost = new(ip);
                _hostList.Add(newHost);
            }
        }

        private bool CanAddHostsFromFile()
        {
            return true;
        }

        private void AddHostsFromFile()
        {
            string text = System.IO.File.ReadAllText(_fromDirectory);

            Regex ipRegex = new Regex(@"\b(\d{1,3}\.){3}\d{1,3}\b");
            MatchCollection resultIP = ipRegex.Matches(text);

            foreach (var ip in resultIP)
            {
                Host newHost = new(ip.ToString());
                _hostList.Add(newHost);
            }
        }

        private bool CanCancel()
        {
            return true;
        }

        private void Cancel()
        {

        }

        private bool CanRunCommand()
        {
            return true;
        }

        private void RunCommand()
        {
            string result = "";
             
            switch (_selectedProtocol)
            {
                case ProtocolsEnum.WinRM:
                    result = _winRMCommands.Commands(_hostList[0], _fromDirectory, _toDirectory, _selectedCommand);
                    break;
                case ProtocolsEnum.SMB:
                    break;
                case ProtocolsEnum.SSH:
                    break;
                default:
                    break;   
            }

            if (result == null)
            {
                return;
            }

            _hostList[0].Details += result + "\n";
        }

        private bool CanRemoveHost(Host host)
        {
            return true;
        }

        private void RemoveHost(Host host)
        {
            _hostList.Remove(host);
        }
        //private bool CanScan()
        //{
        //    return true;
        //}

        //private void Scan()
        //{
        //    try
        //    {
        //        TcpClient client = new(_ip, 135);
        //        _scanDetails = string.Format("Succesfuly pinged host:'" + _ip + ":" + 135 + "'");
        //    }
        //    catch (SocketException)
        //    {
        //        _scanDetails = string.Format("Error pinging host:'" + _ip + ":" + /*portNumber.ToString() +*/ "'");
        //    }
        //}
    }
}
