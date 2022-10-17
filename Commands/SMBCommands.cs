using Marvel.Model;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Marvel.Commands
{
    class SMBCommands : IProtocolCommands
    {
        public char CutDiskFromDirectory(ref string directory)
        {
            if (directory.Length < 3)
            {
                return '\0';
            }

            char disk = directory[0];

            directory = directory.Remove(0, 3);

            return disk;
        }

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

        public string GetUserProfileDirectory(Host host, string directory)
        {
            if (directory != null && directory[0] != '%')
            {
                return directory;
            }

            Process process = InitializeProcess(host);
            process.StartInfo.Arguments += $"echo %USERPROFILE%";

            string output = RunProcess(process);

            string pattern = @"C:\\Users\\.*";

            try
            {
                foreach (Match match in Regex.Matches(output, pattern, RegexOptions.IgnoreCase))
                {
                    output = match.Value;
                }
            }
            catch (RegexMatchTimeoutException) 
            { 

            }

            directory = directory.Replace("%USERPROFILE%", "");

            directory = output + directory;

            return directory;
        }

        public string GetDirectory(Host host, string fromDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);
            char disk = CutDiskFromDirectory(ref fromDirectory);

            if (disk == '\0')
            {
                return null;
            }

            Process process = InitializeProcess(host);
            process.StartInfo.Arguments += @$"dir ""\\{host.IP}\{disk}$\{fromDirectory}""";

            return RunProcess(process);
        }

        public string RunItem(Host host, string fromDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);
            char disk = CutDiskFromDirectory(ref fromDirectory);

            if (disk == '\0')
            {
                return null;
            }

            Process process = InitializeProcess(host);
            process.StartInfo.Arguments += @$"""\\{host.IP}\{disk}$\{fromDirectory}"" /s";

            return RunProcess(process);
        }

        public string ReceiveItem(Host host, string fromDirectory, string toDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);
            char disk = CutDiskFromDirectory(ref fromDirectory);

            if (disk == '\0')
            {
                return null;
            }

            Process process = InitializeProcess(host);
            process.StartInfo.Arguments += @$"copy ""\\{host.IP}\{disk}$\{fromDirectory}"" ""{toDirectory}""";

            return RunProcess(process);
        }

        public string SendItem(Host host, string fromDirectory, string toDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);
            char disk = CutDiskFromDirectory(ref fromDirectory);

            if (disk == '\0')
            {
                return null;
            }

            Process process = InitializeProcess(host);
            // Should fix so to and from will be placed opposite
            process.StartInfo.Arguments += @$"copy ""{toDirectory}"" ""\\{host.IP}\{disk}$\{fromDirectory}""";

            return RunProcess(process);
        }

        public string GetFolder(Host host, string fromDirectory, string toDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);
            char disk = CutDiskFromDirectory(ref fromDirectory);

            if (disk == '\0')
            {
                return null;
            }

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