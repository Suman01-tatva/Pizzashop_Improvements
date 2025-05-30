using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PizzaShop.Entity.Constants;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Web.Controllers;

public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [CustomAuthorize("Orders", "CanView")]
    [HttpGet]
    public async Task<IActionResult> Orders(string searchString = "", int pageIndex = 1, int pageSize = 5, bool isAsc = true, DateOnly? fromDate = null, DateOnly? toDate = null, string sortColumn = "", int status = 0, string dateRange = "AllTime")
    {
        var (orders, count) = await _orderService.GetAllOrders(searchString, pageIndex, pageSize, isAsc, fromDate, toDate, sortColumn, status, dateRange);
        orders = orders.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var pageData = new OrderPageViewModel
        {
            Orders = orders,
            PageIndex = pageIndex,
            PageSize = pageSize,
            IsAsc = isAsc,
            FromDate = fromDate,
            ToDate = toDate,
            Status = status,
            sortColumn = sortColumn,
            SearchString = searchString,
            TotalPage = (int)Math.Ceiling(count / (double)pageSize),
            TotalOrders = count,
            DateRange = dateRange,
        };

        return View(pageData);
    }

    [CustomAuthorize("Orders", "CanView")]
    [HttpGet]
    public async Task<IActionResult> GetOrders(string searchString, int pageIndex, int pageSize, bool isAsc, DateOnly? fromDate, DateOnly? toDate, string sortColumn = "", int status = 0, string dateRange = "AllTime")
    {
        var (orders, count) = await _orderService.GetAllOrders(searchString, pageIndex, pageSize, isAsc, fromDate, toDate, sortColumn, status, dateRange);

        ViewData["OrderIdSortParam"] = string.IsNullOrEmpty(sortColumn) ? "id_desc" : sortColumn == "id_desc" ? "id_asc" : "id_desc";
        ViewData["DateSortParam"] = sortColumn == "date_asc" ? "date_desc" : "date_asc";
        ViewData["CustomerSortParam"] = sortColumn == "cust_asc" ? "cust_desc" : "cust_asc";
        ViewData["TotalAmountSortParam"] = sortColumn == "amount_asc" ? "amount_desc" : "amount_asc";
        orders = orders.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var pageData = new OrderPageViewModel
        {
            Orders = orders,
            PageIndex = pageIndex,
            PageSize = pageSize,
            IsAsc = isAsc,
            FromDate = fromDate,
            ToDate = toDate,
            Status = status,
            sortColumn = sortColumn,
            SearchString = searchString,
            TotalPage = (int)Math.Ceiling(count / (double)pageSize),
            TotalOrders = count,
            DateRange = dateRange,
        };
        if (orders == null)
        {
            return PartialView("_OrderTablePartial", new OrderPageViewModel());
        }

        return PartialView("_OrderTablePartial", pageData);
    }

    [HttpGet]
    public async Task<OrderPageViewModel> GetOrderPageDate(string searchString, int pageIndex, int pageSize, bool isAsc, DateOnly? fromDate, DateOnly? toDate, string sortColumn = "", int status = 0, string dateRange = "AllTime")
    {
        var (orders, count) = await _orderService.GetAllOrders(searchString, pageIndex, pageSize, isAsc, fromDate, toDate, sortColumn, status, dateRange);
        ViewData["OrderIdSortParam"] = string.IsNullOrEmpty(sortColumn) ? "id_desc" : sortColumn == "id_desc" ? "id_asc" : "id_desc";
        ViewData["DateSortParam"] = sortColumn == "date_asc" ? "date_desc" : "date_asc";
        ViewData["CustomerSortParam"] = sortColumn == "cust_asc" ? "cust_desc" : "cust_asc";
        ViewData["TotalAmountSortParam"] = sortColumn == "amount_asc" ? "amount_desc" : "amount_asc";
        orders = orders.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var pageData = new OrderPageViewModel
        {
            Orders = orders,
            PageIndex = pageIndex,
            PageSize = pageSize,
            IsAsc = isAsc,
            FromDate = fromDate,
            ToDate = toDate,
            Status = status,
            sortColumn = sortColumn,
            SearchString = searchString,
            TotalPage = (int)Math.Ceiling(count / (double)pageSize),
            TotalOrders = count,
            DateRange = dateRange,
        };

        return pageData;
    }

    public async Task<ActionResult> ExportToExcel(string searchString, int pageIndex, int pageSize, bool isAsc, DateOnly? fromDate, DateOnly? toDate, string sortColumn = "", int status = 0, string dateRange = "AllTime")
    {
        var ExportsOrders = await _orderService.ExportOrdersExcel(searchString, pageIndex, pageSize, isAsc, fromDate, toDate, sortColumn, status, dateRange);
        if (ExportsOrders == null)
        {
            return Json(new { success = false, message = "No records found to export!" });
        }
        return ExportsOrders;
    }

    [CustomAuthorize("Orders", "CanView")]
    [HttpGet]
    public async Task<IActionResult> OrderDetails(int id)
    {
        if (id == 0)
        {
            TempData["ToastrMessage"] = "Invalid Order ID";
            TempData["ToastrType"] = "error";
            return RedirectToAction("Orders");
        }

        var orderDetails = await _orderService.OrderDetails(id);
        if (orderDetails == null)
        {
            TempData["ToastrMessage"] = "Order Records Not Found";
            TempData["ToastrType"] = "error";
            return RedirectToAction("Orders");
        }

        return View(orderDetails);
    }

    [CustomAuthorize("Orders", "CanView")]
    public async Task<IActionResult> InvoiceDetail(int id)
    {
        OrderDetailsViewModel orderDetail = await _orderService.OrderDetails(id);
        return PartialView("_InvoiceDetails", orderDetail);
    }
}