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
            Recipient = "nvdespotov20@codingburgas.bg",
            Subject = "Hello!",
            Body = "<div><strong>Greetings<strong> 👋🏻 from .NET</div>",
        }) == StandardResult.SuccessfulResult()
            ? this.View("../Home/Index") :
            this.View("Error", new ErrorViewModel());
    }
}