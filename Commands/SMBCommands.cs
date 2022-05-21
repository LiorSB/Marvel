using Marvel.Enum;
using Marvel.Model;
using System.Diagnostics;

namespace Marvel.Commands
{
    class SMBCommands : IProtocolCommands
    {
        public string Commands(Host host, string fromDirectory, string toDirectory, CommandsEnum selectedCommand)
        {
            if (fromDirectory.Length < 3)
            {
                return null;
            }

            string cmdOutPut = "";
            string disk = fromDirectory[0].ToString();

            fromDirectory.Remove(0, 3);

            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = @"/C net use \\" + host.IP + @"\C$ /user:" + host.Username + " " + host.Password + @" && ",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            switch (selectedCommand)
            {
                case CommandsEnum.GetDirectoryFilesList:
                    process.StartInfo.Arguments += @"dir \\" + host.IP + @"\" + disk + @"$\" + fromDirectory;
                    break;
                case CommandsEnum.RunItem:
                    process.StartInfo.Arguments += @"\\" + host.IP + @"\" + disk + @"$\" + fromDirectory + @" /s";
                    break;
                case CommandsEnum.ReceiveItem:
                    process.StartInfo.Arguments += @"copy \\" + host.IP + @"\" + disk + @"$\" + fromDirectory + @" " + toDirectory;
                    break;
                case CommandsEnum.SendItem: // Should fix so to and from will be placed opposite
                    process.StartInfo.Arguments += @"copy " + toDirectory + @" \\" + host.IP + @"\" + disk + @"$\" + fromDirectory;
                    break;
                default:
                    break;
            }

            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                cmdOutPut += process.StandardOutput.ReadLine();
            }

            process.WaitForExit();

            return cmdOutPut;
        }

        public Process InitializeProcess(Host host)
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = @$"/C net use \\{host.IP}\C$ /user:{host.Username} {host.Password}  &&",
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

            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                cmdOutPut += process.StandardOutput.ReadLine() + "\n";
            }

            process.WaitForExit();

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
            process.StartInfo.Arguments += @$"dir \\{host.IP}\{disk}$\{fromDirectory}";

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
            process.StartInfo.Arguments += @$"\\{host.IP}\{disk}$\{fromDirectory} /s";

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
            process.StartInfo.Arguments += @$"copy \\{host.IP}\{disk}$\{fromDirectory} {toDirectory}";

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
            process.StartInfo.Arguments += @$"copy {toDirectory} \\{host.IP}\{disk}$\{fromDirectory}";

            return RunProcess(process);
        }

        public string GetFolder(Host host, string fromDirectory, string toDirectory)
        {
            throw new System.NotImplementedException();
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