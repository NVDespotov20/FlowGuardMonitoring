using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using Essentials.Results;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.BLL.Options;
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
        var emailToSend = new EmailMessage
        {
            From = options.Username,
            To = email.Recipient,
            Subject = email.Subject,
            HtmlBody = email.Body,
        };
        await this.resend.EmailSendAsync(emailToSend);
        return StandardResult.SuccessfulResult();
    }
}