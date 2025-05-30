using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels;

public class ChangePasswordViewModel
{
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Current Password is Required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*#?&]).{6,}$",
                ErrorMessage = "Password must be Valid.")]
        public string CurrentPassword { set; get; } = null!;

        [Required(ErrorMessage = "New Password is Required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*#?&]).{6,}$",
                ErrorMessage = "Password must be Strong.")]
        public string NewPassword { set; get; } = null!;

        [Required(ErrorMessage = "Confirm New Password is Required")]
        [Compare("NewPassword", ErrorMessage = "The new password do not match.")]
        public string ConfirmNewPassword { set; get; } = null!;
}