namespace PizzaShop.Repository.Implementations;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using Repository.Interfaces;

public class TaxAndFeesRepository : ITaxAndFeesRepository
{
    private readonly PizzashopContext _context;

    public TaxAndFeesRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<bool> AddTax(TaxesAndFee tax)
    {
        _context.Set<TaxesAndFee>().Add(tax);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<TaxesAndFee>> GetAllTaxes()
    {
        return await _context.TaxesAndFees.Where(tf => tf.IsDeleted != true).OrderBy(tf => tf.Name).ToListAsync();
    }

    public async Task<TaxesAndFee> GetTaxById(int id)
    {
        return await _context.TaxesAndFees.Where(tf => tf.Id == id).FirstOrDefaultAsync();
    }

    public async Task<TaxesAndFee> GetTaxByName(string name)
    {
        return await _context.TaxesAndFees
            .Where(tf => tf.Name.ToLower() == name.ToLower() && tf.IsDeleted == false)
            .FirstOrDefaultAsync();
    }

    public async Task<TaxesAndFee> IsTaxExist(string name, int id)
    {
        return await _context.TaxesAndFees
            .Where(tf => tf.Name.ToLower() == name.ToLower() && tf.Id != id && tf.IsDeleted == false)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateTax(TaxesAndFee tax)
    {
        _context.Set<TaxesAndFee>().Update(tax);
        return await _context.SaveChangesAsync() > 0;
    }
}