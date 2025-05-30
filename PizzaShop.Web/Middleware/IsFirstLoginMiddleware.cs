using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PizzaShop.Web.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaShop.Web.Middleware
{
    public class IsFirstLoginMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IsFirstLoginMiddleware> _logger;
        private readonly string _secretKey;
        private readonly string _cookieName;

        public IsFirstLoginMiddleware(RequestDelegate next, ILogger<IsFirstLoginMiddleware> logger, string secretKey, string cookieName)
        {
            _next = next;
            _logger = logger;
            _secretKey = secretKey;
            _cookieName = cookieName;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            _logger.LogInformation($"Processing request for path: {path}");

            var token = JwtTokenHelper.GetTokenFromCookies(context, _cookieName);
            if (!string.IsNullOrEmpty(token))
            {
                var principal = JwtTokenHelper.GetPrincipalFromExpiredToken(token, _secretKey);
                var isFirstLogin = principal.Claims.FirstOrDefault(c => c.Type == "IsFirstLogin")?.Value;
                // _logger.LogInformation($"isFirstLogin claim value: {isFirstLogin}");

                if (bool.TryParse(isFirstLogin, out var isFirstLoginValue) && isFirstLoginValue)
                {
                    if (!path.Contains("/Auth/Login") && !path.Contains("/Auth/ForgotPassword") && !path.Contains("/Auth/ResetPassword") && !path.Contains("/User/ChangePassword") && !path.Contains("/Auth/Logout"))
                    {
                        // _logger.LogInformation("isFirstLogin is true, redirecting to /User/ChangePassword");
                        context.Response.Redirect("/User/ChangePassword");
                        return;
                    }
                }
            }
            else
            {
                _logger.LogInformation("User is not authenticated or token is missing.");
            }

            await _next(context);
        }
    }
}