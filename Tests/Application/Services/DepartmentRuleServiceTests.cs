using Application.DTOs;
using Application.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Interfaces;
using Moq;

namespace Tests.Application.Services;

public class DepartmentRuleServiceTests
{
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
    private readonly Mock<IParcelRepository> _mockParcelRepository;
    private readonly DepartmentRuleService _service;
    private readonly Customer _testCustomer;

    public DepartmentRuleServiceTests()
    {
        _mockParcelRepository = new Mock<IParcelRepository>();
        _mockDepartmentRepository = new Mock<IDepartmentRepository>();
        _service = new DepartmentRuleService(_mockParcelRepository.Object, _mockDepartmentRepository.Object);

        var testAddress = new Address(
            "Marijkestraat",
            "28",
            "",
            "Center",
            "Bosschenhoofd",
            "NB",
            "4744AT",
            "Netherlands"
        );

        _testCustomer = new Customer("João Silva", testAddress);
    }

    [Fact]
    public async Task DetermineRequiredDepartmentsAsync_WithHighValueParcel_ShouldIncludeInsurance()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        var parcel = new Parcel(_testCustomer, 5.5m, 1500.0m);
        var insuranceDept = new Department(DepartmentNames.Insurance, "Insurance Department");

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync(parcel);
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(DepartmentNames.Insurance))
            .ReturnsAsync(insuranceDept);
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(DepartmentNames.Regular))
            .ReturnsAsync(new Department(DepartmentNames.Regular, "Regular Department"));

        // Act
        var result = await _service.DetermineRequiredDepartmentsAsync(parcelId);

        // Assert
        var departmentDtos = result.ToList();
        Assert.NotEmpty(departmentDtos);
        Assert.Contains(departmentDtos, d => d.Name == DepartmentNames.Insurance);
        Assert.Contains(departmentDtos, d => d.Name == DepartmentNames.Regular);
    }

    [Fact]
    public async Task DetermineRequiredDepartmentsAsync_WithLowValueParcel_ShouldNotIncludeInsurance()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        var parcel = new Parcel(_testCustomer, 5.5m, 500.0m);
        var regularDept = new Department(DepartmentNames.Regular, "Regular Department");

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync(parcel);
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(DepartmentNames.Regular))
            .ReturnsAsync(regularDept);

        // Act
        var result = await _service.DetermineRequiredDepartmentsAsync(parcelId);

        // Assert
        var departmentDtos = result.ToList();
        Assert.NotEmpty(departmentDtos);
        Assert.DoesNotContain(departmentDtos, d => d.Name == DepartmentNames.Insurance);
        Assert.Contains(departmentDtos, d => d.Name == DepartmentNames.Regular);
    }

    [Theory]
    [InlineData(0.5, DepartmentNames.Mail)]
    [InlineData(1.0, DepartmentNames.Mail)]
    [InlineData(5.0, DepartmentNames.Regular)]
    [InlineData(10.0, DepartmentNames.Regular)]
    [InlineData(15.0, DepartmentNames.Heavy)]
    [InlineData(100.0, DepartmentNames.Heavy)]
    public async Task GetDepartmentsByWeightAsync_ShouldReturnCorrectDepartment(decimal weight, string expectedDept)
    {
        // Arrange
        var department = new Department(expectedDept, $"{expectedDept} Department");
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(expectedDept))
            .ReturnsAsync(department);

        // Act
        var result = await _service.GetDepartmentsByWeightAsync(weight);

        // Assert
        var departmentDtos = result.ToList();
        Assert.Single(departmentDtos);
        Assert.Equal(expectedDept, departmentDtos.First().Name);
    }

    [Theory]
    [InlineData(500.0, false)]
    [InlineData(1000.0, false)]
    [InlineData(1001.0, true)]
    [InlineData(1500.0, true)]
    public async Task GetDepartmentsByValueAsync_ShouldReturnCorrectDepartments(decimal value,
        bool shouldIncludeInsurance)
    {
        // Arrange
        var insuranceDept = new Department(DepartmentNames.Insurance, "Insurance Department");
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(DepartmentNames.Insurance))
            .ReturnsAsync(insuranceDept);

        // Act
        var result = await _service.GetDepartmentsByValueAsync(value);

        // Assert
        if (shouldIncludeInsurance)
        {
            var departmentDtos = result as DepartmentDto[] ?? result.ToArray();
            Assert.Single(departmentDtos);
            Assert.Equal(DepartmentNames.Insurance, departmentDtos.First().Name);
        }
        else
        {
            Assert.Empty(result);
        }
    }

    [Theory]
    [InlineData(500.0, false)]
    [InlineData(1000.0, false)]
    [InlineData(1001.0, true)]
    [InlineData(1500.0, true)]
    public async Task RequiresInsuranceApprovalAsync_ShouldReturnCorrectValue(decimal value, bool expected)
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        var parcel = new Parcel(_testCustomer, 5.5m, value);

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync(parcel);

        // Act
        var result = await _service.RequiresInsuranceApprovalAsync(parcelId);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task DetermineRequiredDepartmentsAsync_WithNonExistentParcel_ShouldThrowArgumentException()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync((Parcel?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DetermineRequiredDepartmentsAsync(parcelId));
    }

    [Fact]
    public async Task RequiresInsuranceApprovalAsync_WithNonExistentParcel_ShouldThrowArgumentException()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync((Parcel?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RequiresInsuranceApprovalAsync(parcelId));
    }
}