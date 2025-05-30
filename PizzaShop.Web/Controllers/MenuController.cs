using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Web.Controllers;
// [Route("api/[controller]")]
// [ApiController]
public class MenuController : Controller
{
    private readonly IMenuService _menuService;
    private readonly IMenuModifierService _menuModifierService;
    private readonly ITokenDataService _tokenDataService;

    private readonly IRolePermissionService _rolePermission;

    public MenuController(IMenuService menuService, IMenuModifierService menuModifierService, ITokenDataService tokenDataService, IRolePermissionService rolePermissionService)
    {
        _menuService = menuService;
        _menuModifierService = menuModifierService;
        _tokenDataService = tokenDataService;
        _rolePermission = rolePermissionService;
    }
    [CustomAuthorize("Menu", "CanView")]
    [HttpGet]
    public async Task<IActionResult> Menu(int pageSize = 5, int pageIndex = 1, string searchString = "")
    {
        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        Console.WriteLine("Role Id", roleId);
        var permission = _rolePermission.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        var categories = await _menuService.GetAllMenuCategoriesAsync();
        if (!categories.Any())
        {
            return NotFound("No categories found.");
        }

        var validCategoryId = categories.First().Id;
        var itemTabDetails = await _menuService.GetItemTabDetails(validCategoryId, pageSize, pageIndex, searchString);

        //modifierTab data
        var modifierGroups = await _menuModifierService.GetAllMenuModifierGroupAsync();

        var validModifierGroupId = modifierGroups.First().Id;
        var modifierTabDetails = await _menuModifierService.GetModifierTabDetails(validModifierGroupId, pageSize, pageIndex, searchString);

        var model = new MenuViewModel
        {
            ItemTab = itemTabDetails,
            ModifierTab = modifierTabDetails
        };

        return View(model);
    }

    [CustomAuthorize("Menu", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> AddEditCategory(AddEditCategoryViewModel model)
    {
        if (!ModelState.IsValid)
            return Json(new { isValid = false, message = "Please fill the required fields" });
        if (model.Id == 0 || model.Id == null)
        {
            try
            {
                bool newCategory = await _menuService.AddNewCategory(model.Name, model);
                if (newCategory == false)
                    return Json(new { isExist = true, message = "Category is already exist" });
                return Json(new { isSuccess = true, message = "New category added successfully" });
            }
            catch (System.Exception)
            {
                return Json(new { isSuccess = false, message = "Error while adding new category" });

            }
        }
        else
        {
            try
            {
                var category = await _menuService.EditCategory(model, model.Id.Value);
                if (category == false)
                    return Json(new { isExist = true, message = "Category is already exist" });
                return Json(new { isSuccess = true, message = "Category updated successfully" });
            }
            catch (System.Exception)
            {
                return Json(new { isSuccess = false, message = "Error while updating category" });

            }
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddEditCategory(int? id)
    {
        if (id == null)
        {
            var modal = new AddEditCategoryViewModel();
            return PartialView("_AddEditCategoryModal", modal);
        }
        else
        {
            AddEditCategoryViewModel model = await _menuService.GetCategoryDetailById(id.Value);
            return PartialView("_AddEditCategoryModal", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetItemsByCategory(int categoryId, int pageSize, int pageIndex, string searchString = "")
    {
        List<MenuItemViewModel> items = await _menuService.GetItemsByCategory(categoryId, pageSize == 0 ? 5 : pageSize, pageIndex == 0 ? 1 : pageIndex, searchString);
        var totalItemCount = _menuService.GetItemsCountByCId(categoryId, searchString);

        var model = new ItemTabViewModel
        {
            itemList = items,
            PageSize = pageSize,
            PageIndex = pageIndex,
            SearchString = searchString,
            TotalPage = (int)Math.Ceiling(totalItemCount / (double)pageSize),
            TotalItems = totalItemCount
        };
        return PartialView("_ItemListPartial", model);
    }

    [CustomAuthorize("Menu", "CanDelete")]
    [HttpPost]
    public IActionResult DeleteCategory(int id)
    {
        var result = _menuService.SoftDeleteCategory(id);
        if (result)
        {
            TempData["ToastrMessage"] = "Category deleted successfully.";
            TempData["ToastrType"] = "success";
            return Json(new { success = true, message = "Category deleted successfully." });
        }
        else
        {
            TempData["ToastrMessage"] = "Failed to delete category.";
            TempData["ToastrType"] = "error";
            return Json(new { success = false, message = "Failed to delete category." });
        }
    }

    [HttpGet]
    public JsonResult GetAllUnits()
    {
        var units = Json(_menuService.GetAllUnits());
        return units;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategory()
    {
        var categories = await _menuService.GetAllMenuCategoriesAsync();
        return PartialView("_CategoryList", categories.ToList());
    }

    [HttpGet]
    public async Task<IActionResult> AddItem()
    {
        List<MenuModifierGroupViewModel> ModifierGroups = await _menuModifierService.GetAllMenuModifierGroupAsync();
        List<Unit> units = _menuService.GetAllUnits();
        List<MenuCategoryViewModel> categories = await _menuService.GetAllMenuCategoriesAsync();
        var model = new MenuItemViewModel
        {
            Units = units,
            Categories = categories,
            ModifierGroups = ModifierGroups
        };
        return PartialView("_AddItem", model);
    }

    [CustomAuthorize("Menu", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> AddItem(MenuItemViewModel model, string ItemModifiers)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { isValid = false, message = "Please fill all required field" });
        }
        var token = Request.Cookies["Token"];
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Authentication");

        var (email, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        try
        {
            bool isItemExist = _menuService.IsItemExist(model.Name, model.CategoryId, model.Id);
            if (isItemExist)
            {
                return Json(new { isExist = true, message = "This item is already exist" });
            }
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Error while add new item" });
        }


        List<ItemModifierViewModel> itemModifiers = new List<ItemModifierViewModel>();
        if (!string.IsNullOrEmpty(ItemModifiers))
        {
            itemModifiers = System.Text.Json.JsonSerializer.Deserialize<List<ItemModifierViewModel>>(ItemModifiers);
        }
        model.ItemModifiersList = itemModifiers;
        try
        {
            var response = await _menuService.AddNewItem(model, int.Parse(id));
            return Json(new { isSuccess = true, message = "Item added successfully" });
        }
        catch (Exception e)
        {
            return Json(new { isSuccess = false, message = "Error while add new item" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditMenuItem(int itemId)
    {
        var menuItem = await _menuService.GetMenuItemById(itemId);
        return PartialView("_EditItem", menuItem);
    }

    [CustomAuthorize("Menu", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult?> EditMenuItem([FromForm] MenuItemViewModel model, string ItemModifiers)
    {
        var token = Request.Cookies["Token"];
        if (string.IsNullOrEmpty(token))
            return null;
        var (email, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        if (email == null)
            return null;

        bool isItemExist = _menuService.IsItemExist(model.Name, model.CategoryId, model.Id);
        if (isItemExist)
        {
            return Json(new { isExist = true, message = "This item is already exist" });
        }

        try
        {
            List<ItemModifierViewModel> itemModifiers = new List<ItemModifierViewModel>();
            if (!string.IsNullOrEmpty(ItemModifiers))
            {
                itemModifiers = System.Text.Json.JsonSerializer.Deserialize<List<ItemModifierViewModel>>(ItemModifiers);
            }
            model.ItemModifiersList = itemModifiers;
            await _menuService.EditItem(model, int.Parse(id));
            return Json(new { isSuccess = true, message = "Item updated successfully" });
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Error while edit item" });
        }
    }

    [CustomAuthorize("Menu", "CanDelete")]
    [HttpPost]
    public async Task<IActionResult>? DeleteMenuItem(int id)
    {
        var token = Request.Cookies["Token"];
        if (token == null)
            return RedirectToAction("Login", "Auth");

        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        if (email == null)
            return null!;
        try
        {
            _menuService.DeleteMenuItem(id);
            return Json(new { isSuccess = true, message = "Item Deleted Successfully" });
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Error While Delete Item" });
        }
    }

    [CustomAuthorize("Menu", "CanDelete")]
    [HttpPost]
    public async Task<IActionResult>? MultiDeleteMenuItem(int[] itemIds)
    {
        var token = Request.Cookies["Token"];
        if (token == null)
            return RedirectToAction("Login", "Auth");
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        if (email == null)
            return null!;
        try
        {
            _menuService.MultiDeleteMenuItem(itemIds);
            return Json(new { isSuccess = true, message = "Items Deleted Successfully" });
        }
        catch (System.Exception)
        {
            return Json(new { isSuccess = false, message = "Error While Delete Item" });
        }
    }

    // Modifier Section

    [HttpGet]
    public async Task<IActionResult> AddModifier()
    {
        var modifierGroup = await _menuModifierService.GetAllMenuModifierGroupAsync();
        var units = _menuService.GetAllUnits();
        var modal = new AddEditModifierViewModel();
        modal.Units = units;
        modal.ModifierGroups = modifierGroup;
        return PartialView("_AddModifier", modal);
    }

    [CustomAuthorize("Menu", "CanView")]
    [HttpGet]
    public async Task<IActionResult> GetModifiersByModifierGroup(int modifierGroupId, int pageSize, int pageIndex, string searchString = "")
    {
        List<MenuModifierViewModel> filteredModifiers = await _menuModifierService.GetModifiersByModifierGroup(modifierGroupId, pageSize == 0 ? 5 : pageSize, pageIndex == 0 ? 1 : pageIndex, searchString);
        int totalModifierCount = _menuModifierService.GetModifiersCountByCId(modifierGroupId, searchString);
        var model = new ModifierTabViewModel
        {
            modifier = filteredModifiers,
            PageSize = pageSize,
            PageIndex = pageIndex,
            SearchString = searchString,
            TotalPage = (int)Math.Ceiling(totalModifierCount / (double)pageSize),
            TotalItems = totalModifierCount
        };
        return PartialView("_Modifier", model);
    }

    [CustomAuthorize("Menu", "CanView")]
    [HttpGet]
    public async Task<IActionResult> GetAllModifierGroups()
    {
        var modifiers = await _menuModifierService.GetAllMenuModifierGroupAsync();
        return PartialView("_ModifierGroup", modifiers);
    }

    [HttpGet]
    public async Task<IActionResult> GetModifiersByGroup(int id, string name)
    {
        List<MenuModifierViewModel> Modifiers = await _menuModifierService.GetModifiersByGroupId(id);
        var itemModifiers = new ItemModifierViewModel
        {
            ModifierList = Modifiers,
            ModifierGroupId = id,
            Name = name,
        };
        return PartialView("_ItemModifiers", itemModifiers);
    }

    [CustomAuthorize("Menu", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> AddModifier(AddEditModifierViewModel model)
    {
        var AuthToken = Request.Cookies["Token"];

        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(AuthToken!);
        if (!ModelState.IsValid)
        {
            return Json(new { isSuccess = false, message = "Please fill the required Values" });
        }
        try
        {
            bool isAdded = _menuModifierService.AddModifier(model, int.Parse(userId));

            if (isAdded)
            {
                return Json(new { isSuccess = true, message = "New modifier added successfully" });
            }
            else
                return Json(new { isSuccess = false, message = "Modifier already exist" });  //if null is Modifier Exist
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
            return Json(new { isSuccess = false, message = "Error while add new modifier" });
        }

    }

    [HttpGet]
    public IActionResult EditModifier(int id)
    {

        var editModifier = _menuModifierService.GetModifierByid(id);
        return PartialView("_EditModifier", editModifier);
    }

    [CustomAuthorize("Menu", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> EditModifier(AddEditModifierViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { isSuccess = false, message = "Please fill the required Values" });
        }
        var AuthToken = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(AuthToken!);
        if (string.IsNullOrEmpty(email))
            return null!;
        try
        {
            bool isEdited = _menuModifierService.EditModifier(model, int.Parse(userId));
            if (isEdited)
                return Json(new { isSuccess = true, message = "Modifier updated successfully" });
            else
                return Json(new { isSuccess = false, message = "Modifier is already exist" });
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
            return Json(new { isSuccess = false, message = "Error while edit modifier" });
        }
    }

    [CustomAuthorize("Menu", "CanDelete")]
    [HttpPost]
    public IActionResult DeleteModifier(int id)
    {
        try
        {
            _menuModifierService.DeleteModifier(id);
            return Json(new { isSuccess = true, message = "Modifier deleted successfully" });
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
            return Json(new { isSuccess = false, isAuthenticate = true, message = "Error while delete modifier" });
        }
    }

    [CustomAuthorize("Menu", "CanDelete")]
    [HttpPost]
    public IActionResult DeleteMultipleModifier(int[] modifierIds)
    {
        try
        {
            _menuModifierService.DeleteMultipleModifiers(modifierIds);
            return Json(new { isSuccess = true, message = "Modifiers deleted successfully" });
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
            return Json(new { isSuccess = true, message = "Error while delete Modifiers" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllModifiers(int pageSize, int pageIndex, string? searchString)
    {
        var modifiers = await _menuModifierService.GetAllModifiers(pageSize, pageIndex, searchString);

        return PartialView("_AddExistingModifier", modifiers);
    }

    [CustomAuthorize("Menu", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult?> AddModifierGroup([FromBody] MenuModifierGroupViewModel model)
    {
        var AuthToken = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(AuthToken!);

        if (string.IsNullOrEmpty(email))
            return null;

        bool isAlreadyExist = _menuModifierService.IsModifierGrpExist(model.Name, model.Id);
        if (isAlreadyExist)
            return null;

        bool IsAdded = _menuModifierService.AddModifierGroup(model, int.Parse(userId));

        var modifierGroups = await _menuModifierService.GetAllMenuModifierGroupAsync();
        if (IsAdded)
        {
            return PartialView("_ModifierGroup", modifierGroups.ToList());
        }
        else
        {
            return PartialView("_ModifierGroup", null);
        }
    }

    [CustomAuthorize("Menu", "CanEdit")]
    [HttpGet]
    public async Task<IActionResult> EditModifierGroup(int id)
    {
        var AuthToken = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(AuthToken!);

        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Authentication");

        var editModifier = _menuModifierService.GetEditModifierGroupDetail(id);

        return Json(new { data = editModifier });
    }

    [CustomAuthorize("Menu", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> EditModifierGroup([FromBody] MenuModifierGroupViewModel model)
    {
        var AuthToken = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(AuthToken!);

        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Authentication");

        bool isAlreadyExist = _menuModifierService.IsModifierGrpExist(model.Name, model.Id);
        if (isAlreadyExist)
            return null!;

        bool isUpdated = _menuModifierService.EditModifierGroup(model, int.Parse(userId));
        // var pagination = new PaginationViewModel { PageIndex = 1, PageSize = 5, SearchString = "" };
        var modifierGroups = await _menuModifierService.GetAllMenuModifierGroupAsync();
        if (isUpdated)
        {
            return PartialView("_ModifierGroup", modifierGroups.ToList());
        }
        else
        {
            return BadRequest();
        }
    }

    public async Task<IActionResult> DeleteModifierGroup(int id)
    {
        var isDeleted = await _menuModifierService.DeleteModifierGroup(id);
        if (isDeleted)
        {
            return Json(new { success = true, message = "ModifierGroup Deleted Successgully" });
        }
        else
        {
            return Json(new { success = false, message = "Error Occurred while deleting the ModifierGroup Please Try Again." });
        }
    }

}