using Marvel.Model;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Marvel.Commands
{
    class WinRMCommands : IProtocolCommands
    {
        public PowerShell InitializePowerShell(Host host)
        {
            PowerShell ps = PowerShell.Create();

            ps.AddScript($"$password = ConvertTo-SecureString '{host.Password}' -AsPlainText -Force");
            ps.AddScript($"$cred = New-Object System.Management.Automation.PSCredential (\"{host.Username}\", $password)");
            ps.AddScript($"$session = New-PSSession -ComputerName {host.IP} -Credential $cred");

            return ps;
        }

        public string RunPowerShell(PowerShell ps)
        {
            Collection<PSObject> result;

            try
            {
                result = ps.Invoke();
            }
            catch (Exception e)
            {
                return e.Message;
            }

            string resultString = "";

            foreach (PSObject item in result)
            {
                resultString += item + "\n";
            }

            return resultString;
        }

        public string GetUserProfileDirectory(Host host, string directory)
        {
            if (directory[0] != '%')
            {
                return directory;
            }

            PowerShell ps = InitializePowerShell(host);
            ps.AddScript("Invoke-Command -Session $session -ScriptBlock { [Environment]::GetEnvironmentVariable('UserProfile') }");
            string output = RunPowerShell(ps);

            directory = directory.Replace("%USERPROFILE%", "");
            directory = output?.Trim() + directory;

            return directory;
        }

        public string GetDirectory(Host host, string fromDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);

            PowerShell ps = InitializePowerShell(host);
            ps.AddScript($"Invoke-Command -Session $session -ScriptBlock {{ Get-ChildItem \"{fromDirectory}\" }}");

            return RunPowerShell(ps);
        }

        public string RunItem(Host host, string fromDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);

            PowerShell ps = InitializePowerShell(host);
            ps.AddScript($"Invoke-Command -Session $session -ScriptBlock {{ \"{fromDirectory}\" /silent}}");

            return RunPowerShell(ps);
        }

        public string ReceiveItem(Host host, string fromDirectory, string toDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);

            PowerShell ps = InitializePowerShell(host);
            ps.AddScript($"Copy-Item -FromSession $session \"{fromDirectory}\" -Destination \"{toDirectory}\"");

            return RunPowerShell(ps);
        }

        public string SendItem(Host host, string fromDirectory, string toDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);

            PowerShell ps = InitializePowerShell(host);
            ps.AddScript($"Copy-Item -ToSession $session \"{fromDirectory}\" -Destination \"{toDirectory}\"");

            return RunPowerShell(ps);
        }

        public string GetFolder(Host host, string fromDirectory, string toDirectory)
        {
            fromDirectory = GetUserProfileDirectory(host, fromDirectory);

            PowerShell ps = InitializePowerShell(host);
            ps.AddScript($"Copy-Item -Recurse -FromSession $session \"{fromDirectory}\" -Destination \"{toDirectory}\"");

            return RunPowerShell(ps);
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
