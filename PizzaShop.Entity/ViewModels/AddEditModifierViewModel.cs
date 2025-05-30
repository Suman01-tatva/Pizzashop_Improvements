namespace PizzaShop.Entity.ViewModels;
using System.ComponentModel.DataAnnotations;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

public class AddEditModifierViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [RegularExpression(@"^[A-Za-z]+(?:\s[A-Za-z]+)*$", ErrorMessage = "use only single space between words.")]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Rate is required.")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Invalid Rate.")]
    [Range(0.01, 999999.99, ErrorMessage = "Invalid Rate.")]
    public decimal Rate { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    // [Range(0, double.MaxValue, ErrorMessage = "Quantity must be Positive.")]
    [Range(1, 9999.99, ErrorMessage = "Quantity must be between 1 and 9999.99")]

    public int? Quantity { get; set; }

    public bool? Isdeleted { get; set; }

    public string? Description { get; set; } = null!;

    [Required(ErrorMessage = "ModifierGroup is required.")]
    public int Modifiergroupid { get; set; }

    public int? UnitId { get; set; }

    public DateTime? Createddate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? Updateddate { get; set; }

    public string? Updatedby { get; set; }

    public List<MenuModifierGroupViewModel>? ModifierGroups { get; set; }

    public List<Unit>? Units { get; set; }
}