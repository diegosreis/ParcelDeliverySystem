using Domain.Entities;

namespace Tests.Domain.Entities;

public class AddressTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateAddress()
    {
        // Arrange & Act
        var address = new Address(
            "Marijkestraat",
            "28",
            "",
            "Center",
            "Bosschenhoofd",
            "NB",
            "4744AT",
            "Netherlands"
        );

        // Assert
        Assert.NotEqual(Guid.Empty, address.Id);
        Assert.Equal("Marijkestraat", address.Street);
        Assert.Equal("28", address.Number);
        Assert.Equal("", address.Complement);
        Assert.Equal("Center", address.Neighborhood);
        Assert.Equal("Bosschenhoofd", address.City);
        Assert.Equal("NB", address.State);
        Assert.Equal("4744AT", address.ZipCode);
        Assert.Equal("Netherlands", address.Country);
        Assert.NotEqual(default, address.CreatedAt);
        Assert.Null(address.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithEmptyStreet_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Address(
            "", "28", "", "Center", "Bosschenhoofd", "NB", "4744AT", "Netherlands"));
        Assert.Contains("Street is required and cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyNumber_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Address(
            "Marijkestraat", "", "", "Center", "Bosschenhoofd", "NB", "4744AT", "Netherlands"));
        Assert.Contains("Number is required and cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithInvalidZipCode_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Address(
            "Marijkestraat", "28", "", "Center", "Bosschenhoofd", "NB", "12AB", "Netherlands"));
        Assert.Contains("Zip code must be in Dutch format", exception.Message);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAddress()
    {
        // Arrange
        var address = new Address(
            "Marijkestraat",
            "28",
            "",
            "Center",
            "Bosschenhoofd",
            "NB",
            "4744AT",
            "Netherlands"
        );

        var originalCreatedAt = address.CreatedAt;

        // Act
        address.Update(
            "Meester Willemstraat",
            "111",
            "Unit 1",
            "Downtown",
            "Rotterdam",
            "ZH",
            "3036MN",
            "Netherlands"
        );

        // Assert
        Assert.Equal("Meester Willemstraat", address.Street);
        Assert.Equal("111", address.Number);
        Assert.Equal("Unit 1", address.Complement);
        Assert.Equal("Downtown", address.Neighborhood);
        Assert.Equal("Rotterdam", address.City);
        Assert.Equal("ZH", address.State);
        Assert.Equal("3036MN", address.ZipCode);
        Assert.Equal("Netherlands", address.Country);
        Assert.Equal(originalCreatedAt, address.CreatedAt);
        Assert.NotNull(address.UpdatedAt);
    }

    [Fact]
    public void GetFormattedAddress_ShouldReturnFormattedString()
    {
        // Arrange
        var address = new Address(
            "Marijkestraat",
            "28",
            "Apt 1",
            "Center",
            "Bosschenhoofd",
            "NB",
            "4744AT",
            "Netherlands"
        );

        // Act
        var formatted = address.GetFormattedAddress();

        // Assert
        Assert.Equal("Marijkestraat, 28, Apt 1, Center, Bosschenhoofd - NB, 4744AT, Netherlands", formatted);
    }

    [Fact]
    public void GetFormattedAddress_WithoutComplement_ShouldReturnFormattedString()
    {
        // Arrange
        var address = new Address(
            "Marijkestraat",
            "28",
            "",
            "Center",
            "Bosschenhoofd",
            "NB",
            "4744AT",
            "Netherlands"
        );

        // Act
        var formatted = address.GetFormattedAddress();

        // Assert
        Assert.Equal("Marijkestraat, 28, Center, Bosschenhoofd - NB, 4744AT, Netherlands", formatted);
    }

    [Fact]
    public void Constructor_WithLowercaseZipCode_ShouldNormalizeToUppercase()
    {
        // Arrange & Act
        var address = new Address(
            "Marijkestraat",
            "28",
            "",
            "Center",
            "Bosschenhoofd",
            "NB",
            "4744at",
            "Netherlands"
        );

        // Assert
        Assert.Equal("4744AT", address.ZipCode);
    }
}