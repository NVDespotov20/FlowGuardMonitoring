namespace FlowGuardMonitoring.BLL.Options;

public class EmailOptions
{
    public required string SmtpServer { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required bool EnableSsl { get; set; }
}