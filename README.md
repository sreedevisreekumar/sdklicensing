# USB Dongle Licensing Demo

A C# console application that demonstrates SDK licensing using USB dongles. This application showcases hardware-based license validation by detecting USB devices, reading encrypted license data, validating cryptographic signatures, and conditionally unlocking SDK features.

## Overview

This project demonstrates how to implement a robust hardware-based licensing system for software applications. It uses USB dongles as physical security keys that contain license information, providing a tangible method to control software access and prevent unauthorized use.

### Key Features

- **USB Dongle Detection**: Automatically detects USB dongles connected to any port
- **License Validation**: Verifies license authenticity using RSA-SHA256 digital signatures
- **Feature Management**: Conditionally unlocks SDK features based on license validity
- **Dual Mode Operation**: 
  - **HID Mode**: Works with physical USB dongles via HID protocol
  - **Simulation Mode**: Uses file-based simulation for testing without hardware
- **Graceful Degradation**: Continues in demo mode when dongle is absent or invalid
- **Real-time Monitoring**: Detects dongle connection/disconnection events during runtime
- **Property-Based Testing**: Comprehensive test coverage using FsCheck

## Architecture

The application follows a layered architecture:

```
Console Application Layer (UI & Commands)
         ↓
SDK Manager Layer (Feature Access Control)
         ↓
License Validator Layer (RSA Signature Verification)
         ↓
USB Dongle Interface Layer (HID or Simulation)
```

## Project Structure

```
/Core              - Interfaces and data models
/Providers         - USB dongle implementations (HID & Simulation)
/Validation        - License validation logic
/SDK               - Feature management
/UI                - Console interface
/Tests             - Unit and property-based tests
/test-licenses     - Sample license files for simulation mode
```

## Getting Started

### Prerequisites

- .NET 6.0 or later
- Windows OS (for HID USB communication)
- Visual Studio 2022 or VS Code with C# extension

### Installation

1. Clone the repository
2. Restore NuGet packages:
   ```
   dotnet restore
   ```

3. Build the project:
   ```
   dotnet build
   ```

### Running the Application

#### Simulation Mode (No Hardware Required)

Run with simulated USB dongle using test license files:

```
dotnet run -- --simulation
```

Or specify a custom path for license files:

```
dotnet run -- --simulation --path ./my-licenses
```

#### HID Mode (Physical USB Dongle)

Run with a physical USB dongle:

```
dotnet run -- --hid
```

Make sure your USB dongle is connected and the vendor/product IDs are configured in `appsettings.json`.

### Configuration

Edit `appsettings.json` to configure the application:

```json
{
  "mode": "simulation",
  "simulationPath": "./test-licenses",
  "usbVendorId": "0x1234",
  "usbProductId": "0x5678",
  "publicKey": "base64-encoded-rsa-public-key"
}
```

## Available Commands

Once the application is running, use these commands:

- `status` - Display current licensing status
- `features` - List all available SDK features
- `run <feature>` - Execute a specific feature (e.g., `run BasicFeature`)
- `help` - Display command help
- `exit` - Exit the application

## SDK Features

The application demonstrates four SDK features:

1. **BasicFeature** - Always available (demo mode)
2. **AdvancedAnalytics** - Requires valid license
3. **DataExport** - Requires valid license
4. **ApiAccess** - Requires valid license

## Test Data Generation

Generate test license files with real RSA signatures for simulation mode:

### Quick Method: Use the TestDataGenerator

Create a simple C# file to generate keys and licenses:

```csharp
using UsbDongleLicensing.Core;
using UsbDongleLicensing.TestData;

var generator = new TestDataGenerator();
generator.GenerateKeyPair();
generator.SaveKeyPair("keys/private_key.xml", "keys/public_key.xml");

var license = generator.CreateLicense(
    "DEMO-1234-5678-ABCD",
    "Test User",
    DateTime.UtcNow,
    DateTime.UtcNow.AddYears(2),
    FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | 
    FeatureFlags.DataExport | FeatureFlags.ApiAccess
);
license.Signature = generator.SignLicense(license);
generator.SaveLicense(license, "test-licenses/license.json");

Console.WriteLine($"Public Key: {generator.GetPublicKeyBase64()}");
```

### Detailed Instructions

See **[GENERATE_REAL_LICENSES.md](GENERATE_REAL_LICENSES.md)** for complete instructions on:
- Generating RSA key pairs
- Creating properly signed licenses
- Configuring the application with the public key
- Testing different license scenarios

This creates sample licenses with various scenarios:
- Valid license with all features
- Valid license with partial features
- Expired license
- License with invalid signature
- Corrupted license data

## Running Tests

Run all tests (unit + property-based):

```
dotnet test
```

Run only property-based tests:

```
dotnet test --filter Category=PropertyBased
```

## Manual Testing

For comprehensive manual testing instructions, see the **[Manual Testing Guide](MANUAL_TESTING_GUIDE.md)**.

The guide includes:
- 7 detailed test scenarios (demo mode, valid license, partial license, expired license, etc.)
- Step-by-step instructions with expected behavior
- Feature flags reference table
- Verification checklist
- Troubleshooting tips

Quick start for manual testing:

1. **Test Demo Mode** (no license):
   ```cmd
   del test-licenses\license.json
   dotnet run --project UsbDongleLicensing
   ```

2. **Test Valid License** (all features):
   ```cmd
   echo {"licenseKey":"DEMO-1234","issuedTo":"Test User","issuedDate":"2024-01-01T00:00:00Z","expirationDate":"2025-12-31T23:59:59Z","enabledFeatures":15,"signature":"dGVzdHNpZ25hdHVyZWZvcmRlbW9wdXJwb3Nlcw=="} > test-licenses\license.json
   dotnet run --project UsbDongleLicensing
   ```

## Documentation

### Specification Documents

Detailed specification documents are available in the `.kiro/specs/usb-dongle-licensing/` directory:

- **[Requirements Document](.kiro/specs/usb-dongle-licensing/requirements.md)** - Complete requirements with user stories and acceptance criteria
- **[Design Document](.kiro/specs/usb-dongle-licensing/design.md)** - Architecture, components, interfaces, and correctness properties
- **[Implementation Tasks](.kiro/specs/usb-dongle-licensing/tasks.md)** - Step-by-step implementation plan with 12 main tasks

### Testing Documentation

- **[Manual Testing Guide](MANUAL_TESTING_GUIDE.md)** - Comprehensive step-by-step manual testing instructions
- **[Project Setup](PROJECT_SETUP.md)** - Project structure and dependencies overview

## How It Works

### License Validation Flow

1. **Detection**: Application detects USB dongle (or simulation file)
2. **Reading**: Reads license data from dongle (JSON format)
3. **Validation**: 
   - Verifies RSA digital signature
   - Checks expiration date
   - Validates required fields
4. **Feature Unlocking**: Enables SDK features based on license flags
5. **Monitoring**: Watches for dongle disconnection events

### Security

- **RSA-SHA256 Signatures**: Prevents license tampering
- **Public Key Verification**: Public key embedded in application
- **Expiration Checks**: Time-based license validity
- **Feature Flags**: Granular control over enabled features

## Demo Mode

When no valid license is found, the application runs in demo mode:
- Only BasicFeature is available
- Premium features display licensing errors
- Clear indication of limited functionality

## Troubleshooting

### Dongle Not Detected

- Verify USB dongle is connected
- Check vendor/product IDs in configuration
- Ensure HID drivers are installed
- Try running as administrator

### License Validation Fails

- Verify license file format (JSON)
- Check signature is valid base64
- Ensure expiration date is in the future
- Verify public key matches the signing key

### Simulation Mode Issues

- Ensure license files exist in configured path
- Check JSON format is valid
- Verify file permissions

## Dependencies

- **HIDSharp** - USB HID communication
- **Newtonsoft.Json** - JSON serialization
- **FsCheck** - Property-based testing
- **xUnit** - Test framework

## License

This is a demonstration project for educational purposes.

## Contributing

This is a demo application. For production use, consider:
- Hardware security modules (HSM) for key storage
- Network-based license validation
- License server infrastructure
- Encrypted communication channels
- Audit logging and monitoring

## Support

For questions or issues, refer to the specification documents or create an issue in the repository.

---

**Note**: This application demonstrates licensing concepts. For production systems, consult security experts and consider commercial licensing solutions like Sentinel HASP, CodeMeter, or similar enterprise-grade systems.
