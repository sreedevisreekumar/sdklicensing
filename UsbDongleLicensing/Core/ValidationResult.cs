namespace UsbDongleLicensing.Core;

/// <summary>
/// Represents the result of license validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the license is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the license validation status.
    /// </summary>
    public LicenseStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the license data that was validated.
    /// This is null if no license was provided or if the license was not found.
    /// </summary>
    public LicenseData? LicenseData { get; set; }
}
