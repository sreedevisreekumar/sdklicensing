namespace UsbDongleLicensing.Tests.Validation;

using UsbDongleLicensing.Core;
using UsbDongleLicensing.Validation;
using Xunit;

/// <summary>
/// Tests for the ILicenseValidator interface definition.
/// </summary>
public class ILicenseValidatorTests
{
    /// <summary>
    /// Verifies that the ILicenseValidator interface can be implemented with a mock class.
    /// This test ensures the interface is properly defined and usable.
    /// </summary>
    [Fact]
    public void ILicenseValidator_CanBeImplemented()
    {
        // Arrange & Act
        ILicenseValidator validator = new MockLicenseValidator();

        // Assert
        Assert.NotNull(validator);
        Assert.True(validator is ILicenseValidator);
    }

    /// <summary>
    /// Verifies that the Validate method signature is correct.
    /// </summary>
    [Fact]
    public void Validate_AcceptsLicenseData_ReturnsValidationResult()
    {
        // Arrange
        var validator = new MockLicenseValidator();
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY",
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            EnabledFeatures = FeatureFlags.BasicFeature,
            IssuedTo = "Test User",
            IssuedDate = DateTime.UtcNow
        };

        // Act
        var result = validator.Validate(licenseData);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ValidationResult>(result);
    }

    /// <summary>
    /// Verifies that the VerifySignature method signature is correct.
    /// </summary>
    [Fact]
    public void VerifySignature_AcceptsByteArrays_ReturnsBool()
    {
        // Arrange
        var validator = new MockLicenseValidator();
        var data = new byte[] { 1, 2, 3 };
        var signature = new byte[] { 4, 5, 6 };

        // Act
        var result = validator.VerifySignature(data, signature);

        // Assert
        Assert.IsType<bool>(result);
    }

    /// <summary>
    /// Verifies that the CheckExpiration method signature is correct.
    /// </summary>
    [Fact]
    public void CheckExpiration_AcceptsDateTime_ReturnsBool()
    {
        // Arrange
        var validator = new MockLicenseValidator();
        var expirationDate = DateTime.UtcNow.AddDays(30);

        // Act
        var result = validator.CheckExpiration(expirationDate);

        // Assert
        Assert.IsType<bool>(result);
    }

    /// <summary>
    /// Mock implementation of ILicenseValidator for testing the interface definition.
    /// </summary>
    private class MockLicenseValidator : ILicenseValidator
    {
        public ValidationResult Validate(LicenseData license)
        {
            return new ValidationResult
            {
                IsValid = true,
                Status = LicenseStatus.Valid
            };
        }

        public bool VerifySignature(byte[] data, byte[] signature)
        {
            return true;
        }

        public bool CheckExpiration(DateTime expirationDate)
        {
            return expirationDate > DateTime.UtcNow;
        }
    }
}
