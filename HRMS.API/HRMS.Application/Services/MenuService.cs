using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Security;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class MenuService : CrudService<Menu, MenuCreateDto, MenuUpdateDto, MenuResponseDto>, IMenuService
{
    public MenuService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not MenuCreateDto d) return;

        if (d.ParentMenuId.HasValue)
        {
            if (currentId.HasValue && d.ParentMenuId.Value == currentId.Value)
                throw new BadRequestException("A menu cannot be its own parent.");

            var map = (await UnitOfWork.Repository<Menu>().GetAllAsync()).ToDictionary(m => m.Id);

            var parent = map.GetValueOrDefault(d.ParentMenuId.Value)
                ?? throw new NotFoundException(nameof(Menu), d.ParentMenuId.Value);

            if (!parent.IsActive)
                throw new BadRequestException("The parent menu is inactive.");

            if (currentId.HasValue)
            {
                var cursor = parent;
                var steps = 0;
                while (cursor != null && steps++ < 20)
                {
                    if (cursor.Id == currentId.Value)
                        throw new BadRequestException("This change would create a loop in the menu tree.");

                    cursor = cursor.ParentMenuId.HasValue ? map.GetValueOrDefault(cursor.ParentMenuId.Value) : null;
                }
            }
        }

        var excludeId = currentId ?? 0;
        var parentId = d.ParentMenuId;
        var name = d.MenuName.Trim();
        if (await UnitOfWork.Repository<Menu>().AnyAsync(m => m.ParentMenuId == parentId && m.MenuName == name && m.Id != excludeId))
            throw new BadRequestException($"A menu named '{name}' already exists under the same parent.");
    }

    protected override async Task OnDeletingAsync(Menu entity)
    {
        var id = entity.Id;
        if (await UnitOfWork.Repository<Menu>().AnyAsync(m => m.ParentMenuId == id && m.IsActive))
            throw new BadRequestException("This menu has active sub-menus. Deactivate them first.");
    }

    protected override async Task<List<MenuResponseDto>> MapAsync(IEnumerable<Menu> items)
    {
        var names = (await UnitOfWork.Repository<Menu>().GetAllAsync()).ToDictionary(m => m.Id, m => m.MenuName);
        var result = Mapper.Map<List<MenuResponseDto>>(items.ToList());
        foreach (var dto in result)
            dto.ParentMenuName = dto.ParentMenuId.HasValue ? names.GetValueOrDefault(dto.ParentMenuId.Value) : null;
        return result.OrderBy(m => m.ParentMenuId ?? 0).ThenBy(m => m.SortOrder).ThenBy(m => m.MenuName).ToList();
    }
}
