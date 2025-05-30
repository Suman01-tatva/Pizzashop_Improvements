using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class TableOrderRepository : ITableOrderRepository
{
    private readonly PizzashopContext _context;

    public TableOrderRepository(PizzashopContext pizzashopContext)
    {
        _context = pizzashopContext;
    }

    public async Task<bool> CreateNewTableOrders(List<TableOrderMapping> tableOrders)
    {
        try
        {
            foreach (var t in tableOrders)
            {
                await _context.Database.ExecuteSqlRawAsync("CALL create_table_order_mapping({0},{1},{2},{3})", t.OrderId!, t.TableId!, t.NoOfPeople!, t.CreatedBy!);
                // await _context.TableOrderMappings.AddAsync(t);
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}
