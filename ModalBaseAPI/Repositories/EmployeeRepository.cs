using Dapper;
using ModelBaseAPI.Interfaces.Repository;
using ModelBaseAPI.Models.Entity;
using ModelBaseAPI.Models.Request;
using ModelBaseAPI.Models.Response;
using System.Data;

namespace ModelBaseAPI.Repositories
{
    public class EmployeeRepository(IDbConnection dbConnection) : IEmployeeRepository
    {

        private readonly IDbConnection _dbConnection = dbConnection;

        public async Task<IEnumerable<EmployeeResponse>> GetAllAsync()
        {
            const string query = @"SELECT * FROM Employee";
            return (await _dbConnection.QueryAsync<EmployeeResponse>(query)).ToList();
        }

        public async Task<EmployeeResponse?> GetByIdAsync(int id)
        {
            const string query = @"SELECT * FROM Employee WHERE Id = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<EmployeeResponse>(query, new { Id = id });
        }

        public async Task<EmployeeResponse> AddAsync(EmployeeRequest employee)
        {
            string query = @"INSERT INTO Employee (Name, Email, Age, Occupation) 
                 OUTPUT INSERTED.Id 
                 VALUES (@Name, @Email, @Age, @Occupation)";

            var id = await _dbConnection.ExecuteScalarAsync<int>(query, new
            {
                employee.Name,
                employee.Email,
                employee.Age,
                employee.Occupation
            });

            return new EmployeeResponse
            {
                Id = id,
                Name = employee.Name,
                Email = employee.Email,
                Age = employee.Age,
                Occupation = employee.Occupation
            };
        }

        public async Task UpdateAsync(int id, EmployeeRequest employee)
        {
            string query = @"UPDATE Employee SET Name = @Name, Email = @Email, Age= @Age, Occupation = @Occupation WHERE Id = @id";
            await _dbConnection.ExecuteAsync(query, new
            {
                employee.Name,
                employee.Email,
                employee.Age,
                employee.Occupation,
                id
            });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string checkQuery = "SELECT COUNT(1) FROM Employee WHERE Id = @Id";
            var exists = await _dbConnection.ExecuteScalarAsync<int>(checkQuery, new { Id = id });

            if (exists == 0)
                return false;

            const string query = "DELETE FROM Employee WHERE Id = @Id";
            await _dbConnection.ExecuteAsync(query, new { Id = id });

            return true;
        }

        public async Task<(int totalRecords, List<EmployeeResponse> employees)> GetEmployeePaginatedOffset(int offset, int limit, string sort, string order, string? searchTerm = null)
        {

            string whereClause = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "WHERE LOWER(Name) LIKE LOWER(@SearchTerm) OR LOWER(Email) LIKE LOWER(@SearchTerm)";

            string query = $@"
                SELECT * FROM Employee
                {whereClause}
                ORDER BY 
                    CASE WHEN @Sort = 'Id' THEN Id 
                         WHEN @Sort = 'Name' THEN Name 
                         WHEN @Sort = 'Email' THEN Email 
                         ELSE Id END 
                " + (order.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? "DESC" : "ASC") + @"
                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;
            ";

            var countQuery = $@"SELECT COUNT(*) FROM Employee {whereClause}";

            using var multi = await _dbConnection.QueryMultipleAsync(countQuery + "; " + query,
                new { Offset = offset, Limit = limit, Sort = sort, SearchTerm = searchTerm });

            int totalRecords = await multi.ReadFirstAsync<int>();
            var employees = (await multi.ReadAsync<EmployeeResponse>()).ToList();

            return (totalRecords, employees);
        }

        public async Task<List<Employee>> GetEmployeePaginatedCursor(string? lastCursor, int limit)
        {
            string query = @"
                SELECT * FROM Employee
                WHERE (@LastCursor IS NULL OR Id > @LastCursor)
                ORDER BY Id
                FETCH NEXT @Limit ROWS ONLY;
            ";

            return (await _dbConnection.QueryAsync<Employee>(query, new { LastCursor = lastCursor, Limit = limit })).ToList();
        }

        public async Task<bool> EmployeeExistsByEmail(string email)
        {
            const string query = @"SELECT CASE WHEN EXISTS 
                          (SELECT 1 FROM Employee WHERE Email = @Email) 
                          THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { Email = email });
        }
    }
}
