using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface IOrderAppTableRepository
{
    public Task<List<Section>> GetOrderAppTableDetails();

}
