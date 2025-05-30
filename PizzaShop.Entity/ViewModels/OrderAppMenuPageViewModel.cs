namespace PizzaShop.Entity.ViewModels;

public class OrderAppMenuPageViewModel
{
    public List<MenuCategoryViewModel>? CategoryList { get; set; }
    public List<MenuItemViewModel>? ItemList { get; set; }
    public MenuOrderDetailsViewModel? OrderDetails { get; set; }
}
