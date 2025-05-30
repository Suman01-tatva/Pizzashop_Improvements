using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Entity.ViewModels;

public class ItemModifierViewModel
{
  public int? Id { get; set; }
  public string? Name { get; set; }
  public bool? Type { get; set; }
  public string? ModifierGroupName { get; set; }
  public int ModifierGroupId { get; set; }
  public int? Minselectionrequired { get; set; }
  public int? Maxselectionrequired { get; set; }
  public List<MenuModifierViewModel>? ModifierList { get; set; }
}