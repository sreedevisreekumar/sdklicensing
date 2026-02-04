namespace UsbDongleLicensing.Core;

/// <summary>
/// Represents license data read from a USB dongle.
/// </summary>
public class LicenseData
{
    /// <summary>
    /// Gets or sets the unique license key identifier.
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the license expiration date.
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the bitmask of enabled features.
    /// </summary>
    public FeatureFlags EnabledFeatures { get; set; }

    /// <summary>
    /// Gets or sets the RSA signature of the license data.
    /// </summary>
    public byte[] Signature { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the name of the licensee.
    /// </summary>
    public string IssuedTo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the license was issued.
    /// </summary>
    public DateTime IssuedDate { get; set; }
}
