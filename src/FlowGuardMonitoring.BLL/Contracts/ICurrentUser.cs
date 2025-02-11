using System.Security.Claims;

namespace FlowGuardMonitoring.BLL.Contracts;

public interface ICurrentUser
{
    string UserId { get; }
    string UserName { get; }
    bool IsAuthenticated { get; }
    ClaimsPrincipal? User { get; }
}