using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface ITaxAndFeesService
{
    public Task<bool> AddTax(TaxAndFeesViewModel model);
    public Task<List<TaxesAndFee>> GetAllTaxes();
    public Task<TaxAndFeesViewModel> GetTaxById(int id);
    public Task<bool> UpdateTax(TaxAndFeesViewModel model);
    public Task<bool> DeleteTax(int id);

}
