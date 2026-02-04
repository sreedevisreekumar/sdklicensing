namespace UsbDongleLicensing.Core;

/// <summary>
/// Represents information about a detected USB dongle.
/// </summary>
public class DongleInfo
{
    /// <summary>
    /// Gets or sets the USB vendor ID of the dongle.
    /// </summary>
    public string VendorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the USB product ID of the dongle.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serial number of the dongle.
    /// </summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the dongle was detected.
    /// </summary>
    public DateTime DetectedAt { get; set; }
}
