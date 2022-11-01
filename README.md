# Marvel
A demo for the program can be seen in the file [MarvelDemo.mp4](https://github.com/LiorSB/Marvel/blob/master/MarvelDemo.mp4)

## Project Background
Marvel is a desktop application that can scan and parse artifacts to extract executables from remote computers through communication protocols. The  user can add hosts manually, from a file or automatically scan for local hosts and get information about their connectivity, OS and ports. The user can execute commands, transfer files and get information about directories from remote computers with the help of the communication protocols WinRM, SSH, SMB and WMI. Once a connection is made, The user can set Marvel to scan and parse every remote computer for artifacts according to "Windows Artifacts Analysis Program Execution" with the help of EZTools to get the paths of executable files, which will be extracted to the main machine computer.

## Project's Target
The target of this project is to improve the time it takes for an IR (Incident Response) team to handle a cyber-incident in a more efficient way.

## Who is the Project for
The project can be used by anyone, but it is dedicated especially for IR teams.

## Setup
**Disable Firewall -**

![image](https://www.wikihow.com/images/thumb/3/3b/Disable-Windows-7-Firewall-Step-2.jpg/v4-460px-Disable-Windows-7-Firewall-Step-2.jpg.webp)
![image](https://www.alphr.com/wp-content/uploads/2021/03/5-2.png)

**Enable PowerShell on Remote PC -**
[How to Run PowerShell Commands on Remote Computers](https://www.howtogeek.com/117192/how-to-run-powershell-commands-on-remote-computers/)

- Run PowerShell as administrator and run the commands:
```PowerShell
Enable-PSRemoting -Force
Set-Item wsman:\localhost\client\trustedhosts * # The asterisk is a wildcard symbol for all PCs. If instead you want to restrict computers that can connect, you can replace the asterisk with a comma-separated list of IP addresses or computer names for approved PCs.
Restart-Service WinRM
Test-WsMan COMPUTER # Replace “COMPUTER” with the name or IP address of the remote PC to test the connection.
```

**Enable SSH on Remote PC -**
[How to Install SSH Server on Windows 10 - Remote into your computer using a Command Line [OpenSSH]](https://www.youtube.com/watch?v=HCmEB5qtkSY)

- Server Installation - Enter "Optional features" in the Windows Search Bar and add the feature "OpenSSH Server" by pressing "Add an optional feature" as shown below:<br>
![image](https://4sysops.com/wp-content/uploads/2019/02/Installing-the-OpenSSH-server-via-the-Settings-app-600x402.png)
<br>
After the installation is done, restart your computer.

- Enable Service for SSH Server - Enter "Services" in the Windows Search Bar, scroll down to "OpenSSH SSH Server" and right click it and select "Properties", change "Startup type" to "Automatic" and under "Service status" press the "Start" button, press the "Okay" button to end.<br>
![image](https://i0.wp.com/www.worldofitech.com/wp-content/uploads/2020/01/ssh-step-5.4-openssh-server-properties-window-9.png?resize=750%2C618&ssl=1)


- Add Firewall Rule for SSH Server - Enter "Firewall & Network Protection" in the Windows Search Bar, go to "Advanced settings", select "Inbound Rules" and press "New Rule...". In the "New Inbound Rule Wizard" window select the "Port" radio button and press next, select "TCP" radio button and in the "Specific local ports:" radio button enter "22" and press next, select "Allow the connection" radio button and press next twice and name the rule SSH.<br>
![image](https://www.howtogeek.com/wp-content/uploads/2014/05/clip_image0271.png?trim=1,1&bg-color=000&pad=1,1)

## Project Inputs
In the application you can enter host by inputting their details manually, use the segment scanner, local IP scanner or input a file of hosts. You can later edit, select or delete existing hosts. In the combo box you can choose a protocol to work with and a command to execute, below that there are two text boxes to input the target and destination directories. Once a command is executed the command details can be seen in the output tab.<br>
![image](https://user-images.githubusercontent.com/92099051/151139290-7b38d062-aa8f-4533-8eb8-954fddcdf5aa.png)

## Commands Inputs
### Requirements for a Command
- A valid host (IP, Username and Password) must be checked from the Hosts panel.
- Chosen protocol from combobox has it's port open and listening.
- Choose a command, fill the directory fields and press the Run Command button.

### GetDirectoryFileList - Get Information about a Directory
- From Directory - Insert the remote directory path to get. Example: C:\Foo

### RunItem - Execute a File at a Remote Computer
- From Directory - Insert the remote directory path with the name of the file to run at the end. Example: C:\Foo\Item.exe

### ReceiveItem - Get an Item from a Remote Computer to the Local Computer
- From Directory - Insert the remote directory path with the name of the file to receive. Example: C:\Foo\File.txt
- To Directory - Insert the local directory path to save the file in. Example: C:\Boo

### SendItem - Send an Item from the Local Computer to a Remote Computer
- From Directory - Insert the local directory path with the name of the file to send. Example: C:\Boo\File.txt
- To Directory - Insert the remote directory path to save the file in. Example: C:\Foo

### ReceiveItem - Get an Item from a Remote Computer to the Local Computer
- From Directory - Insert the remote directory path with the name of the folder to receive. Example: C:\Foo\Hoo
- To Directory - Insert the local directory path to save the folder in. Example: C:\Boo

### ExtractExecutables - Extract Executable Files from a Remote Computer to the Local Computer
- To Directory - Insert the local directory path to extract the files to. Example: C:\Foo

## Troubleshoot
![.NET not installed Error](https://user-images.githubusercontent.com/92099051/190871668-60f888e1-315c-4787-90b4-763d50c4efd5.jpeg)

Incase .NET isn't installed on the computer, please refer to the following link: [.NET 5.0 Desktop Runtime (v5.0.17) - Windows x64 Installer!](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-5.0.17-windows-x64-installer)
