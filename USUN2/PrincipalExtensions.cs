using System.Security.Claims;

namespace USUN2;

public static class PrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal? user) =>
        user?.FindFirstValue(ClaimTypes.NameIdentifier);
}
