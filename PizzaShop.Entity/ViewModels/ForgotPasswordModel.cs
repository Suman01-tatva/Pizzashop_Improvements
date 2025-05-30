using System.ComponentModel.DataAnnotations;
namespace PizzaShop.Entity.ViewModels;
public class ForgotPasswordModel
{
    [EmailAddress]
    [Required(ErrorMessage = "Email is required.")]
    [StringLength(50, ErrorMessage = "Must be between 5 and 50 characters", MinimumLength = 5)]
    public string Email { get; set; } = null!;
}