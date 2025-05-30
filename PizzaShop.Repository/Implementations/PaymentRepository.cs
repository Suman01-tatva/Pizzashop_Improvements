using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class PaymentRepository : IPaymentRepository
{

    private readonly PizzashopContext _context;

    public PaymentRepository(PizzashopContext pizzashopContext)
    {
        _context = pizzashopContext;
    }

    public async Task<bool> AddPayment(Payment payment)
    {
        try
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

}
