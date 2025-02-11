using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.WebHost.Extentions;
using FlowGuardMonitoring.WebHost.Models.Settings;
using FlowGuardMonitoring.WebHost.Resources;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Route("/settings")]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class SettingsController : Controller
{
    private readonly UserManager<User> userManager;

    public SettingsController(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    [HttpGet("/account")]
    public async Task<ActionResult> Account()
    {
        if (!this.IsUserAuthenticated())
        {
            this.RedirectToDefault();
        }

        var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name!);
        if (user == null)
        {
            return this.NotFound();
        }

        return this.View(new AccountViewModel()
        {
            EditInfo = false,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EditEmail = false,
            Email = user.Email!,
            EditPassword = false,
            OldPassword = string.Empty,
            NewPassword = string.Empty,
            ConfirmPassword = string.Empty,
        });
    }

    [HttpPost("/account")]
    public async Task<ActionResult> Account(AccountViewModel model)
    {
        if (this.IsUserAuthenticated())
        {
            this.RedirectToDefault();
        }

        if (this.ModelState.IsValid)
        {
            var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name!);
            if (user == null)
            {
                return this.NotFound();
            }

            if (model.EditInfo == true)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
            }

            if (model.EditEmail == true)
            {
                user.Email = model.Email;
            }

            if (model.EditPassword == true)
            {
                var passwordHasher = new PasswordHasher<User>();
                var passwordVerificationResult =
                    passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, model.OldPassword!);

                if (passwordVerificationResult != PasswordVerificationResult.Success)
                {
                    this.ModelState.AddModelError(string.Empty, AuthLocals.ResetPasswordNewPasswordSameAsCurrent);
                    return this.View(model);
                }

                await this.userManager.ChangePasswordAsync(user, model.OldPassword!, model.NewPassword!);
            }
        }

        return this.View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string token)
    {
        var user = await this.userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return this.NotFound();
        }

        var result = await this.userManager.ChangeEmailAsync(user, email, token);
        if (result.Succeeded)
        {
            this.TempData["SuccessMessage"] = "Email changed successfully.";
            return this.RedirectToAction(nameof(this.Account));
        }

        return this.RedirectToAction(nameof(this.Account));
    }
}