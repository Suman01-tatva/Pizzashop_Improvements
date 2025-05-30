using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class OrderedItemModifierMappingRepoSitory : IOrderedItemModifierMappingRepository
{
    private readonly PizzashopContext _context;

    public OrderedItemModifierMappingRepoSitory(PizzashopContext pizzashopContext)
    {
        _context = pizzashopContext;
    }

    public async Task<bool> AddOrderItemModifiers(OrderedItemModifierMapping orderedItemModifierMapping)
    {
        try
        {
            var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using (var cmd = new Npgsql.NpgsqlCommand(
                @"CALL add_order_item_modifier(
                @order_item_id, 
                @modifier_id, 
        @quantity_of_modifier, 
        @rate_of_modifier, 
        @total_amount, 
        @is_deleted, 
        @created_by)", conn))
            {
                cmd.Parameters.Add("order_item_id", NpgsqlTypes.NpgsqlDbType.Integer).Value = orderedItemModifierMapping.OrderItemId;
                cmd.Parameters.Add("modifier_id", NpgsqlTypes.NpgsqlDbType.Integer).Value = orderedItemModifierMapping.ModifierId;
                cmd.Parameters.Add("quantity_of_modifier", NpgsqlTypes.NpgsqlDbType.Integer).Value = orderedItemModifierMapping.QuantityOfModifier;
                cmd.Parameters.Add("rate_of_modifier", NpgsqlTypes.NpgsqlDbType.Numeric).Value = orderedItemModifierMapping.RateOfModifier;
                cmd.Parameters.Add("total_amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = orderedItemModifierMapping.TotalAmount;
                cmd.Parameters.Add("is_deleted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = orderedItemModifierMapping.IsDeleted;
                cmd.Parameters.Add("created_by", NpgsqlTypes.NpgsqlDbType.Integer).Value = orderedItemModifierMapping.CreatedBy;

                await cmd.ExecuteNonQueryAsync();
            }
            return true;
        }
        catch (Exception e)
        {
            // Optionally log e for debugging
            return false;
        }
    }
}
