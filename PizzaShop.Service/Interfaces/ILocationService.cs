using PizzaShop.Entity.Data;

namespace PizzaShop.Service.Interfaces;

public interface ILocationService
{
    Task<IEnumerable<Country>> GetAllCountriesAsync();
    Task<IEnumerable<State>> GetAllStatesAsync(string countryId);
    Task<IEnumerable<City>> GetAllCitiesAsync(string stateId);
}



