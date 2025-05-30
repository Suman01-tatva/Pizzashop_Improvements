using System.ComponentModel.DataAnnotations;
namespace PizzaShop.Entity.ViewModels;

public class AssignTablePageViewModel
{
    public List<WaitingTokenViewModel>? WaitingTokenList { get; set; }

    public List<SectionViewModel>? SectionList { get; set; }
    public int? Id { get; set; } //waitingTokenId for assignTable in waitingList
    public int? CustomerId { get; set; }
    public int? SectionId { get; set; }

    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can only contain letters.")]
    [MaxLength(50, ErrorMessage = "Name should be maximum 50 characters long!")]
    public string? CustomerName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [RegularExpression(@"^[a-z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Enter valid email.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(50, ErrorMessage = "Email should be maximum 50 characters long!")]
    public string? CustomerEmail { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit phone number.")]
    public string? MobileNumber { get; set; }

    [Range(0, 101, ErrorMessage = "No Of Person must be between 0 and 100.")]
    public int? NoOfPerson { get; set; }
}