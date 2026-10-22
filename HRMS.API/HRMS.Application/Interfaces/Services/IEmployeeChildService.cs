namespace HRMS.Application.Interfaces.Services;

public interface IEmployeeChildService<TCreate, TUpdate, TResponse> : ICrudService<TCreate, TUpdate, TResponse>
{
    Task<List<TResponse>> GetByEmployeeAsync(int employeeId);
}
