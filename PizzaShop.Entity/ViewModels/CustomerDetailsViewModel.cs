namespace PizzaShop.Entity.ViewModels;

public class CustomerDetailsViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? MobileNumber { get; set; }
    public decimal? MaxOrder { get; set; }
    public decimal? AvgBill { get; set; }
    public DateTime? CommingSince { get; set; }
    public int? Visits { get; set; }
    public List<CustomerOrderDetails>? CustomerOrders { get; set; } = new();
}

public class CustomerOrderDetails
{
    public DateOnly? OrderDate { get; set; }
    public string? OrderType { get; set; }
    public int? Payment { get; set; }
    public int? items { get; set; }
    public decimal? Amount { get; set; }
}