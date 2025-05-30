namespace PizzaShop.Service.Implementations;

using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;

using PizzaShop.Service.Interfaces;


public class RolePermissionService : IRolePermissionService
{
    private readonly IRoleRepository _role;
    private readonly IRolePermissionRepository _rolePermission;

    public RolePermissionService(IRoleRepository role, IRolePermissionRepository rolePermission)
    {
        _role = role;
        _rolePermission = rolePermission;
    }

    public async Task<IEnumerable<Role>> GetAllRoles()
    {
        return await _role.GetAllRolesAsync();
    }

    public List<RolePermissionViewModel>? GetRolePermissionByRoleId(int roleId)
    {
        var rolePermission = _rolePermission.GetRolePermissionByRoleId(roleId);
        if (rolePermission != null && rolePermission.Count > 0)
            return rolePermission;
        return null;
    }

    public async Task<bool> UpdateRolePermission(List<RolePermissionViewModel> model, string email)
    {
        bool isRolePermission = await _rolePermission.UpdateRolePermissionAsync(model, email);
        return isRolePermission;
    }

    public RolePermission GetRolePermission(int roleId, string permissionName)
    {
        var rolePermissions = _rolePermission.GetPermissions(roleId, permissionName);
        return rolePermissions;
    }
}
