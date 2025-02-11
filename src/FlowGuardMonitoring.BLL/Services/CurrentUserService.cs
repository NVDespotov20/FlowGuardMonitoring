using System.Security.Claims;
using FlowGuardMonitoring.BLL.Contracts;
using Microsoft.AspNetCore.Http;

namespace FlowGuardMonitoring.BLL.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public string UserId => this.httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public string UserName => this.httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;

    public bool IsAuthenticated => this.httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public ClaimsPrincipal? User => this.httpContextAccessor.HttpContext?.User;
}