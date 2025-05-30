using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface ITaxAndFeesRepository
{
    public Task<bool> AddTax(TaxesAndFee tax);
    public Task<List<TaxesAndFee>> GetAllTaxes();
    public Task<TaxesAndFee> GetTaxById(int id);
    public Task<TaxesAndFee> GetTaxByName(string name);
    public Task<TaxesAndFee> IsTaxExist(string name, int id);
    public Task<bool> UpdateTax(TaxesAndFee tax);

}
