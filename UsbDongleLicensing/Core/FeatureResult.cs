namespace UsbDongleLicensing.Core;

/// <summary>
/// Represents the result of executing an SDK feature.
/// </summary>
public class FeatureResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the feature execution was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message describing the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional data returned by the feature execution.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
}
