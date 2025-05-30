namespace PizzaShop.Web.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    private readonly ITokenDataService _tokenDataService;
    private readonly IRolePermissionService _rolePermissionService;

    public BaseController(ITokenDataService tokenDataService, IRolePermissionService rolePermissionService)
    {
        _tokenDataService = tokenDataService;
        _rolePermissionService = rolePermissionService;
    }

    public override async void OnActionExecuting(ActionExecutingContext context)
    {
        var permissionJson = HttpContext.Session.GetString("permission");
        if (string.IsNullOrEmpty(permissionJson))
        {
            var token = HttpContext.Request.Cookies["Token"];
            if (!string.IsNullOrEmpty(token))
            {
                var roleId = await _tokenDataService.GetRoleFromToken(token);
                var permission = _rolePermissionService.GetRolePermissionByRoleId(roleId);
                HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));
            }
        }

        base.OnActionExecuting(context);
    }
}
