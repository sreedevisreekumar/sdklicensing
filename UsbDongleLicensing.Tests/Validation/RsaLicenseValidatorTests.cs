namespace UsbDongleLicensing.Tests.Validation;

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UsbDongleLicensing.Core;
using UsbDongleLicensing.Validation;
using Xunit;

/// <summary>
/// Unit tests for the RsaLicenseValidator class.
/// </summary>
public class RsaLicenseValidatorTests
{
    private readonly string _publicKeyXml;
    private readonly string _privateKeyXml;
    private readonly string _publicKeyBase64;

    public RsaLicenseValidatorTests()
    {
        // Generate a test RSA key pair for testing
        using var rsa = RSA.Create(2048);
        _publicKeyXml = rsa.ToXmlString(false);
        _privateKeyXml = rsa.ToXmlString(true);
        _publicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(_publicKeyXml));
    }

    [Fact]
    public void Constructor_WithValidPublicKey_InitializesSuccessfully()
    {
        // Act
        var validator = new RsaLicenseValidator(_publicKeyBase64);

        // Assert
        Assert.NotNull(validator);
    }

    [Fact]
    public void Constructor_WithEmptyPublicKey_InitializesWithoutCrashing()
    {
        // Act
        var validator = new RsaLicenseValidator(string.Empty);

        // Assert
        Assert.NotNull(validator);
    }

    [Fact]
    public void CheckExpiration_WithFutureDate_ReturnsTrue()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var futureDate = DateTime.UtcNow.AddDays(30);

        // Act
        var result = validator.CheckExpiration(futureDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CheckExpiration_WithPastDate_ReturnsFalse()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var pastDate = DateTime.UtcNow.AddDays(-30);

        // Act
        var result = validator.CheckExpiration(pastDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckExpiration_WithCurrentDate_ReturnsFalse()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var currentDate = DateTime.UtcNow;

        // Act
        var result = validator.CheckExpiration(currentDate);

        // Assert
        // Should return false because current time is not > current time
        Assert.False(result);
    }

    [Fact]
    public void VerifySignature_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var data = Encoding.UTF8.GetBytes("Test data for signing");
        
        // Sign the data with the private key
        byte[] signature;
        using (var rsa = RSA.Create())
        {
            rsa.FromXmlString(_privateKeyXml);
            signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        // Act
        var result = validator.VerifySignature(data, signature);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifySignature_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var data = Encoding.UTF8.GetBytes("Test data for signing");
        var invalidSignature = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var result = validator.VerifySignature(data, invalidSignature);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifySignature_WithTamperedData_ReturnsFalse()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var originalData = Encoding.UTF8.GetBytes("Original data");
        var tamperedData = Encoding.UTF8.GetBytes("Tampered data");
        
        // Sign the original data
        byte[] signature;
        using (var rsa = RSA.Create())
        {
            rsa.FromXmlString(_privateKeyXml);
            signature = rsa.SignData(originalData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        // Act - verify with tampered data
        var result = validator.VerifySignature(tamperedData, signature);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifySignature_WithEmptyData_ReturnsFalse()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var emptyData = Array.Empty<byte>();
        var signature = new byte[] { 1, 2, 3 };

        // Act
        var result = validator.VerifySignature(emptyData, signature);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifySignature_WithEmptySignature_ReturnsFalse()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var data = Encoding.UTF8.GetBytes("Test data");
        var emptySignature = Array.Empty<byte>();

        // Act
        var result = validator.VerifySignature(data, emptySignature);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithValidLicense_ReturnsValidResult()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var license = CreateValidLicense();
        
        // Sign the license
        license.Signature = SignLicense(license);

        // Act
        var result = validator.Validate(license);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(LicenseStatus.Valid, result.Status);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithExpiredLicense_ReturnsExpiredResult()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var license = CreateValidLicense();
        license.ExpirationDate = DateTime.UtcNow.AddDays(-30);
        license.Signature = SignLicense(license);

        // Act
        var result = validator.Validate(license);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(LicenseStatus.Expired, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("expired"));
    }

    [Fact]
    public void Validate_WithInvalidSignature_ReturnsInvalidSignatureResult()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var license = CreateValidLicense();
        license.Signature = new byte[] { 1, 2, 3, 4, 5 }; // Invalid signature

        // Act
        var result = validator.Validate(license);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(LicenseStatus.InvalidSignature, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Invalid license signature"));
    }

    [Fact]
    public void Validate_WithMissingLicenseKey_ReturnsCorruptedResult()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var license = CreateValidLicense();
        license.LicenseKey = string.Empty;
        license.Signature = SignLicense(license);

        // Act
        var result = validator.Validate(license);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(LicenseStatus.Corrupted, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Missing required fields") && e.Contains("LicenseKey"));
    }

    [Fact]
    public void Validate_WithMissingIssuedTo_ReturnsCorruptedResult()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var license = CreateValidLicense();
        license.IssuedTo = string.Empty;
        license.Signature = SignLicense(license);

        // Act
        var result = validator.Validate(license);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(LicenseStatus.Corrupted, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Missing required fields") && e.Contains("IssuedTo"));
    }

    [Fact]
    public void Validate_WithMissingSignature_ReturnsCorruptedResult()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var license = CreateValidLicense();
        license.Signature = Array.Empty<byte>();

        // Act
        var result = validator.Validate(license);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(LicenseStatus.Corrupted, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Missing required fields") && e.Contains("Signature"));
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReportsAllErrors()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var license = CreateValidLicense();
        license.ExpirationDate = DateTime.UtcNow.AddDays(-30); // Expired
        license.Signature = new byte[] { 1, 2, 3 }; // Invalid signature

        // Act
        var result = validator.Validate(license);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Contains("expired"));
        Assert.Contains(result.Errors, e => e.Contains("Invalid license signature"));
    }

    [Fact]
    public void Validate_WithMultipleMissingFields_ReportsAllMissingFields()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var license = new LicenseData
        {
            LicenseKey = string.Empty,
            IssuedTo = string.Empty,
            Signature = Array.Empty<byte>(),
            ExpirationDate = default(DateTime),
            IssuedDate = default(DateTime),
            EnabledFeatures = FeatureFlags.None
        };

        // Act
        var result = validator.Validate(license);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(LicenseStatus.Corrupted, result.Status);
        Assert.Single(result.Errors);
        var errorMessage = result.Errors[0];
        Assert.Contains("LicenseKey", errorMessage);
        Assert.Contains("IssuedTo", errorMessage);
        Assert.Contains("Signature", errorMessage);
        Assert.Contains("ExpirationDate", errorMessage);
        Assert.Contains("IssuedDate", errorMessage);
    }

    [Fact]
    public void Validate_WithDefaultDates_ReturnsCorruptedResult()
    {
        // Arrange
        var validator = new RsaLicenseValidator(_publicKeyBase64);
        var license = CreateValidLicense();
        license.ExpirationDate = default(DateTime);
        license.IssuedDate = default(DateTime);
        license.Signature = SignLicense(license);

        // Act
        var result = validator.Validate(license);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(LicenseStatus.Corrupted, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Missing required fields"));
    }

    /// <summary>
    /// Helper method to create a valid license for testing.
    /// </summary>
    private LicenseData CreateValidLicense()
    {
        return new LicenseData
        {
            LicenseKey = "TEST-1234-5678-ABCD",
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics,
            Signature = Array.Empty<byte>() // Will be set by SignLicense
        };
    }

    /// <summary>
    /// Helper method to sign a license using the test private key.
    /// </summary>
    private byte[] SignLicense(LicenseData license)
    {
        var dataForSigning = new
        {
            LicenseKey = license.LicenseKey,
            IssuedTo = license.IssuedTo,
            IssuedDate = license.IssuedDate.ToString("o"),
            ExpirationDate = license.ExpirationDate.ToString("o"),
            EnabledFeatures = (int)license.EnabledFeatures
        };

        var json = JsonSerializer.Serialize(dataForSigning);
        var data = Encoding.UTF8.GetBytes(json);

        using var rsa = RSA.Create();
        rsa.FromXmlString(_privateKeyXml);
        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
