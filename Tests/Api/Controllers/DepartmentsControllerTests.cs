using Api.Controllers;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Api.Controllers;

public class DepartmentsControllerTests
{
    private readonly DepartmentsController _controller;
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
    private readonly Department _testDepartment;
    private readonly DepartmentDto _testDepartmentDto;

    public DepartmentsControllerTests()
    {
        _mockDepartmentRepository = new Mock<IDepartmentRepository>();
        _controller = new DepartmentsController(_mockDepartmentRepository.Object);

        // Setup test data
        _testDepartment = new Department("Mail", "Handles lightweight parcels up to 1kg");
        _testDepartmentDto = new DepartmentDto(
            _testDepartment.Id,
            _testDepartment.Name,
            _testDepartment.Description,
            _testDepartment.IsActive,
            _testDepartment.CreatedAt,
            _testDepartment.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithNullDepartmentRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DepartmentsController(null!));
    }

    [Fact]
    public async Task GetDepartments_WhenDepartmentsExist_ShouldReturnOkWithDepartments()
    {
        // Arrange
        var departments = new List<Department> { _testDepartment };
        _mockDepartmentRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(departments);

        // Act
        var result = await _controller.GetDepartments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartments = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value).ToList();
        Assert.Single(returnedDepartments);
        Assert.Equal(_testDepartment.Name, returnedDepartments.First().Name);
    }

    [Fact]
    public async Task GetDepartments_WhenNoDepartments_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        _mockDepartmentRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Department>());

        // Act
        var result = await _controller.GetDepartments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartments = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value).ToList();
        Assert.Empty(returnedDepartments);
    }

    [Fact]
    public async Task GetDepartments_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockDepartmentRepository.Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetDepartments();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetActiveDepartments_WhenActiveDepartmentsExist_ShouldReturnOkWithActiveDepartments()
    {
        // Arrange
        var activeDepartments = new List<Department> { _testDepartment };
        _mockDepartmentRepository.Setup(r => r.GetActiveDepartmentsAsync())
            .ReturnsAsync(activeDepartments);

        // Act
        var result = await _controller.GetActiveDepartments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartments = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value).ToList();
        Assert.Single(returnedDepartments);
        Assert.True(returnedDepartments.First().IsActive);
    }

    [Fact]
    public async Task GetActiveDepartments_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockDepartmentRepository.Setup(r => r.GetActiveDepartmentsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetActiveDepartments();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetInactiveDepartments_WhenInactiveDepartmentsExist_ShouldReturnOkWithInactiveDepartments()
    {
        // Arrange
        var inactiveDepartment = new Department("Inactive", "Inactive department");
        inactiveDepartment.Deactivate();
        var inactiveDepartments = new List<Department> { inactiveDepartment };

        _mockDepartmentRepository.Setup(r => r.GetInactiveDepartmentsAsync())
            .ReturnsAsync(inactiveDepartments);

        // Act
        var result = await _controller.GetInactiveDepartments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartments = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value).ToList();
        Assert.Single(returnedDepartments);
        Assert.False(returnedDepartments.First().IsActive);
    }

    [Fact]
    public async Task GetDepartment_WithValidId_ShouldReturnOkWithDepartment()
    {
        // Arrange
        var departmentId = _testDepartment.Id;
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync(_testDepartment);

        // Act
        var result = await _controller.GetDepartment(departmentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartment = Assert.IsType<DepartmentDto>(okResult.Value);
        Assert.Equal(_testDepartment.Name, returnedDepartment.Name);
    }

    [Fact]
    public async Task GetDepartment_WithEmptyId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetDepartment(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetDepartment_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _controller.GetDepartment(departmentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetDepartment_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetDepartment(departmentId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentByName_WithValidName_ShouldReturnOkWithDepartment()
    {
        // Arrange
        var departmentName = _testDepartment.Name;
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(departmentName))
            .ReturnsAsync(_testDepartment);

        // Act
        var result = await _controller.GetDepartmentByName(departmentName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartment = Assert.IsType<DepartmentDto>(okResult.Value);
        Assert.Equal(_testDepartment.Name, returnedDepartment.Name);
    }

    [Fact]
    public async Task GetDepartmentByName_WithEmptyName_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetDepartmentByName("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentByName_WithWhitespaceName_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetDepartmentByName("   ");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentByName_WithNonExistentName_ShouldReturnNotFound()
    {
        // Arrange
        var departmentName = "NonExistent";
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(departmentName))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _controller.GetDepartmentByName(departmentName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentByName_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        var departmentName = "TestDepartment";
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(departmentName))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetDepartmentByName(departmentName);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }
}