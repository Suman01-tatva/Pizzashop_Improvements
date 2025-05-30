using System.Security.Claims;

namespace PizzaShop.Service.Interfaces;

public interface IJwtService
{
    public string GenerateJwtToken(string id, string email, string role, bool? IsFirstLogin, bool rememberMe);
    object GenerateJwtToken(string email);

    ClaimsPrincipal? ValidateToken(string token);
}
