using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface IOrderTaxMappingRepository
{
    public Task<bool> AddOrderTaxMapping(OrderTaxMapping orderTaxMapping);
    public Task<bool> EditOrderTaxMapping(int id, decimal taxValue, int userId);
}
