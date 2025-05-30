using PizzaShop.Service.Interfaces;
using System.Security.Claims;

namespace Pizzashop.Service.Implementations;

public class TokenDataService : ITokenDataService
{
    private readonly IJwtService _jwtservice;

    public TokenDataService(IJwtService jwtservice)
    {
        _jwtservice = jwtservice;
    }

    public async Task<(string email, string id, string isFirstLogin)> GetEmailFromToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            var principal = _jwtservice.ValidateToken(token);
            if (principal != null)
            {
                var emailClaim = principal.Claims.First(claim => claim.Type == ClaimTypes.Email);
                var idClaim = principal.Claims.First(claim => claim.Type == "Id");
                var isFirstLoginClaim = principal.Claims.First(claim => claim.Type == "IsFirstLogin");

                var email = emailClaim.Value;
                var id = idClaim.Value;
                var isFirstLogin = isFirstLoginClaim.Value;

                return (email.ToString(), id, isFirstLogin);
            }
        }

        return ("", "", "");
    }

    public async Task<int> GetRoleFromToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            var principal = _jwtservice.ValidateToken(token);
            if (principal != null)
            {
                var role = principal.Claims.First(claim => claim.Type == ClaimTypes.Role);
                return int.Parse(role.Value);
            }
        }

        return 0;
    }
}
