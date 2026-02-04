// This is a demonstration file showing how to use the ConsoleUI class
// This file is not part of the project - it's just for documentation purposes

using UsbDongleLicensing.Core;
using UsbDongleLicensing.UI;

// Create ConsoleUI instance
var ui = new ConsoleUI();

// 1. Display welcome banner
ui.DisplayWelcomeBanner();

// 2. Display license status - Valid license
var validLicense = new LicenseData
{
    LicenseKey = "ABCD-1234-EFGH-5678",
    IssuedTo = "Acme Corporation",
    IssuedDate = new DateTime(2024, 1, 1),
    ExpirationDate = new DateTime(2025, 12, 31),
    EnabledFeatures = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | FeatureFlags.DataExport
};

var validResult = new ValidationResult
{
    IsValid = true,
    Status = LicenseStatus.Valid,
    LicenseData = validLicense,
    Errors = new List<string>()
};

ui.DisplayLicenseStatus(validResult);

// 3. Display feature list
var availableFeatures = new List<SdkFeature>
{
    SdkFeature.BasicFeature,
    SdkFeature.AdvancedAnalytics,
    SdkFeature.DataExport
};

ui.DisplayFeatureList(availableFeatures);

// 4. Display help
ui.DisplayHelp();

// 5. Display error
ui.DisplayError("Feature not licensed - please upgrade your license");

// 6. Display license status - No license (Demo mode)
var noLicenseResult = new ValidationResult
{
    IsValid = false,
    Status = LicenseStatus.NotFound,
    LicenseData = null,
    Errors = new List<string>()
};

ui.DisplayLicenseStatus(noLicenseResult);

// 7. Display license status - Invalid license
var expiredLicense = new LicenseData
{
    LicenseKey = "EXPIRED-KEY",
    IssuedTo = "Test User",
    IssuedDate = new DateTime(2023, 1, 1),
    ExpirationDate = new DateTime(2023, 12, 31),
    EnabledFeatures = FeatureFlags.None
};

var invalidResult = new ValidationResult
{
    IsValid = false,
    Status = LicenseStatus.Expired,
    LicenseData = expiredLicense,
    Errors = new List<string> { "License has expired on 2023-12-31" }
};

ui.DisplayLicenseStatus(invalidResult);
