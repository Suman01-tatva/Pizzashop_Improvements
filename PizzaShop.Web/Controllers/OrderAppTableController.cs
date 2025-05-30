using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Web.Controllers;
// [CustomAuthorize]
public class OrderAppTableController : Controller
{
    private readonly IOrderAppTableService _orderAppTableService;
    private readonly ITokenDataService _tokenDataService;
    public OrderAppTableController(IOrderAppTableService orderAppTableService, ITokenDataService tokenDataService)
    {
        _orderAppTableService = orderAppTableService;
        _tokenDataService = tokenDataService;
    }

    [Route("/orderapptable/table")]
    // [Authorize(Roles = "2")]
    [CustomAuthorize("Table and Section", "CanView")]
    public async Task<IActionResult> Table()
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        try
        {
            OrderAppTableViewModel model = await _orderAppTableService.GetTableView();

            return View(model);
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e);
            return View();
        }
    }

    [HttpGet]
    [CustomAuthorize("Table and Section", "CanView")]
    public async Task<IActionResult> GetWaitingTokensBySectionId(int sectionId)
    {
        var tokenList = await _orderAppTableService.GetWaitingTokensBySectionId(sectionId);
        return PartialView("_AssignTableSidebar", tokenList);
    }

    [HttpPost]
    [CustomAuthorize("Table and Section", "CanEdit")]
    public async Task<IActionResult> AssignTable([FromForm] AssignTableViewModel model, string tableIds)
    {
        var token = Request.Cookies["Token"];
        var (currentUserEmail, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        List<tableDetails> tables = JsonConvert.DeserializeObject<List<tableDetails>>(tableIds)!;
        model.TableOrders = tables;
        var result = await _orderAppTableService.AssignTableToCustomer(model, int.Parse(id));

        if (result.Split(':')[0] == "customer have already running order")
        {
            var Id = result.Split(':')[1];
            TempData["ToastrMessage"] = "Customer Have Already Running Order.";
            TempData["ToastrType"] = "warning";
            return Json(new { success = false, message = "Customer Have Already Running Order", id = Id });
        }
        else if (result == "Please Select Tables of Same Section")
        {
            return Json(new { success = false, message = "Please Select Tables of Same Section" });
        }
        else if (result == "fail" || result == null)
        {
            return Json(new { success = false, message = "Error Occurred in Assign Table, Please Try Again." });
        }
        var orderId = result.Split(':')[1];
        return Json(new { success = true, message = "Table Assigned Successfully", id = orderId });
    }
}