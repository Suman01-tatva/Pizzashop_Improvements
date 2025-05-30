
namespace PizzaShop.Entity.ViewModels;

public class ItemTabViewModel
{
    public List<MenuCategoryViewModel> categoryList { get; set; }
    public List<MenuItemViewModel> itemList { get; set; }

    public int PageSize { get; set; }
    public int PageIndex { get; set; }
    public string? SearchString { get; set; } = null!;
    public int TotalPage { get; set; }
    public int TotalItems { get; set; }
}