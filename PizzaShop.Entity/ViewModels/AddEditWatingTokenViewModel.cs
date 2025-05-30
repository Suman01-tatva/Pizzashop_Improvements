using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels;

public class AddEditWatingTokenViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [MaxLength(50, ErrorMessage = "Email should be less then 50 character")]
    [RegularExpression(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$", ErrorMessage = "Invalid Email")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(50, ErrorMessage = "Fist Name should be less then 50 character")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First Name can only contain letters.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "MobileNumber is required")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter a valid mobile number.")]
    public string? MobileNumber { get; set; }

    public int? CustomerId { get; set; }

    public bool? IsDeleted { get; set; }

    public bool? IsAssigned { get; set; }

    public int? TableId { get; set; }

    [Required(ErrorMessage = "Section is required")]
    public int? SectionId { get; set; }

    [Required(ErrorMessage = "No of person is required")]
    [Range(1, 50, ErrorMessage = "No of persons should be between 1-50")]
    public int? NoOfPersons { get; set; }

    public List<SectionViewModel>? SectionList { get; set; }

    public List<WaitingTokenViewModel>? WaitingList { get; set; }
}