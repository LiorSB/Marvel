﻿using Marvel.Commands;
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
            return selectedCommand switch
            {
                CommandsEnum.GetDirectoryFilesList => Task.Run(() => host.Details += _protocolCommands[selectedProtocol].GetDirectory(host, fromDirectory) + "\n"),
                CommandsEnum.RunItem => Task.Run(() => host.Details += _protocolCommands[selectedProtocol].RunItem(host, fromDirectory) + "\n"),
                CommandsEnum.ReceiveItem => Task.Run(() => host.Details += _protocolCommands[selectedProtocol].ReceiveItem(host, fromDirectory, toDirectory) + "\n"),
                CommandsEnum.SendItem => Task.Run(() => host.Details += _protocolCommands[selectedProtocol].SendItem(host, fromDirectory, toDirectory) + "\n"),
                CommandsEnum.GetFolder => Task.Run(() => host.Details += _protocolCommands[selectedProtocol].GetFolder(host, fromDirectory, toDirectory) + "\n"),
                CommandsEnum.GetSystemInformation => Task.Run(() => host.Details += _protocolCommands[selectedProtocol].GetSystemInformation(host.IP) + "\n"),
                CommandsEnum.PingIP => Task.Run(() => host.PortsConnectivity += _protocolCommands[selectedProtocol].PingIP(host.IP) + "\n"),
                CommandsEnum.PortConnectivity => Task.Run(() => host.PortsConnectivity += _protocolCommands[selectedProtocol].PortConnectivity(host.IP) + "\n"),
                CommandsEnum.ExtractExecutables => Task.Run(() => host.SystemInformation += _executableExtractor.ExtractFiles(host, toDirectory, selectedProtocol) + "\n"),
                _ => null,
            };
        }
    }
}
