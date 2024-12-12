using System.ComponentModel.DataAnnotations;
using FlowGuardMonitoring.WebHost.Resources;

namespace FlowGuardMonitoring.WebHost.Models;

public class RegisterViewModel
{
    [Required(
        ErrorMessageResourceType = typeof(AuthLocals),
        ErrorMessageResourceName = "EmailFieldRequiredErrorMsg")]
    [EmailAddress(
        ErrorMessageResourceType = typeof(AuthLocals),
        ErrorMessageResourceName = "EmailFieldInvalidErrorMsg")]
    public string? Email { get; set; }
    [Required(
        ErrorMessageResourceType = typeof(AuthLocals),
        ErrorMessageResourceName = "PasswordFieldRequiredErrorMsg")]
    public string? Password { get; set; }
    [Compare(
        nameof(Password),
        ErrorMessageResourceType = typeof(AuthLocals),
        ErrorMessageResourceName = "PasswordsDontMatchErrorMsg")]
    public string? ConfirmPassword { get; set; }
}