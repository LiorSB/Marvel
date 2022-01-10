using Marvel.Enum;
using Marvel.Model;
using Renci.SshNet;
using System.IO;

namespace Marvel.ProtocolCommands
{
    class SSHCommands
    {
        public string Commands(Host host, string fromDirectory, string toDirectory, CommandsEnum selectedCommand)
        {
            if (selectedCommand is CommandsEnum.ReceiveItem or CommandsEnum.SendItem)
            {
                int index = fromDirectory.LastIndexOf('\\');

                if (index == -1)
                {
                    return null;
                }

                toDirectory += fromDirectory[index..];

                ScpClient scpClient = new(host.IP, host.Username, host.Password);
                scpClient.Connect();

                if (selectedCommand == CommandsEnum.ReceiveItem)
                {
                    scpClient.Download(fromDirectory, File.OpenWrite(toDirectory));
                }
                else
                {
                    scpClient.Upload(File.OpenWrite(fromDirectory), toDirectory);
                }

                return "Downloaded to " + toDirectory;
            }

            SshClient client = new(host.IP, host.Username, host.Password);
            client.Connect();

            SshCommand sshCommand = selectedCommand == CommandsEnum.GetDirectoryFilesList
                ? client.RunCommand("dir " + fromDirectory) // CommandsEnum.GetDirectoryFilesList
                : client.RunCommand(fromDirectory + @" /s"); // CommandsEnum.RunItem

            return sshCommand.Result;
        }
    }
}
