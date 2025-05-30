using PizzaShop.Entity.Data;

namespace PizzaShop.Entity.ViewModels;

public class MenuModifierGroupViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }
    public List<MenuModifierViewModel>? Modifiers { get; set; }
    public List<ExistingModifierViewModel>? ExistingModifiers { get; set; }
    public List<int>? RemovedModifiers { get; set; }
}