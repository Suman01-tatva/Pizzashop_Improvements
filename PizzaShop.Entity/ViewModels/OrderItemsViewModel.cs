namespace PizzaShop.Entity.ViewModels;

public class OrderItemsViewModel
{
    public int? Id { get; set; }
    public int? MenuItemId { get; set; }
    public string? ItemName { get; set; }
    public string? Instruction { get; set; }
    public int? Quantity { get; set; }
    public int? ReadyItemQuantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<OrderItemModifierViewModel>? Modifiers { get; set; }
    public int? QuantityOfModifier { get; set; }
    public decimal? ModifiersPrice { get; set; } // consider this
    public decimal? TotalModifierAmount { get; set; }
    public string? Comment { get; set; }
    public decimal? Rate { get; set; }
    public decimal? TotalPrice { get; set; } // consider this TotalAmount
    public decimal? Tax { get; set; } // consider this TotalAmount
}
