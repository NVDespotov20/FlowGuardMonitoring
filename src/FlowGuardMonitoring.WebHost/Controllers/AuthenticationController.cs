using FlowGuardMonitoring.WebHost.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlowGuardMonitoring.WebHost.Controllers;
public class AuthenticationController : Controller
{
    [HttpGet("/login")]
    public IActionResult Login()
    {
        return this.View();
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (this.ModelState.IsValid)
        {
           // test
        }

        return this.View(model);
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        return this.View();
    }

    [HttpPost("/register")]
    public IActionResult Register(RegisterViewModel model)
    {
        if (this.ModelState.IsValid)
        {
            // test
        }

        return this.View(model);
    }
}