using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BankProductsRegistry.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    protected int? GetCurrentClientId()
    {
        var clientIdClaim = User.FindFirstValue(Security.CustomClaimTypes.ClientId);
        return int.TryParse(clientIdClaim, out var clientId) ? clientId : null;
    }

    protected bool IsInRole(string role) => User.IsInRole(role);

    protected (int? UserId, string UserName) GetCurrentActor()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "system";

        return (userId, userName);
    }

    protected static ProblemDetails BuildProblem(int statusCode, string title, string detail) =>
        new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

    protected static ProblemDetails BuildIdentityProblem(int statusCode, string title, IdentityResult result) =>
        new()
        {
            Status = statusCode,
            Title = title,
            Detail = string.Join("; ", result.Errors.Select(error => error.Description))
        };
}
