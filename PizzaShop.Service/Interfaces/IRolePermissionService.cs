using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface IRolePermissionService
{
    public Task<IEnumerable<Role>> GetAllRoles();
    public List<RolePermissionViewModel>? GetRolePermissionByRoleId(int roleId);
    Task<bool> UpdateRolePermission(List<RolePermissionViewModel> model, string email);
    public RolePermission GetRolePermission(int roleId, string permissionName);
}
