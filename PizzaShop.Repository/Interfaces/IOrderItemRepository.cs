using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface IOrderItemRepository
{
    public Task<OrderedItem> GetOrderedItemById(int orderItemId);
    public Task<List<OrderedItem>> GetOrderedItemByOrderId(int orderId);
    public Task<bool> DeleteOrderItem(List<int> itemIds, int userId);
    public Task<OrderedItem> AddOrderItems(OrderedItem orderedItems);
    public Task<OrderedItem> EditOrderItems(OrderedItem orderedItems);
    public Task<List<OrderedItem>> GetAllOrderedItems();

}
