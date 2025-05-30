using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Web.Controllers;
// [CustomAuthorize]
public class OrderAppWaitingListController : Controller
{
    private readonly IOrderAppWaitingListService _waitingTokenListService;
    private readonly ITokenDataService _tokenDataService;
    private readonly IRolePermissionService _reolePermissionService;

    public OrderAppWaitingListController(IOrderAppWaitingListService orderAppWaitingListService, ITokenDataService tokenDataService, IRolePermissionService rolePermissionService)
    {
        _waitingTokenListService = orderAppWaitingListService;
        _tokenDataService = tokenDataService;
        _reolePermissionService = rolePermissionService;
    }
    [HttpGet("orderappwaitinglist/waitinglist/")]
    // [Authorize(Roles = "2")]
    [CustomAuthorize("Waiting Token", "CanView")]
    public async Task<IActionResult> WaitingList(int id)
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        var permission = _reolePermissionService.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        WaitingTokenListPageViewModel waitingListPage = new WaitingTokenListPageViewModel();
        try
        {
            waitingListPage = await _waitingTokenListService.GetWaitingListPage(0);
            return View(waitingListPage);
        }
        catch (System.Exception)
        {
            return View(waitingListPage);
        }
    }

    [HttpGet]
    [CustomAuthorize("Waiting Token", "CanView")]
    public async Task<IActionResult> GetWaitingListBySectionId(int? id)
    {
        WaitingTokenListPageViewModel waitingListPage = new WaitingTokenListPageViewModel();
        try
        {
            waitingListPage = await _waitingTokenListService.GetWaitingListPage(id);
            return PartialView("_WaitingTokenList", waitingListPage.WaitingList);
        }
        catch (System.Exception)
        {
            return PartialView("_WaitingTokenList", waitingListPage.WaitingList);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSectionList()
    {
        List<SectionViewModel> sectionList = new List<SectionViewModel>();
        try
        {
            sectionList = await _waitingTokenListService.GetSectionList();
            return PartialView("_SectionList", sectionList);
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Error while fetching the data" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddEditWaitingToken(int? id)
    {
        AddEditWatingTokenViewModel WaitingToken = await _waitingTokenListService.GetAddEditWaitingToken(id);
        return PartialView("_AddEditWaitingTokenModal", WaitingToken);
    }

    [HttpPost]
    [CustomAuthorize("Waiting Token", "CanEdit")]
    public async Task<IActionResult> AddEditWaitingToken(AddEditWatingTokenViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { isValid = false, message = "fill required field" });
        }

        var token = Request.Cookies["Token"];
        var (currentUserEmail, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        string msg;
        try
        {
            bool Wt = await _waitingTokenListService.AddEditWaitingToken(model, int.Parse(userId));

            if (Wt == false)
            {
                return Json(new { isSuccess = false, message = "The selected section does not have enough capacity. Please choose a different section." });
            }
            msg = "Waiting token updated successfully";
            if (model.Id == 0 || model.Id == null)
                msg = "Waiting token created successfully";
            return Json(new { isSuccess = true, message = msg });
        }
        catch (System.Exception)
        {
            msg = "Error while update waiting token";
            if (model.Id == 0 || model.Id == null)
                msg = "Error while create waiting token";
            return Json(new { isSuccess = false, message = msg });
        }

    }

    [HttpPost]
    [CustomAuthorize("Waiting Token", "CanDelete")]
    public async Task<IActionResult> DeleteWaitingToken(int id)
    {
        var token = Request.Cookies["Token"];
        var (currentUserEmail, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        try
        {
            await _waitingTokenListService.DeleteWaitingtoken(id, int.Parse(userId));
            return Json(new { isSuccess = true, message = "Waiting token deleted SuccessFully" });
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Waiting token deleted SuccessFully" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerSuggestion(string? email)
    {
        try
        {
            List<CustomerSuggestionListViewModel> customerSuggestions = await _waitingTokenListService.SearchCustomerByEmail(email);
            return Json(new { isSuccess = true, customerSuggestions });
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false });
        }
    }

    [HttpGet]
    public async Task<IActionResult> AssignWaitingToken(int id)
    {
        try
        {
            AssignTablePageViewModel model = await _waitingTokenListService.GetWaitingTokensById(id);
            return PartialView("_AssignTableModal", model);
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Error while fetching the data" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTables(int id)
    {
        try
        {
            JsonArray tables = await _waitingTokenListService.GetTablesBySectionId(id);
            return Json(new { isSuccess = true, tables });
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Error while fetching the data" });
        }
    }
}
