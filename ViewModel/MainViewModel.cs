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
using Marvel.Model;
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
        private string _hostsFileDirectory = "";

        private Regex _ipRegex = new(@"\b(\d{1,3}\.){3}\d{1,3}\b");
        private Regex _typeRegex = new(@"Type|static|dynamic");

        private Dictionary<ProtocolsEnum, List<CommandsEnum>> _commandsByProtocol;
        private IEnumerable<ProtocolsEnum> _protocols = null;
        private ProtocolsEnum _selectedProtocol;
        private IEnumerable<CommandsEnum> _commands = null;
        private CommandsEnum _selectedCommand;
        private ObservableCollection<Host> _hostList = new();
        private Host _selectedHost;

        public List<CommandsEnum> FromDirectoryEnabled => new()
        {
            CommandsEnum.GetDirectoryFilesList,
            CommandsEnum.RunItem,
            CommandsEnum.ReceiveItem,
            CommandsEnum.SendItem,
            CommandsEnum.GetFolder
        };

        public List<CommandsEnum> ToDirectoryEnabled => new()
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

            _commandsByProtocol.Add(ProtocolsEnum.SSH, new List<CommandsEnum>
            {
                CommandsEnum.GetDirectoryFilesList,
                CommandsEnum.RunItem,
                CommandsEnum.ReceiveItem,
                CommandsEnum.SendItem,
                CommandsEnum.GetFolder,
                CommandsEnum.ExtractExecutables
            });

            _commandsByProtocol.Add(ProtocolsEnum.WinRM, new List<CommandsEnum>
            {
                CommandsEnum.GetDirectoryFilesList,
                CommandsEnum.RunItem,
                CommandsEnum.ReceiveItem,
                CommandsEnum.SendItem,
                CommandsEnum.GetFolder,
                CommandsEnum.ExtractExecutables
            });

            _commandsByProtocol.Add(ProtocolsEnum.SMB, new List<CommandsEnum>
            {
                CommandsEnum.GetDirectoryFilesList,
                CommandsEnum.RunItem,
                CommandsEnum.ReceiveItem,
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

        public string HostsFileDirectory
        {
            get
            {
                return _hostsFileDirectory;
            }
            set
            {
                _hostsFileDirectory = value;
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
            if (string.IsNullOrEmpty(_ip))
            {
                return;
            }
            
            if (!_ipRegex.IsMatch(_ip))
            {
                return;
            }
            
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

            MatchCollection resultIP = _ipRegex.Matches(cmdOutput);
            MatchCollection resultType = _typeRegex.Matches(cmdOutput);
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
            int index = _hostsFileDirectory.LastIndexOf('.');

            if (index == -1 || _hostsFileDirectory[index..] != ".txt")
            {
                return;
            }

            string text = System.IO.File.ReadAllText(_hostsFileDirectory);

            /*Regex ipRegex = new Regex(@"\b(\d{1,3}\.){3}\d{1,3}\b");
            MatchCollection resultIP = ipRegex.Matches(text);

            foreach (var ip in resultIP)
            {
                Host newHost = new(ip.ToString());
                HostList.Add(newHost);
            }*/

            string[] splittedText = text.Split();

            for (int i = 0; i < splittedText.Length; i += 3)
            {
                Host newHost = new(splittedText[i], splittedText[i + 1], splittedText[i + 2]);
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
            if ((_selectedCommand == CommandsEnum.GetDirectoryFilesList || _selectedCommand == CommandsEnum.RunItem) 
                && string.IsNullOrEmpty(_fromDirectory))
            {
                return;
            }

            if ((_selectedCommand == CommandsEnum.ReceiveItem || _selectedCommand == CommandsEnum.SendItem || _selectedCommand == CommandsEnum.GetFolder)
                && (string.IsNullOrEmpty(_fromDirectory) || string.IsNullOrEmpty(_toDirectory)))
            {
                return;
            }

            if (_selectedCommand == CommandsEnum.ExtractExecutables && string.IsNullOrEmpty(_toDirectory))
            {
                return;
            }

            for (int i = 0; i < _hostList.Count; i++)
            {
                if (_hostList[i].IsChecked)
                {
                    int index = i;

                    CommandUtilities.Instance.RunCommand(_selectedProtocol, HostList[index], _fromDirectory, _toDirectory, _selectedCommand);
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
    }
}