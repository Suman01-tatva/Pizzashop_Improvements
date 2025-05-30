namespace PizzaShop.Entity.ViewModels;
using System.ComponentModel.DataAnnotations;

public class RolePermissionViewModel
{
    [Required(ErrorMessage = "rolePermission Id is required.")]
    public int Id { get; set; }
    [Required(ErrorMessage = "Role Id is required.")]
    public int? RoleId { get; set; }
    [Required(ErrorMessage = "Role Name is required.")]
    public string RoleName { get; set; } = null!;
    [Required(ErrorMessage = "PErmission Id is required.")]
    public int? Permissionid { get; set; }
    public string? PermissionName { get; set; }
    [Required(ErrorMessage = "Can View field is required.")]
    public bool CanView { get; set; }
    [Required(ErrorMessage = "Can add/Edit Field is required.")]
    public bool CanEdit { get; set; }
    [Required(ErrorMessage = "Can Delete is required.")]
    public bool CanDelete { get; set; }
}
