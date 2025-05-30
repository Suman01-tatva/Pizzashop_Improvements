using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface IOrderAppMenuService
{
    public Task<bool> AddEditOrderComments(int orderId, string comments);
    public Task<List<ItemModifierViewModel>> GetItemModifiers(int itemId);
    public Task<bool> MarkAsFavourite(int itemId);
    public Task<bool> SaveOrder(SaveOrderViewModel model, int userId);
    public Task<int> CancelOrder(int orderId, int userId);
    public Task<int> CompleteOrder(int id, int userId);
    public Task<bool> SaveCustomerFeedBack(int orderId, int? food, int? service, int? ambience, string? comment, int? userId);
    public Task<bool> CheckItemIsReady(int orderItemId, int userId);
    public Task<bool> CheckReadyItemsQuantity(int orderItemId, int quantity);
}
