using Marvel.Enum;
using Marvel.Model;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Marvel.ProtocolCommands
{
    class WinRMCommands
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
    }
}
