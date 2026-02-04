namespace UsbDongleLicensing.SDK;

using UsbDongleLicensing.Core;

/// <summary>
/// Interface for managing SDK features based on license validation.
/// Controls access to SDK features and handles feature execution.
/// </summary>
public interface ISdkManager
{
    /// <summary>
    /// Initializes the SDK manager with the license validation result.
    /// Sets up enabled features based on the validation outcome.
    /// </summary>
    /// <param name="validationResult">The result of license validation containing license status and enabled features.</param>
    void Initialize(ValidationResult validationResult);

    /// <summary>
    /// Checks whether a specific SDK feature is currently enabled.
    /// </summary>
    /// <param name="feature">The SDK feature to check.</param>
    /// <returns>True if the feature is enabled; otherwise, false.</returns>
    bool IsFeatureEnabled(SdkFeature feature);

    /// <summary>
    /// Executes a specific SDK feature if it is enabled.
    /// </summary>
    /// <param name="feature">The SDK feature to execute.</param>
    /// <returns>A FeatureResult containing the execution outcome, message, and any data.</returns>
    FeatureResult ExecuteFeature(SdkFeature feature);

    /// <summary>
    /// Gets a list of all currently available (enabled) SDK features.
    /// </summary>
    /// <returns>A list of enabled SDK features.</returns>
    List<SdkFeature> GetAvailableFeatures();
}
