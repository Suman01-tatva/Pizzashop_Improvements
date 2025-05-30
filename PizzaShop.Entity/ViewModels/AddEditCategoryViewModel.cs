using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels;

public class AddEditCategoryViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Category Name is required")]
    [RegularExpression(@"^[A-Za-z0-9]+(?:\s[A-Za-z0-9]+)*$", ErrorMessage = "Invalid name.")]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; } = null!;
}