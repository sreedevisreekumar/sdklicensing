using System.Text;
using System.Text.Json;
using UsbDongleLicensing.Core;

namespace UsbDongleLicensing.Providers;

/// <summary>
/// Simulates USB dongle behavior using the file system for testing purposes.
/// Reads license data from JSON files and monitors file creation/deletion to simulate connection events.
/// </summary>
public class SimulatedDongleProvider : IUsbDongleProvider, IDisposable
{
    private readonly string _simulationPath;
    private readonly string _licenseFileName = "license.json";
    private FileSystemWatcher? _fileWatcher;
    private bool _disposed;

    /// <summary>
    /// Event raised when a simulated dongle is connected (license file created).
    /// </summary>
    public event EventHandler<DongleEventArgs>? DongleConnected;

    /// <summary>
    /// Event raised when a simulated dongle is disconnected (license file deleted).
    /// </summary>
    public event EventHandler<DongleEventArgs>? DongleDisconnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedDongleProvider"/> class.
    /// </summary>
    /// <param name="simulationPath">The directory path where license files are stored.</param>
    /// <exception cref="ArgumentException">Thrown when simulationPath is null or empty.</exception>
    public SimulatedDongleProvider(string simulationPath)
    {
        if (string.IsNullOrWhiteSpace(simulationPath))
        {
            throw new ArgumentException("Simulation path cannot be null or empty.", nameof(simulationPath));
        }

        _simulationPath = simulationPath;

        // Create the simulation directory if it doesn't exist
        if (!Directory.Exists(_simulationPath))
        {
            Directory.CreateDirectory(_simulationPath);
        }

        // Set up file system watcher to monitor license file changes
        InitializeFileWatcher();
    }

    /// <summary>
    /// Initializes the file system watcher to detect file creation and deletion events.
    /// </summary>
    private void InitializeFileWatcher()
    {
        _fileWatcher = new FileSystemWatcher(_simulationPath)
        {
            Filter = _licenseFileName,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };

        _fileWatcher.Created += OnLicenseFileCreated;
        _fileWatcher.Deleted += OnLicenseFileDeleted;
    }

    /// <summary>
    /// Handles the file created event, raising the DongleConnected event.
    /// </summary>
    private void OnLicenseFileCreated(object sender, FileSystemEventArgs e)
    {
        var dongleInfo = DetectDongle();
        DongleConnected?.Invoke(this, new DongleEventArgs(dongleInfo));
    }

    /// <summary>
    /// Handles the file deleted event, raising the DongleDisconnected event.
    /// </summary>
    private void OnLicenseFileDeleted(object sender, FileSystemEventArgs e)
    {
        var dongleInfo = new DongleInfo
        {
            VendorId = "SIM",
            ProductId = "0001",
            SerialNumber = "SIMULATED",
            DetectedAt = DateTime.UtcNow
        };
        DongleDisconnected?.Invoke(this, new DongleEventArgs(dongleInfo));
    }

    /// <summary>
    /// Checks if a simulated dongle is currently connected by verifying license file existence.
    /// </summary>
    /// <returns>True if the license file exists; otherwise, false.</returns>
    public bool IsConnected()
    {
        var licenseFilePath = Path.Combine(_simulationPath, _licenseFileName);
        return File.Exists(licenseFilePath);
    }

    /// <summary>
    /// Detects and retrieves information about the simulated dongle.
    /// </summary>
    /// <returns>
    /// A <see cref="DongleInfo"/> object if the license file exists; otherwise, null.
    /// </returns>
    public DongleInfo? DetectDongle()
    {
        if (!IsConnected())
        {
            return null;
        }

        return new DongleInfo
        {
            VendorId = "SIM",
            ProductId = "0001",
            SerialNumber = "SIMULATED",
            DetectedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Reads the license data from the simulated dongle (JSON file).
    /// </summary>
    /// <returns>A byte array containing the serialized license data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no license file is present.</exception>
    /// <exception cref="IOException">Thrown when the license file cannot be read or parsed.</exception>
    public byte[] ReadLicenseData()
    {
        if (!IsConnected())
        {
            throw new InvalidOperationException("No simulated dongle is connected. License file not found.");
        }

        var licenseFilePath = Path.Combine(_simulationPath, _licenseFileName);

        try
        {
            // Read the JSON file
            var jsonContent = File.ReadAllText(licenseFilePath);

            // Parse the JSON to validate it's well-formed
            var licenseData = DeserializeLicenseData(jsonContent);

            // Serialize back to JSON and convert to bytes for consistency with the interface
            var serializedJson = JsonSerializer.Serialize(licenseData);
            return Encoding.UTF8.GetBytes(serializedJson);
        }
        catch (FileNotFoundException ex)
        {
            throw new IOException($"License file not found at path: {licenseFilePath}", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"Access denied when reading license file: {licenseFilePath}", ex);
        }
        catch (IOException)
        {
            // Re-throw IOException from DeserializeLicenseData
            throw;
        }
        catch (Exception ex)
        {
            throw new IOException($"An error occurred while reading license data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes license data from JSON content.
    /// </summary>
    /// <param name="jsonContent">The JSON string containing license data.</param>
    /// <returns>A <see cref="LicenseData"/> object.</returns>
    /// <exception cref="IOException">Thrown when deserialization fails.</exception>
    private LicenseData DeserializeLicenseData(string jsonContent)
    {
        try
        {
            // Define JSON options for case-insensitive property matching
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var licenseDto = JsonSerializer.Deserialize<LicenseDataDto>(jsonContent, options);

            if (licenseDto == null)
            {
                throw new IOException("Failed to deserialize license data. The JSON content is null or invalid.");
            }

            // Convert DTO to LicenseData
            return new LicenseData
            {
                LicenseKey = licenseDto.LicenseKey ?? string.Empty,
                IssuedTo = licenseDto.IssuedTo ?? string.Empty,
                IssuedDate = licenseDto.IssuedDate,
                ExpirationDate = licenseDto.ExpirationDate,
                EnabledFeatures = (FeatureFlags)licenseDto.EnabledFeatures,
                Signature = string.IsNullOrEmpty(licenseDto.Signature)
                    ? Array.Empty<byte>()
                    : Convert.FromBase64String(licenseDto.Signature)
            };
        }
        catch (JsonException ex)
        {
            throw new IOException($"Failed to deserialize license data: {ex.Message}", ex);
        }
        catch (FormatException ex)
        {
            throw new IOException($"Failed to decode signature from base64: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Data transfer object for deserializing license JSON files.
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
    /// Disposes the file system watcher and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the file system watcher and releases resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_fileWatcher != null)
                {
                    _fileWatcher.Created -= OnLicenseFileCreated;
                    _fileWatcher.Deleted -= OnLicenseFileDeleted;
                    _fileWatcher.Dispose();
                    _fileWatcher = null;
                }
            }

            _disposed = true;
        }
    }
}
