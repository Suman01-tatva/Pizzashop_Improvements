using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly PizzashopContext _context;

    public OrderItemRepository(PizzashopContext pizzashopContext)
    {
        _context = pizzashopContext;
    }

    public async Task<OrderedItem> GetOrderedItemById(int orderItemId)
    {
        try
        {
            var orderedItems = await _context.OrderedItems.Where(o => o.Id == orderItemId).FirstOrDefaultAsync();
            return orderedItems!;
        }
        catch (Exception e)
        {
            return null!;
        }
    }

    public async Task<List<OrderedItem>> GetOrderedItemByOrderId(int orderId)
    {
        try
        {
            var orderedItems = await _context.OrderedItems.Where(o => o.OrderId == orderId && o.IsDeleted == false).ToListAsync();
            return orderedItems;
        }
        catch (Exception e)
        {
            return null!;
        }
    }

    public async Task<List<OrderedItem>> GetAllOrderedItems()
    {
        try
        {
            var orderedItems = await _context.OrderedItems.Where(o => o.IsDeleted == false)
                                                        .Include(o => o.Order)
                                                        .ThenInclude(m => m.OrderedItems)
                                                        .ThenInclude(m => m.MenuItem)
                                                        .ToListAsync();
            return orderedItems;
        }
        catch (Exception e)
        {
            return null!;
        }
    }

    public async Task<OrderedItem> AddOrderItems(OrderedItem orderedItems)
    {
        try
        {
            var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using (var cmd = new Npgsql.NpgsqlCommand(
                @"SELECT add_ordered_item(
                @order_id, @name, @menu_item_id, @quantity, @rate, @amount, @total_amount, 
                @tax, @total_modifier_amount, @instruction, @ready_item_quantity, 
                @is_deleted,@created_by, @order_status)", conn))
            {
                cmd.Parameters.Add("order_id", NpgsqlDbType.Integer).Value = orderedItems.OrderId;
                cmd.Parameters.Add("name", NpgsqlDbType.Text).Value = (object?)orderedItems.Name ?? DBNull.Value;
                cmd.Parameters.Add("menu_item_id", NpgsqlDbType.Integer).Value = orderedItems.MenuItemId;
                cmd.Parameters.Add("quantity", NpgsqlDbType.Integer).Value = orderedItems.Quantity;
                cmd.Parameters.Add("rate", NpgsqlDbType.Numeric).Value = (object?)orderedItems.Rate ?? DBNull.Value;
                cmd.Parameters.Add("amount", NpgsqlDbType.Numeric).Value = (object?)orderedItems.Amount ?? DBNull.Value;
                cmd.Parameters.Add("total_amount", NpgsqlDbType.Numeric).Value = (object?)orderedItems.TotalAmount ?? DBNull.Value;
                cmd.Parameters.Add("tax", NpgsqlDbType.Numeric).Value = (object?)orderedItems.Tax ?? DBNull.Value;
                cmd.Parameters.Add("total_modifier_amount", NpgsqlDbType.Numeric).Value = (object?)orderedItems.TotalModifierAmount ?? DBNull.Value;
                cmd.Parameters.Add("instruction", NpgsqlDbType.Text).Value = (object?)orderedItems.Instruction ?? DBNull.Value;
                cmd.Parameters.Add("ready_item_quantity", NpgsqlDbType.Integer).Value = (object?)orderedItems.ReadyItemQuantity ?? DBNull.Value;
                cmd.Parameters.Add("is_deleted", NpgsqlDbType.Boolean).Value = (object?)orderedItems.IsDeleted ?? DBNull.Value;
                cmd.Parameters.Add("created_by", NpgsqlDbType.Integer).Value = orderedItems.CreatedBy;
                cmd.Parameters.Add("order_status", NpgsqlDbType.Integer).Value = (object?)orderedItems.OrderStatus ?? DBNull.Value;

                var result = await cmd.ExecuteScalarAsync();
                orderedItems.Id = Convert.ToInt32(result!);
            }
            return orderedItems;
        }
        catch (Exception e)
        {
            return null!;
        }
    }

    public async Task<OrderedItem> EditOrderItems(OrderedItem orderedItems)
    {
        try
        {
            var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using (var cmd = new Npgsql.NpgsqlCommand(
                "SELECT update_ordered_item(" +
                "@id, @quantity, @amount, @total_amount, @tax, @total_modifier_amount, " +
                "@instruction, @ready_item_quantity,@modified_by, @order_status)", conn))
            {
                cmd.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Integer).Value = orderedItems.Id;
                cmd.Parameters.Add("quantity", NpgsqlTypes.NpgsqlDbType.Integer).Value = orderedItems.Quantity;
                cmd.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (object?)orderedItems.Amount ?? DBNull.Value;
                cmd.Parameters.Add("total_amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (object?)orderedItems.TotalAmount ?? DBNull.Value;
                cmd.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (object?)orderedItems.Tax ?? DBNull.Value;
                cmd.Parameters.Add("total_modifier_amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (object?)orderedItems.TotalModifierAmount ?? DBNull.Value;
                cmd.Parameters.Add("instruction", NpgsqlTypes.NpgsqlDbType.Text).Value = (object?)orderedItems.Instruction ?? DBNull.Value;
                cmd.Parameters.Add("ready_item_quantity", NpgsqlTypes.NpgsqlDbType.Integer).Value = (object?)orderedItems.ReadyItemQuantity ?? DBNull.Value;
                cmd.Parameters.Add("modified_by", NpgsqlTypes.NpgsqlDbType.Integer).Value = (object?)orderedItems.ModifiedBy ?? DBNull.Value;
                cmd.Parameters.Add("order_status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (object?)orderedItems.OrderStatus ?? DBNull.Value;

                var result = await cmd.ExecuteScalarAsync();
                orderedItems.Id = Convert.ToInt32(result!);
            }
            return orderedItems;
        }
        catch (Exception e)
        {
            return null!;
        }
    }
    public async Task<bool> DeleteOrderItem(List<int> itemIds, int userId)
    {
        try
        {
            // foreach (var itemId in itemIds)
            // {
            //     var orderItem = _context.OrderedItems.Where(i => i.Id == itemId).FirstOrDefault();
            //     orderItem!.IsDeleted = true;
            //     orderItem.ModifiedBy = userId;
            //     orderItem.ModifiedAt = DateTime.UtcNow;
            //     _context.OrderedItems.Update(orderItem);
            // }
            // await _context.SaveChangesAsync();
            var itemIdsArray = itemIds.ToArray();
            var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using (var cmd = new Npgsql.NpgsqlCommand(
                "SELECT delete_order_items(@p_item_ids, @p_user_id);", conn))
            {
                cmd.Parameters.Add("p_item_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = itemIdsArray;
                cmd.Parameters.Add("p_user_id", NpgsqlDbType.Integer).Value = userId;

                var result = await cmd.ExecuteScalarAsync();
                if ((int)result! > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            return false;
        }
    }
}
