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
using Marvel.Commands;
using System.Threading.Tasks;
using Marvel.Model;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Marvel.Utilities;

namespace Marvel.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler OnHostAdded;

        private string _ip = "";
        private string _username = "";
        private string _password = "";
        private string _segment = "";
        private string _fromDirectory = "";
        private string _toDirectory = "";

        private Dictionary<ProtocolsEnum, List<CommandsEnum>> _commandsByProtocol;

        private IEnumerable<ProtocolsEnum> _protocols = null;
        private ProtocolsEnum _selectedProtocol;

        private IEnumerable<CommandsEnum> _commands = null;
        private CommandsEnum _selectedCommand;

        private ObservableCollection<Host> _hostList = new();

        /*private WinRMCommands _winRMCommands = new();
        private SMBCommands _smbCommands = new();
        private SSHCommands _sshCommands = new();
        private NetworkCommands _networkCommands = new();*/

        private Host _selectedHost;

        public List<CommandsEnum> DirectoryEnabled => new()
        {
            CommandsEnum.ReceiveItem,
            CommandsEnum.SendItem,
            CommandsEnum.GetFolder,
            CommandsEnum.ExtractExecutables
        };

        public Dictionary<ProtocolsEnum, List<CommandsEnum>> CommandsByProtocol 
        { 
            get
            {
                return _commandsByProtocol;
            }
        }

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

        public Host SelectedHost
        {
            get
            {
                return _selectedHost;
            }
            set
            {
                _selectedHost = value;
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

            InitializeCommandsByProtocol();
        }

        private void InitializeCommandsByProtocol()
        {
            _commandsByProtocol = new();

            _commandsByProtocol.Add(ProtocolsEnum.WinRM, new List<CommandsEnum>
            {
                CommandsEnum.GetDirectoryFilesList,
                CommandsEnum.ReceiveItem,
                CommandsEnum.RunItem,
                CommandsEnum.SendItem,
                CommandsEnum.ExtractExecutables
            });

            _commandsByProtocol.Add(ProtocolsEnum.SMB, new List<CommandsEnum>
            {
                CommandsEnum.GetDirectoryFilesList,
                CommandsEnum.ReceiveItem,
                CommandsEnum.RunItem,
                CommandsEnum.SendItem,
                CommandsEnum.ExtractExecutables
            });

            _commandsByProtocol.Add(ProtocolsEnum.SSH, new List<CommandsEnum>
            {
                CommandsEnum.GetDirectoryFilesList,
                CommandsEnum.ReceiveItem,
                CommandsEnum.RunItem,
                CommandsEnum.SendItem,
                CommandsEnum.GetFolder,
                CommandsEnum.ExtractExecutables
            });

            _commandsByProtocol.Add(ProtocolsEnum.RPC, new List<CommandsEnum>
            {
                CommandsEnum.GetSystemInformation,
                CommandsEnum.PingIP,
                CommandsEnum.PortConnectivity
            });

            NotifyPropertyChanged("CommandsByProtocol");
        }

        public string IP
        {
            get
            {
                return _ip;
            }
            set
            {
                _ip = value.Trim();
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
                _username = value.Trim();
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
                _password = value.Trim();
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
                _segment = value.Trim();
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
            OnHostAdded?.Invoke(this, new EventArgs());
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

            string cmdOutput = "";

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
                HostList.Add(newHost);
            }
        }

        private bool CanAddHostsFromFile()
        {
            return true;
        }

        private void AddHostsFromFile()
        {
            int index = _fromDirectory.LastIndexOf('.');

            if (index == -1 || _fromDirectory[index..] != ".txt")
            {
                return;
            }

            string text = System.IO.File.ReadAllText(_fromDirectory);

            Regex ipRegex = new Regex(@"\b(\d{1,3}\.){3}\d{1,3}\b");
            MatchCollection resultIP = ipRegex.Matches(text);

            foreach (var ip in resultIP)
            {
                Host newHost = new(ip.ToString());
                HostList.Add(newHost);
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
            for (int i = 0; i < _hostList.Count; i++)
            {
                if (_hostList[i].IsChecked)
                {
                    int index = i;


                    CommandUtilities.Instance.RunCommand(_selectedProtocol, HostList[index], _fromDirectory, _toDirectory, _selectedCommand);

                    /*switch (_selectedProtocol)
                    {
                        case ProtocolsEnum.WinRM:
                            Task.Run(() => HostList[index].Details += _winRMCommands.Commands(_hostList[index], _fromDirectory, _toDirectory, _selectedCommand) + "\n");
                            break;
                        case ProtocolsEnum.SMB:
                            Task.Run(() => HostList[index].Details += _smbCommands.Commands(_hostList[index], _fromDirectory, _toDirectory, _selectedCommand) + "\n");
                            break;
                        case ProtocolsEnum.SSH:
                            Task.Run(() => HostList[index].Details += _sshCommands.Commands(_hostList[index], _fromDirectory, _toDirectory, _selectedCommand) + "\n");
                            break;
                        default:
                            break;
                    }*/
                }
            }
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
