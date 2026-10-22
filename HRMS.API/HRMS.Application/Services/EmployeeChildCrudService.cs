using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

// Base for records that belong to one employee (skills, previous jobs, addresses, bank accounts).
// The owner of a record can never be changed by an update (update DTOs have no EmployeeId).
public abstract class EmployeeChildCrudService<TEntity, TCreate, TUpdate, TResponse>
    : CrudService<TEntity, TCreate, TUpdate, TResponse>, IEmployeeChildService<TCreate, TUpdate, TResponse>
    where TEntity : BaseEntity, IEmployeeOwned
    where TCreate : IHasEmployeeId
{
    protected EmployeeChildCrudService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    // Rules specific to the record type (duplicates, foreign keys, date checks).
    protected abstract Task ValidateChildAsync(object dto, int employeeId, int? currentId);

    protected sealed override async Task ValidateAsync(object dto, int? currentId)
    {
        int employeeId;

        if (currentId.HasValue)
        {
            var existing = await UnitOfWork.Repository<TEntity>().GetByIdAsync(currentId.Value)
                ?? throw new NotFoundException(typeof(TEntity).Name, currentId.Value);
            employeeId = existing.EmployeeId;
        }
        else
        {
            employeeId = ((IHasEmployeeId)dto).EmployeeId;
            var employee = await UnitOfWork.Repository<Employee>().GetByIdAsync(employeeId)
                ?? throw new NotFoundException(nameof(Employee), employeeId);

            if (!employee.IsActive)
                throw new BadRequestException("This employee is inactive.");
        }

        await ValidateChildAsync(dto, employeeId, currentId);
    }

    public async Task<List<TResponse>> GetByEmployeeAsync(int employeeId)
    {
        if (await UnitOfWork.Repository<Employee>().GetByIdAsync(employeeId) is null)
            throw new NotFoundException(nameof(Employee), employeeId);

        var items = await UnitOfWork.Repository<TEntity>().FindAsync(x => x.EmployeeId == employeeId && x.IsActive);
        return await MapAsync(items.OrderBy(x => x.Id));
    }
}
