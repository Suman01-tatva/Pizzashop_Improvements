namespace PizzaShop.Repository.Implementations
{
    using Microsoft.EntityFrameworkCore;

    using PizzaShop.Entity.Data;
    using PizzaShop.Entity.ViewModels;
    using PizzaShop.Repository.Interfaces;
    using System.Collections.Generic;
    using System.Linq;

    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly PizzashopContext _context;
        private readonly IRoleRepository _role;

        public RolePermissionRepository(PizzashopContext context, IRoleRepository role)
        {
            _context = context;
            _role = role;
        }

        public List<Permission>? GetAllPermissions()
        {
            List<Permission> permissions = _context.Permissions.ToList();
            if (permissions.Count > 0)
                return permissions;
            return null;
        }

        public List<RolePermissionViewModel>? GetRolePermissionByRoleId(int roleId)
        {
            List<RolePermission> rolePermisssion = _context.RolePermissions.Where(rp => rp.RoleId == roleId).OrderBy(rp => rp.PermissionId).ToList();

            if (rolePermisssion.Count > 0)
            {
                var role = _context.Roles.FirstOrDefault(r => r.Id == roleId);
                var rolePermissionList = new List<RolePermissionViewModel>();

                foreach (var rp in rolePermisssion)
                {
                    rolePermissionList.Add(new RolePermissionViewModel
                    {
                        Id = rp.Id,
                        RoleId = rp.RoleId,
                        RoleName = role?.Name!,
                        Permissionid = rp.PermissionId,
                        PermissionName = _context.Permissions.FirstOrDefault(p => p.Id == rp.PermissionId)?.Name,
                        CanEdit = rp.CanEdit,
                        CanView = rp.CanView,
                        CanDelete = rp.CanDelete
                    });
                }

                return rolePermissionList;
            }

            return null;
        }

        public async Task<bool> UpdateRolePermissionAsync(List<RolePermissionViewModel> model, string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            // var role = await _role.GetRoleByIdAsync(user!.RoleId);

            foreach (var rp in model)
            {
                var rolePermission = await _context.RolePermissions.FirstOrDefaultAsync(x => x.Id == rp.Id);
                if (rolePermission != null)
                {
                    rolePermission.CanEdit = rp.CanEdit;
                    rolePermission.CanView = rp.CanView;
                    rolePermission.CanDelete = rp.CanDelete;
                    rolePermission.ModifiedAt = DateTime.UtcNow;
                    rolePermission.ModifiedBy = user!.Id;
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public RolePermission GetPermissions(int roleId, string moduleName)
        {
            var permission = _context.RolePermissions
                .Include(rp => rp.Permission)
                .Include(p => p.Role)
                .Where(p => p.RoleId == roleId && p.Permission.Name == moduleName)
                .Select(p => new RolePermission
                {
                    CanView = p.CanView,
                    CanEdit = p.CanEdit,
                    CanDelete = p.CanDelete
                })
                .FirstOrDefault();

            return permission ?? new RolePermission();
        }
    }
}