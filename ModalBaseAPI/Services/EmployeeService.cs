using Microsoft.IdentityModel.Tokens;
using ModelBaseAPI.Interfaces.Repository;
using ModelBaseAPI.Interfaces.Service;
using ModelBaseAPI.Models.Request;
using ModelBaseAPI.Models.Response;
using ModelBaseAPI.Models.Response.Wrappers;
using ModelBaseAPI.Models.Validator;

namespace ModelBaseAPI.Services
{
    public class EmployeeService(IEmployeeRepository employeeRepository) : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly HashSet<string> _allowedColumns = ["Id", "Name", "Email", "Age", "Occupation"];

        public async Task<IEnumerable<EmployeeResponse>> GetAllAsync()
        {
            var response = await _employeeRepository.GetAllAsync();

            return response.Any() ? response : [];
        }

        public async Task<ServiceResponse<EmployeeResponse?>> GetByIdAsync(int id)
        {
            if (id <= 0)
                return ServiceResponse<EmployeeResponse?>.Fail("Invalid passed Id.");
            var result = await _employeeRepository.GetByIdAsync(id);

            if (result is null)
                return ServiceResponse<EmployeeResponse?>.Fail($"No employee found with the passed Id: {id}");

            return ServiceResponse<EmployeeResponse?>.Ok(new EmployeeResponse
            {
                Id = result.Id,
                Name = result.Name,
                Email = result.Email,
                Age = result.Age,
                Occupation = result.Occupation
            });
        }
        public async Task<EmployeePagination> GetEmployeePagination(int offset, int limit, string? sort, string? order, string? searchTerm = null)
        {

            sort = sort.IsNullOrEmpty() ? "Id" : sort;
            order = order.IsNullOrEmpty() ? "ASC" : order;

            // Validation
            if (_allowedColumns.Contains(sort!))
            {
                // Data returned
                var (totalRecords, employees) = await _employeeRepository.GetEmployeePaginatedOffset(offset, limit, sort!, order!, PrepareSearchTerm(searchTerm));

                int totalPages = (int)Math.Ceiling((double)totalRecords / limit);
                int currentPage = (offset / limit) + 1;

                return new EmployeePagination
                {
                    Employees = employees,
                    CurrentPage = currentPage,
                    TotalPages = totalPages
                };
            }

            throw new ArgumentException("The order is invalid.");
        }

        public async Task<ServiceResponse<EmployeeResponse>> CreateEmployeeAsync(EmployeeRequest employee)
        {
            var validator = new EmployeeValidator();

            var result = validator.Validate(employee);
            if (!result.IsValid)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
                return ServiceResponse<EmployeeResponse>.Fail($"Invalid arguments: {errors}");
            }

            var existEmail = await _employeeRepository.EmployeeExistsByEmail(employee.Email);

            if (existEmail)
                return ServiceResponse<EmployeeResponse>.Fail("Already exist a Employee with the passed email.");
            
            var newRecord = await _employeeRepository.AddAsync(employee);

            return ServiceResponse<EmployeeResponse>.Ok(new EmployeeResponse
            {
                Id = newRecord.Id,
                Name = newRecord.Name,
                Email = newRecord.Email,
                Age = newRecord.Age,
                Occupation = newRecord.Occupation
            });
        }
        
        public async Task<ServiceResponse<string>> UpdateEmployeeAsync(int id, EmployeeRequest employee)
        {
            var existEmployee = await _employeeRepository.GetByIdAsync(id);
            var existEmail = await _employeeRepository.EmployeeExistsByEmail(employee.Email);
            
            var validator = new EmployeeValidator();
            var result = validator.Validate(employee);

            if (existEmail)
                throw new ArgumentException("Another employee already has this email.");

            if (existEmployee == null)
                throw new ArgumentException($"No Employee found with the passed Id: {id}");

            if (!result.IsValid)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException($"Invalid arguments: {errors}");
            }

            await _employeeRepository.UpdateAsync(id, employee);
            return ServiceResponse<string>.Ok("Employee updated successfully");
        }

        public async Task<ServiceResponse<string>> DeleteEmployeeAsync(int id)
        {
            _ = await _employeeRepository.GetByIdAsync(id) ?? throw new ArgumentException($"No Employee found with the passed Id: {id}");

            await _employeeRepository.DeleteAsync(id);
            return ServiceResponse<string>.Ok("Employee removed successfully");
        }

        private static string? PrepareSearchTerm(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return null;

            var cleaned = searchTerm.Trim();
            cleaned = cleaned.Replace("%", "").Replace("_", "");

            return $"%{cleaned.ToLower()}%";
        }
    }
}
