using System;
using UsbDongleLicensing.Core;
using UsbDongleLicensing.TestData;

/// <summary>
/// Utility program to generate RSA keys and test licenses with valid signatures.
/// Run this to create properly signed licenses for testing.
/// </summary>
class GenerateTestLicenses
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== USB Dongle Licensing - Test Data Generator ===");
        Console.WriteLine();

        var generator = new TestDataGenerator();

        // Generate RSA key pair
        Console.WriteLine("Generating RSA key pair (2048-bit)...");
        generator.GenerateKeyPair(2048);
        Console.WriteLine("✓ Key pair generated");
        Console.WriteLine();

        // Save keys to files
        Console.WriteLine("Saving keys to files...");
        generator.SaveKeyPair("keys/private_key.xml", "keys/public_key.xml");
        Console.WriteLine();

        // Get public key for configuration
        var publicKeyBase64 = generator.GetPublicKeyBase64();
        Console.WriteLine("Public Key (Base64) for appsettings.json:");
        Console.WriteLine(publicKeyBase64);
        Console.WriteLine();

        // Scenario 1: Valid license with all features
        Console.WriteLine("Creating test licenses...");
        Console.WriteLine();

        Console.WriteLine("1. Valid license with ALL features");
        var validAllFeatures = generator.CreateLicense(
            "DEMO-1234-5678-ABCD",
            "Test User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(2), // Valid for 2 years
            FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | 
            FeatureFlags.DataExport | FeatureFlags.ApiAccess
        );
        validAllFeatures.Signature = generator.SignLicense(validAllFeatures);
        generator.SaveLicense(validAllFeatures, "test-licenses/valid_all_features.json");

        // Scenario 2: Valid license with partial features
        Console.WriteLine("2. Valid license with PARTIAL features (Basic + Analytics)");
        var validPartial = generator.CreateLicense(
            "PARTIAL-1234-5678-ABCD",
            "Limited User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(2),
            FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics
        );
        validPartial.Signature = generator.SignLicense(validPartial);
        generator.SaveLicense(validPartial, "test-licenses/valid_partial_features.json");

        // Scenario 3: Expired license
        Console.WriteLine("3. EXPIRED license");
        var expired = generator.CreateLicense(
            "EXPIRED-1234-5678-ABCD",
            "Expired User",
            DateTime.UtcNow.AddYears(-2),
            DateTime.UtcNow.AddMonths(-1), // Expired 1 month ago
            FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics
        );
        expired.Signature = generator.SignLicense(expired);
        generator.SaveLicense(expired, "test-licenses/expired_license.json");

        // Scenario 4: License with invalid signature (tampered)
        Console.WriteLine("4. License with INVALID signature (tampered)");
        var tampered = generator.CreateLicense(
            "TAMPERED-1234-5678-ABCD",
            "Original User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(2),
            FeatureFlags.BasicFeature
        );
        tampered.Signature = generator.SignLicense(tampered);
        // Tamper with the data after signing
        tampered.IssuedTo = "Hacker";
        generator.SaveLicense(tampered, "test-licenses/invalid_signature.json");

        // Scenario 5: License with only basic feature
        Console.WriteLine("5. Valid license with BASIC feature only");
        var basicOnly = generator.CreateLicense(
            "BASIC-1234-5678-ABCD",
            "Basic User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(2),
            FeatureFlags.BasicFeature
        );
        basicOnly.Signature = generator.SignLicense(basicOnly);
        generator.SaveLicense(basicOnly, "test-licenses/valid_basic_only.json");

        Console.WriteLine();
        Console.WriteLine("=== Test Data Generation Complete ===");
        Console.WriteLine();
        Console.WriteLine("Next Steps:");
        Console.WriteLine("1. Copy the Public Key (Base64) shown above");
        Console.WriteLine("2. Paste it into UsbDongleLicensing/appsettings.json in the 'PublicKey' field");
        Console.WriteLine("3. Run the application: dotnet run --project UsbDongleLicensing");
        Console.WriteLine("4. Test with different license files in the test-licenses/ directory");
        Console.WriteLine();
        Console.WriteLine("Available test licenses:");
        Console.WriteLine("  - valid_all_features.json     (All features enabled)");
        Console.WriteLine("  - valid_partial_features.json (Basic + Analytics only)");
        Console.WriteLine("  - valid_basic_only.json       (Basic feature only)");
        Console.WriteLine("  - expired_license.json        (Expired license)");
        Console.WriteLine("  - invalid_signature.json      (Tampered license)");
        Console.WriteLine();
        Console.WriteLine("To use a specific license, copy it to test-licenses/license.json");
    }
}
