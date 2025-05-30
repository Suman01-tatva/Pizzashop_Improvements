using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Web.Controllers;

public class TableSectionController : Controller
{
    private readonly ITableService _tableService;
    private readonly ISectionService _sectionService;
    private readonly ITokenDataService _tokenDataService;
    private readonly IRolePermissionService _rolePermission;
    public TableSectionController(ITableService tableService, ISectionService sectionService, ITokenDataService tokenDataService, IRolePermissionService rolePermissionService)
    {
        _tableService = tableService;
        _sectionService = sectionService;
        _tokenDataService = tokenDataService;
        _rolePermission = rolePermissionService;
    }
    [CustomAuthorize("Table and Section", "CanView")]
    [HttpGet]
    public async Task<IActionResult> TableSection(int? id, int pageSize = 5, int pageIndex = 1, string searchString = "")
    {

        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        Console.WriteLine("Role Id", roleId);
        var permission = _rolePermission.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        var sections = _sectionService.GetAllSections();
        if (!sections.Any())
        {
            return NotFound("No Tables found.");
        }
        var validSectionId = id ?? sections.First().Id;

        var tables = _tableService.GetTablesBySectionId(validSectionId, pageSize, pageIndex, searchString);
        tables.Sections = sections;
        return View(tables);
    }

    public IActionResult GetAllTables()
    {
        var tables = _tableService.GetAllTables();
        return PartialView("_TableList", tables);
    }

    [CustomAuthorize("Table and Section", "CanView")]
    [HttpGet]
    public async Task<IActionResult> GetTablesBySectionId(int sectionId, int pageSize, int pageIndex, string searchString = "")
    {
        var sections = _sectionService.GetAllSections();

        TableSectionViewModel model = _tableService.GetTablesBySectionId(sectionId, pageSize == 0 ? 5 : pageSize, pageIndex == 0 ? 1 : pageIndex, searchString);
        model.Sections = sections;
        return PartialView("_TableList", model);
    }


    [CustomAuthorize("Table and Section", "CanView")]
    [HttpGet]
    public async Task<JsonResult> GetAllSections()
    {
        var sections = Json(_sectionService.GetAllSections());
        return sections;
    }


    [CustomAuthorize("Table and Section", "CanView")]
    [HttpGet]
    public IActionResult GetAllSectionsForFilter()
    {
        var sections = _sectionService.GetAllSections();
        return PartialView("_SectionList", sections);
    }

    [HttpGet]
    public IActionResult AddNewTable()
    {
        return PartialView("_AddEditTable", new TableViewModel());
    }

    [CustomAuthorize("Table and Section", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> AddNewTable(TableViewModel model)
    {
        if (ModelState.IsValid)
        {
            var token = Request.Cookies["Token"];
            var (currentUserEmail, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
            var table = _tableService.AdddTable(model, int.Parse(id));

            if (table)
            {
                return Json(new { success = true, message = "Table Added successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Table already exist!" });
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
            return Json(new { success = false, message = errorMessage });
        }
    }

    [CustomAuthorize("Table and Section", "CanDelete")]
    [HttpPost]
    public async Task<IActionResult>? DeleteTable(int id)
    {
        var token = Request.Cookies["Token"];
        if (token == null)
            return RedirectToAction("Login", "Auth");

        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        if (email == null)
            return null!;

        try
        {
            var isDeleted = _tableService.DeleteTable(id, int.Parse(userId));
            if (!isDeleted)
                return Json(new { isSuccess = false, message = "Table is Occupied so you can not delete it." });
            return Json(new { isSuccess = true, message = "Table Deleted Successfully" });
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Error While Delete Table. Please Try again!" });
        }
    }

    [CustomAuthorize("Table and Section", "CanDelete")]
    [HttpPost]
    public async Task<IActionResult>? MultiDeleteTable(int[] itemIds)
    {
        var token = Request.Cookies["Token"];
        if (token == null)
            return RedirectToAction("Login", "Auth");
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        if (email == null)
            return null!;
        try
        {
            var isDeleted = _tableService.MultiDeleteTable(itemIds, int.Parse(userId));
            if (!isDeleted)
                return Json(new { isSuccess = false, message = "You can not delete the occupied table." });
            return Json(new { isSuccess = true, message = "Tables Deleted Successfully" });
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Error While Delete Table. Please Try again!" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditTable(int id)
    {
        var table = await _tableService.GetTableById(id);
        if (table == null)
        {
            return NotFound();
        }
        return PartialView("_AddEditTable", table);
    }

    [CustomAuthorize("Table and Section", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> EditTable(TableViewModel model)
    {
        if (ModelState.IsValid)
        {
            var token = Request.Cookies["Token"];
            var (currentUserEmail, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
            var updated = _tableService.UpdateTable(model, int.Parse(userId));

            if (updated)
            {
                return Json(new { success = true, message = "Table updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Table Already Exist!" });
            }
        }
        else
        {
            var errorMessage = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return Json(new { success = false, message = errorMessage });
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddEditSection(int? id)
    {
        if (id == null || id == 0)
            return PartialView("_AddEditSection", new SectionViewModel());

        var tableSection = await _sectionService.GetSectionByIdAsync(id.Value);
        return PartialView("_AddEditSection", tableSection);
    }

    [CustomAuthorize("Table and Section", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> AddEditSection(SectionViewModel sectionViewModel)
    {
        if (!ModelState.IsValid)
            return PartialView("_AddEditSection", sectionViewModel);

        try
        {
            var token = Request.Cookies["Token"];
            var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);

            if (sectionViewModel.Id == 0)
                await _sectionService.AddSectionAsync(sectionViewModel, int.Parse(userId));
            else
                await _sectionService.UpdateSectionAsync(sectionViewModel, int.Parse(userId));

            return Json(new { success = true, message = "Section saved successfully." });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [CustomAuthorize("Table and Section", "CanDelete")]
    [HttpPost]
    public async Task<IActionResult> DeleteSection(int id, bool softDelete = true)
    {
        var token = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        var isDeleted = await _sectionService.DeleteSectionAsync(id, softDelete, int.Parse(userId));
        if (isDeleted == "table is occupied")
        {
            return Json(new { isSuccess = false, message = "You can not delete this section because any table of this section is occupied" });
        }
        else if (isDeleted == "success")
        {
            return Json(new { isSuccess = true, message = "Section deleted successfully" });
        }
        else
        {
            return Json(new { isSuccess = false, message = "section not found" });
        }
    }

}
