using ModelBaseAPI.Interfaces.Repository;
using ModelBaseAPI.Models.Request;
using ModelBaseAPI.Models.Response;
using ModelBaseAPI.Services;
using Moq;

namespace ModelBaseTest.ServicesTest
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> _employeeRepoMock;
        private readonly EmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _employeeRepoMock = new Mock<IEmployeeRepository>();
            _employeeService = new EmployeeService(_employeeRepoMock.Object);
        }

        #region GetById Method Tests
        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsFailResponse()
        {
            var result = await _employeeService.GetByIdAsync(0);

            Assert.False(result.Success);
            Assert.Equal("Invalid passed Id.", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_EmployeeNotFound_ReturnsFailResponse()
        {
            _employeeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((EmployeeResponse?)null);

            var result = await _employeeService.GetByIdAsync(99);

            Assert.False(result.Success);
            Assert.Contains("No employee found", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_EmployeeExists_ReturnsSuccessResponse()
        {
            var employee = new EmployeeResponse { id = 1, name = "John", email = "john@test.com", age = 30, occupation = "Dev" };
            _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);

            var result = await _employeeService.GetByIdAsync(1);

            Assert.True(result.Success);
            Assert.Equal("John", result.Data?.name);
        }
        #endregion

        #region GetWithPagination Method Tests

        [Fact]
        public async Task GetEmployeePagination_UsesDefaultValues_WhenSortAndOrderAreNull()
        {
            // Arrange
            var mockEmployees = new List<EmployeeResponse>
            {
                new() { id = 1, name = "Alice", email = "alice@test.com", age = 28, occupation = "Analyst" }
            };

            _employeeRepoMock.Setup(r => r.GetEmployeePaginatedOffset(0, 10, "Id", "ASC", null))
                     .ReturnsAsync((1, mockEmployees));

            // Act
            var result = await _employeeService.GetEmployeePagination(0, 10, null, null);

            // Assert
            Assert.Single(result.Employees);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(1, result.CurrentPage);
        }

        [Fact]
        public async Task GetEmployeePagination_ReturnsCorrectPagination()
        {
            // Arrange
            var employees = Enumerable.Range(1, 10).Select(i =>
                new EmployeeResponse
                {
                    id = i,
                    name = $"Emp{i}",
                    email = $"emp{i}@test.com",
                    age = 25 + i,
                    occupation = "Dev"
                }).ToList();

            _employeeRepoMock.Setup(r => r.GetEmployeePaginatedOffset(10, 10, "Name", "DESC", null))
                     .ReturnsAsync((30, employees));

            // Act
            var result = await _employeeService.GetEmployeePagination(10, 10, "Name", "DESC");

            // Assert
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(10, result.Employees.Count);
        }

        [Fact]
        public async Task GetEmployeePagination_InvalidSortColumn_ThrowsArgumentException()
        {
            // Arrange
            var invalidSort = "InvalidColumn";

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _employeeService.GetEmployeePagination(0, 10, invalidSort, "ASC"));

            // Assert
            Assert.Equal("The order is invalid.", ex.Message);
        }

        #endregion

        #region Create Method Test

        [Fact]
        public async Task CreateEmployeeAsync_ReturnsError_WhenValidationFails()
        {
            // Arrange
            var invalidRequest = new EmployeeRequest { Email = "", Name = "" }; // inválido para o FluentValidation
            var service = new EmployeeService(_employeeRepoMock.Object); // novo para isolar _allowedColumns

            // Act
            var result = await service.CreateEmployeeAsync(invalidRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Invalid arguments", result.Message);
        }

        [Fact]
        public async Task CreateEmployeeAsync_ReturnsError_WhenEmailExists()
        {
            // Arrange
            var request = new EmployeeRequest { Email = "test@example.com", Name = "Test", Age = 30, Occupation = "Dev" };
            _employeeRepoMock.Setup(r => r.EmployeeExistsByEmail(request.Email)).ReturnsAsync(true);

            var service = new EmployeeService(_employeeRepoMock.Object);

            // Act
            var result = await service.CreateEmployeeAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Already exist a Employee with the passed email.", result.Message);
        }

        [Fact]
        public async Task CreateEmployeeAsync_ReturnsSuccess_WhenValid()
        {
            // Arrange
            var request = new EmployeeRequest { Email = "new@example.com", Name = "New", Age = 25, Occupation = "QA" };
            var newEmployee = new EmployeeResponse { id = 1, name = request.Name, email = request.Email, age = request.Age, occupation = request.Occupation };

            _employeeRepoMock.Setup(r => r.EmployeeExistsByEmail(request.Email)).ReturnsAsync(false);
            _employeeRepoMock.Setup(r => r.AddAsync(request)).ReturnsAsync(newEmployee);

            var service = new EmployeeService(_employeeRepoMock.Object);

            // Act
            var result = await service.CreateEmployeeAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(request.Email, result.Data?.email);
        }

        #endregion

        #region Update Method Test
        [Fact]
        public async Task UpdateEmployeeAsync_Throws_WhenEmailExists()
        {
            var request = new EmployeeRequest { Email = "used@example.com", Name = "A", Age = 20, Occupation = "Dev" };
            _employeeRepoMock.Setup(r => r.EmployeeExistsByEmail(request.Email)).ReturnsAsync(true);
            _employeeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new EmployeeResponse());

            var service = new EmployeeService(_employeeRepoMock.Object);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateEmployeeAsync(1, request));

            Assert.Contains("Another employee already has this email", ex.Message);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_Throws_WhenEmployeeNotFound()
        {
            var request = new EmployeeRequest { Email = "x@example.com", Name = "A", Age = 20, Occupation = "Dev" };
            _employeeRepoMock.Setup(r => r.EmployeeExistsByEmail(request.Email)).ReturnsAsync(false);
            _employeeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((EmployeeResponse?)null);

            var service = new EmployeeService(_employeeRepoMock.Object);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateEmployeeAsync(1, request));

            Assert.Contains("No Employee found", ex.Message);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_Success_WhenValid()
        {
            var request = new EmployeeRequest { Email = "x@example.com", Name = "A", Age = 20, Occupation = "Dev" };
            _employeeRepoMock.Setup(r => r.EmployeeExistsByEmail(request.Email)).ReturnsAsync(false);
            _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new EmployeeResponse());
            _employeeRepoMock.Setup(r => r.UpdateAsync(1, request)).Returns(Task.CompletedTask);

            var service = new EmployeeService(_employeeRepoMock.Object);

            var result = await service.UpdateEmployeeAsync(1, request);

            Assert.True(result.Success);
            Assert.Equal("Employee updated successfully", result.Message);
        }
        #endregion
        
        #region Delete Method Tests

        [Fact]
        public async Task DeleteEmployeeAsync_Throws_WhenNotFound()
        {
         
            // Arrange

            _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((EmployeeResponse?)null);

            // Act

            var service = new EmployeeService(_employeeRepoMock.Object);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.DeleteEmployeeAsync(1));

            // Assert

            Assert.Contains("No Employee found", ex.Message);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_Success_WhenExists()
        {
            // Arrange

            _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new EmployeeResponse());
            _employeeRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act

            var service = new EmployeeService(_employeeRepoMock.Object);

            var result = await service.DeleteEmployeeAsync(1);

            // Assert 

            Assert.True(result.Success);
            Assert.Equal("Employee removed successfully", result.Message);
        }
        
        #endregion

    }
}
