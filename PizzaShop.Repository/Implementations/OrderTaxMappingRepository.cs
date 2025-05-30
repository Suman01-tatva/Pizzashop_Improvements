using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class OrderTaxMappingRepository : IOrderTaxMappingRepository
{
    private readonly PizzashopContext _context;

    public OrderTaxMappingRepository(PizzashopContext pizzashopContext)
    {
        _context = pizzashopContext;
    }

    public async Task<bool> AddOrderTaxMapping(OrderTaxMapping orderTaxMapping)
    {
        try
        {
            // _context.OrderTaxMappings.Add(orderTaxMapping);
            // await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlRawAsync(
                "CALL add_order_tax_mapping({0}::integer,{1}::integer,{2}::numeric,{3}::boolean,{4}::integer)",
                orderTaxMapping.OrderId,
                orderTaxMapping.TaxId,
                orderTaxMapping.TaxValue!,
                orderTaxMapping.IsDeleted!,
                orderTaxMapping.CreatedBy
            );
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<bool> EditOrderTaxMapping(int id, decimal taxValue, int userId)
    {
        try
        {
            // var existingOrderTaxMapping = await _context.OrderTaxMappings.Where(o => o.Id == id).FirstOrDefaultAsync();
            // existingOrderTaxMapping!.TaxValue = taxValue;
            // existingOrderTaxMapping.ModifiedBy = userId;
            // existingOrderTaxMapping.ModifiedAt = DateTime.UtcNow;
            // _context.OrderTaxMappings.Update(existingOrderTaxMapping);
            // await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlRawAsync(
                "CALL edit_order_tax_mapping({0}::integer,{1}::numeric,{2}::integer)",
               id,
               taxValue,
               userId
            );
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}
