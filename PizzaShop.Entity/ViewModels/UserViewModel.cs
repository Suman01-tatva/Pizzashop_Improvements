using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PizzaShop.Entity.Data;

namespace PizzaShop.Entity.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name can only contain letters.")]
        [MaxLength(50, ErrorMessage = "FirstName should be maximum 50 characters long!")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last name can only contain letters.")]
        [MaxLength(50, ErrorMessage = "LastName should be maximum 50 characters long!")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "User name is required.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "UserName can only contain letters, numbers, and underscores.")]
        [MaxLength(50, ErrorMessage = "UserName should be maximum 50 characters long!")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email can only contain small letters, numbers, dots, underscores, and special characters like %, +, and -.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(50, ErrorMessage = "UserName should be maximum 50 characters long!")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password), MinLength(6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*#?&]).{6,}$",
                ErrorMessage = "Password must be Strong.")]
        [MaxLength(50, ErrorMessage = "Password should be maximum 50 characters long!")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit phone number that does not start with zero.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public int RoleId { get; set; }

        public string? RoleName { get; set; }

        public string? ProfileImg { get; set; }

        public IFormFile? ProfileImagePath { get; set; } = null!;

        [Required(ErrorMessage = "Zipcode is required.")]
        [RegularExpression(@"^[1-9]\d{5}$", ErrorMessage = "Zipcode must be 6 digits and valid.")]
        public string? Zipcode { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(100, ErrorMessage = "Address should be maximum 100 characters long!")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public int? CountryId { get; set; }

        [Required(ErrorMessage = "State is required.")]
        public int? StateId { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public int? CityId { get; set; }

        public bool IsDeleted { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsFirstLogin { get; set; }
    }
}