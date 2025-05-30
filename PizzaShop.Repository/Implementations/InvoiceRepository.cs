using Microsoft.EntityFrameworkCore;
using Npgsql;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly PizzashopContext _context;

    public InvoiceRepository(PizzashopContext pizzashopContext)
    {
        _context = pizzashopContext;
    }

    public async Task<bool> AddInvoice(Invoice invoice, int paymentMethod, int paymentStatus)
    {
        try
        {
            // _context.Invoices.Add(invoice);
            // await _context.SaveChangesAsync();
            // return invoice;
            var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using (var cmd = new Npgsql.NpgsqlCommand(
                "SELECT create_invoice_and_payment(@OrderId, @TotalAmount, @CreatedBy, @PaymentMethod, @PaymentStatus);", conn
            ))
            {
                cmd.Parameters.Add("OrderId", NpgsqlTypes.NpgsqlDbType.Integer).Value = invoice.OrderId;
                cmd.Parameters.Add("TotalAmount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = invoice.TotalAmount;
                cmd.Parameters.Add("CreatedBy", NpgsqlTypes.NpgsqlDbType.Integer).Value = invoice.CreatedBy;
                cmd.Parameters.Add("PaymentMethod", NpgsqlTypes.NpgsqlDbType.Integer).Value = paymentMethod;
                cmd.Parameters.Add("PaymentStatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = paymentStatus;

                await cmd.ExecuteNonQueryAsync();
            }
            ;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}