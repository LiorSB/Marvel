using Marvel.Model;
using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Marvel.Commands
{
    class RPCCommands : IProtocolCommands
    {
        public string GetDirectory(Host host, string fromDirectory)
        {
            throw new System.NotImplementedException();
        }

        public string GetFolder(Host host, string fromDirectory, string toDirectory)
        {
            throw new System.NotImplementedException();
        }

        public string GetSystemInformation(string ip)
        {
            string cmdOutPut = "";

            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = @"/C systeminfo /s " + ip,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

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

        public string PingIP(string ip)
        {
            string cmdOutPut = "";

            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = @"/C ping " + ip,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

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

        //public string PingIP(string ip)
        //{
        //    Ping pingSender = new Ping();
        //    PingOptions options = new PingOptions();

        //    // Use the default Ttl value which is 128,
        //    // but change the fragmentation behavior.
        //    options.DontFragment = true;

        //    // Create a buffer of 32 bytes of data to be transmitted.
        //    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        //    byte[] buffer = Encoding.ASCII.GetBytes(data);
        //    int timeout = 120;

        //    PingReply reply;

        //    try
        //    {
        //        reply = pingSender.Send(ip, timeout, buffer, options);
        //    }
        //    catch (PingException pe)
        //    {
        //        return pe.Message;
        //    }

        //    string pingReply = "";

        //    if (reply.Status == IPStatus.Success)
        //    {
        //        pingReply += "Address: " + reply.Address.ToString() + "\n";
        //        pingReply += "RoundTrip time: " + reply.RoundtripTime.ToString() + "\n";
        //        pingReply += "Time to live: " + reply.Options.Ttl.ToString() + "\n";
        //        pingReply += "Don't fragment: " + reply.Options.DontFragment.ToString() + "\n";
        //        pingReply += "Buffer size: " + reply.Buffer.Length.ToString() + "\n";
        //    }

        //    return pingReply;
        //}

        public string PortConnectivity(string ip)
        {
            int[] ports = new int[] { 22, 80, 135, 137, 138, 139, 443, 445, 593, 5985, 5986 };

            string portsConnectivity = "";

            foreach (int port in ports)
            {
                TcpClient Scan = new();

                try
                {
                    Scan.Connect(ip, port);
                    portsConnectivity += $"[{port}] | OPEN" + "\n";
                }
                catch
                {
                    portsConnectivity += $"[{port}] | CLOSED" + "\n";
                }
            }

            return portsConnectivity;
        }

        public string ReceiveItem(Host host, string fromDirectory, string toDirectory)
        {
            throw new System.NotImplementedException();
        }

        public string RunItem(Host host, string fromDirectory)
        {
            throw new System.NotImplementedException();
        }

        public string SendItem(Host host, string fromDirectory, string toDirectory)
        {
            throw new System.NotImplementedException();
        }
    }
}
