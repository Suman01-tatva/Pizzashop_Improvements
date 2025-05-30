namespace PizzaShop.Entity.ViewModels;

public class AddEditExistingModifiersViewModel
{
    public List<MenuModifierViewModel>? modifier { get; set; }
    public int PageSize { get; set; }
    public int PageIndex { get; set; }
    public string? SearchString { get; set; } = null!;
    public int TotalPage { get; set; }
    public int TotalItems { get; set; }
}