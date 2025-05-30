using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Repository.Interfaces;

public interface IOrderRepository
{
    public Task<(List<Order> list, int count)> GetAllOrders(string searchString, int pageIndex, int pageSize, bool isAsc, DateOnly? fromDate, DateOnly? toDate, string sortColumn, int status, string dateRange);

    public Task<Order> OrderDetails(int id);

    public Task<List<Order>> GetOrderssByCustomerId(int id);

    public Task<bool> UpdateOrder(Order order);

    public Task<Order> CreateNewOrder(Order order);

    public Task<Order> GetOrderById(int id);

    public Task UpdateOrderStatus(int id, int status, int userId);

    public Task<List<Order>> GetOrdersForDashboard();
}
