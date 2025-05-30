using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface IAuthRepository
{
    Task<User?> GetUserByEmail(string email);
    Task<Role?> CheckRole(string role);
}