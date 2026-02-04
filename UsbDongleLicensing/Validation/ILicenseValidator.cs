namespace UsbDongleLicensing.Validation;

using UsbDongleLicensing.Core;

/// <summary>
/// Interface for validating license data authenticity and expiration.
/// </summary>
public interface ILicenseValidator
{
    /// <summary>
    /// Validates the provided license data by checking signature and expiration.
    /// </summary>
    /// <param name="license">The license data to validate.</param>
    /// <returns>A ValidationResult containing the validation status and any errors.</returns>
    ValidationResult Validate(LicenseData license);

    /// <summary>
    /// Verifies the RSA signature of the provided data.
    /// </summary>
    /// <param name="data">The data that was signed.</param>
    /// <param name="signature">The RSA signature to verify.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    bool VerifySignature(byte[] data, byte[] signature);

    /// <summary>
    /// Checks if the provided expiration date is still valid.
    /// </summary>
    /// <param name="expirationDate">The expiration date to check.</param>
    /// <returns>True if the expiration date is in the future; otherwise, false.</returns>
    bool CheckExpiration(DateTime expirationDate);
}
