namespace PizzaShop.Web.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

// [Route("tax")]
// [Authorize]

public class TaxAndFeeController : Controller
{
    private readonly ITaxAndFeesService _taxesAndFeesService;

    private readonly IRolePermissionService _rolePermission;
    private readonly ITokenDataService _tokenDataService;
    public TaxAndFeeController(ITaxAndFeesService taxesAndFeesService, ITokenDataService tokenDataService, IRolePermissionService rolePermissionService)
    {
        _taxesAndFeesService = taxesAndFeesService;
        _rolePermission = rolePermissionService;
        _tokenDataService = tokenDataService;
    }

    // [HttpGet("taxlist")]
    [CustomAuthorize("Tax and Fees", "CanView")]
    [HttpGet]
    public async Task<IActionResult> TaxAndFee()
    {
        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        Console.WriteLine("Role Id", roleId);
        var permission = _rolePermission.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        var taxes = await _taxesAndFeesService.GetAllTaxes();
        return View(taxes);
    }

    public IActionResult Test()
    {
        return View();
    }

    [CustomAuthorize("Tax and Fees", "CanView")]
    // [HttpGet("taxform")]
    [HttpGet]
    public async Task<IActionResult> TaxForm(int? id)
    {
        if (id.HasValue)
        {
            var tax = await _taxesAndFeesService.GetTaxById(id.Value);
            return PartialView("_TaxForm", tax);
        }
        return PartialView("_TaxForm", new TaxAndFeesViewModel());
    }

    [CustomAuthorize("Tax and Fees", "CanEdit")]
    // [HttpPost("addtax")]
    [HttpPost]
    public async Task<IActionResult> AddTax(TaxAndFeesViewModel model)
    {
        if (ModelState.IsValid)
        {
            var tax = await _taxesAndFeesService.AddTax(model);
            if (tax)
            {
                return Json(new { success = true, message = "Tax created successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Tax Already Exist!" });
            }
        }
        else
        {
            var errorMessage = "Please Enter the valid data.";
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    errorMessage += $"{state.Key}: {state.Value.Errors.First().ErrorMessage}; ";
                }
            }
            TempData["ToastrMessage"] = errorMessage;
            TempData["ToastrType"] = "error";
            return PartialView("_TaxForm", model);
        }
    }

    [CustomAuthorize("Tax and Fees", "CanEdit")]
    // [HttpPost("edit")]
    [HttpPost]
    public async Task<IActionResult> EditTax(TaxAndFeesViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _taxesAndFeesService.UpdateTax(model);
            if (result)
            {
                return Json(new { success = true, message = "Tax updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Tax Already Exist!" });
            }
        }
        else
        {
            var errorMessage = "";
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    errorMessage += $"{state.Key}: {state.Value.Errors.First().ErrorMessage}; ";
                }
            }
            TempData["ToastrMessage"] = errorMessage;
            TempData["ToastrType"] = "error";
            return PartialView("_TaxForm", model);
        }
    }

    [CustomAuthorize("Tax and Fees", "CanDelete")]
    // [HttpPost("delete/{id}")]
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _taxesAndFeesService.DeleteTax(id);
        if (result)
        {
            return Json(new { success = true, message = "Tax deleted successfully." });
        }
        return Json(new { success = false, message = "Failed to delete tax." });
    }

    // [HttpGet("search")]
    [HttpGet]
    public async Task<IActionResult> Search(string query)
    {
        var taxes = await _taxesAndFeesService.GetAllTaxes();
        if (!string.IsNullOrEmpty(query))
        {
            query = query.ToLower();
            taxes = taxes.Where(t => t.Name.ToLower().Contains(query)).ToList();
        }
        return PartialView("_TaxTablePartial", taxes);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTaxesForFilter()
    {
        var taxes = await _taxesAndFeesService.GetAllTaxes();
        return PartialView("_TaxTablePartial", taxes);
    }

    [CustomAuthorize("Tax and Fees", "CanEdit")]
    // [HttpPost("updateStatus")]
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, bool isActive, bool isDefault)
    {
        var tax = await _taxesAndFeesService.GetTaxById(id);

        if (tax == null)
        {
            return Json(new { success = false, message = "Tax not found." });
        }

        tax.IsActive = isActive;
        tax.IsDefault = isDefault;

        var result = await _taxesAndFeesService.UpdateTax(tax);

        if (result)
        {
            return Json(new { success = true, message = "Status updated successfully." });
        }

        return Json(new { success = false, message = "Failed to update status." });
    }
}