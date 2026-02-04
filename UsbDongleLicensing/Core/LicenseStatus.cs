namespace UsbDongleLicensing.Core;

/// <summary>
/// Represents the status of a license validation.
/// </summary>
public enum LicenseStatus
{
    /// <summary>
    /// The license is valid and active.
    /// </summary>
    Valid,

    /// <summary>
    /// The license has expired.
    /// </summary>
    Expired,

    /// <summary>
    /// The license signature is invalid.
    /// </summary>
    InvalidSignature,

    /// <summary>
    /// The license data is corrupted.
    /// </summary>
    Corrupted,

    /// <summary>
    /// No license was found.
    /// </summary>
    NotFound
}
