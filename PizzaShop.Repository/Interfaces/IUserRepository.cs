using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);

    Task UpdateUserAsync(User user);

    IEnumerable<User> GetUserList(string searchString, string sortOrder, int pageIndex, int pageSize, int userId);
    User GetUserById(int id);
    void DeleteUser(int id);
    Task AddUserAsync(User user);
    public int GetTotalUsers(string searchString);
    public Task<User?> IsExistUser(string email, string userName);
    public Task<string> GetResetPasswordToken(string userMail);
    public Task<bool> SetResetPasswordToken(string token, int userId);
}