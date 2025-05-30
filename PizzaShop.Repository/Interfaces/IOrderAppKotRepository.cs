using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Repository.Interfaces;

public interface IOrderAppKotRepository
{
    public Task<IEnumerable<KOTResultViewModel>> GetKotDetails(int? categoryId, int? statusId);
    public Task<bool> MarkAsPreparedItems(List<UpdateOrderItems> items, int userId);
    public Task<bool> MarkAsInProgress(List<UpdateOrderItems> items, int userId);

}
