using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface IOrderService
{
    Task<(List<OrderViewModel> list, int count)> GetAllOrders(string searchString, int pageIndex, int pageSize, bool isAsc, DateOnly? fromDate, DateOnly? toDate, string sortColumn, int status, string dateRange);

    public Task<OrderDetailsViewModel> OrderDetails(int id);
    public Task<FileContentResult> ExportOrdersExcel(string searchString, int pageIndex, int pageSize, bool isAsc, DateOnly? fromDate, DateOnly? toDate, string sortColumn = "", int status = 0, string dateRange = "AllTime");

    public Task<MenuOrderDetailsViewModel> GetMenuOrderDetails(int id);

}
