using UsbDongleLicensing.Core;
using Xunit;

namespace UsbDongleLicensing.Tests.Core;

/// <summary>
/// Unit tests for core data model classes.
/// </summary>
public class DataModelsTests
{
    [Fact]
    public void DongleInfo_CanBeInstantiated()
    {
        // Arrange & Act
        var dongleInfo = new DongleInfo
        {
            VendorId = "0x1234",
            ProductId = "0x5678",
            SerialNumber = "ABC123",
            DetectedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("0x1234", dongleInfo.VendorId);
        Assert.Equal("0x5678", dongleInfo.ProductId);
        Assert.Equal("ABC123", dongleInfo.SerialNumber);
        Assert.NotEqual(default(DateTime), dongleInfo.DetectedAt);
    }

    [Fact]
    public void LicenseData_CanBeInstantiated()
    {
        // Arrange & Act
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY-1234",
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics,
            Signature = new byte[] { 1, 2, 3, 4 },
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("TEST-KEY-1234", licenseData.LicenseKey);
        Assert.True(licenseData.ExpirationDate > DateTime.UtcNow);
        Assert.Equal(FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics, licenseData.EnabledFeatures);
        Assert.Equal(4, licenseData.Signature.Length);
        Assert.Equal("Test User", licenseData.IssuedTo);
    }

    [Fact]
    public void ValidationResult_CanBeInstantiated()
    {
        // Arrange & Act
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Errors = new List<string>(),
            Status = LicenseStatus.Valid
        };

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Errors);
        Assert.Equal(LicenseStatus.Valid, validationResult.Status);
    }

    [Fact]
    public void ValidationResult_CanContainErrors()
    {
        // Arrange & Act
        var validationResult = new ValidationResult
        {
            IsValid = false,
            Errors = new List<string> { "Signature invalid", "License expired" },
            Status = LicenseStatus.Expired
        };

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Equal(2, validationResult.Errors.Count);
        Assert.Contains("Signature invalid", validationResult.Errors);
        Assert.Contains("License expired", validationResult.Errors);
    }

    [Fact]
    public void FeatureResult_CanBeInstantiated()
    {
        // Arrange & Act
        var featureResult = new FeatureResult
        {
            Success = true,
            Message = "Feature executed successfully",
            Data = new Dictionary<string, object>
            {
                { "result", "test data" },
                { "count", 42 }
            }
        };

        // Assert
        Assert.True(featureResult.Success);
        Assert.Equal("Feature executed successfully", featureResult.Message);
        Assert.Equal(2, featureResult.Data.Count);
        Assert.Equal("test data", featureResult.Data["result"]);
        Assert.Equal(42, featureResult.Data["count"]);
    }

    [Fact]
    public void FeatureFlags_CanBeCombined()
    {
        // Arrange & Act
        var flags = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | FeatureFlags.DataExport;

        // Assert
        Assert.True(flags.HasFlag(FeatureFlags.BasicFeature));
        Assert.True(flags.HasFlag(FeatureFlags.AdvancedAnalytics));
        Assert.True(flags.HasFlag(FeatureFlags.DataExport));
        Assert.False(flags.HasFlag(FeatureFlags.ApiAccess));
    }

    [Fact]
    public void FeatureFlags_NoneHasNoFlags()
    {
        // Arrange & Act
        var flags = FeatureFlags.None;

        // Assert
        Assert.False(flags.HasFlag(FeatureFlags.BasicFeature));
        Assert.False(flags.HasFlag(FeatureFlags.AdvancedAnalytics));
        Assert.False(flags.HasFlag(FeatureFlags.DataExport));
        Assert.False(flags.HasFlag(FeatureFlags.ApiAccess));
    }

    [Fact]
    public void LicenseStatus_HasAllExpectedValues()
    {
        // Arrange & Act & Assert
        Assert.Equal(5, Enum.GetValues<LicenseStatus>().Length);
        Assert.True(Enum.IsDefined(typeof(LicenseStatus), LicenseStatus.Valid));
        Assert.True(Enum.IsDefined(typeof(LicenseStatus), LicenseStatus.Expired));
        Assert.True(Enum.IsDefined(typeof(LicenseStatus), LicenseStatus.InvalidSignature));
        Assert.True(Enum.IsDefined(typeof(LicenseStatus), LicenseStatus.Corrupted));
        Assert.True(Enum.IsDefined(typeof(LicenseStatus), LicenseStatus.NotFound));
    }

    [Fact]
    public void SdkFeature_HasAllExpectedValues()
    {
        // Arrange & Act & Assert
        Assert.Equal(4, Enum.GetValues<SdkFeature>().Length);
        Assert.True(Enum.IsDefined(typeof(SdkFeature), SdkFeature.BasicFeature));
        Assert.True(Enum.IsDefined(typeof(SdkFeature), SdkFeature.AdvancedAnalytics));
        Assert.True(Enum.IsDefined(typeof(SdkFeature), SdkFeature.DataExport));
        Assert.True(Enum.IsDefined(typeof(SdkFeature), SdkFeature.ApiAccess));
    }

    [Fact]
    public void FeatureFlags_BitmaskValuesAreCorrect()
    {
        // Arrange & Act & Assert
        Assert.Equal(0, (int)FeatureFlags.None);
        Assert.Equal(1, (int)FeatureFlags.BasicFeature);
        Assert.Equal(2, (int)FeatureFlags.AdvancedAnalytics);
        Assert.Equal(4, (int)FeatureFlags.DataExport);
        Assert.Equal(8, (int)FeatureFlags.ApiAccess);
    }
}
