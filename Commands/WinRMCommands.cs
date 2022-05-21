using Marvel.Enum;
using Marvel.Model;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Marvel.Commands
{
    class WinRMCommands : IProtocolCommands
    {
        public string Commands(Host host, string fromDirectory, string toDirectory, CommandsEnum selectedCommand)
        {
            PowerShell ps = PowerShell.Create();

            // Set credentials
            ps.AddScript("$password = ConvertTo-SecureString '" +
                host.Password + "' -AsPlainText -Force");
            ps.AddScript("$cred = New-Object System.Management.Automation.PSCredential" +
                " (\"" + host.Username + "\", $password)");

            // Start Session
            ps.AddScript("$session = New-PSSession -ComputerName " + host.IP + " -Credential $cred");

            switch (selectedCommand)
            {
                case CommandsEnum.GetDirectoryFilesList:
                    ps.AddScript("Invoke-Command -Session $session -ScriptBlock { Get-ChildItem " + fromDirectory + " }");
                    break;
                case CommandsEnum.RunItem:
                    ps.AddScript("Invoke-Command -Session $session -ScriptBlock {" + fromDirectory + " /silent}");
                    break;
                case CommandsEnum.ReceiveItem:
                    ps.AddScript("Copy-Item -FromSession $session " + fromDirectory + " -Destination " + toDirectory);
                    break;
                case CommandsEnum.SendItem:
                    ps.AddScript("Copy-Item -ToSession $session " + fromDirectory + " -Destination " + toDirectory);
                    break;
                default:
                    return null;
            }

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

        public PowerShell InitializePowerShell(Host host)
        {
            PowerShell ps = PowerShell.Create();

            // Set credentials
            ps.AddScript($"$password = ConvertTo-SecureString '{host.Password}' -AsPlainText -Force");
            ps.AddScript($"$cred = New-Object System.Management.Automation.PSCredential (\"{host.Username}\", $password)");

            // Start Session
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

        public string GetDirectory(Host host, string fromDirectory)
        {
            PowerShell ps = InitializePowerShell(host);
            ps.AddScript($"Invoke-Command -Session $session -ScriptBlock {{ Get-ChildItem {fromDirectory} }}");

            return RunPowerShell(ps);
        }

        public string RunItem(Host host, string fromDirectory)
        {
            PowerShell ps = InitializePowerShell(host);
            ps.AddScript($"Invoke-Command -Session $session -ScriptBlock {{ {fromDirectory} /silent}}");

            return RunPowerShell(ps);
        }

        public string ReceiveItem(Host host, string fromDirectory, string toDirectory)
        {
            PowerShell ps = InitializePowerShell(host);
            ps.AddScript($"Copy-Item -FromSession $session {fromDirectory} -Destination {toDirectory}");

            return RunPowerShell(ps);
        }

        public string SendItem(Host host, string fromDirectory, string toDirectory)
        {
            PowerShell ps = InitializePowerShell(host);
            ps.AddScript($"Copy-Item -ToSession $session {fromDirectory} -Destination {toDirectory}");

            return RunPowerShell(ps);
        }

        public string GetFolder(Host host, string fromDirectory, string toDirectory)
        {
            throw new NotImplementedException();
        }

        public string GetSystemInformation(string ip)
        {
            throw new NotImplementedException();
        }

        public string PingIP(string ip)
        {
            throw new NotImplementedException();
        }

        public string PortConnectivity(string ip)
        {
            throw new NotImplementedException();
        }
    }
}
