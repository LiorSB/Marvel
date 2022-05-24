using Marvel.Model;
using System;
using System.Diagnostics;

namespace Marvel.Commands
{
    class SMBCommands : IProtocolCommands
    {
        public Process InitializeProcess(Host host)
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = @$"/C net use \\{host.IP}\C$ /user:{host.Username} {host.Password}  && ",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            return process;
        }

        public string RunProcess(Process process)
        {
            string cmdOutPut = "";

            try
            {
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    cmdOutPut += process.StandardOutput.ReadLine() + "\n";
                }

                process.WaitForExit();
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return cmdOutPut;
        }

        public string GetDirectory(Host host, string fromDirectory)
        {
            if (fromDirectory.Length < 3)
            {
                return null;
            }

            char disk = fromDirectory[0];

            fromDirectory = fromDirectory.Remove(0, 3);

            Process process = InitializeProcess(host);
            process.StartInfo.Arguments += @$"dir ""\\{host.IP}\{disk}$\{fromDirectory}""";

            return RunProcess(process);
        }

        public string RunItem(Host host, string fromDirectory)
        {
            if (fromDirectory.Length < 3)
            {
                return null;
            }

            char disk = fromDirectory[0];

            fromDirectory = fromDirectory.Remove(0, 3);

            Process process = InitializeProcess(host);
            process.StartInfo.Arguments += @$"""\\{host.IP}\{disk}$\{fromDirectory}"" /s";

            return RunProcess(process);
        }

        public string ReceiveItem(Host host, string fromDirectory, string toDirectory)
        {
            if (fromDirectory.Length < 3)
            {
                return null;
            }

            char disk = fromDirectory[0];

            fromDirectory = fromDirectory.Remove(0, 3);

            Process process = InitializeProcess(host);
            process.StartInfo.Arguments += @$"copy ""\\{host.IP}\{disk}$\{fromDirectory}"" ""{toDirectory}""";

            return RunProcess(process);
        }

        public string SendItem(Host host, string fromDirectory, string toDirectory)
        {
            if (fromDirectory.Length < 3)
            {
                return null;
            }

            char disk = fromDirectory[0];

            fromDirectory = fromDirectory.Remove(0, 3);

            Process process = InitializeProcess(host);
            // Should fix so to and from will be placed opposite
            process.StartInfo.Arguments += @$"copy ""{toDirectory}"" ""\\{host.IP}\{disk}$\{fromDirectory}""";

            return RunProcess(process);
        }

        public string GetFolder(Host host, string fromDirectory, string toDirectory)
        {
            if (fromDirectory.Length < 3)
            {
                return null;
            }

            char disk = fromDirectory[0];

            fromDirectory = fromDirectory.Remove(0, 3);

            Process process = InitializeProcess(host);
            // Can also use xcopy instead of robocopy.
            process.StartInfo.Arguments += @$"robocopy ""\\{host.IP}\{disk}$\{fromDirectory}"" ""{toDirectory}""";

            return RunProcess(process);
        }

        public string GetSystemInformation(string ip)
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