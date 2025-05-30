using PizzaShop.Entity.Constants;

namespace PizzaShop.Entity.ViewModels;

public class OrderAppSectionListViewModel
{

    public int? Id { get; set; }
    public string? Name { get; set; } = null!;
    public int? Available { get; set; }
    public int? Assigned { get; set; }
    public int? Running { get; set; }
    public List<TableOrderAppViewModel>? TableList { get; set; }

}

public class TableOrderAppViewModel
{
    public int? Id { get; set; }

    public int? Sectionid { get; set; }

    public int? OrderId { get; set; }

    public string? Name { get; set; } = null!;

    public int? Capacity { get; set; }

    public bool? Status { get; set; }

    public string? OrderStatus { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? TimeDuration { get; set; }
}
