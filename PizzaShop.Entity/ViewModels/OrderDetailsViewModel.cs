using PizzaShop.Entity.Constants;
using PizzaShop.Entity.Data;

namespace PizzaShop.Entity.ViewModels;

public class OrderDetailsViewModel
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string? InvoiceNo { get; set; }

    public int? OrderNo { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? Tax { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? Discount { get; set; }

    public decimal? PaidAmount { get; set; }

    public string? Notes { get; set; }

    public bool? IsSgstSelected { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }
    public DateOnly? Date { get; set; }

    public string? CustomerName { get; set; }

    public int? OrderDuration { get; set; }

    public OrderConstants.OrderStatusEnum? Status { get; set; }

    public OrderConstants.PaymentModeEnum? PaymentMode { get; set; }

    public int? Rating { get; set; }

    //customer

    public string? CustomerEmail { get; set; } = null!;

    public string? CustomerPhone { get; set; } = null!;

    //TableOrder

    // public int? TableId { get; set; }
    public List<string>? TableName { get; set; }

    public int? NoOfPeople { get; set; }

    public string? SectionName { get; set; }

    public List<OrderItemsViewModel>? OrderedItems { get; set; }
    //taxList
    public List<OrderTaxMapping>? OrderTaxes { get; set; }
}
