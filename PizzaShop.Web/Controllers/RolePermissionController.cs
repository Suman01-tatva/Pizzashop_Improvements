using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Web.Controllers;

public class RolePermissionController : Controller
{
    private readonly IUserService _user;
    private readonly IRolePermissionService _rolePermission;
    private readonly ITokenDataService _tokenDataService;
    public RolePermissionController(IUserService user, IRolePermissionService rolePermission, ITokenDataService tokenDataService)
    {
        _user = user;
        _rolePermission = rolePermission;
        _tokenDataService = tokenDataService;
    }

    [CustomAuthorize("Role And Permissions", "CanView")]
    [HttpGet]
    public async Task<IActionResult> Role()
    {
        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        Console.WriteLine("Role Id", roleId);
        var permission = _rolePermission.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));
        // var email = Request.Cookies["email"];
        // var AuthToken = Request.Cookies["Token"];
        // if (String.IsNullOrEmpty(AuthToken))
        // {
        //     return RedirectToAction("Login", "Auth");
        // }

        // if (string.IsNullOrEmpty(AuthToken))
        //     return RedirectToAction("Login", "Auth");

        // var user = _user.GetUserByEmailAsync(email);
        // if (user == null || user.RoleId != 1)
        //     return RedirectToAction("Dashboard", "Dashboard");

        var roles = await _rolePermission.GetAllRoles();
        return View(roles);
    }

    [CustomAuthorize("Role And Permissions", "CanView")]
    [HttpGet]
    public async Task<IActionResult> Permission(int id)
    {
        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        Console.WriteLine("Role Id", roleId);
        var permission = _rolePermission.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        var email = Request.Cookies["email"];
        var AuthToken = Request.Cookies["Token"];
        if (String.IsNullOrEmpty(AuthToken))
        {
            return RedirectToAction("Login", "Auth");
        }
        var model = _rolePermission.GetRolePermissionByRoleId(id);
        if (model != null)
            return View(model);
        return RedirectToAction("Permission", "RolePermission");
    }

    [CustomAuthorize("Role And Permissions", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> Permission(List<RolePermissionViewModel> model)
    {
        if (ModelState.IsValid)
        {
            var token = Request.Cookies["Token"];
            var (email, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token);
            var AuthToken = Request.Cookies["Token"];
            if (String.IsNullOrEmpty(AuthToken))
            {
                return RedirectToAction("Login", "Auth");
            }

            bool updateRolePermission = await _rolePermission.UpdateRolePermission(model, email);
            if (updateRolePermission)
            {
                TempData["SuccessUpdate"] = "Role And Permissions Updated Successfully";
                 var roleId = await _tokenDataService.GetRoleFromToken(token!);
                Console.WriteLine("Role Id", roleId);
                var permission = _rolePermission.GetRolePermissionByRoleId(roleId);
                HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));
                return View(model);
            }
            TempData["ErrorUpdate"] = "Something went wrong Pls Try Again";
            return View(model);
        }
        else
        {
            return View(model);
        }
    }
}