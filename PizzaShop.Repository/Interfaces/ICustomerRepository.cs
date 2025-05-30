using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Repository.Interfaces;

public interface ICustomerRepository
{
    public Task<(List<CustomerViewModel>, int count)> GetCustomerList(string searchString, string sortOrder, int pageIndex, int pageSize, string dateRange, DateOnly? fromDate, DateOnly? toDate);

    public Task<Customer> CustomerDetails(int id);

    public Task<Customer> GetCustomerById(int id);

    public Task<Customer> UpdateCustomer(Customer customer);

    public Task<Customer> CreateCustomer(Customer customer);

    public Task<Customer> GetCustomerByEmail(string email);

    public Task<List<Customer>> SearchCustomer(string? searchString);

    public Task<List<Customer>> GetAllCustomers();
}