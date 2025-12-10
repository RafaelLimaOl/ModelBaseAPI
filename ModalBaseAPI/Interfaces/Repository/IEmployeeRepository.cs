using ModelBaseAPI.Models.Entity;
using ModelBaseAPI.Models.Request;
using ModelBaseAPI.Models.Response;

namespace ModelBaseAPI.Interfaces.Repository
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<EmployeeResponse>> GetAllAsync();
        Task<(int totalRecords, List<EmployeeResponse> employees)> GetEmployeePaginatedOffset(int offset, int limit, string sort, string order, string? searchTerm = null);
        Task<List<Employee>> GetEmployeePaginatedCursor(string lastCursor, int limit);
        Task<EmployeeResponse?> GetByIdAsync(int id);
        Task<EmployeeResponse> AddAsync(EmployeeRequest employee);
        Task UpdateAsync(int id, EmployeeRequest employee);
        Task<bool> DeleteAsync(int id);
        Task<bool> EmployeeExistsByEmail(string email);
    }
}
