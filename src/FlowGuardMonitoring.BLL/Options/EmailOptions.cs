using Microsoft.Extensions.Options;
using Resend;

namespace FlowGuardMonitoring.BLL.Options;

public class EmailOptions
{
    public required string Username { get; set; }
    public required string ApiKey { get; set; }
}