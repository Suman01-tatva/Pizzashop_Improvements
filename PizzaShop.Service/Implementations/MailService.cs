using System.Net;
using System.Net.Mail;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Implementations;

public class MailService : IMailService
{
    public void SendMail(string toEmail, string body, string subject)
    {
        string senderMail = "test.dotnet@etatvasoft.com";
        string senderPassword = "P}N^{z-]7Ilp";
        string host = "mail.etatvasoft.com";
        int port = 587;
        var smtpClient = new SmtpClient(host)
        {
            Port = port,
            Credentials = new NetworkCredential(senderMail, senderPassword),
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderMail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toEmail);

        smtpClient.Send(mailMessage);
    }
}
