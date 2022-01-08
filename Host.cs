namespace Marvel
{
    public class Host
    {
        public string IP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Details { get; set; }
        public bool IsSelected { get; set; }

        public Host(Host newHost)
        {
            IP = newHost.IP;
            Username = newHost.Username;
            Password = newHost.Password;
        }

        public Host(string IP, string Username, string Password)
        {
            this.IP = IP;
            this.Username = Username;
            this.Password = Password;
        }

        public Host(string IP)
        {
            this.IP = IP;
        }
    }
}
