using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;
using PizzaShop.Web.Notifiers.cs;

namespace PizzaShop.Web.Controllers;
// [CustomAuthorize]
public class OrderAppKOTController : Controller
{
    private readonly IKotService _kotService;
    private readonly ITokenDataService _tokenDataService;
    private readonly IRolePermissionService _roelPermissionService;
    private readonly IKotNotifier _kotNotifier;

    public OrderAppKOTController(IKotService kotService, ITokenDataService tokenDataService, IRolePermissionService rolePermissionService, IKotNotifier kotNotifier)
    {
        _kotService = kotService;
        _tokenDataService = tokenDataService;
        _roelPermissionService = rolePermissionService;
        _kotNotifier = kotNotifier;
    }

    [HttpGet]
    [Route("/orderappkot/kot")]
    // [Authorize(Roles = "2")]
    [CustomAuthorize("KOT", "CanView")]
    public async Task<IActionResult> KOT(int? id, int? status = 3)
    {
        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        var permission = _roelPermissionService.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        var data = await _kotService.GetKotDetails(id, status);

        return View(data);
    }

    public async Task<IActionResult> GetTables(int? id, int? status)
    {
        var data = await _kotService.GetKotDetails(id, status);
        return PartialView("_KotTables", data.TableDetails);
    }

    [CustomAuthorize("KOT", "CanView")]
    public async Task<IActionResult> GetOrderDetails(string data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        var model = System.Text.Json.JsonSerializer.Deserialize<KotTableDetailViewModel>(data, options);
        return PartialView("_MarkAsPrepared", model);
    }

    [HttpPost]
    [CustomAuthorize("KOT", "CanEdit")]
    public async Task<IActionResult> MarkAsPrepared(List<UpdateOrderItems> items, List<UpdateOrderItems> inProgressitems, int status)
    {
        var token = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        if (items != null && items!.Count > 0)
        {
            var isPrepared = await _kotService.MarkAsPreparedItems(items, int.Parse(userId));
            if (isPrepared)
            {
                await _kotNotifier.NotifyKotUpdates();
                return Json(new { success = true, message = "Items prepared successfully" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to prepare items. Please try again." });
            }
        }
        else if (inProgressitems != null && inProgressitems!.Count > 0)
        {
            var isInProgress = await _kotService.MarkAsInProgress(inProgressitems, int.Parse(userId));
            if (isInProgress)
            {
                await _kotNotifier.NotifyKotUpdates();
                return Json(new { success = true, message = "Items MoveInto InProgress successfully" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to move InProgress. Please try again." });
            }
        }

        return Json(new { success = false, message = "No items received" });
    }
}