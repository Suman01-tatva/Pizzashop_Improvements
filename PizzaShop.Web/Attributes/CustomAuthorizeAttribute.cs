namespace PizzaShop.Service.Attributes;
using System.Security.Claims;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PizzaShop.Service.Interfaces;
using PizzaShop.Service.Utils;

public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _moduleName;
    private readonly string _permissionType;

    public CustomAuthorizeAttribute(string? moduleName = null, string? permissionType = null)
    {
        _moduleName = moduleName;
        _permissionType = permissionType;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var response = context.HttpContext.Response;
        response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        response.Headers["Pragma"] = "no-cache";
        response.Headers["Expires"] = "-1";

        var jwtService = context.HttpContext.RequestServices.GetService(typeof(IJwtService)) as IJwtService;
        var permissionService = context.HttpContext.RequestServices.GetService(typeof(IRolePermissionService)) as IRolePermissionService;

        // Get the token from Cookie
        // var token = CookieUtils.GetJWTToken();
        var token = context.HttpContext.Request.Cookies["Token"];

        // Validate Token
        var principal = jwtService?.ValidateToken(token ?? "");
        if (principal == null)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        context.HttpContext.User = principal;

        // If no module is provided, only authentication is required, skip permission check
        if (string.IsNullOrEmpty(_moduleName) || string.IsNullOrEmpty(_permissionType))
        {
            return; // Authenticated users are allowed
        }

        // Get RoleId from Claims
        var userRole = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (userRole == null)
        {
            context.Result = new RedirectToActionResult("Error", "Auth", new { statusCode = 401 });
            return;
        }
        // Fetch permissions for the role and module
        var permission = permissionService?.GetRolePermission(int.Parse(userRole), _moduleName);

        // Check the required permission
        bool hasPermission = _permissionType switch
        {
            "CanView" => (bool)permission!.CanView,
            "CanEdit" => (bool)permission!.CanEdit,
            "CanDelete" => (bool)permission!.CanDelete,
            _ => false
        };

        if (!hasPermission)
        {
            if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Result = new JsonResult(new { message = "You are not Authorized to Perform this Action." });
            }
            else
            {
                context.Result = new RedirectToActionResult("Forbidden", "Home", new { statusCode = 403 });
            }
            return;
            // context.Result = new RedirectToActionResult("Error", "Auth", new { statusCode = 403 });
        }
    }
}
