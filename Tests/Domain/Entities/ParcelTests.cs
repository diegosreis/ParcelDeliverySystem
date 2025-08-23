using Domain.Entities;
using Domain.Enums;

namespace Tests.Domain.Entities;

public class ParcelTests
{
    private readonly Customer _testCustomer;

    public ParcelTests()
    {
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
    public void Constructor_WithValidData_ShouldCreateParcel()
    {
        // Arrange & Act
        var parcel = new Parcel(_testCustomer, 5.5m, 100.0m);

        // Assert
        Assert.NotEqual(Guid.Empty, parcel.Id);
        Assert.Equal(_testCustomer, parcel.Recipient);
        Assert.Equal(5.5m, parcel.Weight);
        Assert.Equal(100.0m, parcel.Value);
        Assert.Equal(ParcelStatus.Pending, parcel.Status);
        Assert.Empty(parcel.AssignedDepartments);
        Assert.NotEqual(default, parcel.CreatedAt);
        Assert.Null(parcel.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithNullRecipient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Parcel(null!, 5.5m, 100.0m));
    }

    [Fact]
    public void Constructor_WithZeroWeight_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Parcel(_testCustomer, 0m, 100.0m));
        Assert.Contains("Weight must be greater than 0", exception.Message);
    }

    [Fact]
    public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Parcel(_testCustomer, 5.5m, -10.0m));
        Assert.Contains("Value cannot be negative", exception.Message);
    }

    [Theory]
    [InlineData(0.5, false)]
    [InlineData(1.0, false)]
    [InlineData(5.0, false)]
    [InlineData(10.0, false)]
    [InlineData(15.0, false)]
    [InlineData(1000.0, false)]
    [InlineData(1001.0, true)]
    [InlineData(1500.0, true)]
    public void RequiresInsuranceApproval_ShouldReturnCorrectValue(decimal value, bool expected)
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 5.5m, value);

        // Act
        var result = parcel.RequiresInsuranceApproval;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.5, true, false, false)]
    [InlineData(1.0, true, false, false)]
    [InlineData(5.0, false, true, false)]
    [InlineData(10.0, false, true, false)]
    [InlineData(15.0, false, false, true)]
    [InlineData(100.0, false, false, true)]
    public void WeightBasedProperties_ShouldReturnCorrectValues(decimal weight, bool isMail, bool isRegular,
        bool isHeavy)
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, weight, 100.0m);

        // Act & Assert
        Assert.Equal(isMail, parcel.IsMailParcel);
        Assert.Equal(isRegular, parcel.IsRegularParcel);
        Assert.Equal(isHeavy, parcel.IsHeavyParcel);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateParcel()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 5.5m, 100.0m);
        var originalCreatedAt = parcel.CreatedAt;

        // Act
        parcel.Update(7.5m, 150.0m);

        // Assert
        Assert.Equal(7.5m, parcel.Weight);
        Assert.Equal(150.0m, parcel.Value);
        Assert.Equal(originalCreatedAt, parcel.CreatedAt);
        Assert.NotNull(parcel.UpdatedAt);
    }

    [Fact]
    public void AssignDepartment_WithValidDepartment_ShouldAddDepartment()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 5.5m, 100.0m);
        var department = new Department("Test Department", "Test Description");

        // Act
        parcel.AssignDepartment(department);

        // Assert
        Assert.Single(parcel.AssignedDepartments);
        Assert.Contains(department, parcel.AssignedDepartments);
    }

    [Fact]
    public void AssignDepartment_WithNullDepartment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 5.5m, 100.0m);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => parcel.AssignDepartment(null!));
    }

    [Fact]
    public void AssignDepartment_WithSameDepartmentTwice_ShouldNotAddDuplicate()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 5.5m, 100.0m);
        var department = new Department("Test Department", "Test Description");

        // Act
        parcel.AssignDepartment(department);
        parcel.AssignDepartment(department);

        // Assert
        Assert.Single(parcel.AssignedDepartments);
    }

    [Fact]
    public void RemoveDepartment_WithValidDepartment_ShouldRemoveDepartment()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 5.5m, 100.0m);
        var department = new Department("Test Department", "Test Description");
        parcel.AssignDepartment(department);

        // Act
        parcel.RemoveDepartment(department);

        // Assert
        Assert.Empty(parcel.AssignedDepartments);
    }

    [Fact]
    public void RemoveDepartment_WithNullDepartment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 5.5m, 100.0m);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => parcel.RemoveDepartment(null!));
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateStatus()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 5.5m, 100.0m);

        // Act
        parcel.UpdateStatus(ParcelStatus.Processing);

        // Assert
        Assert.Equal(ParcelStatus.Processing, parcel.Status);
        Assert.NotNull(parcel.UpdatedAt);
    }
}