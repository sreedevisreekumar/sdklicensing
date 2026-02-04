using System.Text;
using System.Text.Json;
using HidSharp;
using UsbDongleLicensing.Core;

namespace UsbDongleLicensing.Providers;

/// <summary>
/// Implements USB dongle detection and communication using the HIDSharp library.
/// Filters HID devices by vendor and product IDs to identify licensing dongles.
/// </summary>
public class HidDongleProvider : IUsbDongleProvider, IDisposable
{
    private readonly int _vendorId;
    private readonly int _productId;
    private readonly int _maxRetries = 3;
    private readonly int _baseDelayMs = 100;
    private DeviceList? _deviceList;
    private bool _disposed;
    private HidDevice? _currentDevice;

    /// <summary>
    /// Event raised when a USB dongle is connected.
    /// </summary>
    public event EventHandler<DongleEventArgs>? DongleConnected;

    /// <summary>
    /// Event raised when a USB dongle is disconnected.
    /// </summary>
    public event EventHandler<DongleEventArgs>? DongleDisconnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="HidDongleProvider"/> class.
    /// </summary>
    /// <param name="vendorId">The USB vendor ID to filter devices (e.g., 0x1234).</param>
    /// <param name="productId">The USB product ID to filter devices (e.g., 0x5678).</param>
    /// <exception cref="ArgumentException">Thrown when vendor or product ID is invalid.</exception>
    public HidDongleProvider(int vendorId, int productId)
    {
        if (vendorId <= 0)
        {
            throw new ArgumentException("Vendor ID must be greater than zero.", nameof(vendorId));
        }

        if (productId <= 0)
        {
            throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
        }

        _vendorId = vendorId;
        _productId = productId;

        // Initialize device list and set up monitoring
        InitializeDeviceMonitoring();
    }

    /// <summary>
    /// Initializes device monitoring for arrival and removal events.
    /// </summary>
    private void InitializeDeviceMonitoring()
    {
        _deviceList = DeviceList.Local;
        _deviceList.Changed += OnDeviceListChanged;
    }

    /// <summary>
    /// Handles device list changes (arrival/removal events).
    /// </summary>
    private void OnDeviceListChanged(object? sender, DeviceListChangedEventArgs e)
    {
        // Check if our target device was added or removed
        var currentlyConnected = IsConnected();
        var wasConnected = _currentDevice != null;

        if (currentlyConnected && !wasConnected)
        {
            // Device connected
            var dongleInfo = DetectDongle();
            if (dongleInfo != null)
            {
                DongleConnected?.Invoke(this, new DongleEventArgs(dongleInfo));
            }
        }
        else if (!currentlyConnected && wasConnected)
        {
            // Device disconnected
            var dongleInfo = new DongleInfo
            {
                VendorId = $"0x{_vendorId:X4}",
                ProductId = $"0x{_productId:X4}",
                SerialNumber = _currentDevice?.GetSerialNumber() ?? "UNKNOWN",
                DetectedAt = DateTime.UtcNow
            };
            _currentDevice = null;
            DongleDisconnected?.Invoke(this, new DongleEventArgs(dongleInfo));
        }
    }

    /// <summary>
    /// Checks if a USB dongle matching the configured vendor and product IDs is currently connected.
    /// </summary>
    /// <returns>True if a matching dongle is connected; otherwise, false.</returns>
    public bool IsConnected()
    {
        var device = FindTargetDevice();
        return device != null;
    }

    /// <summary>
    /// Detects and retrieves information about the connected USB dongle.
    /// </summary>
    /// <returns>
    /// A <see cref="DongleInfo"/> object if a matching dongle is detected; otherwise, null.
    /// </returns>
    public DongleInfo? DetectDongle()
    {
        var device = FindTargetDevice();
        if (device == null)
        {
            _currentDevice = null;
            return null;
        }

        _currentDevice = device;

        return new DongleInfo
        {
            VendorId = $"0x{device.VendorID:X4}",
            ProductId = $"0x{device.ProductID:X4}",
            SerialNumber = device.GetSerialNumber() ?? "UNKNOWN",
            DetectedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Finds the target HID device matching the configured vendor and product IDs.
    /// </summary>
    /// <returns>The matching HidDevice, or null if not found.</returns>
    private HidDevice? FindTargetDevice()
    {
        if (_deviceList == null)
        {
            return null;
        }

        var devices = _deviceList.GetHidDevices();
        return devices.FirstOrDefault(d => d.VendorID == _vendorId && d.ProductID == _productId);
    }

    /// <summary>
    /// Reads the license data from the connected USB dongle using HID feature reports.
    /// Implements retry logic with exponential backoff for handling transient read errors.
    /// </summary>
    /// <returns>A byte array containing the serialized license data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no dongle is connected.</exception>
    /// <exception cref="IOException">Thrown when the license data cannot be read after all retry attempts.</exception>
    public byte[] ReadLicenseData()
    {
        if (!IsConnected())
        {
            throw new InvalidOperationException("No USB dongle is connected.");
        }

        var device = FindTargetDevice();
        if (device == null)
        {
            throw new InvalidOperationException("USB dongle was disconnected during read operation.");
        }

        // Retry logic with exponential backoff
        for (int attempt = 0; attempt < _maxRetries; attempt++)
        {
            try
            {
                return ReadLicenseDataFromDevice(device);
            }
            catch (IOException ex) when (attempt < _maxRetries - 1)
            {
                // Calculate exponential backoff delay: 100ms, 200ms, 400ms
                int delayMs = _baseDelayMs * (int)Math.Pow(2, attempt);
                Console.WriteLine($"Read attempt {attempt + 1} failed: {ex.Message}. Retrying in {delayMs}ms...");
                Thread.Sleep(delayMs);
            }
            catch (Exception ex) when (attempt < _maxRetries - 1)
            {
                // Handle other exceptions with retry
                int delayMs = _baseDelayMs * (int)Math.Pow(2, attempt);
                Console.WriteLine($"Read attempt {attempt + 1} failed: {ex.Message}. Retrying in {delayMs}ms...");
                Thread.Sleep(delayMs);
            }
        }

        // All retries exhausted
        throw new IOException($"Failed to read license data from USB dongle after {_maxRetries} attempts.");
    }

    /// <summary>
    /// Reads license data from the HID device using feature reports.
    /// </summary>
    /// <param name="device">The HID device to read from.</param>
    /// <returns>A byte array containing the license data.</returns>
    /// <exception cref="IOException">Thrown when the device cannot be opened or read.</exception>
    private byte[] ReadLicenseDataFromDevice(HidDevice device)
    {
        try
        {
            // Try to open the device
            if (!device.TryOpen(out var stream))
            {
                throw new IOException("Failed to open USB dongle device for reading.");
            }

            using (stream)
            {
                // Read feature report (Report ID 0)
                // The exact report ID and size would depend on the actual hardware
                // For this implementation, we'll use a reasonable buffer size
                byte[] buffer = new byte[256];
                
                try
                {
                    // Attempt to get a feature report
                    // Note: The actual implementation would need to know the correct report ID
                    // For now, we'll try report ID 0
                    stream.GetFeature(buffer, 0, buffer.Length);
                    
                    // Parse the buffer to extract license data
                    // The format would depend on how the dongle stores data
                    // For this implementation, we'll assume the data is JSON after a header
                    return ExtractLicenseDataFromBuffer(buffer);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to read feature report from device: {ex.Message}", ex);
                }
            }
        }
        catch (IOException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new IOException($"An error occurred while reading from the USB dongle: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extracts license data from the raw buffer read from the HID device.
    /// </summary>
    /// <param name="buffer">The raw buffer containing license data.</param>
    /// <returns>A byte array containing the extracted license data.</returns>
    /// <exception cref="IOException">Thrown when the buffer cannot be parsed.</exception>
    private byte[] ExtractLicenseDataFromBuffer(byte[] buffer)
    {
        try
        {
            // Find the start of JSON data (skip any header bytes)
            // Look for the opening brace '{' which indicates JSON start
            int jsonStart = Array.IndexOf(buffer, (byte)'{');
            
            if (jsonStart == -1)
            {
                throw new IOException("No valid JSON data found in device buffer.");
            }

            // Find the end of JSON data (look for closing brace)
            int jsonEnd = Array.LastIndexOf(buffer, (byte)'}');
            
            if (jsonEnd == -1 || jsonEnd <= jsonStart)
            {
                throw new IOException("Incomplete JSON data found in device buffer.");
            }

            // Extract the JSON portion
            int jsonLength = jsonEnd - jsonStart + 1;
            byte[] jsonData = new byte[jsonLength];
            Array.Copy(buffer, jsonStart, jsonData, 0, jsonLength);

            // Validate that it's valid JSON by attempting to parse it
            string jsonString = Encoding.UTF8.GetString(jsonData);
            ValidateLicenseJson(jsonString);

            return jsonData;
        }
        catch (IOException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to extract license data from buffer: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates that the JSON string contains valid license data.
    /// </summary>
    /// <param name="jsonString">The JSON string to validate.</param>
    /// <exception cref="IOException">Thrown when the JSON is invalid or missing required fields.</exception>
    private void ValidateLicenseJson(string jsonString)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var licenseDto = JsonSerializer.Deserialize<LicenseDataDto>(jsonString, options);

            if (licenseDto == null)
            {
                throw new IOException("Failed to deserialize license data from device.");
            }

            // Validate required fields
            if (string.IsNullOrEmpty(licenseDto.LicenseKey))
            {
                throw new IOException("License data is missing required field: LicenseKey");
            }
        }
        catch (JsonException ex)
        {
            throw new IOException($"Invalid JSON format in license data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Data transfer object for deserializing license JSON data from the device.
    /// </summary>
    private class LicenseDataDto
    {
        public string? LicenseKey { get; set; }
        public string? IssuedTo { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int EnabledFeatures { get; set; }
        public string? Signature { get; set; }
    }

    /// <summary>
    /// Disposes the device list and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the device list and releases resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_deviceList != null)
                {
                    _deviceList.Changed -= OnDeviceListChanged;
                    _deviceList = null;
                }

                _currentDevice = null;
            }

            _disposed = true;
        }
    }
}
