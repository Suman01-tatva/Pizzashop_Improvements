using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly PizzashopContext _context;

    public RoleRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<Role> GetRoleByIdAsync(int roleId)
    {
        return await _context.Roles.FirstOrDefaultAsync(a => a.Id == roleId);
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.OrderBy(o => o.Name).ToListAsync();
    }
}