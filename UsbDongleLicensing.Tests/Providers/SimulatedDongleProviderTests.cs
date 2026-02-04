using System.Text;
using System.Text.Json;
using UsbDongleLicensing.Core;
using UsbDongleLicensing.Providers;
using Xunit;

namespace UsbDongleLicensing.Tests.Providers;

/// <summary>
/// Unit tests for the SimulatedDongleProvider class.
/// </summary>
public class SimulatedDongleProviderTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _licenseFileName = "license.json";

    public SimulatedDongleProviderTests()
    {
        // Create a unique test directory for each test run
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SimulatedDongleTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        // Clean up test directory after tests
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public void Constructor_CreatesSimulationDirectory_WhenItDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "new_directory");

        // Act
        using var provider = new SimulatedDongleProvider(nonExistentPath);

        // Assert
        Assert.True(Directory.Exists(nonExistentPath));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenPathIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SimulatedDongleProvider(null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenPathIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SimulatedDongleProvider(string.Empty));
    }

    [Fact]
    public void IsConnected_ReturnsFalse_WhenLicenseFileDoesNotExist()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);

        // Act
        var isConnected = provider.IsConnected();

        // Assert
        Assert.False(isConnected);
    }

    [Fact]
    public void IsConnected_ReturnsTrue_WhenLicenseFileExists()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);
        var licenseFilePath = Path.Combine(_testDirectory, _licenseFileName);
        CreateValidLicenseFile(licenseFilePath);

        // Act
        var isConnected = provider.IsConnected();

        // Assert
        Assert.True(isConnected);
    }

    [Fact]
    public void DetectDongle_ReturnsNull_WhenLicenseFileDoesNotExist()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);

        // Act
        var dongleInfo = provider.DetectDongle();

        // Assert
        Assert.Null(dongleInfo);
    }

    [Fact]
    public void DetectDongle_ReturnsDongleInfo_WhenLicenseFileExists()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);
        var licenseFilePath = Path.Combine(_testDirectory, _licenseFileName);
        CreateValidLicenseFile(licenseFilePath);

        // Act
        var dongleInfo = provider.DetectDongle();

        // Assert
        Assert.NotNull(dongleInfo);
        Assert.Equal("SIM", dongleInfo.VendorId);
        Assert.Equal("0001", dongleInfo.ProductId);
        Assert.Equal("SIMULATED", dongleInfo.SerialNumber);
        Assert.True((DateTime.UtcNow - dongleInfo.DetectedAt).TotalSeconds < 5);
    }

    [Fact]
    public void ReadLicenseData_ThrowsInvalidOperationException_WhenNoLicenseFileExists()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => provider.ReadLicenseData());
        Assert.Contains("No simulated dongle is connected", exception.Message);
    }

    [Fact]
    public void ReadLicenseData_ReturnsValidData_WhenLicenseFileExists()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);
        var licenseFilePath = Path.Combine(_testDirectory, _licenseFileName);
        var expectedLicense = CreateValidLicenseFile(licenseFilePath);

        // Act
        var licenseDataBytes = provider.ReadLicenseData();
        var licenseDataJson = Encoding.UTF8.GetString(licenseDataBytes);
        var licenseData = JsonSerializer.Deserialize<LicenseData>(licenseDataJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(licenseData);
        Assert.Equal(expectedLicense.LicenseKey, licenseData.LicenseKey);
        Assert.Equal(expectedLicense.IssuedTo, licenseData.IssuedTo);
        Assert.Equal(expectedLicense.EnabledFeatures, licenseData.EnabledFeatures);
        Assert.Equal(expectedLicense.ExpirationDate, licenseData.ExpirationDate);
        Assert.Equal(expectedLicense.IssuedDate, licenseData.IssuedDate);
    }

    [Fact]
    public void ReadLicenseData_ThrowsIOException_WhenLicenseFileIsCorrupted()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);
        var licenseFilePath = Path.Combine(_testDirectory, _licenseFileName);
        File.WriteAllText(licenseFilePath, "{ invalid json content }");

        // Act & Assert
        var exception = Assert.Throws<IOException>(() => provider.ReadLicenseData());
        Assert.Contains("Failed to deserialize license data", exception.Message);
    }

    [Fact]
    public void ReadLicenseData_ThrowsIOException_WhenSignatureIsInvalidBase64()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);
        var licenseFilePath = Path.Combine(_testDirectory, _licenseFileName);
        var licenseJson = @"{
            ""licenseKey"": ""TEST-KEY"",
            ""issuedTo"": ""Test User"",
            ""issuedDate"": ""2024-01-01T00:00:00Z"",
            ""expirationDate"": ""2025-01-01T00:00:00Z"",
            ""enabledFeatures"": 15,
            ""signature"": ""invalid-base64!!!""
        }";
        File.WriteAllText(licenseFilePath, licenseJson);

        // Act & Assert
        var exception = Assert.Throws<IOException>(() => provider.ReadLicenseData());
        Assert.Contains("Failed to decode signature from base64", exception.Message);
    }

    [Fact]
    public void DongleConnected_EventRaised_WhenLicenseFileIsCreated()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);
        var licenseFilePath = Path.Combine(_testDirectory, _licenseFileName);
        DongleEventArgs? capturedEventArgs = null;
        var eventRaised = false;

        provider.DongleConnected += (sender, args) =>
        {
            eventRaised = true;
            capturedEventArgs = args;
        };

        // Act
        CreateValidLicenseFile(licenseFilePath);

        // Wait for file system watcher to trigger
        Thread.Sleep(500);

        // Assert
        Assert.True(eventRaised);
        Assert.NotNull(capturedEventArgs);
        Assert.NotNull(capturedEventArgs.DongleInfo);
        Assert.Equal("SIM", capturedEventArgs.DongleInfo.VendorId);
    }

    [Fact]
    public void DongleDisconnected_EventRaised_WhenLicenseFileIsDeleted()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);
        var licenseFilePath = Path.Combine(_testDirectory, _licenseFileName);
        CreateValidLicenseFile(licenseFilePath);

        // Wait for initial file creation to settle
        Thread.Sleep(500);

        DongleEventArgs? capturedEventArgs = null;
        var eventRaised = false;

        provider.DongleDisconnected += (sender, args) =>
        {
            eventRaised = true;
            capturedEventArgs = args;
        };

        // Act
        File.Delete(licenseFilePath);

        // Wait for file system watcher to trigger
        Thread.Sleep(500);

        // Assert
        Assert.True(eventRaised);
        Assert.NotNull(capturedEventArgs);
        Assert.NotNull(capturedEventArgs.DongleInfo);
        Assert.Equal("SIM", capturedEventArgs.DongleInfo.VendorId);
    }

    [Fact]
    public void ReadLicenseData_HandlesAllFeatureFlags_Correctly()
    {
        // Arrange
        using var provider = new SimulatedDongleProvider(_testDirectory);
        var licenseFilePath = Path.Combine(_testDirectory, _licenseFileName);
        var expectedFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | FeatureFlags.DataExport;
        
        var licenseJson = $@"{{
            ""licenseKey"": ""TEST-KEY"",
            ""issuedTo"": ""Test User"",
            ""issuedDate"": ""2024-01-01T00:00:00Z"",
            ""expirationDate"": ""2025-01-01T00:00:00Z"",
            ""enabledFeatures"": {(int)expectedFeatures},
            ""signature"": ""dGVzdHNpZ25hdHVyZQ==""
        }}";
        File.WriteAllText(licenseFilePath, licenseJson);

        // Act
        var licenseDataBytes = provider.ReadLicenseData();
        var licenseDataJson = Encoding.UTF8.GetString(licenseDataBytes);
        var licenseData = JsonSerializer.Deserialize<LicenseData>(licenseDataJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(licenseData);
        Assert.Equal(expectedFeatures, licenseData.EnabledFeatures);
    }

    /// <summary>
    /// Helper method to create a valid license file for testing.
    /// </summary>
    private LicenseData CreateValidLicenseFile(string filePath)
    {
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-1234-5678-ABCD",
            IssuedTo = "Test User",
            IssuedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ExpirationDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics,
            Signature = Convert.FromBase64String("dGVzdHNpZ25hdHVyZQ==")
        };

        var licenseJson = $@"{{
            ""licenseKey"": ""{licenseData.LicenseKey}"",
            ""issuedTo"": ""{licenseData.IssuedTo}"",
            ""issuedDate"": ""{licenseData.IssuedDate:yyyy-MM-ddTHH:mm:ssZ}"",
            ""expirationDate"": ""{licenseData.ExpirationDate:yyyy-MM-ddTHH:mm:ssZ}"",
            ""enabledFeatures"": {(int)licenseData.EnabledFeatures},
            ""signature"": ""{Convert.ToBase64String(licenseData.Signature)}""
        }}";

        File.WriteAllText(filePath, licenseJson);
        return licenseData;
    }
}
