using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class CityRepository : ICityRepository
{
    private readonly PizzashopContext _context;

    public CityRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<City> GetCityByIdAsync(int? cityId)
    {
        return await _context.Cities.FirstOrDefaultAsync(c => c.Id == cityId);
    }

    public async Task<List<City>> GetCitiesByStateIdAsync(int? stateId)
    {
        return await _context.Cities.Where(c => c.StateId == stateId).OrderBy(c => c.Name).ToListAsync();
    }
}
