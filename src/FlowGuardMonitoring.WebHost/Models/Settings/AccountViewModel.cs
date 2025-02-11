namespace FlowGuardMonitoring.WebHost.Models.Settings;

public class AccountViewModel
{
    public bool EditInfo { get; set; } = false;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public bool EditEmail { get; set; } = false;
    public string Email { get; set; } = string.Empty;

    public bool EditPassword { get; set; } = false;
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}