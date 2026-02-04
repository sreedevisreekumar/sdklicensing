using Xunit;
using UsbDongleLicensing.Providers;
using UsbDongleLicensing.Core;

namespace UsbDongleLicensing.Tests.Providers;

/// <summary>
/// Unit tests for the HidDongleProvider class.
/// Note: These tests verify the provider's behavior without requiring physical hardware.
/// </summary>
public class HidDongleProviderTests
{
    [Fact]
    public void Constructor_WithValidVendorAndProductIds_ShouldInitialize()
    {
        // Arrange & Act
        var provider = new HidDongleProvider(0x1234, 0x5678);

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public void Constructor_WithZeroVendorId_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new HidDongleProvider(0, 0x5678));
        Assert.Contains("Vendor ID must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_WithNegativeVendorId_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new HidDongleProvider(-1, 0x5678));
        Assert.Contains("Vendor ID must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_WithZeroProductId_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new HidDongleProvider(0x1234, 0));
        Assert.Contains("Product ID must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_WithNegativeProductId_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new HidDongleProvider(0x1234, -1));
        Assert.Contains("Product ID must be greater than zero", exception.Message);
    }

    [Fact]
    public void IsConnected_WithNoMatchingDevice_ShouldReturnFalse()
    {
        // Arrange
        // Use unlikely vendor/product ID combination that won't match any real device
        using var provider = new HidDongleProvider(0xFFFF, 0xFFFF);

        // Act
        var isConnected = provider.IsConnected();

        // Assert
        Assert.False(isConnected);
    }

    [Fact]
    public void DetectDongle_WithNoMatchingDevice_ShouldReturnNull()
    {
        // Arrange
        // Use unlikely vendor/product ID combination that won't match any real device
        using var provider = new HidDongleProvider(0xFFFF, 0xFFFF);

        // Act
        var dongleInfo = provider.DetectDongle();

        // Assert
        Assert.Null(dongleInfo);
    }

    [Fact]
    public void ReadLicenseData_WhenNotConnected_ShouldThrowInvalidOperationException()
    {
        // Arrange
        // Use unlikely vendor/product ID combination that won't match any real device
        using var provider = new HidDongleProvider(0xFFFF, 0xFFFF);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => provider.ReadLicenseData());
        Assert.Contains("No USB dongle is connected", exception.Message);
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // Arrange
        var provider = new HidDongleProvider(0x1234, 0x5678);

        // Act & Assert
        provider.Dispose();
        // No exception should be thrown
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrowException()
    {
        // Arrange
        var provider = new HidDongleProvider(0x1234, 0x5678);

        // Act & Assert
        provider.Dispose();
        provider.Dispose(); // Second call should be safe
        // No exception should be thrown
    }

    [Fact]
    public void DongleConnected_Event_ShouldBeInitialized()
    {
        // Arrange
        using var provider = new HidDongleProvider(0x1234, 0x5678);
        bool eventRaised = false;

        // Act
        provider.DongleConnected += (sender, args) => { eventRaised = true; };

        // Assert
        // Event handler should be attached without throwing
        Assert.False(eventRaised); // Event shouldn't be raised just by attaching handler
    }

    [Fact]
    public void DongleDisconnected_Event_ShouldBeInitialized()
    {
        // Arrange
        using var provider = new HidDongleProvider(0x1234, 0x5678);
        bool eventRaised = false;

        // Act
        provider.DongleDisconnected += (sender, args) => { eventRaised = true; };

        // Assert
        // Event handler should be attached without throwing
        Assert.False(eventRaised); // Event shouldn't be raised just by attaching handler
    }
}
