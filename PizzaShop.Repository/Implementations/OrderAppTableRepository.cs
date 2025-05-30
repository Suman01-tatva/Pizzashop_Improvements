using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class OrderAppTableRepository : IOrderAppTableRepository
{

    private readonly PizzashopContext _context;

    public OrderAppTableRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<List<Section>> GetOrderAppTableDetails()
    {
        var section = await _context.Sections.Where(s => s.IsDeleted == false)
                                            .Include(s => s.Tables)
                                            .ThenInclude(t => t.TableOrderMappings)
                                            .ThenInclude(t => t.Order)
                                            .ToListAsync();
        return section;
    }

}
