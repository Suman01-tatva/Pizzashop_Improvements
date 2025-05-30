namespace PizzaShop.Service.Interfaces;

public interface IMailService
{
    public void SendMail(string toEmail, string body, string subject);
}
