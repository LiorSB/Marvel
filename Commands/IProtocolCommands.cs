using Marvel.Model;

namespace Marvel.Commands
{
    interface IProtocolCommands
    {
        public string GetDirectory(Host host, string fromDirectory);
        public string RunItem(Host host, string fromDirectory);
        public string ReceiveItem(Host host, string fromDirectory, string toDirectory);
        public string SendItem(Host host, string fromDirectory, string toDirectory);
        public string GetFolder(Host host, string fromDirectory, string toDirectory);
        public string GetSystemInformation(string ip);
        public string PingIP(string ip);
        public string PortConnectivity(string ip);
    }
}
