namespace UsbDongleLicensing.Core;

/// <summary>
/// Represents feature flags as a bitmask for enabled SDK features.
/// </summary>
[Flags]
public enum FeatureFlags
{
    /// <summary>
    /// No features enabled.
    /// </summary>
    None = 0,

    /// <summary>
    /// Basic feature - always available in demo mode.
    /// </summary>
    BasicFeature = 1,

    /// <summary>
    /// Advanced analytics feature - requires license.
    /// </summary>
    AdvancedAnalytics = 2,

    /// <summary>
    /// Data export feature - requires license.
    /// </summary>
    DataExport = 4,

    /// <summary>
    /// API access feature - requires license.
    /// </summary>
    ApiAccess = 8
}
