using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;
using PizzaShop.Web.Models;

namespace PizzaShop.Web.Controllers;
[CustomAuthorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRolePermissionService _rolePermissionService;
    private readonly ITokenDataService _tokenDataService;

    private readonly IDashboardPageService _dashboardPageService;
    public HomeController(ILogger<HomeController> logger, IRolePermissionService rolePermissionService, ITokenDataService tokenDataService, IDashboardPageService dashboardPageService)
    {
        _logger = logger;
        _rolePermissionService = rolePermissionService;
        _tokenDataService = tokenDataService;
        _dashboardPageService = dashboardPageService;
    }

    public async Task<IActionResult> AdminDashboard()
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "-1";

        // Retrieve token
        var token = Request.Cookies["Token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth"); // Redirect if token is missing
        }

        // Extract role from token
        var roleId = await _tokenDataService.GetRoleFromToken(token);
        if (roleId == null)
        {
            return RedirectToAction("Login", "Auth"); // Redirect if role is not found
        }
        var permission = _rolePermissionService.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        DashboardViewModel dashboard = new DashboardViewModel();
        dashboard = await _dashboardPageService.GetDashboardPage("Last 30 days", DateTime.Now, DateTime.Now);
        return View(dashboard);
    }

    public async Task<IActionResult> GetDashboardData(string? timePeriod, DateTime? startDate, DateTime? endDate)
    {

        DashboardViewModel dashboard = new DashboardViewModel();
        try
        {
            dashboard = await _dashboardPageService.GetDashboardPage(timePeriod, startDate, endDate);
            return PartialView("_DashboardContent", dashboard);
        }
        catch (System.Exception)
        {
            return PartialView("_DashboardContent", dashboard);
        }
    }

    public IActionResult NotFound()
    {
        return View("Error");
    }

    public IActionResult Error()
    {
        return View("Error");
    }
    public IActionResult Unauthorized()
    {
        return RedirectToAction("Login", "Accounts");
    }
    public IActionResult Forbidden()
    {
        return View("Forbidden");
    }
}
