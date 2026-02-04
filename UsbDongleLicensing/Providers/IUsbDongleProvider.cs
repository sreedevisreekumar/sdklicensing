using UsbDongleLicensing.Core;

namespace UsbDongleLicensing.Providers;

/// <summary>
/// Interface for USB dongle detection and communication.
/// Abstracts the underlying USB communication mechanism to support both real hardware (HID) and simulation modes.
/// </summary>
public interface IUsbDongleProvider
{
    /// <summary>
    /// Checks if a USB dongle is currently connected.
    /// </summary>
    /// <returns>True if a dongle is connected; otherwise, false.</returns>
    bool IsConnected();

    /// <summary>
    /// Detects and retrieves information about the connected USB dongle.
    /// </summary>
    /// <returns>
    /// A <see cref="DongleInfo"/> object containing details about the detected dongle,
    /// or null if no dongle is detected.
    /// </returns>
    DongleInfo? DetectDongle();

    /// <summary>
    /// Reads the license data from the connected USB dongle.
    /// </summary>
    /// <returns>A byte array containing the raw license data read from the dongle.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no dongle is connected.</exception>
    /// <exception cref="IOException">Thrown when the license data cannot be read from the dongle.</exception>
    byte[] ReadLicenseData();

    /// <summary>
    /// Event raised when a USB dongle is connected.
    /// </summary>
    event EventHandler<DongleEventArgs>? DongleConnected;

    /// <summary>
    /// Event raised when a USB dongle is disconnected.
    /// </summary>
    event EventHandler<DongleEventArgs>? DongleDisconnected;
}
