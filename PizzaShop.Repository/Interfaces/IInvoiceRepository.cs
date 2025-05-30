using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface IInvoiceRepository
{
    public Task<bool> AddInvoice(Invoice invoice, int paymentMethod, int status);
}
