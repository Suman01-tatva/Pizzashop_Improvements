using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface ITableOrderRepository
{
    public Task<bool> CreateNewTableOrders(List<TableOrderMapping> tableOrders);

}
