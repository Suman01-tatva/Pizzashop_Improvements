using PizzaShop.Entity.Data;
using PizzaShop.Repository.Implementations;

namespace PizzaShop.Repository.Interfaces;

public interface IOrderedItemModifierMappingRepository
{
    public Task<bool> AddOrderItemModifiers(OrderedItemModifierMapping orderedItemModifierMapping);

}
