using Marvel.Enum;
using Marvel.Model;
using System.Diagnostics;

namespace Marvel.ProtocolCommands
{
    class SMBCommands
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
    }
}