using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class UnitRepository : IUnitRepository
{
    private readonly PizzashopContext _context;

    public UnitRepository(PizzashopContext context)
    {
        _context = context;
    }

    public List<Unit> GetAllUnits()
    {
        var units = _context.Units.OrderBy(u => u.Name).ToList();
        return units;
    }

}
