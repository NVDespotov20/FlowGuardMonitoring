namespace FlowGuardMonitoring.BLL.Models;

public class EmailModel
{
    public string Recipient { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
}