using Marvel.Model;
using Renci.SshNet;
using System;
using System.IO;

namespace Marvel.Commands
{
    class SSHCommands : IProtocolCommands
    {
        public string GetDirectory(Host host, string fromDirectory)
        {
            SshClient sshClient = new(host.IP, host.Username, host.Password);
            SshCommand sshCommand;

            try
            {
                sshClient.Connect();
                sshCommand = sshClient.RunCommand("dir " + fromDirectory);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                sshClient.Disconnect();
            }

            return sshCommand.Result;
        }

        public string RunItem(Host host, string fromDirectory)
        {
            SshClient sshClient = new(host.IP, host.Username, host.Password);
            SshCommand sshCommand;

            try
            {
                sshClient.Connect();
                sshCommand = sshClient.RunCommand(fromDirectory + @" /s");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                sshClient.Disconnect();
            }
            

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
            catch (Exception e)
            {
                return e.Message;
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

            try
            {
                scpClient.Connect();
                scpClient.Upload(File.OpenWrite(fromDirectory), toDirectory);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                scpClient.Disconnect();
            }

            return "Downloaded to " + toDirectory;
        }

        public string GetFolder(Host host, string fromDirectory, string toDirectory)
        {
            DirectoryInfo hostDirectoryInfo = new DirectoryInfo(toDirectory);
            ScpClient scpClient = new(host.IP, host.Username, host.Password);

            try
            {
                scpClient.Connect();
                scpClient.Download(fromDirectory, hostDirectoryInfo);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                scpClient.Disconnect();
            }

            return "Downloaded to " + toDirectory;
        }

        public string GetSystemInformation(Host host)
        {
            return null;
        }

        public string PingIP(string ip)
        {
            return null;
        }

        public string PortConnectivity(string ip)
        {
            return null;
        }
    }
}
