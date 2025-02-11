using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using Essentials.Results;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.BLL.Options;
using FlowGuardMonitoring.BLL.Resources;
using Microsoft.Extensions.Options;
using Resend;

namespace FlowGuardMonitoring.BLL.Services;

public class EmailSenderService
{
    private readonly IOptions<EmailOptions> options;
    private readonly IResend resend;

    public EmailSenderService(IOptions<EmailOptions> optionsAccessor, IResend resend)
    {
        this.options = optionsAccessor;
        this.resend = resend;
    }

    public async Task<StandardResult> SendEmailAsync(EmailModel email)
    {
        var options = this.options.Value;

        try
        {
            var message = new EmailMessage
            {
                From = options.Username,
                To = email.Recipient,
                Subject = email.Subject,
                HtmlBody = email.Body,
            };

            await this.resend.EmailSendAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error sending email: {ex.Message}");
            return StandardResult.UnsuccessfulResult();
        }

        return StandardResult.SuccessfulResult();
    }

    public async Task<StandardResult> SendResetPasswordEmailAsync(string email, string resetPasswordUrl)
    {
        return await this.SendEmailAsync(new EmailModel
        {
            Recipient = email,
            Subject = EmailLocals.ResetPasswordTitle,
            Body = EmailLocals.ResetPassword.Replace("{0}", resetPasswordUrl),
        });
    }

    public async Task<StandardResult> SendEmailConfirmationAsync(string email, string confirmation)
    {
        return await this.SendEmailAsync(new EmailModel
        {
            Recipient = email,
            Subject = EmailLocals.ConfirmEmailTitle,
            Body = EmailLocals.ConfirmEmail.Replace("{0}", confirmation),
        });
    }
}