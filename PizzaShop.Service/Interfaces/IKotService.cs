using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface IKotService
{
    public Task<KotViewModel> GetKotDetails(int? categoryId, int? statusId = 3);
    // public Task<KotTableDetailViewModel> GetOrderDetails(int orderId, int categoryId, int statusId);
    public Task<bool> MarkAsPreparedItems(List<UpdateOrderItems> items, int userId);

    public Task<bool> MarkAsInProgress(List<UpdateOrderItems> items, int userId);
}
