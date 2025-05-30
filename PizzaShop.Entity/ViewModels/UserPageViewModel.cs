using PizzaShop.Entity.Data;

namespace PizzaShop.Entity.ViewModels;

public class UserPageViewModel
{
    public IEnumerable<User> UserList { get; set; } = null!;

    public int PageSize { get; set; }
    public int PageIndex { get; set; }
    public string? SearchString { get; set; } = null!;
    public int TotalPage { get; set; }
    public int TotalUsers { get; set; }
    public string? SortOrder { get; set; }
}