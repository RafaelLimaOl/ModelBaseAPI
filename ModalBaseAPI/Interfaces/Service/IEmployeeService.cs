using ModelBaseAPI.Models.Request;
using ModelBaseAPI.Models.Response;
using ModelBaseAPI.Models.Response.Wrappers;

namespace ModelBaseAPI.Interfaces.Service
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponse>> GetAllAsync();
        Task<ServiceResponse<EmployeeResponse?>> GetByIdAsync(int id);
        Task<EmployeePagination> GetEmployeePagination(int offset, int limit, string? sort, string? order, string? searchTerm = null);
        Task<ServiceResponse<EmployeeResponse>> CreateEmployeeAsync(EmployeeRequest employee);
        Task<ServiceResponse<string>> UpdateEmployeeAsync(int id, EmployeeRequest employee);
        Task<ServiceResponse<string>> DeleteEmployeeAsync(int id);
    }
}
