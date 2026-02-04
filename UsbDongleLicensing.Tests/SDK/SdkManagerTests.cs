namespace UsbDongleLicensing.Tests.SDK;

using Xunit;
using UsbDongleLicensing.SDK;
using UsbDongleLicensing.Core;
using UsbDongleLicensing.Providers;
using Moq;

/// <summary>
/// Unit tests for the SdkManager class.
/// </summary>
public class SdkManagerTests
{
    [Fact]
    public void Initialize_WithValidLicense_EnablesAllFeatures()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | FeatureFlags.DataExport | FeatureFlags.ApiAccess,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };

        // Act
        sdkManager.Initialize(validationResult);

        // Assert
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.BasicFeature));
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.AdvancedAnalytics));
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.DataExport));
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.ApiAccess));
    }

    [Fact]
    public void Initialize_WithPartialFeatures_EnablesOnlySpecifiedFeatures()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };

        // Act
        sdkManager.Initialize(validationResult);

        // Assert
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.BasicFeature));
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.AdvancedAnalytics));
        Assert.False(sdkManager.IsFeatureEnabled(SdkFeature.DataExport));
        Assert.False(sdkManager.IsFeatureEnabled(SdkFeature.ApiAccess));
    }

    [Fact]
    public void Initialize_WithInvalidLicense_EnablesOnlyBasicFeature()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var validationResult = new ValidationResult
        {
            IsValid = false,
            Status = LicenseStatus.Expired,
            LicenseData = null
        };

        // Act
        sdkManager.Initialize(validationResult);

        // Assert
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.BasicFeature));
        Assert.False(sdkManager.IsFeatureEnabled(SdkFeature.AdvancedAnalytics));
        Assert.False(sdkManager.IsFeatureEnabled(SdkFeature.DataExport));
        Assert.False(sdkManager.IsFeatureEnabled(SdkFeature.ApiAccess));
    }

    [Fact]
    public void Initialize_WithNullValidationResult_ThrowsArgumentNullException()
    {
        // Arrange
        var sdkManager = new SdkManager();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => sdkManager.Initialize(null!));
    }

    [Fact]
    public void GetAvailableFeatures_WithAllFeaturesEnabled_ReturnsAllFeatures()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | FeatureFlags.DataExport | FeatureFlags.ApiAccess,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Act
        var availableFeatures = sdkManager.GetAvailableFeatures();

        // Assert
        Assert.Equal(4, availableFeatures.Count);
        Assert.Contains(SdkFeature.BasicFeature, availableFeatures);
        Assert.Contains(SdkFeature.AdvancedAnalytics, availableFeatures);
        Assert.Contains(SdkFeature.DataExport, availableFeatures);
        Assert.Contains(SdkFeature.ApiAccess, availableFeatures);
    }

    [Fact]
    public void GetAvailableFeatures_WithPartialFeatures_ReturnsOnlyEnabledFeatures()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.DataExport,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Act
        var availableFeatures = sdkManager.GetAvailableFeatures();

        // Assert
        Assert.Equal(2, availableFeatures.Count);
        Assert.Contains(SdkFeature.BasicFeature, availableFeatures);
        Assert.Contains(SdkFeature.DataExport, availableFeatures);
        Assert.DoesNotContain(SdkFeature.AdvancedAnalytics, availableFeatures);
        Assert.DoesNotContain(SdkFeature.ApiAccess, availableFeatures);
    }

    [Fact]
    public void ExecuteFeature_BasicFeature_ReturnsSuccessResult()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.BasicFeature,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Act
        var result = sdkManager.ExecuteFeature(SdkFeature.BasicFeature);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Basic feature executed successfully", result.Message);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public void ExecuteFeature_AdvancedAnalytics_WithLicense_ReturnsSuccessResult()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.AdvancedAnalytics,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Act
        var result = sdkManager.ExecuteFeature(SdkFeature.AdvancedAnalytics);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Advanced analytics completed successfully", result.Message);
        Assert.NotEmpty(result.Data);
        Assert.True(result.Data.ContainsKey("totalRecords"));
        Assert.True(result.Data.ContainsKey("averageValue"));
    }

    [Fact]
    public void ExecuteFeature_DataExport_WithLicense_ReturnsSuccessResult()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.DataExport,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Act
        var result = sdkManager.ExecuteFeature(SdkFeature.DataExport);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Data export completed successfully", result.Message);
        Assert.NotEmpty(result.Data);
        Assert.True(result.Data.ContainsKey("fileName"));
        Assert.True(result.Data.ContainsKey("filePath"));
    }

    [Fact]
    public void ExecuteFeature_ApiAccess_WithLicense_ReturnsSuccessResult()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.ApiAccess,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Act
        var result = sdkManager.ExecuteFeature(SdkFeature.ApiAccess);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("API access successful", result.Message);
        Assert.NotEmpty(result.Data);
        Assert.True(result.Data.ContainsKey("endpoint"));
        Assert.True(result.Data.ContainsKey("statusCode"));
    }

    [Fact]
    public void ExecuteFeature_UnlicensedFeature_ReturnsErrorResult()
    {
        // Arrange
        var sdkManager = new SdkManager();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.BasicFeature, // Only basic feature enabled
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Act
        var result = sdkManager.ExecuteFeature(SdkFeature.AdvancedAnalytics);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not licensed", result.Message);
        Assert.Contains("upgrade your license", result.Message);
    }

    [Fact]
    public void DongleDisconnection_DisablesPremiumFeatures()
    {
        // Arrange
        var mockDongleProvider = new Mock<IUsbDongleProvider>();
        var sdkManager = new SdkManager(mockDongleProvider.Object);
        
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | FeatureFlags.DataExport | FeatureFlags.ApiAccess,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Verify all features are enabled
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.AdvancedAnalytics));
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.DataExport));
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.ApiAccess));

        // Act - Simulate dongle disconnection
        mockDongleProvider.Raise(
            p => p.DongleDisconnected += null,
            new DongleEventArgs(new DongleInfo
            {
                VendorId = "1234",
                ProductId = "5678",
                SerialNumber = "TEST123",
                DetectedAt = DateTime.UtcNow
            })
        );

        // Assert - Only basic feature should remain enabled
        Assert.True(sdkManager.IsFeatureEnabled(SdkFeature.BasicFeature));
        Assert.False(sdkManager.IsFeatureEnabled(SdkFeature.AdvancedAnalytics));
        Assert.False(sdkManager.IsFeatureEnabled(SdkFeature.DataExport));
        Assert.False(sdkManager.IsFeatureEnabled(SdkFeature.ApiAccess));
    }

    [Fact]
    public void ExecuteFeature_AfterDongleDisconnection_ReturnsErrorForPremiumFeatures()
    {
        // Arrange
        var mockDongleProvider = new Mock<IUsbDongleProvider>();
        var sdkManager = new SdkManager(mockDongleProvider.Object);
        
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Simulate dongle disconnection
        mockDongleProvider.Raise(
            p => p.DongleDisconnected += null,
            new DongleEventArgs(new DongleInfo
            {
                VendorId = "1234",
                ProductId = "5678",
                SerialNumber = "TEST123",
                DetectedAt = DateTime.UtcNow
            })
        );

        // Act
        var result = sdkManager.ExecuteFeature(SdkFeature.AdvancedAnalytics);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not licensed", result.Message);
    }

    [Fact]
    public void ExecuteFeature_BasicFeature_AfterDongleDisconnection_StillWorks()
    {
        // Arrange
        var mockDongleProvider = new Mock<IUsbDongleProvider>();
        var sdkManager = new SdkManager(mockDongleProvider.Object);
        
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow,
            Signature = new byte[] { 1, 2, 3 }
        };
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData
        };
        sdkManager.Initialize(validationResult);

        // Simulate dongle disconnection
        mockDongleProvider.Raise(
            p => p.DongleDisconnected += null,
            new DongleEventArgs(new DongleInfo
            {
                VendorId = "1234",
                ProductId = "5678",
                SerialNumber = "TEST123",
                DetectedAt = DateTime.UtcNow
            })
        );

        // Act
        var result = sdkManager.ExecuteFeature(SdkFeature.BasicFeature);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Basic feature executed successfully", result.Message);
    }
}
