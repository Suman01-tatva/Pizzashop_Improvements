using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class AuthRepository : IAuthRepository
{
    private readonly PizzashopContext _context;

    public AuthRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Role?> CheckRole(string role)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == role);
    }
}