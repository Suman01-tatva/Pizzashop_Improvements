using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;


public class StateRepository : IStateRepository
{
    private readonly PizzashopContext _context;

    public StateRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<State> GetStateByIdAsync(int? stateId)
    {
        return await _context.States.FirstOrDefaultAsync(s => s.Id == stateId);
    }

    public async Task<List<State>> GetStatesByCountryIdAsync(int? countryId)
    {
        return await _context.States.Where(s => s.CountryId == countryId).OrderBy(s => s.Name).ToListAsync();
    }
}
