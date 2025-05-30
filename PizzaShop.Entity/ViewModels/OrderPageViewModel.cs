
namespace PizzaShop.Entity.ViewModels;

public class OrderPageViewModel
{
    public List<OrderViewModel>? Orders { get; set; }
    public string? SearchString { get; set; }
    public int Status { get; set; }
    public bool IsAsc { get; set; }
    public int PageSize { get; set; }
    public int PageIndex { get; set; }
    public int TotalPage { get; set; }
    public string? DateRange { get; set; }
    public int TotalOrders { get; set; }
    public string? sortColumn { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }

}
