using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class CountryRepository : ICountryRepository
{
    private readonly PizzashopContext _context;

    public CountryRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<Country> GetCountryByIdAsync(int countryId)
    {
        return await _context.Countries.FirstOrDefaultAsync(c => c.Id == countryId);
    }

    public async Task<List<Country>> GetAllCountriesAsync()
    {
        return await _context.Countries.OrderBy(c => c.Name).ToListAsync();
    }

}
