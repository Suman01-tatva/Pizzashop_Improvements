using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface ICityRepository
{
    Task<City> GetCityByIdAsync(int? cityId);
    Task<List<City>> GetCitiesByStateIdAsync(int? stateId);
}
