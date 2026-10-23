using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Security;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

// NOTE: these permissions decide what the frontend SHOWS (sidebar and buttons).
// API access is still controlled by roles ([Authorize(Roles = ...)]) on the controllers.
// The Admin role always sees everything and cannot be edited here.
public class MenuPermissionService : IMenuPermissionService
{
    private const string AdminRole = "Admin";

    private readonly IUnitOfWork _uow;

    public MenuPermissionService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<MenuPermissionResponseDto>> GetByRoleAsync(int roleId)
    {
        await LoadRoleAsync(roleId);

        var menus = await _uow.Repository<Menu>().FindAsync(m => m.IsActive);
        var permissions = (await _uow.Repository<MenuPermission>().FindAsync(p => p.RoleId == roleId)).ToDictionary(p => p.MenuId);

        return menus
            .OrderBy(m => m.ParentMenuId ?? 0).ThenBy(m => m.SortOrder).ThenBy(m => m.MenuName)
            .Select(m =>
            {
                permissions.TryGetValue(m.Id, out var p);
                return new MenuPermissionResponseDto
                {
                    MenuId = m.Id,
                    ParentMenuId = m.ParentMenuId,
                    MenuName = m.MenuName,
                    MenuUrl = m.MenuUrl,
                    CanView = p?.CanView ?? false,
                    CanCreate = p?.CanCreate ?? false,
                    CanEdit = p?.CanEdit ?? false,
                    CanDelete = p?.CanDelete ?? false
                };
            })
            .ToList();
    }

    // Replaces the role's permissions with the given list. Menus that are not listed (or have no access) lose access.
    public async Task<List<MenuPermissionResponseDto>> SaveForRoleAsync(int roleId, MenuPermissionSaveDto dto)
    {
        var role = await LoadRoleAsync(roleId);
        if (role.RoleName == AdminRole)
            throw new BadRequestException("The Admin role always has full access and cannot be changed.");

        var menuIds = dto.Items.Select(i => i.MenuId).ToList();
        var menus = (await _uow.Repository<Menu>().FindAsync(m => menuIds.Contains(m.Id))).ToDictionary(m => m.Id);

        foreach (var id in menuIds)
        {
            if (!menus.TryGetValue(id, out var menu))
                throw new NotFoundException(nameof(Menu), id);

            if (!menu.IsActive)
                throw new BadRequestException($"Menu '{menu.MenuName}' is inactive.");
        }

        var wanted = dto.Items.Where(i => i.CanView || i.CanCreate || i.CanEdit || i.CanDelete).ToDictionary(i => i.MenuId);
        var existing = await _uow.Repository<MenuPermission>().FindAsync(p => p.RoleId == roleId);
        var existingIds = existing.Select(p => p.MenuId).ToHashSet();

        foreach (var row in existing)
        {
            if (wanted.TryGetValue(row.MenuId, out var item))
            {
                row.CanView = item.CanView;
                row.CanCreate = item.CanCreate;
                row.CanEdit = item.CanEdit;
                row.CanDelete = item.CanDelete;
                row.UpdatedDate = DateTime.UtcNow;
                _uow.Repository<MenuPermission>().Update(row);
            }
            else
            {
                _uow.Repository<MenuPermission>().Delete(row);
            }
        }

        var added = wanted.Values.Where(i => !existingIds.Contains(i.MenuId)).Select(i => new MenuPermission
        {
            RoleId = roleId,
            MenuId = i.MenuId,
            CanView = i.CanView,
            CanCreate = i.CanCreate,
            CanEdit = i.CanEdit,
            CanDelete = i.CanDelete
        }).ToList();

        if (added.Count > 0)
            await _uow.Repository<MenuPermission>().AddRangeAsync(added);

        await _uow.SaveChangesAsync();
        return await GetByRoleAsync(roleId);
    }

    // The menu tree of one user = union of the permissions of all his/her active roles.
    public async Task<List<MyMenuDto>> GetMyMenusAsync(int userId)
    {
        var roleIds = (await _uow.Repository<UserRole>().FindAsync(ur => ur.UserId == userId)).Select(ur => ur.RoleId).ToList();
        var roles = roleIds.Count == 0 ? new List<Role>() : await _uow.Repository<Role>().FindAsync(r => r.IsActive && roleIds.Contains(r.Id));
        var activeRoleIds = roles.Select(r => r.Id).ToList();
        var isAdmin = roles.Any(r => r.RoleName == AdminRole);

        var menus = (await _uow.Repository<Menu>().FindAsync(m => m.IsActive)).ToDictionary(m => m.Id);
        var flags = new Dictionary<int, (bool View, bool Create, bool Edit, bool Delete)>();

        if (isAdmin)
        {
            foreach (var id in menus.Keys)
                flags[id] = (true, true, true, true);
        }
        else if (activeRoleIds.Count > 0)
        {
            foreach (var p in await _uow.Repository<MenuPermission>().FindAsync(p => activeRoleIds.Contains(p.RoleId)))
            {
                flags.TryGetValue(p.MenuId, out var current);
                flags[p.MenuId] = (current.View || p.CanView, current.Create || p.CanCreate, current.Edit || p.CanEdit, current.Delete || p.CanDelete);
            }
        }

        // visible = menus with View access + all their ancestors (so groups appear when a child is visible)
        var visible = flags.Where(f => f.Value.View && menus.ContainsKey(f.Key)).Select(f => f.Key).ToHashSet();
        foreach (var id in visible.ToList())
        {
            var cursor = menus[id];
            var steps = 0;
            while (cursor.ParentMenuId.HasValue && menus.TryGetValue(cursor.ParentMenuId.Value, out var parent) && steps++ < 20)
            {
                visible.Add(parent.Id);
                cursor = parent;
            }
        }

        var nodes = visible.ToDictionary(id => id, id =>
        {
            var m = menus[id];
            flags.TryGetValue(id, out var f);
            return new MyMenuDto
            {
                Id = m.Id,
                Name = m.MenuName,
                Url = m.MenuUrl,
                Icon = m.IconClass,
                SortOrder = m.SortOrder,
                CanView = f.View,
                CanCreate = f.Create,
                CanEdit = f.Edit,
                CanDelete = f.Delete
            };
        });

        var roots = new List<MyMenuDto>();
        foreach (var id in visible)
        {
            var m = menus[id];
            if (m.ParentMenuId.HasValue && nodes.TryGetValue(m.ParentMenuId.Value, out var parentNode))
                parentNode.Children.Add(nodes[id]);
            else
                roots.Add(nodes[id]);
        }

        static void Sort(List<MyMenuDto> list)
        {
            list.Sort((a, b) => a.SortOrder != b.SortOrder ? a.SortOrder.CompareTo(b.SortOrder) : string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            foreach (var item in list)
                Sort(item.Children);
        }

        Sort(roots);
        return roots;
    }

    private async Task<Role> LoadRoleAsync(int roleId)
        => await _uow.Repository<Role>().GetByIdAsync(roleId) ?? throw new NotFoundException(nameof(Role), roleId);
}
