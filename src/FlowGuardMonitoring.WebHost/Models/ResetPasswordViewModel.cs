using System.ComponentModel.DataAnnotations;
using Azure.Core;
using FlowGuardMonitoring.WebHost.Resources;

namespace FlowGuardMonitoring.WebHost.Models;

public class ResetPasswordViewModel
{
    [Required]
    public string? Email { get; set; }

    [Required]
    public string? Token { get; set; }

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