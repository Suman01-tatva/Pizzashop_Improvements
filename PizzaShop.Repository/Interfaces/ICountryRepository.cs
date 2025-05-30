using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface ICountryRepository
{
    Task<Country> GetCountryByIdAsync(int countryId);
    Task<List<Country>> GetAllCountriesAsync();

}
