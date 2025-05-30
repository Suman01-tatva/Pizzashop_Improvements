namespace PizzaShop.Entity.ViewModels;

public class CustomerPageViewModel
{
    public List<CustomerViewModel> CustomerList { get; set; } = null!;
    public int PageSize { get; set; }
    public int PageIndex { get; set; }
    public string? SearchString { get; set; } = null!;
    public int TotalPage { get; set; }
    public int TotalCustomers { get; set; }
    public string? SortOrder { get; set; }
    public string? DateRange { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}