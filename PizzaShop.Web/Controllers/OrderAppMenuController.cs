using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Implementations;
using PizzaShop.Service.Interfaces;
using PizzaShop.Web.Models;
using PizzaShop.Web.Notifiers.cs;

namespace PizzaShop.Web.Controllers;

// [CustomAuthorize]
public class OrderAppMenuController : Controller
{
    private readonly IMenuService _menuService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IOrderAppMenuService _orderAppMenuService;
    private readonly ITokenDataService _tokenDataService;
    private readonly IRolePermissionService _roelPermissionService;
    private readonly IKotNotifier _kotNotifier;
    public OrderAppMenuController(IMenuService menuService, IOrderService orderService, ICustomerService customerService, IOrderAppMenuService orderAppMenuService, ITokenDataService tokenDataService, IRolePermissionService rolePermissionService, IKotNotifier kotNotifier)
    {
        _menuService = menuService;
        _orderService = orderService;
        _customerService = customerService;
        _orderAppMenuService = orderAppMenuService;
        _tokenDataService = tokenDataService;
        _roelPermissionService = rolePermissionService;
        _kotNotifier = kotNotifier;
    }

    [HttpGet("orderappmenu/menu/{id?}")]
    [CustomAuthorize("Menu", "CanView")]
    // [Authorize(Roles = "2")]
    public async Task<IActionResult> Menu(int? id)
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        var permission = _roelPermissionService.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        var categories = await _menuService.GetAllMenuCategoriesAsync();
        var items = await _menuService.GetAllMenuItems();

        var model = new OrderAppMenuPageViewModel
        {
            CategoryList = categories,
            ItemList = items,
            OrderDetails = new MenuOrderDetailsViewModel()
        };
        if (id != null)
        {
            var orderDetails = await _orderService.GetMenuOrderDetails((int)id);
            if (orderDetails == null)
                return RedirectToAction("Error", "Home");
            else if (orderDetails != null && (orderDetails.OrderStatus != 6))
                model.OrderDetails = orderDetails;
            else
            {
                TempData["ToastrMessage"] = "This order has already been canceled. You cannot view or modify it.";
                TempData["ToastrType"] = "error";
                return RedirectToAction("Table", "OrderAppTable");
            }
        }

        return View(model);
    }

    [HttpGet]
    [CustomAuthorize("Menu", "CanView")]
    public async Task<IActionResult> GetMenuItemsByCategoryId(int categoryId, string searchString, string type)
    {
        var items = await _menuService.GetMenuItemsByCategoryId(categoryId, searchString, type);

        return PartialView("_MenuItems", items);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderedItems(int id)
    {
        var orderDetails = await _orderService.GetMenuOrderDetails(id);
        var model = new OrderAppMenuPageViewModel
        {
            OrderDetails = orderDetails
        };
        return PartialView("_OrderItemDetails", model);
    }

    [HttpGet]
    [CustomAuthorize("Menu", "CanView")]
    public async Task<IActionResult> GetCustomerDetails(int id)
    {
        var orderDetail = await _orderService.GetMenuOrderDetails(id);
        var model = new OrderAppMenuPageViewModel
        {
            OrderDetails = orderDetail
        };
        return PartialView("_CustomerDetailsModal", model);
    }

    [HttpPost]
    [CustomAuthorize("Menu", "CanEdit")]
    public async Task<IActionResult> UpdateCustomerDetails(int id, string name, string mobileNumber)
    {
        var AuthToken = Request.Cookies["Token"];
        var (email, userId, isFirsLogin) = await _tokenDataService.GetEmailFromToken(AuthToken!);
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Auth", "Login");

        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Please Fill the required details." });
        }

        var isUpdated = await _customerService.UpdateCustomerDetails(id, name, mobileNumber, int.Parse(userId));
        if (isUpdated)
        {
            return Json(new { success = true, message = "Customer Updated successfully" });
        }
        else
        {
            return Json(new { success = false, message = "Error occured in updated customer, Please Try again." });
        }
    }

    [HttpPost]
    [CustomAuthorize("Menu", "CanEdit")]
    public async Task<IActionResult> AddEditOrderComments(int orderId, string comments)
    {

        var isUpdated = await _orderAppMenuService.AddEditOrderComments(orderId, comments);
        if (isUpdated)
        {
            return Json(new { success = true, message = "Comment added successfully" });
        }
        else
        {
            return Json(new { success = false, message = "Fail to added comment, Please try again." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetItemModifiers(int itemId)
    {
        var itemModifiers = await _orderAppMenuService.GetItemModifiers(itemId);
        if (itemModifiers.Count() > 0)
            return PartialView("_ItemModifierModal", itemModifiers);
        else
            return Json(new { success = false, count = 0 });
    }

    [HttpPost]
    [CustomAuthorize("Menu", "CanView")]
    public async Task<IActionResult> MarkAsFavourite(int itemId)
    {
        try
        {
            var result = await _orderAppMenuService.MarkAsFavourite(itemId);
            if (!result)
            {
                return Json(new { success = false, message = "Item not found" });
            }
            return Json(new { success = true, message = "Item marked as favourite" });
        }
        catch (System.Exception)
        {
            return Json(new { success = false, message = "Something went wrong" });
        }
    }

    [HttpPost]
    [CustomAuthorize("Menu", "CanEdit")]
    // [EnableRateLimiting("SaveOrderPolicy")]
    public async Task<IActionResult> SaveOrder([FromForm] SaveOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Item marked as favourite" });
        }
        var token = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        var data = model;
        var isOrderSaved = await _orderAppMenuService.SaveOrder(model, int.Parse(userId));
        if (isOrderSaved)
        {
            await _kotNotifier.NotifyKotUpdates();
            return Json(new { success = true, message = "Order Saved Successfully." });
        }
        else
            return Json(new { success = true, message = "Some Error Occurred in Save Order please try again." });
    }

    [HttpPost]
    [CustomAuthorize("Menu", "CanEdit")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var token = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        var OrderCanceled = await _orderAppMenuService.CancelOrder(orderId, int.Parse(userId));
        if (OrderCanceled == -1)
        {
            return Json(new { success = false, message = "Some Error Occurred while Cancel the Order, please try again." });
        }
        else if (OrderCanceled == 0)
        {
            return Json(new { success = false, message = "Sorry You can't cancel the order,Your ordered Item is ready." });
        }
        await _kotNotifier.NotifyKotUpdates();
        TempData["ToastrMessage"] = "Order Canceled Successfully.";
        TempData["ToastrType"] = "success";
        return Json(new { success = true, message = "Order Canceled Successfully." });
    }

    [HttpPost]
    [CustomAuthorize("Menu", "CanEdit")]
    public async Task<IActionResult> CompleteOrder(int id)
    {
        var token = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);

        int response = await _orderAppMenuService.CompleteOrder(id, int.Parse(userId));
        if (response == 0)
            return Json(new { success = false, message = "All items must be served before completing the order" });
        else if (response == -1)
            return Json(new { success = false, message = "Error while completing the order" });

        await _kotNotifier.NotifyKotUpdates();
        TempData["ToastrMessage"] = "Order completed Successfully.";
        TempData["ToastrType"] = "success";
        return Json(new { success = true, message = "Order completed successfully" });
    }

    [HttpPost]
    [CustomAuthorize("Menu", "CanEdit")]
    public async Task<IActionResult> SaveCustomerFeedBack(int orderId, short? food, short? service, short? ambience, string? comment)
    {
        var token = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        bool response = await _orderAppMenuService.SaveCustomerFeedBack(orderId, food, service, ambience, comment, int.Parse(userId));
        if (response)
            return Json(new { success = true, message = "Thank you for your review!" });
        else
            return Json(new { success = false, message = "Error while add review." });
    }

    [HttpPost]
    public async Task<IActionResult> CheckItemIsReady(int id)
    {
        var token = Request.Cookies["Token"];
        var (email, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        bool isItemReady = await _orderAppMenuService.CheckItemIsReady(id, int.Parse(userId));

        if (isItemReady)
            return Json(new { success = false, message = "The item is already ready and can not be deleted." });
        else
            return Json(new { success = true, message = "The item has been successfully deleted from your order." });
    }

    [HttpPost]
    [CustomAuthorize("Menu", "CanDelete")]
    public async Task<IActionResult> CheckReadyItemsQuantity(int id, int quantity)
    {
        bool isItemReady = await _orderAppMenuService.CheckReadyItemsQuantity(id, quantity);

        if (isItemReady)
            return Json(new { success = true, message = "Decrement Successfully" });
        else
            return Json(new { success = false, message = "You can't Remove Ready Item." });
    }
}