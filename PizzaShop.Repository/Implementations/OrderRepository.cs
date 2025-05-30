using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly PizzashopContext _context;

    public OrderRepository(PizzashopContext pizzashopContext)
    {
        _context = pizzashopContext;
    }
    public async Task<(List<Order> list, int count)> GetAllOrders(string searchString, int pageIndex, int pageSize, bool isAsc, DateOnly? fromDate, DateOnly? toDate, string sortColumn, int status, string dateRange)
    {

        var query = _context.Orders.Include(i => i.Customer).Include(i => i.Feedbacks).Include(i => i.Invoices).ThenInclude(i => i.Payments).AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.Trim().ToLower();
            query = query.Where(i => i.Customer.Name!.ToLower().Contains(searchString) || i.Id.ToString().Contains(searchString));
        }

        if (status != 0)
        {
            query = query.Where(i => i.OrderStatus.ToString()!.Contains(status.ToString()));
        }

        switch (dateRange)
        {
            case "Last7Days":
                fromDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
                break;
            case "Last30Days":
                fromDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
                break;
            case "CurrentMonth":
                var now = DateTime.Now;
                fromDate = new DateOnly(now.Year, now.Month, 1);
                toDate = new DateOnly(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                break;
            case "AllTime":
            default:
                break;
        }

        if (fromDate != null)
        {
            query = query.Where(i => i.OrderDate >= fromDate);
        }

        if (toDate != null)
        {
            query = query.Where(i => i.OrderDate <= toDate);
        }

        //Sorting
        switch (sortColumn)
        {
            case "id_asc":
                query = query.OrderBy(o => o.Id);
                break;
            case "id_desc":
                query = query.OrderByDescending(o => o.Id);
                break;

            case "date_asc":
                query = query.OrderBy(o => o.OrderDate);
                break;
            case "date_desc":
                query = query.OrderByDescending(o => o.OrderDate);
                break;

            case "cust_asc":
                query = query.OrderBy(o => o.Customer.Name).ThenBy(o => o.OrderDate);
                break;
            case "cust_desc":
                query = query.OrderByDescending(o => o.Customer.Name).ThenByDescending(o => o.OrderDate);
                break;
            case "amount_asc":
                query = query.OrderBy(o => o.TotalAmount).ThenBy(o => o.Customer.Name);
                break;
            case "amount_desc":
                query = query.OrderByDescending(o => o.TotalAmount).ThenByDescending(o => o.Customer.Name);
                break;
            default:
                query = query.OrderBy(o => o.Id);
                break;
        }

        // var orders = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return (query.ToList(), query.ToList().Count());
    }

    public async Task<Order> OrderDetails(int id)
    {
        var orderDetails = await _context.Orders.Where(o => o.Id == id)
                                    .Include(i => i.Customer)
                                    .Include(o => o.Invoices)
                                    .ThenInclude(p => p.Payments)
                                    .Include(o => o.OrderedItems.Where(i => i.IsDeleted == false))
                                    .ThenInclude(O => O.OrderedItemModifierMappings)
                                    .ThenInclude(m => m.Modifier)
                                    .Include(o => o.TableOrderMappings)
                                    .ThenInclude(o => o.Table)
                                    .ThenInclude(o => o.Section)
                                    .Include(o => o.OrderTaxMappings)
                                    .ThenInclude(o => o.Tax)
                                    .FirstOrDefaultAsync();
        return orderDetails!;

    }

    public async Task<List<Order>> GetOrderssByCustomerId(int id)
    {
        var orders = await _context.Orders.Where(t => t.CustomerId == id).ToListAsync();
        return orders!;
    }

    public async Task<bool> UpdateOrder(Order order)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("CALL update_order({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10})",
            order.Id,
            order.CustomerId,
            order.OrderNo!,
            order.TotalAmount!,
            order.Tax!,
            order.SubTotal!,
            order.PaidAmount!,
            order.Notes!,
            order.IsSgstSelected!,
            order.ModifiedBy!,
            order.OrderStatus!
            );
            // _context.Orders.Update(order);
            // await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<Order> CreateNewOrder(Order order)
    {
        try
        {
            // Call the PostgreSQL function and get the new order ID
            var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using (var cmd = new Npgsql.NpgsqlCommand("SELECT create_new_order(@customer_id, @order_status, @created_by)", conn))
            {
                cmd.Parameters.AddWithValue("customer_id", order.CustomerId);
                cmd.Parameters.AddWithValue("order_status", order.OrderStatus ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("created_by", order.CreatedBy ?? (object)DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                //typically used when your query returns a single value
                // var result = await cmd.ExecuteReaderAsync();       //used for any result set with multiple rows/columns
                order.Id = (int)result!;
            }

            return order;
        }
        catch (Exception ex)
        {
            // Log or handle exception as needed
            Console.WriteLine(ex);
            return null!;
        }
    }


    public async Task<Order> GetOrderById(int id)
    {
        var order = await _context.Orders.Where(o => o.Id == id).FirstOrDefaultAsync();
        return order!;
    }

    public async Task UpdateOrderStatus(int id, int status, int userId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(i => i.Id == id);
        if (order != null)
        {
            // order.OrderStatus = status;
            // order.ModifiedBy = userId;
            // order.ModifiedAt = DateTime.UtcNow;
            // await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlRawAsync("CALL update_order_status({0},{1},{2})", order.Id, status, userId);
        }
    }

    public async Task<List<Order>> GetOrdersForDashboard()
    {
        List<Order> orders = await _context.Orders.ToListAsync();
        return orders;
    }
}

public class IntResult
{
    public int Value { get; set; }
}