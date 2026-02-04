using UsbDongleLicensing.Core;

namespace UsbDongleLicensing.Providers;

/// <summary>
/// Event arguments for dongle connection and disconnection events.
/// </summary>
public class DongleEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the information about the dongle that triggered the event.
    /// </summary>
    public DongleInfo? DongleInfo { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DongleEventArgs"/> class.
    /// </summary>
    public DongleEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DongleEventArgs"/> class with dongle information.
    /// </summary>
    /// <param name="dongleInfo">The information about the dongle.</param>
    public DongleEventArgs(DongleInfo? dongleInfo)
    {
        DongleInfo = dongleInfo;
    }
}
