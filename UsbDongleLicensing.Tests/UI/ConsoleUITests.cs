using UsbDongleLicensing.Core;
using UsbDongleLicensing.UI;
using Xunit;
using System.Text;

namespace UsbDongleLicensing.Tests.UI;

/// <summary>
/// Unit tests for ConsoleUI class.
/// Tests UI output formatting and display methods.
/// </summary>
public class ConsoleUITests
{
    private readonly ConsoleUI _consoleUI;

    public ConsoleUITests()
    {
        _consoleUI = new ConsoleUI();
    }

    /// <summary>
    /// Helper method to capture console output during test execution.
    /// </summary>
    private string CaptureConsoleOutput(Action action)
    {
        var originalOut = Console.Out;
        var originalForeground = Console.ForegroundColor;
        
        try
        {
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            action();
            
            return stringWriter.ToString();
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.ForegroundColor = originalForeground;
        }
    }

    [Fact]
    public void DisplayWelcomeBanner_ContainsApplicationTitle()
    {
        // Arrange & Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayWelcomeBanner());

        // Assert
        Assert.Contains("USB Dongle Licensing Demo", output);
    }

    [Fact]
    public void DisplayWelcomeBanner_ContainsVersion()
    {
        // Arrange & Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayWelcomeBanner());

        // Assert
        Assert.Contains("Version:", output);
        Assert.Contains("1.0.0", output);
    }

    [Fact]
    public void DisplayLicenseStatus_ValidLicense_ShowsValidStatus()
    {
        // Arrange
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-1234-5678-ABCD",
            IssuedTo = "Test User",
            IssuedDate = new DateTime(2024, 1, 1),
            ExpirationDate = new DateTime(2025, 12, 31),
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics
        };

        var validationResult = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            LicenseData = licenseData,
            Errors = new List<string>()
        };

        // Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayLicenseStatus(validationResult));

        // Assert
        Assert.Contains("License is VALID", output);
        Assert.Contains("TEST-1234-5678-ABCD", output);
        Assert.Contains("Test User", output);
    }

    [Fact]
    public void DisplayLicenseStatus_NotFound_ShowsDemoMode()
    {
        // Arrange
        var validationResult = new ValidationResult
        {
            IsValid = false,
            Status = LicenseStatus.NotFound,
            LicenseData = null,
            Errors = new List<string>()
        };

        // Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayLicenseStatus(validationResult));

        // Assert
        Assert.Contains("No license found", output);
        Assert.Contains("DEMO MODE", output);
        Assert.Contains("Limited functionality", output);
    }

    [Fact]
    public void DisplayLicenseStatus_InvalidLicense_ShowsInvalidStatus()
    {
        // Arrange
        var licenseData = new LicenseData
        {
            LicenseKey = "INVALID-KEY",
            IssuedTo = "Test User",
            IssuedDate = new DateTime(2024, 1, 1),
            ExpirationDate = new DateTime(2023, 12, 31),
            EnabledFeatures = FeatureFlags.None
        };

        var validationResult = new ValidationResult
        {
            IsValid = false,
            Status = LicenseStatus.Expired,
            LicenseData = licenseData,
            Errors = new List<string> { "License has expired" }
        };

        // Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayLicenseStatus(validationResult));

        // Assert
        Assert.Contains("License is INVALID", output);
        Assert.Contains("Expired", output);
        Assert.Contains("License has expired", output);
    }

    [Fact]
    public void DisplayLicenseStatus_MultipleErrors_ShowsAllErrors()
    {
        // Arrange
        var validationResult = new ValidationResult
        {
            IsValid = false,
            Status = LicenseStatus.InvalidSignature,
            LicenseData = null,
            Errors = new List<string> 
            { 
                "Invalid signature",
                "License expired",
                "Missing required fields"
            }
        };

        // Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayLicenseStatus(validationResult));

        // Assert
        Assert.Contains("Invalid signature", output);
        Assert.Contains("License expired", output);
        Assert.Contains("Missing required fields", output);
    }

    [Fact]
    public void DisplayFeatureList_ShowsAllFeatures()
    {
        // Arrange
        var availableFeatures = new List<SdkFeature>
        {
            SdkFeature.BasicFeature,
            SdkFeature.AdvancedAnalytics
        };

        // Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayFeatureList(availableFeatures));

        // Assert
        Assert.Contains("Basic Feature", output);
        Assert.Contains("Advanced Analytics", output);
        Assert.Contains("Data Export", output);
        Assert.Contains("API Access", output);
    }

    [Fact]
    public void DisplayFeatureList_ShowsLicensingIndicators()
    {
        // Arrange
        var availableFeatures = new List<SdkFeature>
        {
            SdkFeature.BasicFeature
        };

        // Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayFeatureList(availableFeatures));

        // Assert
        Assert.Contains("Always Available", output);
        Assert.Contains("Requires License", output);
    }

    [Fact]
    public void DisplayFeatureList_EnabledFeatures_ShowsLicensedIndicator()
    {
        // Arrange
        var availableFeatures = new List<SdkFeature>
        {
            SdkFeature.BasicFeature,
            SdkFeature.AdvancedAnalytics,
            SdkFeature.DataExport
        };

        // Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayFeatureList(availableFeatures));

        // Assert
        Assert.Contains("[Licensed]", output);
    }

    [Fact]
    public void DisplayHelp_ShowsAllCommands()
    {
        // Arrange & Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayHelp());

        // Assert
        Assert.Contains("status", output);
        Assert.Contains("features", output);
        Assert.Contains("run <feature>", output);
        Assert.Contains("help", output);
        Assert.Contains("exit", output);
    }

    [Fact]
    public void DisplayHelp_ShowsCommandDescriptions()
    {
        // Arrange & Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayHelp());

        // Assert
        Assert.Contains("Display current licensing status", output);
        Assert.Contains("List all available features", output);
        Assert.Contains("Execute a specific SDK feature", output);
        Assert.Contains("Display this help message", output);
        Assert.Contains("Exit the application", output);
    }

    [Fact]
    public void DisplayError_ShowsErrorMessage()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var output = CaptureConsoleOutput(() => _consoleUI.DisplayError(errorMessage));

        // Assert
        Assert.Contains("Test error message", output);
    }

    [Fact]
    public void FormatLicenseInfo_ContainsAllFields()
    {
        // Arrange
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST-KEY-1234",
            IssuedTo = "John Doe",
            IssuedDate = new DateTime(2024, 1, 15),
            ExpirationDate = new DateTime(2025, 1, 15),
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | FeatureFlags.DataExport
        };

        // Act
        var output = _consoleUI.FormatLicenseInfo(licenseData);

        // Assert
        Assert.Contains("License Key:", output);
        Assert.Contains("TEST-KEY-1234", output);
        Assert.Contains("Issued To:", output);
        Assert.Contains("John Doe", output);
        Assert.Contains("Issued Date:", output);
        Assert.Contains("2024-01-15", output);
        Assert.Contains("Expiration Date:", output);
        Assert.Contains("2025-01-15", output);
        Assert.Contains("Enabled Features:", output);
    }

    [Fact]
    public void FormatLicenseInfo_FormatsFeatureFlags_AllFeatures()
    {
        // Arrange
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST",
            IssuedTo = "Test",
            IssuedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | 
                             FeatureFlags.DataExport | FeatureFlags.ApiAccess
        };

        // Act
        var output = _consoleUI.FormatLicenseInfo(licenseData);

        // Assert
        Assert.Contains("Basic", output);
        Assert.Contains("Analytics", output);
        Assert.Contains("Export", output);
        Assert.Contains("API", output);
    }

    [Fact]
    public void FormatLicenseInfo_FormatsFeatureFlags_NoFeatures()
    {
        // Arrange
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST",
            IssuedTo = "Test",
            IssuedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            EnabledFeatures = FeatureFlags.None
        };

        // Act
        var output = _consoleUI.FormatLicenseInfo(licenseData);

        // Assert
        Assert.Contains("None", output);
    }

    [Fact]
    public void FormatLicenseInfo_FormatsFeatureFlags_PartialFeatures()
    {
        // Arrange
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST",
            IssuedTo = "Test",
            IssuedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.DataExport
        };

        // Act
        var output = _consoleUI.FormatLicenseInfo(licenseData);

        // Assert
        Assert.Contains("Basic", output);
        Assert.Contains("Export", output);
        Assert.DoesNotContain("Analytics", output);
        Assert.DoesNotContain("API", output);
    }

    [Fact]
    public void FormatLicenseInfo_FormatsDatesProperly()
    {
        // Arrange
        var licenseData = new LicenseData
        {
            LicenseKey = "TEST",
            IssuedTo = "Test",
            IssuedDate = new DateTime(2024, 3, 15),
            ExpirationDate = new DateTime(2025, 6, 20),
            EnabledFeatures = FeatureFlags.None
        };

        // Act
        var output = _consoleUI.FormatLicenseInfo(licenseData);

        // Assert
        Assert.Contains("2024-03-15", output);
        Assert.Contains("2025-06-20", output);
    }
}
