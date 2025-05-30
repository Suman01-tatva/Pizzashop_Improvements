namespace PizzaShop.Entity.ViewModels;

public class OrderItemModifierViewModel
{
    public int? Id { get; set; }

    public int? OrderItemid { get; set; }

    public string? Name { get; set; }

    public int? Quantity { get; set; }

    public decimal? Rate { get; set; }

    public decimal? TotalAmount { get; set; }
}