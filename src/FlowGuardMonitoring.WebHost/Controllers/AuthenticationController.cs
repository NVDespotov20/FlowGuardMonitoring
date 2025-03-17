using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Web;
using Essentials.Extensions;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.WebHost.Extentions;
using FlowGuardMonitoring.WebHost.Models;
using FlowGuardMonitoring.WebHost.Models.Authentication;
using FlowGuardMonitoring.WebHost.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FlowGuardMonitoring.WebHost.Controllers;
public class AuthenticationController : Controller
{
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly EmailSenderService emailService;
    private readonly ILogger<AuthenticationController> logger;
    public AuthenticationController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        EmailSenderService emailService,
        ILogger<AuthenticationController> logger)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
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

            if (!await this.userManager.IsEmailConfirmedAsync(user))
            {
                this.ModelState.AddModelError(string.Empty, AuthLocals.EmailNotConfirmedErrorMsg);
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

        if (!this.ModelState.IsValid || model.Email.IsNullOrEmpty())
        {
            return this.View(model);
        }

        var existingUser = await this.userManager.FindByEmailAsync(model.Email!);
        if (existingUser != null)
        {
            this.ModelState.AddModelError(string.Empty, AuthLocals.EmailAlreadyUsedErrorMsg);
            return this.View(model);
        }

        var user = new User()
        {
            Email = model.Email,
            UserName = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
        };

        var result = await this.userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            this.ModelState.AssignIdentityErrors(result.Errors);
        }

        var emailVerificationToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(emailVerificationToken);
        var emailVerificationUrl =
            this.Url.ActionLink(
                nameof(this.ConfirmEmail),
                "Authentication",
                new
                {
                    email = user.Email,
                    token = encodedToken,
                });

        if (emailVerificationUrl != null && user.Email != null)
        {
            await this.emailService.SendEmailConfirmationAsync(user.Email, emailVerificationUrl);
        }

        return this.RedirectToAction(nameof(this.Login));
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
                return this.View(model);
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

    [HttpGet("/email-confirm")]
    public async Task<IActionResult> ConfirmEmail(string? email, string? token)
    {
        if (email == null || token == null)
        {
            return this.RedirectToDefault();
        }

        var user = await this.userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return this.NotFound("User not found");
        }

        var result = await this.userManager.ConfirmEmailAsync(user, HttpUtility.UrlDecode(token));
        if (result.Succeeded)
        {
            return this.View("EmailConfirmed");
        }
        else
        {
            return this.View("Error", new ErrorViewModel { RequestId = this.HttpContext.TraceIdentifier });
        }
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