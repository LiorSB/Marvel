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
        private const string NEW_LINE = "\n******************************************************************************************\n";

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
            return selectedCommand switch
            {
                CommandsEnum.GetDirectoryFilesList => Task.Run(() => host.UpdateHostDetails(_protocolCommands[selectedProtocol].GetDirectory(host, fromDirectory) + NEW_LINE)),
                CommandsEnum.RunItem => Task.Run(() => host.UpdateHostDetails(_protocolCommands[selectedProtocol].RunItem(host, fromDirectory) + NEW_LINE)),
                CommandsEnum.ReceiveItem => Task.Run(() => host.UpdateHostDetails(_protocolCommands[selectedProtocol].ReceiveItem(host, fromDirectory, toDirectory) + NEW_LINE)),
                CommandsEnum.SendItem => Task.Run(() => host.UpdateHostDetails(_protocolCommands[selectedProtocol].SendItem(host, fromDirectory, toDirectory) + NEW_LINE)),
                CommandsEnum.GetFolder => Task.Run(() => host.UpdateHostDetails(_protocolCommands[selectedProtocol].GetFolder(host, fromDirectory, toDirectory) + NEW_LINE)),
                CommandsEnum.GetSystemInformation => Task.Run(() => host.SystemInformation += _protocolCommands[selectedProtocol].GetSystemInformation(host) + NEW_LINE),
                CommandsEnum.PingIP => Task.Run(() => host.PortsConnectivity += _protocolCommands[selectedProtocol].PingIP(host.IP) + NEW_LINE),
                CommandsEnum.PortConnectivity => Task.Run(() => host.PortsConnectivity += _protocolCommands[selectedProtocol].PortConnectivity(host.IP) + NEW_LINE),
                CommandsEnum.ExtractExecutables => Task.Run(() => host.UpdateHostDetails(_executableExtractor.ExtractFiles(host, toDirectory, selectedProtocol) + NEW_LINE)),
                _ => null,
            };
        }
    }
}
