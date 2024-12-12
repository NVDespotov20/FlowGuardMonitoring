using Essentials.Results;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.WebHost.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Route("api/email")]
public class EmailController : Controller
{
    private readonly EmailSenderService emailSenderService;

    public EmailController(EmailSenderService emailSenderService)
    {
        this.emailSenderService = emailSenderService;
    }

    [HttpGet("send")]
    public async Task<IActionResult> Get()
    {
        return await this.emailSenderService.SendEmailAsync(new EmailModel()
        {
            Recipient = "ni_des@outlook.com",
            Subject = "Test",
            Body = "Hello world!",
        }) == StandardResult.SuccessfulResult()
            ? this.View("../Home/Index") :
            this.View("Error", new ErrorViewModel());
    }
}