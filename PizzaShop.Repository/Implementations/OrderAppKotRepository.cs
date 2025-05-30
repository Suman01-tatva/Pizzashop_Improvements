using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using Dapper;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using System.Data;

namespace PizzaShop.Repository.Implementations;

public class OrderAppKotRepository : IOrderAppKotRepository
{
    private readonly PizzashopContext _context;

    public OrderAppKotRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KOTResultViewModel>> GetKotDetails(int? categoryId, int? statusId)
    {
        try
        {
            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            var sql = "SELECT * FROM get_kot_details(@categoryid, @status_id)";
            var result = await connection.QueryAsync<KOTResultViewModel>(
                sql,
                new { categoryid = categoryId, status_id = statusId }
            );

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null!;
        }
    }

    public async Task<bool> MarkAsPreparedItems(List<UpdateOrderItems> items, int userId)
    {
        try
        {
            foreach (var i in items)
            {
                var item = _context.OrderedItems.FirstOrDefault(item => item.Id == i.Id);
                item!.ReadyItemQuantity += i.Quantity;
                if (item.Quantity == item.ReadyItemQuantity)
                    item!.OrderStatus = 2;
                item.ModifiedBy = userId;
                //Stored Procedures
                // await using var dataSource = NpgsqlDataSource.Create("Host=localhost; Database=pizzashop;Username=postgres; password=Tatva@123");
                // await using var cmd = dataSource.CreateCommand("CALL MarkAsPreparedItems($1,$2,$3,$4)");

                // cmd.Parameters.AddWithValue(item.Id);
                // cmd.Parameters.AddWithValue(item.ReadyItemQuantity!);
                // cmd.Parameters.AddWithValue(item.OrderStatus!);
                // cmd.Parameters.AddWithValue(item.ModifiedBy);
                // await using var reader = await cmd.ExecuteReaderAsync();

                await _context.Database.ExecuteSqlRawAsync("CALL MarkAsPreparedItems({0},{1},{2},{3})", item.Id, item.ReadyItemQuantity!, item.OrderStatus!, item.ModifiedBy);
                // _context.OrderedItems.Update(item);
            }
            // await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> MarkAsInProgress(List<UpdateOrderItems> items, int userId)
    {
        try
        {
            foreach (var i in items)
            {
                var item = _context.OrderedItems.FirstOrDefault(item => item.Id == i.Id);
                if (item!.ReadyItemQuantity == item.Quantity)
                    item.OrderStatus = 3;

                item!.ReadyItemQuantity -= i.Quantity;
                if (item.ReadyItemQuantity == 0)
                    item!.OrderStatus = 3;
                item.ModifiedBy = userId;
                //Stored Procedures
                // await using var dataSource = NpgsqlDataSource.Create("Host=localhost; Database=pizzashop;Username=postgres; password=Tatva@123");
                // await using var cmd = dataSource.CreateCommand("CALL MarkAsPreparedItems($1,$2,$3,$4)");

                // cmd.Parameters.AddWithValue(item.Id);
                // cmd.Parameters.AddWithValue(item.ReadyItemQuantity!);
                // cmd.Parameters.AddWithValue(item.OrderStatus!);
                // cmd.Parameters.AddWithValue(item.ModifiedBy);
                // await using var reader = await cmd.ExecuteReaderAsync();

                await _context.Database.ExecuteSqlRawAsync("CALL MarkAsPreparedItems({0},{1},{2},{3})", item.Id, item.ReadyItemQuantity!, item.OrderStatus!, item.ModifiedBy);
                // _context.OrderedItems.Update(item);
            }
            // await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

}
