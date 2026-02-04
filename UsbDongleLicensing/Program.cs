using System.Text;
using Newtonsoft.Json;
using UsbDongleLicensing.Core;
using UsbDongleLicensing.Providers;
using UsbDongleLicensing.SDK;
using UsbDongleLicensing.UI;
using UsbDongleLicensing.Validation;

namespace UsbDongleLicensing;

/// <summary>
/// Main program class for the USB Dongle Licensing Demo application.
/// Orchestrates the initialization and execution of the licensing system.
/// </summary>
class Program
{
    private static AppConfiguration? _configuration;
    private static IUsbDongleProvider? _dongleProvider;
    private static ILicenseValidator? _validator;
    private static ISdkManager? _sdkManager;
    private static ConsoleUI? _ui;
    private static bool _isRunning = true;

    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments for mode selection (--simulation or --hid).</param>
    static void Main(string[] args)
    {
        try
        {
            // Parse command-line arguments
            var mode = ParseCommandLineArguments(args);

            // Load configuration from appsettings.json
            _configuration = LoadConfiguration();

            // Override mode if specified in command-line arguments
            if (!string.IsNullOrEmpty(mode))
            {
                _configuration.Mode = mode;
            }

            // Initialize UI
            _ui = new ConsoleUI();

            // Display welcome banner
            _ui.DisplayWelcomeBanner();

            // Display mode information
            DisplayModeInformation(_configuration.Mode);

            // Initialize components
            InitializeComponents(_configuration);

            // Run startup sequence
            RunStartupSequence();

            // Enter main application loop
            RunApplicationLoop();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nFatal Error: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Parses command-line arguments to determine the operation mode.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>The operation mode (simulation or hid), or null if not specified.</returns>
    private static string? ParseCommandLineArguments(string[] args)
    {
        foreach (var arg in args)
        {
            if (arg.Equals("--simulation", StringComparison.OrdinalIgnoreCase))
            {
                return "simulation";
            }
            else if (arg.Equals("--hid", StringComparison.OrdinalIgnoreCase))
            {
                return "hid";
            }
            else if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) || arg.Equals("-h", StringComparison.OrdinalIgnoreCase))
            {
                DisplayUsageHelp();
                Environment.Exit(0);
            }
        }

        return null;
    }

    /// <summary>
    /// Displays usage help information.
    /// </summary>
    private static void DisplayUsageHelp()
    {
        Console.WriteLine("USB Dongle Licensing Demo - Usage");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("Usage: UsbDongleLicensing [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --simulation    Run in simulation mode (uses file-based license)");
        Console.WriteLine("  --hid           Run in HID mode (uses physical USB dongle)");
        Console.WriteLine("  --help, -h      Display this help message");
        Console.WriteLine();
        Console.WriteLine("If no mode is specified, the mode from appsettings.json will be used.");
        Console.WriteLine();
    }

    /// <summary>
    /// Loads configuration from appsettings.json file.
    /// </summary>
    /// <returns>The loaded application configuration.</returns>
    private static AppConfiguration LoadConfiguration()
    {
        const string configFileName = "appsettings.json";

        try
        {
            if (!File.Exists(configFileName))
            {
                Console.WriteLine($"Warning: Configuration file '{configFileName}' not found. Using default configuration.");
                return new AppConfiguration();
            }

            var jsonContent = File.ReadAllText(configFileName);
            var config = JsonConvert.DeserializeObject<AppConfiguration>(jsonContent);

            if (config == null)
            {
                Console.WriteLine("Warning: Failed to parse configuration file. Using default configuration.");
                return new AppConfiguration();
            }

            return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error loading configuration: {ex.Message}. Using default configuration.");
            return new AppConfiguration();
        }
    }

    /// <summary>
    /// Displays information about the current operation mode.
    /// </summary>
    /// <param name="mode">The operation mode.</param>
    private static void DisplayModeInformation(string mode)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Mode: {mode.ToUpper()}");
        Console.ResetColor();

        if (mode.Equals("simulation", StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ Running in SIMULATION mode - using file-based license");
            Console.ResetColor();
        }
        else if (mode.Equals("hid", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Using physical USB dongle (HID mode)");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Initializes all application components based on configuration.
    /// </summary>
    /// <param name="config">The application configuration.</param>
    private static void InitializeComponents(AppConfiguration config)
    {
        // Initialize appropriate USB provider based on mode
        if (config.Mode.Equals("simulation", StringComparison.OrdinalIgnoreCase))
        {
            _dongleProvider = new SimulatedDongleProvider(config.SimulationPath);
        }
        else if (config.Mode.Equals("hid", StringComparison.OrdinalIgnoreCase))
        {
            // Parse vendor and product IDs from hex strings
            var vendorId = ParseHexId(config.UsbVendorId, "VendorId");
            var productId = ParseHexId(config.UsbProductId, "ProductId");
            
            _dongleProvider = new HidDongleProvider(vendorId, productId);
        }
        else
        {
            throw new InvalidOperationException($"Invalid mode specified: {config.Mode}. Valid modes are 'simulation' or 'hid'.");
        }

        // Initialize validator with public key from configuration
        _validator = new RsaLicenseValidator(config.PublicKey);

        // Initialize SDK manager with dongle provider for disconnection handling
        _sdkManager = new SdkManager(_dongleProvider);

        // Subscribe to dongle connection events
        if (_dongleProvider != null)
        {
            _dongleProvider.DongleConnected += OnDongleConnected;
            _dongleProvider.DongleDisconnected += OnDongleDisconnected;
        }
    }

    /// <summary>
    /// Parses a hexadecimal ID string (e.g., "0x1234") to an integer.
    /// </summary>
    /// <param name="hexString">The hexadecimal string to parse.</param>
    /// <param name="paramName">The parameter name for error messages.</param>
    /// <returns>The parsed integer value.</returns>
    private static int ParseHexId(string hexString, string paramName)
    {
        try
        {
            // Remove "0x" prefix if present
            var cleanHex = hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? hexString.Substring(2)
                : hexString;

            return Convert.ToInt32(cleanHex, 16);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid {paramName} format: {hexString}. Expected hexadecimal format (e.g., 0x1234).", ex);
        }
    }

    /// <summary>
    /// Runs the startup sequence: detect dongle, read license, validate, and initialize SDK.
    /// </summary>
    private static void RunStartupSequence()
    {
        Console.WriteLine("Initializing licensing system...");
        Console.WriteLine();

        // Detect USB dongle
        var dongleInfo = _dongleProvider?.DetectDongle();

        if (dongleInfo == null)
        {
            // No dongle found - enter demo mode
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ No USB dongle detected");
            Console.ResetColor();
            Console.WriteLine();

            var demoResult = new ValidationResult
            {
                IsValid = false,
                Status = LicenseStatus.NotFound,
                Errors = new List<string> { "No USB dongle detected" }
            };

            _sdkManager?.Initialize(demoResult);
            _ui?.DisplayLicenseStatus(demoResult);
        }
        else
        {
            // Dongle found - read and validate license
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ USB dongle detected: {dongleInfo.VendorId}:{dongleInfo.ProductId}");
            Console.ResetColor();
            Console.WriteLine();

            try
            {
                // Read license data
                var licenseDataBytes = _dongleProvider?.ReadLicenseData();

                if (licenseDataBytes != null && licenseDataBytes.Length > 0)
                {
                    // Deserialize license data
                    var licenseJson = Encoding.UTF8.GetString(licenseDataBytes);
                    var licenseData = JsonConvert.DeserializeObject<LicenseData>(licenseJson);

                    if (licenseData != null)
                    {
                        // Validate license
                        var validationResult = _validator?.Validate(licenseData);

                        if (validationResult != null)
                        {
                            // Initialize SDK manager with validation result
                            _sdkManager?.Initialize(validationResult);

                            // Display license status
                            _ui?.DisplayLicenseStatus(validationResult);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to deserialize license data");
                    }
                }
                else
                {
                    throw new InvalidOperationException("No license data read from dongle");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error reading or validating license: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine();

                // Enter demo mode on error
                var errorResult = new ValidationResult
                {
                    IsValid = false,
                    Status = LicenseStatus.Corrupted,
                    Errors = new List<string> { ex.Message }
                };

                _sdkManager?.Initialize(errorResult);
                _ui?.DisplayLicenseStatus(errorResult);
            }
        }
    }

    /// <summary>
    /// Runs the main application loop for command processing.
    /// </summary>
    private static void RunApplicationLoop()
    {
        Console.WriteLine("Type 'help' for available commands or 'exit' to quit.");
        Console.WriteLine();

        while (_isRunning)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            ProcessCommand(input);
        }
    }

    /// <summary>
    /// Processes a user command.
    /// </summary>
    /// <param name="input">The user input command.</param>
    private static void ProcessCommand(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return;
        }

        var command = parts[0].ToLower();

        try
        {
            switch (command)
            {
                case "status":
                    HandleStatusCommand();
                    break;

                case "features":
                    HandleFeaturesCommand();
                    break;

                case "run":
                    if (parts.Length < 2)
                    {
                        _ui?.DisplayError("Usage: run <feature>");
                        Console.WriteLine("Available features: basic, analytics, export, api");
                    }
                    else
                    {
                        HandleRunCommand(parts[1]);
                    }
                    break;

                case "help":
                    _ui?.DisplayHelp();
                    break;

                case "exit":
                case "quit":
                    _isRunning = false;
                    Console.WriteLine("Exiting application...");
                    break;

                default:
                    _ui?.DisplayError($"Unknown command: {command}");
                    Console.WriteLine("Type 'help' for available commands.");
                    break;
            }
        }
        catch (Exception ex)
        {
            _ui?.DisplayError($"Error executing command: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Handles the 'status' command to display current licensing status.
    /// </summary>
    private static void HandleStatusCommand()
    {
        // Re-check dongle status and display current state
        var dongleInfo = _dongleProvider?.DetectDongle();

        if (dongleInfo == null)
        {
            var demoResult = new ValidationResult
            {
                IsValid = false,
                Status = LicenseStatus.NotFound,
                Errors = new List<string> { "No USB dongle detected" }
            };
            _ui?.DisplayLicenseStatus(demoResult);
        }
        else
        {
            try
            {
                var licenseDataBytes = _dongleProvider?.ReadLicenseData();
                if (licenseDataBytes != null && licenseDataBytes.Length > 0)
                {
                    var licenseJson = Encoding.UTF8.GetString(licenseDataBytes);
                    var licenseData = JsonConvert.DeserializeObject<LicenseData>(licenseJson);

                    if (licenseData != null)
                    {
                        var validationResult = _validator?.Validate(licenseData);
                        if (validationResult != null)
                        {
                            _ui?.DisplayLicenseStatus(validationResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ui?.DisplayError($"Error reading license: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles the 'features' command to list available features.
    /// </summary>
    private static void HandleFeaturesCommand()
    {
        var availableFeatures = _sdkManager?.GetAvailableFeatures() ?? new List<SdkFeature>();
        _ui?.DisplayFeatureList(availableFeatures);
    }

    /// <summary>
    /// Handles the 'run' command to execute a specific feature.
    /// </summary>
    /// <param name="featureName">The name of the feature to execute.</param>
    private static void HandleRunCommand(string featureName)
    {
        var feature = ParseFeatureName(featureName);

        if (feature == null)
        {
            _ui?.DisplayError($"Unknown feature: {featureName}");
            Console.WriteLine("Available features: basic, analytics, export, api");
            return;
        }

        Console.WriteLine($"Executing feature: {feature.Value}...");
        Console.WriteLine();

        var result = _sdkManager?.ExecuteFeature(feature.Value);

        if (result != null)
        {
            if (result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ {result.Message}");
                Console.ResetColor();

                if (result.Data != null && result.Data.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Result Data:");
                    foreach (var kvp in result.Data)
                    {
                        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                    }
                }
            }
            else
            {
                _ui?.DisplayError(result.Message);
            }
        }
    }

    /// <summary>
    /// Parses a feature name string to an SdkFeature enum value.
    /// </summary>
    /// <param name="featureName">The feature name to parse.</param>
    /// <returns>The corresponding SdkFeature enum value, or null if not found.</returns>
    private static SdkFeature? ParseFeatureName(string featureName)
    {
        return featureName.ToLower() switch
        {
            "basic" => SdkFeature.BasicFeature,
            "analytics" => SdkFeature.AdvancedAnalytics,
            "export" => SdkFeature.DataExport,
            "api" => SdkFeature.ApiAccess,
            _ => null
        };
    }

    /// <summary>
    /// Handles dongle connected events.
    /// </summary>
    private static void OnDongleConnected(object? sender, DongleEventArgs e)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ USB dongle connected: {e.DongleInfo?.VendorId}:{e.DongleInfo?.ProductId}");
        Console.ResetColor();
        Console.WriteLine("Re-validating license...");
        Console.WriteLine();

        // Re-run startup sequence to validate the new dongle
        RunStartupSequence();
    }

    /// <summary>
    /// Handles dongle disconnected events.
    /// </summary>
    private static void OnDongleDisconnected(object? sender, DongleEventArgs e)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠ USB dongle disconnected");
        Console.ResetColor();
        Console.WriteLine("Premium features have been disabled. Running in demo mode.");
        Console.WriteLine();
    }
}
