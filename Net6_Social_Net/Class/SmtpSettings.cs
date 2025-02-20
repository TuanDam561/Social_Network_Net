namespace Net7_Social_Net.Class
{
    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
    }
}
