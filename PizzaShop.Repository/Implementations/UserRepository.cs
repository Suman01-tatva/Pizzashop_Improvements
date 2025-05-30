using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class UserRepostory : IUserRepository
{
    private readonly PizzashopContext _context;

    public UserRepostory(PizzashopContext context)
    {
        _context = context;
    }
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> IsExistUser(string email, string userName)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Email == email || u.Username == userName);
    }
    public async Task UpdateUserAsync(User user)
    {
        if (user == null) return;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public IEnumerable<User> GetUserList(string searchString, string sortOrder, int pageIndex, int pageSize, int userId)
    {
        try
        {
            var userQuery = _context.Users.Where(u => u.IsDeleted == false && u.Id != userId);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();

                userQuery = userQuery.Where(n =>
                    (n.FirstName + " " + n.LastName).ToLower().Contains(searchString) ||
                    n.FirstName!.ToLower().Contains(searchString) ||
                    n.LastName!.ToLower().Contains(searchString) ||
                    n.Email!.ToLower().Contains(searchString)
                );
            }

            switch (sortOrder)
            {
                case "username_asc":
                    userQuery = userQuery.OrderBy(u => u.FirstName);
                    break;

                case "username_desc":
                    userQuery = userQuery.OrderByDescending(u => u.FirstName);
                    break;

                case "role_asc":
                    userQuery = userQuery.OrderBy(u => u.Role.Name).ThenBy(u => u.FirstName);
                    break;

                case "role_desc":
                    userQuery = userQuery.OrderByDescending(u => u.Role.Name).ThenBy(u => u.FirstName);
                    break;

                default:
                    userQuery = userQuery.OrderBy(u => u.Id);
                    break;
            }

            return userQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        catch (Exception)
        {
            // In case of any exception, return the full list
            var allUsersQuery = _context.Users.Where(u => u.IsDeleted == false);

            switch (sortOrder)
            {
                case "username_asc":
                    allUsersQuery = allUsersQuery.OrderBy(u => u.FirstName);
                    break;

                case "username_desc":
                    allUsersQuery = allUsersQuery.OrderByDescending(u => u.FirstName);
                    break;

                case "role_asc":
                    allUsersQuery = allUsersQuery.OrderBy(u => u.Role.Name);
                    break;

                case "role_desc":
                    allUsersQuery = allUsersQuery.OrderByDescending(u => u.Role.Name);
                    break;

                default:
                    allUsersQuery = allUsersQuery.OrderBy(u => u.Id);
                    break;
            }

            return allUsersQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }

    public int GetTotalUsers(string searchString)
    {
        var userQuery = _context.Users.Where(u => u.IsDeleted == false);
        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.Trim().ToLower();

            userQuery = userQuery.Where(n =>
                (n.FirstName + " " + n.LastName).ToLower().Contains(searchString) ||
                n.FirstName!.ToLower().Contains(searchString) ||
                n.LastName!.ToLower().Contains(searchString) ||
                n.Email!.ToLower().Contains(searchString)
            );
        }
        return userQuery.ToList().Count();
    }

    public User GetUserById(int id)
    {
        return _context.Users.Find(id);
    }

    public void DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user != null)
        {
            user.IsDeleted = true;
            _context.SaveChanges();
        }
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> SetResetPasswordToken(string token, int userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(i => i.Id == userId);
            user!.Token = token;
            _context.Users.Update(user);
            _context.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<string> GetResetPasswordToken(string userMail)
    {
        try
        {
            var user = await _context.Users.FindAsync(userMail);
            return user?.Token!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null!;
        }
    }
}