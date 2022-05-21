namespace Marvel.Model
{
    public class Host
    {
        public string IP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsChecked { get; set; }
        public string Details { get; set; }
        public string PortsConnectivity { get; set; }
        public string SystemInformation { get; set; }

        public Host(Host newHost)
        {
            IP = newHost.IP;
            Username = newHost.Username;
            Password = newHost.Password;
            IsChecked = true;
            Details = "";
            PortsConnectivity = "";
            SystemInformation = "";
        }

        public Host(string IP, string Username, string Password)
        {
            this.IP = IP;
            this.Username = Username;
            this.Password = Password;
            IsChecked = true;
            Details = "";
            PortsConnectivity = "";
            SystemInformation = "";
        }

        public Host(string IP)
        {
            this.IP = IP;
            Details = "";
            PortsConnectivity = "";
            SystemInformation = "";
        }
    }
}
