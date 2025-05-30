namespace PizzaShop.Entity.ViewModels;

public class SaveOrderViewModel
{
    public int Id { get; set; }
    public List<OrderItemsViewModel>? OrderItems { get; set; }
    public List<TaxesViewModel>? OrderTax { get; set; }
    public bool IsSgstSelected { get; set; }
    public int? Status { get; set; }
    public decimal? Tax { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? Total { get; set; }
}
