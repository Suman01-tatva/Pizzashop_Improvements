using Microsoft.AspNetCore.Mvc;

namespace PizzaShop.Web.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pizzashop.Service.Implementations;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly ITokenDataService _tokenDataService;
    private readonly IRolePermissionService _rolePermissionService;
    public CustomerController(ICustomerService customerService, ITokenDataService tokenDataService, IRolePermissionService rolePermissionService)
    {
        _customerService = customerService;
        _tokenDataService = tokenDataService;
        _rolePermissionService = rolePermissionService;
    }

    [CustomAuthorize("Customers", "CanView")]
    public async Task<IActionResult> Customers(string searchString = "", string sortOrder = "", int pageIndex = 1, int pageSize = 5, string dateRange = "AllTime", DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        Console.WriteLine("Role Id", roleId);
        var permission = _rolePermissionService.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        var customers = await _customerService.GetCustomerList(searchString, sortOrder, pageIndex, pageSize, dateRange, fromDate, toDate);
        ViewData["nameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : sortOrder == "name_desc" ? "name_asc" : "name_desc";
        ViewData["dateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
        ViewData["totalOrderSortParam"] = sortOrder == "totalOrder_asc" ? "totalOrder_desc" : "totalOrder_asc";
        customers.SortOrder = sortOrder;

        return View(customers);
    }

    [CustomAuthorize("Customers", "CanView")]
    public async Task<IActionResult> GetCustomers(string searchString = "", string sortOrder = "", int pageIndex = 1, int pageSize = 5, string dateRange = "AllTime", DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var customers = await _customerService.GetCustomerList(searchString, sortOrder, pageIndex, pageSize, dateRange, fromDate, toDate);
        ViewData["nameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : sortOrder == "name_desc" ? "name_asc" : "name_desc";
        ViewData["dateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
        ViewData["totalOrderSortParam"] = sortOrder == "totalOrder_asc" ? "totalOrder_desc" : "totalOrder_asc";
        customers.SortOrder = sortOrder;
        return PartialView("_CustomerList", customers);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateExcelFile(string searchString = "", string sortOrder = "", int pageIndex = 1, int pageSize = 5, string dateRange = "AllTime", DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var excelFile = await _customerService.ExportCustomersExcel(searchString, sortOrder, pageIndex, pageSize, dateRange, fromDate, toDate);

        if (excelFile == null)
        {
            return Json(new { success = false, message = "No records found to export!" });
        }
        return excelFile;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerDetails(int id)
    {
        var customerDetails = await _customerService.GetCustomerDetails(id);
        return PartialView("_CustomerDetails", customerDetails);
    }

}