namespace PizzaShop.Service.Interfaces;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

public interface IAuthService
{
    Task<User?> AuthenticateUser(string email, string password);
    Task<User?> GetUser(string email);
    Task<string> ResetPassword(ResetPasswordModel model, string email);
    Task SendForgotPasswordEmailAsync(string email, string resetLink, string filePath);
    public Task<bool> SetResetPasswordToken(int userId, string token);
    public Task<string> GetResetPasswordToken(string userMail);
}
