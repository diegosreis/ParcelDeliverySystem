using Api.Controllers;
using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Api.Controllers;

public class DepartmentsControllerTests
{
    private readonly DepartmentsController _controller;
    private readonly Mock<IDepartmentService> _mockDepartmentService;
    private readonly DepartmentDto _testDepartmentDto;

    public DepartmentsControllerTests()
    {
        _mockDepartmentService = new Mock<IDepartmentService>();
        _controller = new DepartmentsController(_mockDepartmentService.Object);

        // Setup test data
        _testDepartmentDto = new DepartmentDto(
            Guid.NewGuid(),
            "Mail",
            "Handles lightweight parcels up to 1kg",
            true,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
    }

    [Fact]
    public void Constructor_WithNullDepartmentService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DepartmentsController(null!));
    }

    [Fact]
    public async Task GetDepartments_WhenDepartmentsExist_ShouldReturnOkWithDepartments()
    {
        // Arrange
        var departments = new List<DepartmentDto> { _testDepartmentDto };
        _mockDepartmentService.Setup(s => s.GetAllDepartmentsAsync())
            .ReturnsAsync(departments);

        // Act
        var result = await _controller.GetDepartments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartments = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value).ToList();
        Assert.Single(returnedDepartments);
        Assert.Equal(_testDepartmentDto.Name, returnedDepartments.First().Name);
    }

    [Fact]
    public async Task GetDepartments_WhenNoDepartments_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.GetAllDepartmentsAsync())
            .ReturnsAsync(new List<DepartmentDto>());

        // Act
        var result = await _controller.GetDepartments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartments = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value).ToList();
        Assert.Empty(returnedDepartments);
    }

    [Fact]
    public async Task GetDepartments_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.GetAllDepartmentsAsync())
            .ThrowsAsync(new Exception("Service error"));

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
        var activeDepartments = new List<DepartmentDto> { _testDepartmentDto };
        _mockDepartmentService.Setup(s => s.GetActiveDepartmentsAsync())
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
    public async Task GetActiveDepartments_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.GetActiveDepartmentsAsync())
            .ThrowsAsync(new Exception("Service error"));

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
        var inactiveDepartmentDto = new DepartmentDto(
            Guid.NewGuid(),
            "Inactive",
            "Inactive department",
            false,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        var inactiveDepartments = new List<DepartmentDto> { inactiveDepartmentDto };

        _mockDepartmentService.Setup(s => s.GetInactiveDepartmentsAsync())
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
        var departmentId = _testDepartmentDto.Id;
        _mockDepartmentService.Setup(s => s.GetDepartmentByIdAsync(departmentId))
            .ReturnsAsync(_testDepartmentDto);

        // Act
        var result = await _controller.GetDepartment(departmentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartment = Assert.IsType<DepartmentDto>(okResult.Value);
        Assert.Equal(_testDepartmentDto.Name, returnedDepartment.Name);
    }

    [Fact]
    public async Task GetDepartment_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.GetDepartmentByIdAsync(Guid.Empty))
            .ThrowsAsync(new ArgumentException("Department ID cannot be empty"));

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
        _mockDepartmentService.Setup(s => s.GetDepartmentByIdAsync(departmentId))
            .ReturnsAsync((DepartmentDto?)null);

        // Act
        var result = await _controller.GetDepartment(departmentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetDepartment_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentService.Setup(s => s.GetDepartmentByIdAsync(departmentId))
            .ThrowsAsync(new Exception("Service error"));

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
        var departmentName = _testDepartmentDto.Name;
        _mockDepartmentService.Setup(s => s.GetDepartmentByNameAsync(departmentName))
            .ReturnsAsync(_testDepartmentDto);

        // Act
        var result = await _controller.GetDepartmentByName(departmentName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartment = Assert.IsType<DepartmentDto>(okResult.Value);
        Assert.Equal(_testDepartmentDto.Name, returnedDepartment.Name);
    }

    [Fact]
    public async Task GetDepartmentByName_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.GetDepartmentByNameAsync(""))
            .ThrowsAsync(new ArgumentException("Department name cannot be empty or null"));

        // Act
        var result = await _controller.GetDepartmentByName("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentByName_WithWhitespaceName_ShouldReturnBadRequest()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.GetDepartmentByNameAsync("   "))
            .ThrowsAsync(new ArgumentException("Department name cannot be empty or null"));

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
        const string departmentName = "NonExistent";
        _mockDepartmentService.Setup(s => s.GetDepartmentByNameAsync(departmentName))
            .ReturnsAsync((DepartmentDto?)null);

        // Act
        var result = await _controller.GetDepartmentByName(departmentName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentByName_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        const string departmentName = "TestDepartment";
        _mockDepartmentService.Setup(s => s.GetDepartmentByNameAsync(departmentName))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetDepartmentByName(departmentName);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_WithValidData_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateDepartmentDto("Express", "Fast delivery department");
        var createdDepartmentDto = new DepartmentDto(
            Guid.NewGuid(),
            createDto.Name,
            createDto.Description!,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _mockDepartmentService.Setup(s => s.CreateDepartmentAsync(createDto))
            .ReturnsAsync(createdDepartmentDto);

        // Act
        var result = await _controller.CreateDepartment(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(_controller.GetDepartment), createdResult.ActionName);

        var returnedDepartment = Assert.IsType<DepartmentDto>(createdResult.Value);
        Assert.Equal(createDto.Name, returnedDepartment.Name);
        Assert.Equal(createDto.Description, returnedDepartment.Description);
    }

    [Fact]
    public async Task CreateDepartment_WithNullData_ShouldReturnBadRequest()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.CreateDepartmentAsync(null!))
            .ThrowsAsync(new ArgumentNullException("createDepartmentDto"));

        // Act
        var result = await _controller.CreateDepartment(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateDepartmentDto("", "Description");
        _mockDepartmentService.Setup(s => s.CreateDepartmentAsync(createDto))
            .ThrowsAsync(new ArgumentException("Department name is required"));

        // Act
        var result = await _controller.CreateDepartment(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_WithExistingName_ShouldReturnConflict()
    {
        // Arrange
        var createDto = new CreateDepartmentDto("Mail", "Another mail department");
        _mockDepartmentService.Setup(s => s.CreateDepartmentAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("A department with name 'Mail' already exists"));

        // Act
        var result = await _controller.CreateDepartment(createDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var createDto = new CreateDepartmentDto("Express", "Fast delivery");
        _mockDepartmentService.Setup(s => s.CreateDepartmentAsync(createDto))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.CreateDepartment(createDto);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateDepartment_WithValidData_ShouldReturnOkWithUpdatedDepartment()
    {
        // Arrange
        var departmentId = _testDepartmentDto.Id;
        var updateDto = new UpdateDepartmentDto("Updated Mail", "Updated description");
        var updatedDepartmentDto = new DepartmentDto(
            departmentId,
            updateDto.Name,
            updateDto.Description!,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _mockDepartmentService.Setup(s => s.UpdateDepartmentAsync(departmentId, updateDto))
            .ReturnsAsync(updatedDepartmentDto);

        // Act
        var result = await _controller.UpdateDepartment(departmentId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartment = Assert.IsType<DepartmentDto>(okResult.Value);
        Assert.Equal(departmentId, returnedDepartment.Id);

        // Verify service call
        _mockDepartmentService.Verify(s => s.UpdateDepartmentAsync(departmentId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateDepartment_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var updateDto = new UpdateDepartmentDto("Updated Name", "Updated description");
        _mockDepartmentService.Setup(s => s.UpdateDepartmentAsync(Guid.Empty, updateDto))
            .ThrowsAsync(new ArgumentException("Department ID cannot be empty"));

        // Act
        var result = await _controller.UpdateDepartment(Guid.Empty, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task UpdateDepartment_WithNullData_ShouldReturnBadRequest()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentService.Setup(s => s.UpdateDepartmentAsync(departmentId, null!))
            .ThrowsAsync(new ArgumentNullException("updateDepartmentDto"));

        // Act
        var result = await _controller.UpdateDepartment(departmentId, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task UpdateDepartment_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var updateDto = new UpdateDepartmentDto("Updated Name", "Updated description");

        _mockDepartmentService.Setup(s => s.UpdateDepartmentAsync(departmentId, updateDto))
            .ThrowsAsync(new InvalidOperationException($"No department found with ID: {departmentId}"));

        // Act
        var result = await _controller.UpdateDepartment(departmentId, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task UpdateDepartment_WithConflictingName_ShouldReturnConflict()
    {
        // Arrange
        var departmentId = _testDepartmentDto.Id;
        var updateDto = new UpdateDepartmentDto("ConflictingName", "Description");

        _mockDepartmentService.Setup(s => s.UpdateDepartmentAsync(departmentId, updateDto))
            .ThrowsAsync(new InvalidOperationException("Another department with name 'ConflictingName' already exists"));

        // Act
        var result = await _controller.UpdateDepartment(departmentId, updateDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
    }

    [Fact]
    public async Task ActivateDepartment_WithValidId_ShouldReturnOkWithActivatedDepartment()
    {
        // Arrange
        var departmentId = _testDepartmentDto.Id;
        var activatedDepartmentDto = new DepartmentDto(
            departmentId,
            _testDepartmentDto.Name,
            _testDepartmentDto.Description,
            true,
            _testDepartmentDto.CreatedAt,
            DateTime.UtcNow
        );

        _mockDepartmentService.Setup(s => s.ActivateDepartmentAsync(departmentId))
            .ReturnsAsync(activatedDepartmentDto);

        // Act
        var result = await _controller.ActivateDepartment(departmentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartment = Assert.IsType<DepartmentDto>(okResult.Value);
        Assert.Equal(departmentId, returnedDepartment.Id);

        // Verify service call
        _mockDepartmentService.Verify(s => s.ActivateDepartmentAsync(departmentId), Times.Once);
    }

    [Fact]
    public async Task ActivateDepartment_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.ActivateDepartmentAsync(Guid.Empty))
            .ThrowsAsync(new ArgumentException("Department ID cannot be empty"));

        // Act
        var result = await _controller.ActivateDepartment(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ActivateDepartment_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentService.Setup(s => s.ActivateDepartmentAsync(departmentId))
            .ThrowsAsync(new InvalidOperationException($"No department found with ID: {departmentId}"));

        // Act
        var result = await _controller.ActivateDepartment(departmentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task DeactivateDepartment_WithValidId_ShouldReturnOkWithDeactivatedDepartment()
    {
        // Arrange
        var departmentId = _testDepartmentDto.Id;
        var deactivatedDepartmentDto = new DepartmentDto(
            departmentId,
            _testDepartmentDto.Name,
            _testDepartmentDto.Description,
            false,
            _testDepartmentDto.CreatedAt,
            DateTime.UtcNow
        );

        _mockDepartmentService.Setup(s => s.DeactivateDepartmentAsync(departmentId))
            .ReturnsAsync(deactivatedDepartmentDto);

        // Act
        var result = await _controller.DeactivateDepartment(departmentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartment = Assert.IsType<DepartmentDto>(okResult.Value);
        Assert.Equal(departmentId, returnedDepartment.Id);

        // Verify service call
        _mockDepartmentService.Verify(s => s.DeactivateDepartmentAsync(departmentId), Times.Once);
    }

    [Fact]
    public async Task DeactivateDepartment_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.DeactivateDepartmentAsync(Guid.Empty))
            .ThrowsAsync(new ArgumentException("Department ID cannot be empty"));

        // Act
        var result = await _controller.DeactivateDepartment(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task DeactivateDepartment_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentService.Setup(s => s.DeactivateDepartmentAsync(departmentId))
            .ThrowsAsync(new InvalidOperationException($"No department found with ID: {departmentId}"));

        // Act
        var result = await _controller.DeactivateDepartment(departmentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task DeleteDepartment_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var departmentId = _testDepartmentDto.Id;

        _mockDepartmentService.Setup(s => s.DeleteDepartmentAsync(departmentId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteDepartment(departmentId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);

        // Verify service call
        _mockDepartmentService.Verify(s => s.DeleteDepartmentAsync(departmentId), Times.Once);
    }

    [Fact]
    public async Task DeleteDepartment_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        _mockDepartmentService.Setup(s => s.DeleteDepartmentAsync(Guid.Empty))
            .ThrowsAsync(new ArgumentException("Department ID cannot be empty"));

        // Act
        var result = await _controller.DeleteDepartment(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task DeleteDepartment_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockDepartmentService.Setup(s => s.DeleteDepartmentAsync(departmentId))
            .ThrowsAsync(new InvalidOperationException($"No department found with ID: {departmentId}"));

        // Act
        var result = await _controller.DeleteDepartment(departmentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task DeleteDepartment_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var departmentId = _testDepartmentDto.Id;
        _mockDepartmentService.Setup(s => s.DeleteDepartmentAsync(departmentId))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.DeleteDepartment(departmentId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }
}