using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Common;

namespace HRMS.Application.Services;

public abstract class CrudService<TEntity, TCreate, TUpdate, TResponse> : ICrudService<TCreate, TUpdate, TResponse>
    where TEntity : BaseEntity
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IMapper Mapper;

    protected CrudService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        UnitOfWork = unitOfWork;
        Mapper = mapper;
    }

    // Override for duplicate checks / foreign key checks. Called for both create and update.
    protected virtual Task ValidateAsync(object dto, int? currentId) => Task.CompletedTask;

    protected virtual Task<List<TResponse>> MapAsync(IEnumerable<TEntity> items)
        => Task.FromResult(Mapper.Map<List<TResponse>>(items.ToList()));

    public async Task<List<TResponse>> GetAllAsync()
        => await MapAsync((await UnitOfWork.Repository<TEntity>().GetAllAsync()).OrderBy(x => x.Id));

    public async Task<TResponse> GetByIdAsync(int id)
    {
        var entity = await UnitOfWork.Repository<TEntity>().GetByIdAsync(id)
            ?? throw new NotFoundException(typeof(TEntity).Name, id);

        return (await MapAsync(new[] { entity })).First();
    }

    public async Task<TResponse> CreateAsync(TCreate dto)
    {
        await ValidateAsync(dto!, null);

        var entity = Mapper.Map<TEntity>(dto);
        await UnitOfWork.Repository<TEntity>().AddAsync(entity);
        await UnitOfWork.SaveChangesAsync();

        return (await MapAsync(new[] { entity })).First();
    }

    public async Task<TResponse> UpdateAsync(int id, TUpdate dto)
    {
        var entity = await UnitOfWork.Repository<TEntity>().GetByIdAsync(id)
            ?? throw new NotFoundException(typeof(TEntity).Name, id);

        await ValidateAsync(dto!, id);

        Mapper.Map(dto, entity);
        entity.UpdatedDate = DateTime.UtcNow;

        UnitOfWork.Repository<TEntity>().Update(entity);
        await UnitOfWork.SaveChangesAsync();

        return (await MapAsync(new[] { entity })).First();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await UnitOfWork.Repository<TEntity>().GetByIdAsync(id)
            ?? throw new NotFoundException(typeof(TEntity).Name, id);

        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;

        UnitOfWork.Repository<TEntity>().Update(entity);
        await UnitOfWork.SaveChangesAsync();
    }
}
