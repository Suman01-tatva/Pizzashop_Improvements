namespace PizzaShop.Entity.ViewModels;

public class DashboardViewModel
{
    public string? TotalSales { get; set; }
    public decimal TotalOrders { get; set; }
    public string? AverageOrderValue { get; set; }
    public double? AverageWaitingTime { get; set; }
    public GraphDetailViewModel? Revenue { get; set; }
    public GraphDetailViewModel? CustomerGrowth { get; set; }
    public List<DashboardItemViewModel>? TopSellingItem { get; set; }
    public List<DashboardItemViewModel>? LeastSellingItem { get; set; }
    public int WaitingListCount { get; set; }
    public int NewCustomer { get; set; }
}

public class GraphDetailViewModel
{
    public List<string>? Labels { get; set; }
    public List<decimal>? Values { get; set; }
}

public class DashboardItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ItemImage { get; set; }

    public int OrderCount { get; set; }
}
