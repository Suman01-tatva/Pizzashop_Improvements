using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface IPaymentRepository
{
    public Task<bool> AddPayment(Payment payment);
}
