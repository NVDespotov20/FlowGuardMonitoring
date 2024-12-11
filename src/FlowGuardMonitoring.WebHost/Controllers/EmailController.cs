using Essentials.Results;
using FlowGuardMonitoring.BLL.Models;
using Microsoft.AspNetCore.Mvc;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.WebHost.Models;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Route("api/email")]
public class EmailController : Controller
{
    private readonly EmailSenderService _emailSenderService;

    public EmailController(EmailSenderService emailSenderService)
    {
        _emailSenderService = emailSenderService;
    }
    
    [HttpGet("send")]
    public async Task<IActionResult> Get()
    {
        return await _emailSenderService.SendEmailAsync(new EmailModel()
        {
            Recipient = "ni_des@outlook.com",
            Subject = "Test",
            Body = "Hello world!"
        }) == StandardResult.SuccessfulResult()
            ? View("../Home/Index") :
            View("Error", new ErrorViewModel());
    }
}