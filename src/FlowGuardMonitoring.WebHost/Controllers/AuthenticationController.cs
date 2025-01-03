using System.Text.Encodings.Web;
using Essentials.Extensions;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.WebHost.Extentions;
using FlowGuardMonitoring.WebHost.Models;
using FlowGuardMonitoring.WebHost.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlowGuardMonitoring.WebHost.Controllers;
public class AuthenticationController : Controller
{
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly UrlEncoder urlEncoder;
    private readonly EmailSenderService emailService;
    private readonly ILogger<AuthenticationController> logger;
    public AuthenticationController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        UrlEncoder urlEncoder,
        EmailSenderService emailService,
        ILogger<AuthenticationController> logger)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.urlEncoder = urlEncoder;
        this.emailService = emailService;
        this.logger = logger;
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        if (this.IsUserAuthenticated())
        {
            return this.RedirectToDefault();
        }

        var model = new LoginViewModel();
        return this.View(model);
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (this.IsUserAuthenticated())
        {
            return this.RedirectToDefault();
        }

        if (this.ModelState.IsValid)
        {
            var user = await this.userManager.FindByEmailAsync(model.Email!);
            if (user == null || !(await this.userManager.CheckPasswordAsync(user, model.Password!)))
            {
                this.ModelState.AddModelError(string.Empty, AuthLocals.InvalidLoginErrorMsg);
                return this.View(model);
            }

            if (await this.userManager.IsLockedOutAsync(user))
            {
                this.ModelState.AddModelError(string.Empty, AuthLocals.UserLockedOutErrorMsg);
                return this.View(model);
            }

            await this.SignInAsync(user, model.RememberMe);

            return this.RedirectToDefault();
        }

        return this.View(model);
    }

    [HttpGet("/register")]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (this.IsUserAuthenticated())
        {
            return this.RedirectToDefault();
        }

        var model = new RegisterViewModel();
        return this.View(model);
    }

    [HttpPost("/register")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (this.IsUserAuthenticated())
        {
            return this.RedirectToDefault();
        }

        if (this.ModelState.IsValid)
        {
            var result = await this.userManager.CreateAsync(
                new User
                {
                    FirstName = model.FirstName!,
                    LastName = model.LastName!,
                    UserName = model.Email,
                    Email = model.Email,
                },
                model.Password!);

            if (result.Succeeded)
            {
                this.TempData["MessageText"] = AuthLocals.RegisterSuccessMessage;
                this.TempData["MessageVariant"] = "success";
                return this.RedirectToAction(nameof(this.Login));
            }

            this.ModelState.AssignIdentityErrors(result.Errors);
        }

        return this.View(model);
    }

    [HttpGet("/forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        if (this.IsUserAuthenticated())
        {
            return this.RedirectToDefault();
        }

        var model = new ForgotPasswordViewModel();
        return this.View(model);
    }

    [HttpPost("/forgot-password")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (this.IsUserAuthenticated())
        {
            return this.RedirectToDefault();
        }

        if (this.ModelState.IsValid)
        {
            var user = await this.userManager.FindByEmailAsync(model.Email!);
            if (user != null)
            {
                var resetPasswordToken = await this.userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordEncodedToken = UrlEncoder.Default.Encode(resetPasswordToken);
                var resetPasswordUrl = this.HttpContext
                    .GetAbsoluteRoute($"/reset-password?email={user.Email}&token={resetPasswordEncodedToken}");

                var result = await this.emailService.SendResetPasswordEmailAsync(
                    user.Email!,
                    resetPasswordUrl);

                this.logger.LogInformation("Reset password email send result {Result}", result);
            }

            this.TempData["MessageText"] = AuthLocals.ForgotPasswordSuccessMessage;
            this.TempData["MessageVariant"] = "success";

            return this.RedirectToAction(nameof(this.Login));
        }

        return this.View(model);
    }

    [HttpGet("/reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string email, string token)
    {
        if (this.IsUserAuthenticated())
        {
            return this.RedirectToDefault();
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return this.NotFound();
        }

        var user = await this.userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return this.NotFound();
        }

        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email,
        };

        return this.View(model);
    }

    [HttpPost("/reset-password")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (this.IsUserAuthenticated())
        {
            return this.RedirectToDefault();
        }

        if (this.ModelState.IsValid)
        {
            var user = await this.userManager.FindByEmailAsync(model.Email!);
            if (user == null)
            {
                return this.NotFound();
            }

            var passwordHasher = new PasswordHasher<User>();
            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, model.Password!);

            if (passwordVerificationResult == PasswordVerificationResult.Success)
            {
                this.ModelState.AddModelError(string.Empty, AuthLocals.ResetPasswordNewPasswordSameAsCurrent);
            }

            var passwordValidator = new PasswordValidator<User>();
            var passwordValidationResult = await passwordValidator.ValidateAsync(this.userManager, user, model.Password!);

            if (!passwordValidationResult.Succeeded)
            {
                this.ModelState.AssignIdentityErrors(passwordValidationResult.Errors);
            }

            var result = await this.userManager.ResetPasswordAsync(user, model.Token!, model.Password!);
            if (result.Succeeded)
            {
                return this.RedirectToAction(nameof(this.Login));
            }

            this.ModelState.AssignIdentityErrors(result.Errors);
        }

        return this.View(model);
    }

    [HttpPost("/logout")]
    public async Task<IActionResult> Logout()
    {
        await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return this.RedirectToAction(nameof(this.Login));
    }

    private async Task SignInAsync(
        User user,
        bool rememberMe)
    {
        var claimsPrinciple = await this.signInManager
            .ClaimsFactory
            .CreateAsync(user);

        await this.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrinciple,
            new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                IsPersistent = rememberMe,
            });
    }
}