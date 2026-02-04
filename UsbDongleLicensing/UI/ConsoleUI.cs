namespace UsbDongleLicensing.UI;

using UsbDongleLicensing.Core;

/// <summary>
/// Provides console user interface functionality for displaying licensing information,
/// features, and error messages with color-coded output.
/// </summary>
public class ConsoleUI
{
    private const string ApplicationTitle = "USB Dongle Licensing Demo";
    private const string ApplicationVersion = "1.0.0";

    /// <summary>
    /// Displays the welcome banner with application title and version.
    /// </summary>
    public void DisplayWelcomeBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║  {ApplicationTitle,-54}  ║");
        Console.WriteLine($"║  Version: {ApplicationVersion,-47}  ║");
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Displays the current license status with formatted license information.
    /// </summary>
    /// <param name="validationResult">The validation result containing license status and data.</param>
    public void DisplayLicenseStatus(ValidationResult validationResult)
    {
        Console.WriteLine("License Status:");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();

        if (validationResult.Status == LicenseStatus.NotFound)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ No license found - Running in DEMO MODE");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Limited functionality available.");
            Console.WriteLine("Connect a valid USB dongle to unlock all features.");
        }
        else if (validationResult.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ License is VALID");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine(FormatLicenseInfo(validationResult.LicenseData!));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ License is INVALID");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"Status: {validationResult.Status}");
            
            if (validationResult.Errors.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Validation Errors:");
                foreach (var error in validationResult.Errors)
                {
                    DisplayError(error);
                }
            }

            if (validationResult.LicenseData != null)
            {
                Console.WriteLine();
                Console.WriteLine("License Information:");
                Console.WriteLine(FormatLicenseInfo(validationResult.LicenseData));
            }
        }

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays a list of available features with licensing indicators.
    /// </summary>
    /// <param name="availableFeatures">List of currently enabled features.</param>
    /// <param name="allFeatures">Optional list of all possible features to show licensing requirements.</param>
    public void DisplayFeatureList(List<SdkFeature> availableFeatures, List<SdkFeature>? allFeatures = null)
    {
        Console.WriteLine("Available Features:");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();

        // If allFeatures is not provided, use all enum values
        var featuresToDisplay = allFeatures ?? Enum.GetValues<SdkFeature>().ToList();

        foreach (var feature in featuresToDisplay)
        {
            bool isEnabled = availableFeatures.Contains(feature);
            bool requiresLicense = feature != SdkFeature.BasicFeature;

            // Display feature with status indicator
            if (isEnabled)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  ✓ ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  ✗ ");
            }

            Console.ResetColor();
            Console.Write($"{GetFeatureName(feature),-25}");

            // Display licensing requirement
            if (requiresLicense)
            {
                if (isEnabled)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(" [Licensed]");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(" [Requires License]");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" [Always Available]");
            }

            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays help information showing available commands.
    /// </summary>
    public void DisplayHelp()
    {
        Console.WriteLine("Available Commands:");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("  status              Display current licensing status");
        Console.WriteLine("  features            List all available features");
        Console.WriteLine("  run <feature>       Execute a specific SDK feature");
        Console.WriteLine("                      Features: basic, analytics, export, api");
        Console.WriteLine("  help                Display this help message");
        Console.WriteLine("  exit                Exit the application");
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays an error message with red color coding.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    public void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Formats license information into a readable string.
    /// </summary>
    /// <param name="license">The license data to format.</param>
    /// <returns>A formatted string containing license information.</returns>
    public string FormatLicenseInfo(LicenseData license)
    {
        var lines = new List<string>
        {
            $"License Key:     {license.LicenseKey}",
            $"Issued To:       {license.IssuedTo}",
            $"Issued Date:     {license.IssuedDate:yyyy-MM-dd}",
            $"Expiration Date: {license.ExpirationDate:yyyy-MM-dd}",
            $"Enabled Features: {FormatFeatureFlags(license.EnabledFeatures)}"
        };

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Converts a SdkFeature enum value to a user-friendly display name.
    /// </summary>
    /// <param name="feature">The SDK feature to convert.</param>
    /// <returns>A user-friendly feature name.</returns>
    private string GetFeatureName(SdkFeature feature)
    {
        return feature switch
        {
            SdkFeature.BasicFeature => "Basic Feature",
            SdkFeature.AdvancedAnalytics => "Advanced Analytics",
            SdkFeature.DataExport => "Data Export",
            SdkFeature.ApiAccess => "API Access",
            _ => feature.ToString()
        };
    }

    /// <summary>
    /// Formats feature flags into a readable comma-separated list.
    /// </summary>
    /// <param name="flags">The feature flags to format.</param>
    /// <returns>A comma-separated string of enabled features.</returns>
    private string FormatFeatureFlags(FeatureFlags flags)
    {
        if (flags == FeatureFlags.None)
        {
            return "None";
        }

        var enabledFeatures = new List<string>();

        if (flags.HasFlag(FeatureFlags.BasicFeature))
            enabledFeatures.Add("Basic");
        if (flags.HasFlag(FeatureFlags.AdvancedAnalytics))
            enabledFeatures.Add("Analytics");
        if (flags.HasFlag(FeatureFlags.DataExport))
            enabledFeatures.Add("Export");
        if (flags.HasFlag(FeatureFlags.ApiAccess))
            enabledFeatures.Add("API");

        return string.Join(", ", enabledFeatures);
    }
}
