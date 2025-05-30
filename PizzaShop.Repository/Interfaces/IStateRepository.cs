using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface IStateRepository
{
    Task<State> GetStateByIdAsync(int? stateId);
    Task<List<State>> GetStatesByCountryIdAsync(int? countryId);
}