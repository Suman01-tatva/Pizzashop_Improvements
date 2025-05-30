using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Repository.Interfaces;

public interface IRolePermissionRepository
{
    public List<Permission>? GetAllPermissions();
    public List<RolePermissionViewModel>? GetRolePermissionByRoleId(int roleId);
    Task<bool> UpdateRolePermissionAsync(List<RolePermissionViewModel> model, string email);
    public RolePermission GetPermissions(int roleId, string moduleName);
}
