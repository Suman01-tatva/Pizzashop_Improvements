namespace PizzaShop.Entity.ViewModels;

public class OrderAppTableViewModel
{
    public List<OrderAppSectionListViewModel>? SectionList { get; set; }
    public List<WaitingTokenViewModel>? WaitingTokenList { get; set; }
}