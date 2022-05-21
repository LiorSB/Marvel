using Marvel.Commands;
using Marvel.Enum;
using Marvel.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marvel.Utilities
{
    public class CommandUtilities
    {
        private WinRMCommands _winRMCommands;
        private SMBCommands _smbCommands;
        private SSHCommands _sshCommands;
        private RPCCommands _rpcCommands;
        private Dictionary<ProtocolsEnum, IProtocolCommands> _protocolCommands;
        private ExecutableExtractor _executableExtractor;
        private static CommandUtilities _instance;
        private static object _synchronizeAccess = new();

        public static CommandUtilities Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Singleton isn't thread-safe, therefore we use a lock to synchorize.
                    lock (_synchronizeAccess)
                    {
                        if (_instance == null)
                        {
                            _instance = new();
                        }
                    }
                }

                return _instance;
            }
        }

        private CommandUtilities()
        {
            _winRMCommands = new();
            _smbCommands = new();
            _sshCommands = new();
            _rpcCommands = new();

            _protocolCommands = new();

            _protocolCommands.Add(ProtocolsEnum.WinRM, _winRMCommands);
            _protocolCommands.Add(ProtocolsEnum.SMB, _smbCommands);
            _protocolCommands.Add(ProtocolsEnum.SSH, _sshCommands);
            _protocolCommands.Add(ProtocolsEnum.RPC, _rpcCommands);

            _executableExtractor = new();
        }

        public Task RunCommand(ProtocolsEnum selectedProtocol, Host host, string fromDirectory, string toDirectory, CommandsEnum selectedCommand)
        {
            switch (selectedCommand)
            {
                case CommandsEnum.GetDirectoryFilesList:
                    return Task.Run(() => host.Details += _protocolCommands[selectedProtocol].GetDirectory(host, fromDirectory) + "\n");
                    break;
                case CommandsEnum.RunItem:
                    return Task.Run(() => host.Details += _protocolCommands[selectedProtocol].RunItem(host, fromDirectory) + "\n");
                    break;
                case CommandsEnum.ReceiveItem:
                    return Task.Run(() => host.Details += _protocolCommands[selectedProtocol].ReceiveItem(host, fromDirectory, toDirectory) + "\n");
                    break;
                case CommandsEnum.SendItem:
                    return Task.Run(() => host.Details += _protocolCommands[selectedProtocol].SendItem(host, fromDirectory, toDirectory) + "\n");
                    break;
                case CommandsEnum.GetFolder:
                    return Task.Run(() => host.Details += _protocolCommands[selectedProtocol].GetFolder(host, fromDirectory, toDirectory) + "\n");
                    break;
                case CommandsEnum.GetSystemInformation:
                    return Task.Run(() => host.Details += _protocolCommands[selectedProtocol].GetSystemInformation(host.IP) + "\n");
                    break;
                case CommandsEnum.PingIP:
                    return Task.Run(() => host.Details += _protocolCommands[selectedProtocol].PingIP(host.IP) + "\n");
                    break;
                case CommandsEnum.PortConnectivity:
                    return Task.Run(() => host.Details += _protocolCommands[selectedProtocol].PortConnectivity(host.IP) + "\n");
                    break;
                case CommandsEnum.ExtractExecutables:
                    return Task.Run(() => host.Details += _executableExtractor.ExtractFiles(host, fromDirectory, toDirectory, selectedProtocol) + "\n");
                    break;
                default:
                    return null;
                    break;
            }

            /*if (selectedCommand == CommandsEnum.ExtractExecutables)
            {
                return Task.Run(() => host.Details += _executableExtractor.ExtractFiles(host, fromDirectory, toDirectory, selectedProtocol) + "\n");
            }

            switch (selectedProtocol)
            {
                case ProtocolsEnum.WinRM:
                    return Task.Run(() => host.Details += _winRMCommands.Commands(host, fromDirectory, toDirectory, selectedCommand) + "\n");
                    break;
                case ProtocolsEnum.SMB:
                    return Task.Run(() => host.Details += _smbCommands.Commands(host, fromDirectory, toDirectory, selectedCommand) + "\n");
                    break;
                case ProtocolsEnum.SSH:
                    switch (selectedCommand)
                    {
                        case CommandsEnum.GetDirectoryFilesList:
                            return Task.Run(() => host.Details += _sshCommands.GetDirectory(host, fromDirectory) + "\n");
                            break;
                        case CommandsEnum.RunItem:
                            return Task.Run(() => host.Details += _sshCommands.RunItem(host, fromDirectory) + "\n");
                            break;
                        case CommandsEnum.ReceiveItem:
                            return Task.Run(() => host.Details += _sshCommands.ReceiveItem(host, fromDirectory, toDirectory) + "\n");
                            break;
                        case CommandsEnum.SendItem:
                            return Task.Run(() => host.Details += _sshCommands.SendItem(host, fromDirectory, toDirectory) + "\n");
                            break;
                        case CommandsEnum.GetFolder:
                            return Task.Run(() => host.Details += _sshCommands.GetFolder(host, fromDirectory, toDirectory) + "\n");
                            break;
                        default:
                            break;
                    }
                    //Task.Run(() => host.Details += _sshCommands.Commands(host, fromDirectory, toDirectory, selectedCommand) + "\n");
                    break;
                case ProtocolsEnum.RPC:
                    switch (selectedCommand)
                    {
                        case CommandsEnum.GetSystemInformation:
                            return Task.Run(() => host.SystemInformation += _rpcCommands.GetSystemInformation(host.IP) + "\n");
                            break;
                        case CommandsEnum.PingIP:
                            return Task.Run(() => host.SystemInformation += _rpcCommands.PingIP(host.IP) + "\n");
                            break;
                        case CommandsEnum.PortConnectivity:
                            return Task.Run(() => host.PortsConnectivity += _rpcCommands.PortConnectivity(host.IP) + "\n");
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return null;*/
        }
    }
}
