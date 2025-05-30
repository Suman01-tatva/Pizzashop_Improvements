using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels;

public class CustomerViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Name can only contain letters.")]
    [MaxLength(50, ErrorMessage = "Name should be maximum 50 characters long!")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [RegularExpression(@"^[a-z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email can only contain small letters, numbers, dots, underscores, and special characters like %, +, and -.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(50, ErrorMessage = "UserName should be maximum 50 characters long!")]
    public string Email { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter a valid  phone number.")]
    public string Phone { get; set; } = null!;

    public int? TotalOrders { get; set; }

    public int? NoOfPerson { get; set; }

    public DateOnly? LastOrderDate { get; set; }
}