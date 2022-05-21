using Marvel.Enum;
using Marvel.Model;
using Renci.SshNet;
using System.IO;

namespace Marvel.Commands
{
    class SSHCommands : IProtocolCommands
    {
        public string GetDirectory(Host host, string fromDirectory)
        {
            SshClient sshClient = new(host.IP, host.Username, host.Password);
            sshClient.Connect();

            SshCommand sshCommand = sshClient.RunCommand("dir " + fromDirectory);

            return sshCommand.Result;
        }

        public string RunItem(Host host, string fromDirectory)
        {
            SshClient sshClient = new(host.IP, host.Username, host.Password);
            sshClient.Connect();

            SshCommand sshCommand = sshClient.RunCommand(fromDirectory + @" /s");

            return sshCommand.Result;
        }

        public string ReceiveItem(Host host, string fromDirectory, string toDirectory)
        {
            int index = fromDirectory.LastIndexOf('\\');

            if (index == -1)
            {
                return null;
            }

            toDirectory += fromDirectory[index..];

            ScpClient scpClient = new(host.IP, host.Username, host.Password);

            try
            {
                scpClient.Connect();

                scpClient.Download(fromDirectory, File.OpenWrite(toDirectory));
            }
            catch
            {

            }
            finally
            {
                scpClient.Disconnect();
            }

            return "Downloaded to " + toDirectory;
        }

        public string SendItem(Host host, string fromDirectory, string toDirectory)
        {
            int index = fromDirectory.LastIndexOf('\\');

            if (index == -1)
            {
                return null;
            }

            toDirectory += fromDirectory[index..];

            ScpClient scpClient = new(host.IP, host.Username, host.Password);
            scpClient.Connect();

            scpClient.Upload(File.OpenWrite(fromDirectory), toDirectory);

            return "Downloaded to " + toDirectory;
        }

        public string GetFolder(Host host, string fromDirectory, string toDirectory)
        {
            ScpClient scpClient = new(host.IP, host.Username, host.Password);
            scpClient.Connect();

            DirectoryInfo hostDirectoryInfo = new DirectoryInfo(toDirectory);

            scpClient.Download(fromDirectory, hostDirectoryInfo);

            return "Downloaded to " + toDirectory;
        }

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

            SshClient sshClient = new(host.IP, host.Username, host.Password);
            sshClient.Connect();

            SshCommand sshCommand = selectedCommand == CommandsEnum.GetDirectoryFilesList
                ? sshClient.RunCommand("dir " + fromDirectory) // CommandsEnum.GetDirectoryFilesList
                : sshClient.RunCommand(fromDirectory + @" /s"); // CommandsEnum.RunItem

            return sshCommand.Result;
        }

        public string GetSystemInformation(string ip)
        {
            throw new System.NotImplementedException();
        }

        public string PingIP(string ip)
        {
            throw new System.NotImplementedException();
        }

        public string PortConnectivity(string ip)
        {
            throw new System.NotImplementedException();
        }
    }
}
