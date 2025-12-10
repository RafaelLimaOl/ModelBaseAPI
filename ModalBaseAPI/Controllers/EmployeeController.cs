using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ModelBaseAPI.Interfaces.Repository;
using ModelBaseAPI.Interfaces.Service;
using ModelBaseAPI.Models.Request;
using ModelBaseAPI.Models.Response;
using ModelBaseAPI.Models.Response.Wrappers;
using ModelBaseAPI.Utilities;

namespace ModelBaseAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController(IEmployeeRepository employeeRepository, IEmployeeService employeeService, IMemoryCache memoryCache) : ControllerBase
    {

        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IEmployeeService _employeeService = employeeService;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Your cache key
        private readonly string CacheKey = "GetEmployeeCacheKey";

        /// <summary>
        /// Retrieves a list of all employees.
        /// </summary>
        /// <param name="cleanCache">Set true to clean the cache and make another db request (optional)</param>
        /// <response code="204">No content</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] bool cleanCache = false)
        {
            if (cleanCache)
                _memoryCache.Remove(CacheKey);

            if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<EmployeeResponse> cachedData))
            {
                var result = await _employeeService.GetAllAsync();
                if (result is null || !result.Any())
                    return NoContent();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(45))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(CacheKey, result, cacheEntryOptions);

                return Ok(new ApiResponse<IEnumerable<EmployeeResponse>>(true, "Employee list successfully retrieved!", result));
            }

            return Ok(new ApiResponse<IEnumerable<EmployeeResponse>>(true, "Employee list successfully retrieved from cache!", cachedData));
        }

        /// <summary>
        /// Retrieves a paginated list of employees using offset-based pagination.
        /// </summary>
        /// <param name="offset">The number of records to skip.</param>
        /// <param name="limit">The maximum number of records to return.</param>
        /// <param name="sort">The field to sort by (optional).</param>
        /// <param name="order">The sorting order (ascending or descending, optional).</param>
        /// <param name="searchTerm">The term to filter data (name or email, optional).</param>
        /// <response code="204">No content</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("offset")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeePagination>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOffsetPagination([FromQuery] int offset, [FromQuery] int limit, [FromQuery] string? sort, [FromQuery] string? order, [FromQuery] string? searchTerm = null)
        {
            var result = await _employeeService.GetEmployeePagination(offset, limit, sort, order, searchTerm);

            if (result.Employees is null || result.Employees.Count == 0)
                return NoContent();

            return Ok(new ApiResponse<EmployeePagination>(true, "Employee list successfully retrieved!", result));
        }

        /// <summary>
        /// Retrieves an employee by their unique identifier.
        /// </summary>
        /// <param name="id">The employee's unique ID.</param>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Funcionário não encontrado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _employeeService.GetByIdAsync(id);

            if (!result.Success && result.Message == "Invalid passed Id.")
                throw new ProblemExeption("Bad Request", $"{result.Message}", StatusCodes.Status400BadRequest);

            if (result.Data is null)
                throw new ProblemExeption("Not Found", $"{result.Message}", StatusCodes.Status404NotFound);

            return Ok(new ApiResponse<EmployeeResponse>(true, "Employee successfully retrieved!", result.Data));
        }

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/employees
        ///     {
        ///         "name": "John Doe",
        ///         "email": "john.doe@example.com",
        ///         "age": 31,
        ///         "occupation": "Manager"
        ///     }
        ///
        /// </remarks>
        /// <param name="employee">The employee details to create.</param>
        /// <response code="400">The provided data is incorrect.</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<EmployeeRequest>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(EmployeeRequest employee)
        {
            try
            {
                var result = await _employeeService.CreateEmployeeAsync(employee);

                if (!result.Success)
                    throw new ProblemExeption("Bad Request", $"{result.Message}", StatusCodes.Status400BadRequest);
                
                return Ok(new ApiResponse<EmployeeResponse>(true, "Employee successfully created", result.Data));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing employee's information.
        /// </summary>
        /// <param name="id">The ID of the employee to update.</param>
        /// <param name="employee">The updated employee details.</param>
        /// <response code="400">The provided ID is invalid or data is incorrect.</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">The provided ID is invalid or data is incorrect.</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, EmployeeRequest employee)
        {
            var existingEmployee = await _employeeRepository.GetByIdAsync(id);
            if (existingEmployee is null)
                return NotFound(new { message = "No employee found with the passed Id" });

            var result = await _employeeService.UpdateEmployeeAsync(id, employee);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Deletes an employee by their unique identifier.
        /// </summary>
        /// <param name="id">The ID of the employee to delete.</param>
        /// <response code="200">The employee was successfully deleted.</response>
        /// <response code="400">The provided ID is invalid or data is incorrect.</response>
        /// <response code="404">No employee matches the given ID.</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid Id passed" });

            var result = await _employeeService.DeleteEmployeeAsync(id);

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
