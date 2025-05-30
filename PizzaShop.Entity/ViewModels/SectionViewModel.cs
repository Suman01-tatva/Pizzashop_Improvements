using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels;

public class SectionViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Section Name is Required")]
    [RegularExpression(@"^[A-Za-z0-9]+(?:\s[A-Za-z0-9]+)*$", ErrorMessage = "Invalid SectionName.")]
    [StringLength(25, ErrorMessage = "Name cannot be longer than 25 characters.")]
    public string Name { get; set; } = null!;

    [RegularExpression(@"^[A-Za-z0-9]+(?:\s[A-Za-z0-9]+)*$", ErrorMessage = "Invalid Discription.")]
    [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
    public string? Description { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public int? WaitingTokenCount { get; set; }
}

