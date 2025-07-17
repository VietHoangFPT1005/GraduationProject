namespace SWP391.EventFlowerExchange.Domain.Entities
{
    /// <summary>
    /// Represents the SMTP settings required for sending emails, including server details and credentials.
    /// </summary>
    public class SmtpSetting
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool EnableSsl { get; set; }
    }
}

