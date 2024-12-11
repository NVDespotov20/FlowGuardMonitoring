using Microsoft.AspNetCore.Mvc;

namespace FlowGuardMonitoring.WebHost.Controllers;
[Route("auth")]
public class AuthenticationController : Controller
{
    // GET
    [HttpGet("login")]
    public IActionResult Login()
    {
        return View();
    }
    // GET
    [HttpGet("register")]
    public IActionResult Register()
    {
        return View();
    }
}