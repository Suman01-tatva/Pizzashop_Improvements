using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels;

public class ResetPasswordModel
{

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "New Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*#?&]).{6,}$",
                ErrorMessage = "Password must be Strong.")]
        public string NewPassword { set; get; } = null!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "Password Do not matched")]
        public string ConfirmNewPassword { set; get; } = null!;
}
