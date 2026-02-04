namespace UsbDongleLicensing.Core;

/// <summary>
/// Represents individual SDK features that can be executed.
/// </summary>
public enum SdkFeature
{
    /// <summary>
    /// Basic feature - always available in demo mode.
    /// </summary>
    BasicFeature,

    /// <summary>
    /// Advanced analytics feature - requires license.
    /// </summary>
    AdvancedAnalytics,

    /// <summary>
    /// Data export feature - requires license.
    /// </summary>
    DataExport,

    /// <summary>
    /// API access feature - requires license.
    /// </summary>
    ApiAccess
}
