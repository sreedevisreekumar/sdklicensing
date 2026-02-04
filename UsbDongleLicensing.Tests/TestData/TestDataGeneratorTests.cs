namespace UsbDongleLicensing.Tests.TestData;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using UsbDongleLicensing.Core;
using UsbDongleLicensing.TestData;

/// <summary>
/// Unit tests for the TestDataGenerator class.
/// </summary>
public class TestDataGeneratorTests : IDisposable
{
    private readonly string _testOutputDir;

    public TestDataGeneratorTests()
    {
        // Create a unique test output directory for each test run
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"TestDataGeneratorTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDir);
    }

    [Fact]
    public void GenerateKeyPair_CreatesValidKeyPair()
    {
        // Arrange
        var generator = new TestDataGenerator();

        // Act
        generator.GenerateKeyPair();

        // Assert - Should not throw and should be able to get public key
        var publicKeyBase64 = generator.GetPublicKeyBase64();
        Assert.False(string.IsNullOrEmpty(publicKeyBase64));
    }

    [Fact]
    public void GenerateKeyPair_WithCustomKeySize_CreatesValidKeyPair()
    {
        // Arrange
        var generator = new TestDataGenerator();

        // Act
        generator.GenerateKeyPair(1024);

        // Assert
        var publicKeyBase64 = generator.GetPublicKeyBase64();
        Assert.False(string.IsNullOrEmpty(publicKeyBase64));
    }

    [Fact]
    public void SaveKeyPair_SavesKeysToFiles()
    {
        // Arrange
        var generator = new TestDataGenerator();
        generator.GenerateKeyPair();
        var privateKeyPath = Path.Combine(_testOutputDir, "private_key.xml");
        var publicKeyPath = Path.Combine(_testOutputDir, "public_key.xml");

        // Act
        generator.SaveKeyPair(privateKeyPath, publicKeyPath);

        // Assert
        Assert.True(File.Exists(privateKeyPath));
        Assert.True(File.Exists(publicKeyPath));
        
        // Verify files contain XML content
        var privateKeyContent = File.ReadAllText(privateKeyPath);
        var publicKeyContent = File.ReadAllText(publicKeyPath);
        Assert.Contains("<RSAKeyValue>", privateKeyContent);
        Assert.Contains("<RSAKeyValue>", publicKeyContent);
        
        // Private key should contain more data than public key
        Assert.True(privateKeyContent.Length > publicKeyContent.Length);
    }

    [Fact]
    public void SaveKeyPair_WithoutGeneratingFirst_ThrowsException()
    {
        // Arrange
        var generator = new TestDataGenerator();
        var privateKeyPath = Path.Combine(_testOutputDir, "private_key.xml");
        var publicKeyPath = Path.Combine(_testOutputDir, "public_key.xml");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            generator.SaveKeyPair(privateKeyPath, publicKeyPath));
        Assert.Contains("No key pair has been generated", exception.Message);
    }

    [Fact]
    public void CreateLicense_CreatesLicenseWithSpecifiedParameters()
    {
        // Arrange
        var generator = new TestDataGenerator();
        var licenseKey = "TEST-1234-5678-ABCD";
        var issuedTo = "Test User";
        var issuedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var expirationDate = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var enabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics;

        // Act
        var license = generator.CreateLicense(licenseKey, issuedTo, issuedDate, expirationDate, enabledFeatures);

        // Assert
        Assert.Equal(licenseKey, license.LicenseKey);
        Assert.Equal(issuedTo, license.IssuedTo);
        Assert.Equal(issuedDate, license.IssuedDate);
        Assert.Equal(expirationDate, license.ExpirationDate);
        Assert.Equal(enabledFeatures, license.EnabledFeatures);
        Assert.Empty(license.Signature); // Signature not yet generated
    }

    [Fact]
    public void SignLicense_GeneratesValidSignature()
    {
        // Arrange
        var generator = new TestDataGenerator();
        generator.GenerateKeyPair();
        var license = generator.CreateLicense(
            "TEST-1234-5678-ABCD",
            "Test User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            FeatureFlags.BasicFeature);

        // Act
        var signature = generator.SignLicense(license);

        // Assert
        Assert.NotNull(signature);
        Assert.NotEmpty(signature);
        Assert.True(signature.Length > 0);
    }

    [Fact]
    public void SignLicense_WithoutGeneratingKeyPair_ThrowsException()
    {
        // Arrange
        var generator = new TestDataGenerator();
        var license = generator.CreateLicense(
            "TEST-1234-5678-ABCD",
            "Test User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            FeatureFlags.BasicFeature);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            generator.SignLicense(license));
        Assert.Contains("No private key available", exception.Message);
    }

    [Fact]
    public void SaveLicense_CreatesJsonFile()
    {
        // Arrange
        var generator = new TestDataGenerator();
        generator.GenerateKeyPair();
        var license = generator.CreateLicense(
            "TEST-1234-5678-ABCD",
            "Test User",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            FeatureFlags.BasicFeature | FeatureFlags.DataExport);
        license.Signature = generator.SignLicense(license);
        var filePath = Path.Combine(_testOutputDir, "test_license.json");

        // Act
        generator.SaveLicense(license, filePath);

        // Assert
        Assert.True(File.Exists(filePath));
        
        var content = File.ReadAllText(filePath);
        Assert.Contains("TEST-1234-5678-ABCD", content);
        Assert.Contains("Test User", content);
        Assert.Contains("licenseKey", content);
        Assert.Contains("issuedTo", content);
        Assert.Contains("expirationDate", content);
        Assert.Contains("signature", content);
    }

    [Fact]
    public void SaveLicense_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var generator = new TestDataGenerator();
        generator.GenerateKeyPair();
        var license = generator.CreateLicense(
            "TEST-1234-5678-ABCD",
            "Test User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            FeatureFlags.BasicFeature);
        license.Signature = generator.SignLicense(license);
        var subDir = Path.Combine(_testOutputDir, "subdir", "nested");
        var filePath = Path.Combine(subDir, "test_license.json");

        // Act
        generator.SaveLicense(license, filePath);

        // Assert
        Assert.True(Directory.Exists(subDir));
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void GetPublicKeyBase64_ReturnsValidBase64String()
    {
        // Arrange
        var generator = new TestDataGenerator();
        generator.GenerateKeyPair();

        // Act
        var publicKeyBase64 = generator.GetPublicKeyBase64();

        // Assert
        Assert.False(string.IsNullOrEmpty(publicKeyBase64));
        
        // Should be valid base64
        var bytes = Convert.FromBase64String(publicKeyBase64);
        Assert.True(bytes.Length > 0);
        
        // Should decode to XML
        var xml = Encoding.UTF8.GetString(bytes);
        Assert.Contains("<RSAKeyValue>", xml);
    }

    [Fact]
    public void GetPublicKeyBase64_WithoutGeneratingKeyPair_ThrowsException()
    {
        // Arrange
        var generator = new TestDataGenerator();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            generator.GetPublicKeyBase64());
        Assert.Contains("No key pair has been generated", exception.Message);
    }

    [Fact]
    public void LoadKeyPair_LoadsExistingKeys()
    {
        // Arrange
        var generator1 = new TestDataGenerator();
        generator1.GenerateKeyPair();
        var privateKeyPath = Path.Combine(_testOutputDir, "private_key.xml");
        var publicKeyPath = Path.Combine(_testOutputDir, "public_key.xml");
        generator1.SaveKeyPair(privateKeyPath, publicKeyPath);

        var generator2 = new TestDataGenerator();

        // Act
        generator2.LoadKeyPair(privateKeyPath, publicKeyPath);

        // Assert - Should be able to use the loaded keys
        var license = generator2.CreateLicense(
            "TEST-1234-5678-ABCD",
            "Test User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            FeatureFlags.BasicFeature);
        var signature = generator2.SignLicense(license);
        Assert.NotEmpty(signature);
    }

    [Fact]
    public void LoadKeyPair_WithMissingPrivateKey_ThrowsException()
    {
        // Arrange
        var generator = new TestDataGenerator();
        var privateKeyPath = Path.Combine(_testOutputDir, "nonexistent_private.xml");
        var publicKeyPath = Path.Combine(_testOutputDir, "nonexistent_public.xml");

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => 
            generator.LoadKeyPair(privateKeyPath, publicKeyPath));
        Assert.Contains("Private key file not found", exception.Message);
    }

    [Fact]
    public void LoadKeyPair_WithMissingPublicKey_ThrowsException()
    {
        // Arrange
        var generator = new TestDataGenerator();
        generator.GenerateKeyPair();
        var privateKeyPath = Path.Combine(_testOutputDir, "private_key.xml");
        var publicKeyPath = Path.Combine(_testOutputDir, "public_key.xml");
        generator.SaveKeyPair(privateKeyPath, publicKeyPath);
        
        // Delete public key
        File.Delete(publicKeyPath);

        var generator2 = new TestDataGenerator();

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => 
            generator2.LoadKeyPair(privateKeyPath, publicKeyPath));
        Assert.Contains("Public key file not found", exception.Message);
    }

    [Fact]
    public void SignedLicense_CanBeVerifiedWithPublicKey()
    {
        // Arrange
        var generator = new TestDataGenerator();
        generator.GenerateKeyPair();
        var license = generator.CreateLicense(
            "TEST-1234-5678-ABCD",
            "Test User",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DateTime.UtcNow.AddYears(1), // Ensure expiration is in the future
            FeatureFlags.BasicFeature);
        license.Signature = generator.SignLicense(license);

        // Get public key for verification
        var publicKeyBase64 = generator.GetPublicKeyBase64();

        // Act - Verify using RsaLicenseValidator
        var validator = new UsbDongleLicensing.Validation.RsaLicenseValidator(publicKeyBase64);
        var result = validator.Validate(license);

        // Assert
        Assert.True(result.IsValid, $"License validation failed. Errors: {string.Join(", ", result.Errors)}");
        Assert.Equal(LicenseStatus.Valid, result.Status);
        Assert.Empty(result.Errors);
    }

    // Cleanup after tests
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testOutputDir))
            {
                Directory.Delete(_testOutputDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
