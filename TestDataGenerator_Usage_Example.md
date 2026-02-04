# TestDataGenerator Usage Guide

## Overview

The `TestDataGenerator` class provides utilities for creating test license data for the USB Dongle Licensing system. It can generate RSA key pairs, create licenses with custom parameters, sign licenses, and save them to JSON files.

## Basic Usage

### 1. Generate a New Key Pair

```csharp
using UsbDongleLicensing.TestData;

var generator = new TestDataGenerator();

// Generate a 2048-bit RSA key pair (default)
generator.GenerateKeyPair();

// Or specify a custom key size
generator.GenerateKeyPair(4096);
```

### 2. Save the Key Pair

```csharp
// Save keys to files
generator.SaveKeyPair("private_key.xml", "public_key.xml");

// Get the public key in base64 format for configuration
string publicKeyBase64 = generator.GetPublicKeyBase64();
Console.WriteLine($"Public Key (Base64): {publicKeyBase64}");
```

### 3. Create a License

```csharp
using UsbDongleLicensing.Core;

// Create a license with specific parameters
var license = generator.CreateLicense(
    licenseKey: "PROD-1234-5678-ABCD",
    issuedTo: "Acme Corporation",
    issuedDate: DateTime.UtcNow,
    expirationDate: DateTime.UtcNow.AddYears(1),
    enabledFeatures: FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | FeatureFlags.DataExport
);
```

### 4. Sign the License

```csharp
// Generate RSA signature for the license
byte[] signature = generator.SignLicense(license);
license.Signature = signature;
```

### 5. Save the License to a File

```csharp
// Save the signed license to a JSON file
generator.SaveLicense(license, "test-licenses/valid_license.json");
```

## Complete Example: Creating Test Scenarios

```csharp
using System;
using UsbDongleLicensing.Core;
using UsbDongleLicensing.TestData;

class Program
{
    static void Main()
    {
        var generator = new TestDataGenerator();
        
        // Generate and save key pair
        generator.GenerateKeyPair();
        generator.SaveKeyPair("keys/private_key.xml", "keys/public_key.xml");
        
        // Scenario 1: Valid license with all features
        var validLicense = generator.CreateLicense(
            "VALID-1234-5678-ABCD",
            "Test User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | 
            FeatureFlags.DataExport | FeatureFlags.ApiAccess
        );
        validLicense.Signature = generator.SignLicense(validLicense);
        generator.SaveLicense(validLicense, "test-licenses/valid_all_features.json");
        
        // Scenario 2: Valid license with partial features
        var partialLicense = generator.CreateLicense(
            "PARTIAL-1234-5678-ABCD",
            "Test User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics
        );
        partialLicense.Signature = generator.SignLicense(partialLicense);
        generator.SaveLicense(partialLicense, "test-licenses/valid_partial_features.json");
        
        // Scenario 3: Expired license
        var expiredLicense = generator.CreateLicense(
            "EXPIRED-1234-5678-ABCD",
            "Test User",
            DateTime.UtcNow.AddYears(-2),
            DateTime.UtcNow.AddMonths(-1), // Expired 1 month ago
            FeatureFlags.BasicFeature
        );
        expiredLicense.Signature = generator.SignLicense(expiredLicense);
        generator.SaveLicense(expiredLicense, "test-licenses/expired_license.json");
        
        // Scenario 4: License with invalid signature (tampered)
        var tamperedLicense = generator.CreateLicense(
            "TAMPERED-1234-5678-ABCD",
            "Test User",
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            FeatureFlags.BasicFeature
        );
        tamperedLicense.Signature = generator.SignLicense(tamperedLicense);
        // Tamper with the license data after signing
        tamperedLicense.IssuedTo = "Hacker";
        generator.SaveLicense(tamperedLicense, "test-licenses/invalid_signature.json");
        
        Console.WriteLine("Test licenses created successfully!");
        Console.WriteLine($"Public key (for appsettings.json): {generator.GetPublicKeyBase64()}");
    }
}
```

## Loading Existing Keys

If you already have a key pair, you can load it instead of generating a new one:

```csharp
var generator = new TestDataGenerator();
generator.LoadKeyPair("keys/private_key.xml", "keys/public_key.xml");

// Now you can sign licenses with the loaded keys
var license = generator.CreateLicense(...);
license.Signature = generator.SignLicense(license);
```

## Feature Flags

Available feature flags that can be combined:

- `FeatureFlags.None` - No features enabled
- `FeatureFlags.BasicFeature` - Basic feature (always available in demo mode)
- `FeatureFlags.AdvancedAnalytics` - Advanced analytics (requires license)
- `FeatureFlags.DataExport` - Data export capability (requires license)
- `FeatureFlags.ApiAccess` - API access (requires license)

Combine multiple features using the bitwise OR operator (`|`):

```csharp
var features = FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | FeatureFlags.DataExport;
```

## Integration with RsaLicenseValidator

Licenses created with TestDataGenerator can be validated using the RsaLicenseValidator:

```csharp
using UsbDongleLicensing.Validation;

// Create and sign a license
var generator = new TestDataGenerator();
generator.GenerateKeyPair();
var license = generator.CreateLicense(...);
license.Signature = generator.SignLicense(license);

// Get the public key for validation
string publicKeyBase64 = generator.GetPublicKeyBase64();

// Validate the license
var validator = new RsaLicenseValidator(publicKeyBase64);
var result = validator.Validate(license);

Console.WriteLine($"Valid: {result.IsValid}");
Console.WriteLine($"Status: {result.Status}");
if (!result.IsValid)
{
    Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
}
```

## Notes

- Always use UTC dates for consistency
- The signature is generated using RSA-SHA256
- Keys are saved in XML format compatible with .NET's RSACryptoServiceProvider
- The JSON license format matches the format expected by SimulatedDongleProvider
- For production use, keep private keys secure and never distribute them
