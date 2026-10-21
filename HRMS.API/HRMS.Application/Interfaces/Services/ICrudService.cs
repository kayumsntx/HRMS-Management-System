namespace HRMS.Application.Interfaces.Services;

public interface ICrudService<TCreate, TUpdate, TResponse>
{
    Task<List<TResponse>> GetAllAsync();
    Task<TResponse> GetByIdAsync(int id);
    Task<TResponse> CreateAsync(TCreate dto);
    Task<TResponse> UpdateAsync(int id, TUpdate dto);
    Task DeleteAsync(int id);   // soft delete (IsActive = false)
}
