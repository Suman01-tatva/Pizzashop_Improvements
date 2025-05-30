using PizzaShop.Entity.Data;

namespace PizzaShop.Entity.ViewModels;

public class MenuOrderDetailsViewModel
{
    public int? OrderId { get; set; }
    public string? OrderInstructions { get; set; }
    public int? SectionId { get; set; }
    public string? SectionName { get; set; }
    public int? OrderStatus { get; set; }
    public List<string>? TableOrderNames { get; set; }
    public List<OrderItemsViewModel>? OrderItems { get; set; }
    public CustomerViewModel? Customer { get; set; }
    public List<TaxesViewModel>? TaxList { get; set; }
}
