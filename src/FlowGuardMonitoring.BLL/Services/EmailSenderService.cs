using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Essentials.Results;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.BLL.Options;
using Microsoft.Extensions.Options;

namespace FlowGuardMonitoring.BLL.Services;

public class EmailSenderService
{
    private readonly IOptions<EmailOptions> options;

    public EmailSenderService(IOptions<EmailOptions> optionsAccessor)
    {
        this.options = optionsAccessor;
    }

    public async Task<StandardResult> SendEmailAsync(EmailModel email)
    {
        var options = this.options.Value;
        using var client = new SmtpClient(options.SmtpServer, options.Port);

        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(options.Username, options.Password);
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.EnableSsl = options.EnableSsl;

        try
        {
            var mailMessage = new MailMessage(options.Username, email.Recipient);
            mailMessage.Subject = email.Subject;
            mailMessage.Body = email.Body;
            await client.SendMailAsync(mailMessage);
            return StandardResult.SuccessfulResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            return StandardResult.UnsuccessfulResult();
        }
    }
}