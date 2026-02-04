namespace UsbDongleLicensing.SDK;

using UsbDongleLicensing.Core;
using UsbDongleLicensing.Providers;

/// <summary>
/// Manages SDK features based on license validation.
/// Controls access to SDK features and handles feature execution.
/// </summary>
public class SdkManager : ISdkManager
{
    private FeatureFlags _enabledFeatures = FeatureFlags.None;
    private bool _isLicenseValid = false;
    private readonly IUsbDongleProvider? _dongleProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SdkManager"/> class.
    /// </summary>
    public SdkManager()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SdkManager"/> class with a dongle provider.
    /// </summary>
    /// <param name="dongleProvider">The USB dongle provider to monitor for disconnection events.</param>
    public SdkManager(IUsbDongleProvider dongleProvider)
    {
        _dongleProvider = dongleProvider;
        
        // Subscribe to dongle disconnection events to disable features
        if (_dongleProvider != null)
        {
            _dongleProvider.DongleDisconnected += OnDongleDisconnected;
        }
    }

    /// <summary>
    /// Initializes the SDK manager with the license validation result.
    /// Sets up enabled features based on the validation outcome.
    /// </summary>
    /// <param name="validationResult">The result of license validation containing license status and enabled features.</param>
    public void Initialize(ValidationResult validationResult)
    {
        if (validationResult == null)
        {
            throw new ArgumentNullException(nameof(validationResult));
        }

        _isLicenseValid = validationResult.IsValid;

        if (_isLicenseValid && validationResult.LicenseData != null)
        {
            // Enable features from the license data
            _enabledFeatures = validationResult.LicenseData.EnabledFeatures;
        }
        else
        {
            // Only enable basic feature in demo mode
            _enabledFeatures = FeatureFlags.BasicFeature;
        }
    }

    /// <summary>
    /// Initializes the SDK manager with specific feature flags.
    /// This overload allows direct control over which features are enabled.
    /// </summary>
    /// <param name="validationResult">The result of license validation.</param>
    /// <param name="enabledFeatures">The specific features to enable.</param>
    public void Initialize(ValidationResult validationResult, FeatureFlags enabledFeatures)
    {
        if (validationResult == null)
        {
            throw new ArgumentNullException(nameof(validationResult));
        }

        _isLicenseValid = validationResult.IsValid;

        if (_isLicenseValid)
        {
            _enabledFeatures = enabledFeatures;
        }
        else
        {
            // Only enable basic feature in demo mode
            _enabledFeatures = FeatureFlags.BasicFeature;
        }
    }

    /// <summary>
    /// Checks whether a specific SDK feature is currently enabled.
    /// </summary>
    /// <param name="feature">The SDK feature to check.</param>
    /// <returns>True if the feature is enabled; otherwise, false.</returns>
    public bool IsFeatureEnabled(SdkFeature feature)
    {
        var featureFlag = ConvertToFeatureFlag(feature);
        return (_enabledFeatures & featureFlag) == featureFlag;
    }

    /// <summary>
    /// Gets a list of all currently available (enabled) SDK features.
    /// </summary>
    /// <returns>A list of enabled SDK features.</returns>
    public List<SdkFeature> GetAvailableFeatures()
    {
        var availableFeatures = new List<SdkFeature>();

        foreach (SdkFeature feature in Enum.GetValues(typeof(SdkFeature)))
        {
            if (IsFeatureEnabled(feature))
            {
                availableFeatures.Add(feature);
            }
        }

        return availableFeatures;
    }

    /// <summary>
    /// Executes a specific SDK feature if it is enabled.
    /// </summary>
    /// <param name="feature">The SDK feature to execute.</param>
    /// <returns>A FeatureResult containing the execution outcome, message, and any data.</returns>
    public FeatureResult ExecuteFeature(SdkFeature feature)
    {
        // Check if the feature is enabled
        if (!IsFeatureEnabled(feature))
        {
            return new FeatureResult
            {
                Success = false,
                Message = $"Feature '{feature}' is not licensed. Please upgrade your license to access this feature."
            };
        }

        // Execute the feature based on its type
        return feature switch
        {
            SdkFeature.BasicFeature => ExecuteBasicFeature(),
            SdkFeature.AdvancedAnalytics => ExecuteAdvancedAnalytics(),
            SdkFeature.DataExport => ExecuteDataExport(),
            SdkFeature.ApiAccess => ExecuteApiAccess(),
            _ => new FeatureResult
            {
                Success = false,
                Message = $"Unknown feature: {feature}"
            }
        };
    }

    /// <summary>
    /// Handles dongle disconnection events by disabling premium features.
    /// </summary>
    private void OnDongleDisconnected(object? sender, DongleEventArgs e)
    {
        // Disable all premium features, keep only basic feature
        _enabledFeatures = FeatureFlags.BasicFeature;
        _isLicenseValid = false;
    }

    /// <summary>
    /// Converts an SdkFeature enum to its corresponding FeatureFlags value.
    /// </summary>
    private FeatureFlags ConvertToFeatureFlag(SdkFeature feature)
    {
        return feature switch
        {
            SdkFeature.BasicFeature => FeatureFlags.BasicFeature,
            SdkFeature.AdvancedAnalytics => FeatureFlags.AdvancedAnalytics,
            SdkFeature.DataExport => FeatureFlags.DataExport,
            SdkFeature.ApiAccess => FeatureFlags.ApiAccess,
            _ => FeatureFlags.None
        };
    }

    /// <summary>
    /// Executes the basic feature - always available, displays simple message.
    /// </summary>
    private FeatureResult ExecuteBasicFeature()
    {
        return new FeatureResult
        {
            Success = true,
            Message = "Basic feature executed successfully. This feature is available in demo mode.",
            Data = new Dictionary<string, object>
            {
                { "featureName", "BasicFeature" },
                { "executionTime", DateTime.UtcNow },
                { "demoMode", !_isLicenseValid }
            }
        };
    }

    /// <summary>
    /// Executes advanced analytics - requires license, performs mock data analysis.
    /// </summary>
    private FeatureResult ExecuteAdvancedAnalytics()
    {
        // Simulate data analysis
        var analysisResults = new Dictionary<string, object>
        {
            { "totalRecords", 10000 },
            { "averageValue", 42.5 },
            { "maxValue", 99.9 },
            { "minValue", 1.2 },
            { "standardDeviation", 15.3 },
            { "analysisDate", DateTime.UtcNow },
            { "processingTime", "1.23s" }
        };

        return new FeatureResult
        {
            Success = true,
            Message = "Advanced analytics completed successfully. Analysis of 10,000 records performed.",
            Data = analysisResults
        };
    }

    /// <summary>
    /// Executes data export - requires license, generates mock export file.
    /// </summary>
    private FeatureResult ExecuteDataExport()
    {
        // Simulate export file generation
        var exportFileName = $"export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var exportPath = Path.Combine(Path.GetTempPath(), exportFileName);

        return new FeatureResult
        {
            Success = true,
            Message = $"Data export completed successfully. File generated: {exportFileName}",
            Data = new Dictionary<string, object>
            {
                { "fileName", exportFileName },
                { "filePath", exportPath },
                { "fileSize", "2.5 MB" },
                { "recordCount", 5000 },
                { "exportDate", DateTime.UtcNow },
                { "format", "CSV" }
            }
        };
    }

    /// <summary>
    /// Executes API access - requires license, simulates API call.
    /// </summary>
    private FeatureResult ExecuteApiAccess()
    {
        // Simulate API call
        var apiResponse = new Dictionary<string, object>
        {
            { "endpoint", "https://api.example.com/v1/data" },
            { "method", "GET" },
            { "statusCode", 200 },
            { "responseTime", "245ms" },
            { "dataReceived", "1024 bytes" },
            { "timestamp", DateTime.UtcNow },
            { "authenticated", true }
        };

        return new FeatureResult
        {
            Success = true,
            Message = "API access successful. Data retrieved from remote endpoint.",
            Data = apiResponse
        };
    }
}
