using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface ICustomerService
{
    public Task<CustomerPageViewModel> GetCustomerList(string searchString, string sortOrder, int pageIndex, int pageSize, string dateRange, DateOnly? fromDate, DateOnly? toDate);

    public Task<FileContentResult> ExportCustomersExcel(string searchString, string sortOrder, int pageIndex, int pageSize, string dateRange, DateOnly? fromDate, DateOnly? toDate);

    public Task<CustomerDetailsViewModel> GetCustomerDetails(int id);

    public Task<bool> UpdateCustomerDetails(int id, string name, string MobileNumber, int userId);

}
