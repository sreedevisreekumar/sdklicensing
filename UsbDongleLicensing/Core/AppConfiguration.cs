namespace UsbDongleLicensing.Core;

/// <summary>
/// Represents the application configuration loaded from appsettings.json.
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Gets or sets the mode of operation (simulation or hid).
    /// </summary>
    public string Mode { get; set; } = "simulation";

    /// <summary>
    /// Gets or sets the path to the simulation license files directory.
    /// </summary>
    public string SimulationPath { get; set; } = "./test-licenses";

    /// <summary>
    /// Gets or sets the USB vendor ID for HID mode.
    /// </summary>
    public string UsbVendorId { get; set; } = "0x1234";

    /// <summary>
    /// Gets or sets the USB product ID for HID mode.
    /// </summary>
    public string UsbProductId { get; set; } = "0x5678";

    /// <summary>
    /// Gets or sets the RSA public key in base64-encoded XML format.
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the logging configuration.
    /// </summary>
    public LoggingConfiguration Logging { get; set; } = new LoggingConfiguration();
}

/// <summary>
/// Represents logging configuration settings.
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Gets or sets the log level settings.
    /// </summary>
    public LogLevelConfiguration LogLevel { get; set; } = new LogLevelConfiguration();

    /// <summary>
    /// Gets or sets the console logging configuration.
    /// </summary>
    public ConsoleLoggingConfiguration Console { get; set; } = new ConsoleLoggingConfiguration();

    /// <summary>
    /// Gets or sets the file logging configuration.
    /// </summary>
    public FileLoggingConfiguration File { get; set; } = new FileLoggingConfiguration();
}

/// <summary>
/// Represents log level configuration.
/// </summary>
public class LogLevelConfiguration
{
    /// <summary>
    /// Gets or sets the default log level.
    /// </summary>
    public string Default { get; set; } = "Information";

    /// <summary>
    /// Gets or sets the Microsoft log level.
    /// </summary>
    public string Microsoft { get; set; } = "Warning";
}

/// <summary>
/// Represents console logging configuration.
/// </summary>
public class ConsoleLoggingConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether console logging is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Represents file logging configuration.
/// </summary>
public class FileLoggingConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether file logging is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the path to the log file.
    /// </summary>
    public string Path { get; set; } = "logs/app.log";
}
