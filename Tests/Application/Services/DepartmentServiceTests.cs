using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.Services;

public class DepartmentServiceTests
{
    private readonly DepartmentService _service;
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
    private readonly Mock<ILogger<DepartmentService>> _mockLogger;
    private readonly Department _testDepartment;

    public DepartmentServiceTests()
    {
        _mockDepartmentRepository = new Mock<IDepartmentRepository>();
        _mockLogger = new Mock<ILogger<DepartmentService>>();
        _service = new DepartmentService(_mockDepartmentRepository.Object, _mockLogger.Object);

        _testDepartment = new Department("Mail", "Handles lightweight parcels up to 1kg");
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new DepartmentService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new DepartmentService(_mockDepartmentRepository.Object, null!));
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ShouldReturnMappedDepartments()
    {
        // Arrange
        var departments = new List<Department> { _testDepartment };
        _mockDepartmentRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(departments);

        // Act
        var result = await _service.GetAllDepartmentsAsync();

        // Assert
        var departmentDtos = result.ToList();
        Assert.Single(departmentDtos);
        Assert.Equal(_testDepartment.Name, departmentDtos.First().Name);
        Assert.Equal(_testDepartment.Description, departmentDtos.First().Description);
    }

    [Fact]
    public async Task GetActiveDepartmentsAsync_ShouldReturnMappedActiveDepartments()
    {
        // Arrange
        var activeDepartments = new List<Department> { _testDepartment };
        _mockDepartmentRepository.Setup(r => r.GetActiveDepartmentsAsync())
            .ReturnsAsync(activeDepartments);

        // Act
        var result = await _service.GetActiveDepartmentsAsync();

        // Assert
        var departmentDtos = result.ToList();
        Assert.Single(departmentDtos);
        Assert.True(departmentDtos.First().IsActive);
    }

    [Fact]
    public async Task GetInactiveDepartmentsAsync_ShouldReturnMappedInactiveDepartments()
    {
        // Arrange
        var inactiveDepartment = new Department("Inactive", "Inactive department");
        inactiveDepartment.Deactivate();
        var inactiveDepartments = new List<Department> { inactiveDepartment };
        
        _mockDepartmentRepository.Setup(r => r.GetInactiveDepartmentsAsync())
            .ReturnsAsync(inactiveDepartments);

        // Act
        var result = await _service.GetInactiveDepartmentsAsync();

        // Assert
        var departmentDtos = result.ToList();
        Assert.Single(departmentDtos);
        Assert.False(departmentDtos.First().IsActive);
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_WithValidId_ShouldReturnMappedDepartment()
    {
        // Arrange
        var departmentId = _testDepartment.Id;
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync(_testDepartment);

        // Act
        var result = await _service.GetDepartmentByIdAsync(departmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testDepartment.Name, result.Name);
        Assert.Equal(_testDepartment.Id, result.Id);
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetDepartmentByIdAsync(Guid.Empty));
        
        Assert.Contains("Department ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _service.GetDepartmentByIdAsync(departmentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetDepartmentByNameAsync_WithValidName_ShouldReturnMappedDepartment()
    {
        // Arrange
        var departmentName = _testDepartment.Name;
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(departmentName))
            .ReturnsAsync(_testDepartment);

        // Act
        var result = await _service.GetDepartmentByNameAsync(departmentName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testDepartment.Name, result.Name);
    }

    [Fact]
    public async Task GetDepartmentByNameAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetDepartmentByNameAsync(""));
        
        Assert.Contains("Department name cannot be empty or null", exception.Message);
    }

    [Fact]
    public async Task GetDepartmentByNameAsync_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetDepartmentByNameAsync("   "));
        
        Assert.Contains("Department name cannot be empty or null", exception.Message);
    }

    [Fact]
    public async Task GetDepartmentByNameAsync_WithNonExistentName_ShouldReturnNull()
    {
        // Arrange
        const string departmentName = "NonExistent";
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(departmentName))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _service.GetDepartmentByNameAsync(departmentName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateDepartmentAsync_WithValidData_ShouldReturnMappedDepartment()
    {
        // Arrange
        var createDto = new CreateDepartmentDto("Express", "Fast delivery department");
        var createdDepartment = new Department(createDto.Name, createDto.Description ?? "");

        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(createDto.Name))
            .ReturnsAsync((Department?)null);
        _mockDepartmentRepository.Setup(r => r.AddAsync(It.IsAny<Department>()))
            .ReturnsAsync(createdDepartment);

        // Act
        var result = await _service.CreateDepartmentAsync(createDto);

        // Assert
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.Description, result.Description);
        _mockDepartmentRepository.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Once);
    }

    [Fact]
    public async Task CreateDepartmentAsync_WithNullDto_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.CreateDepartmentAsync(null!));
    }

    [Fact]
    public async Task CreateDepartmentAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var createDto = new CreateDepartmentDto("", "Description");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreateDepartmentAsync(createDto));
        
        Assert.Contains("Department name is required", exception.Message);
    }

    [Fact]
    public async Task CreateDepartmentAsync_WithExistingName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createDto = new CreateDepartmentDto("Mail", "Another mail department");
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(createDto.Name))
            .ReturnsAsync(_testDepartment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateDepartmentAsync(createDto));
        
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_WithValidData_ShouldReturnUpdatedDepartment()
    {
        // Arrange
        var departmentId = _testDepartment.Id;
        var updateDto = new UpdateDepartmentDto("Updated Mail", "Updated description");

        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync(_testDepartment);
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(updateDto.Name))
            .ReturnsAsync((Department?)null);
        _mockDepartmentRepository.Setup(r => r.UpdateAsync(It.IsAny<Department>()))
            .ReturnsAsync(_testDepartment);

        // Act
        var result = await _service.UpdateDepartmentAsync(departmentId, updateDto);

        // Assert
        Assert.Equal(_testDepartment.Id, result.Id);
        _mockDepartmentRepository.Verify(r => r.UpdateAsync(It.IsAny<Department>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var updateDto = new UpdateDepartmentDto("Updated Name", "Description");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.UpdateDepartmentAsync(Guid.Empty, updateDto));
        
        Assert.Contains("Department ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_WithNonExistentId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var updateDto = new UpdateDepartmentDto("Updated Name", "Description");

        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UpdateDepartmentAsync(departmentId, updateDto));
        
        Assert.Contains("No department found", exception.Message);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_WithConflictingName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var departmentId = _testDepartment.Id;
        var updateDto = new UpdateDepartmentDto("ConflictingName", "Description");
        var conflictingDepartment = new Department("ConflictingName", "Other department");

        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync(_testDepartment);
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(updateDto.Name))
            .ReturnsAsync(conflictingDepartment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UpdateDepartmentAsync(departmentId, updateDto));
        
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task ActivateDepartmentAsync_WithValidId_ShouldReturnActivatedDepartment()
    {
        // Arrange
        var departmentId = _testDepartment.Id;
        _testDepartment.Deactivate(); // Start with inactive

        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync(_testDepartment);
        _mockDepartmentRepository.Setup(r => r.UpdateAsync(It.IsAny<Department>()))
            .ReturnsAsync(_testDepartment);

        // Act
        var result = await _service.ActivateDepartmentAsync(departmentId);

        // Assert
        Assert.Equal(_testDepartment.Id, result.Id);
        _mockDepartmentRepository.Verify(r => r.UpdateAsync(It.IsAny<Department>()), Times.Once);
    }

    [Fact]
    public async Task ActivateDepartmentAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.ActivateDepartmentAsync(Guid.Empty));
        
        Assert.Contains("Department ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task ActivateDepartmentAsync_WithNonExistentId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.ActivateDepartmentAsync(departmentId));
        
        Assert.Contains("No department found", exception.Message);
    }

    [Fact]
    public async Task DeactivateDepartmentAsync_WithValidId_ShouldReturnDeactivatedDepartment()
    {
        // Arrange
        var departmentId = _testDepartment.Id;

        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync(_testDepartment);
        _mockDepartmentRepository.Setup(r => r.UpdateAsync(It.IsAny<Department>()))
            .ReturnsAsync(_testDepartment);

        // Act
        var result = await _service.DeactivateDepartmentAsync(departmentId);

        // Assert
        Assert.Equal(_testDepartment.Id, result.Id);
        _mockDepartmentRepository.Verify(r => r.UpdateAsync(It.IsAny<Department>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateDepartmentAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.DeactivateDepartmentAsync(Guid.Empty));
        
        Assert.Contains("Department ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task DeactivateDepartmentAsync_WithNonExistentId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.DeactivateDepartmentAsync(departmentId));
        
        Assert.Contains("No department found", exception.Message);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WithValidId_ShouldCallRepositoryDelete()
    {
        // Arrange
        var departmentId = _testDepartment.Id;

        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync(_testDepartment);
        _mockDepartmentRepository.Setup(r => r.DeleteAsync(departmentId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteDepartmentAsync(departmentId);

        // Assert
        _mockDepartmentRepository.Verify(r => r.DeleteAsync(departmentId), Times.Once);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.DeleteDepartmentAsync(Guid.Empty));
        
        Assert.Contains("Department ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WithNonExistentId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.DeleteDepartmentAsync(departmentId));
        
        Assert.Contains("No department found", exception.Message);
    }
}
