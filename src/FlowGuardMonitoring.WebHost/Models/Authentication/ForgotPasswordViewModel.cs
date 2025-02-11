using System.ComponentModel.DataAnnotations;
using FlowGuardMonitoring.WebHost.Resources;

namespace FlowGuardMonitoring.WebHost.Models.Authentication;

public class ForgotPasswordViewModel
{
    [Required(
        ErrorMessageResourceType = typeof(AuthLocals),
        ErrorMessageResourceName = "EmailFieldRequiredErrorMsg")]
    [EmailAddress(
        ErrorMessageResourceType = typeof(AuthLocals),
        ErrorMessageResourceName = "EmailFieldInvalidErrorMsg")]
    public string? Email { get; set; }
}