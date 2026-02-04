# Design Document: USB Dongle Licensing Demo

## Overview

This design describes a C# console application that demonstrates SDK licensing using USB dongles. The application showcases hardware-based license validation by detecting USB devices, reading encrypted license data, validating cryptographic signatures, and conditionally unlocking SDK features.

The system uses HID (Human Interface Device) protocol for USB communication and RSA cryptography for license validation. To enable testing without physical hardware, the design includes a simulation mode that mimics USB dongle behavior using file-based storage.

Key design principles:
- **Separation of concerns**: USB communication, license validation, and feature management are independent components
- **Testability**: Simulation mode allows testing without physical dongles
- **Security**: RSA digital signatures prevent license tampering
- **User feedback**: Clear console output for all operations and error states

## Architecture

The application follows a layered architecture with four main components:

```
┌─────────────────────────────────────────┐
│       Console Application Layer         │
│  (User Interface & Command Processing)  │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         SDK Manager Layer               │
│    (Feature Access Control)             │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      License Validator Layer            │
│  (Signature & Expiration Validation)    │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      USB Dongle Interface Layer         │
│  (Device Detection & Data Reading)      │
│  ┌──────────────┐  ┌─────────────────┐ │
│  │ HID Device   │  │  Simulation     │ │
│  │ Provider     │  │  Provider       │ │
│  └──────────────┘  └─────────────────┘ │
└─────────────────────────────────────────┘
```

**Component Responsibilities:**

1. **Console Application Layer**: Handles user interaction, command parsing, and output formatting
2. **SDK Manager Layer**: Controls access to SDK features based on license status
3. **License Validator Layer**: Verifies license authenticity using RSA signatures and checks expiration
4. **USB Dongle Interface Layer**: Abstracts USB communication with pluggable providers (real HID or simulation)

## Components and Interfaces

### 1. USB Dongle Interface

**Purpose**: Abstract USB device detection and communication

**Interface: `IUsbDongleProvider`**
```csharp
interface IUsbDongleProvider
{
    bool IsConnected();
    DongleInfo? DetectDongle();
    byte[] ReadLicenseData();
    event EventHandler<DongleEventArgs> DongleConnected;
    event EventHandler<DongleEventArgs> DongleDisconnected;
}
```

**Implementation: `HidDongleProvider`**
- Uses HIDSharp library ([https://www.zer7.com/software/hidsharp](https://www.zer7.com/software/hidsharp)) for USB HID communication
- Filters devices by Vendor ID and Product ID to identify licensing dongles
- Reads license data from HID feature reports
- Monitors device arrival/removal events using Windows device notifications

**Implementation: `SimulatedDongleProvider`**
- Reads license data from JSON files in a designated directory
- Simulates connection/disconnection events via file system watcher
- Allows testing without physical hardware

**Data Structure: `DongleInfo`**
```csharp
class DongleInfo
{
    string VendorId;
    string ProductId;
    string SerialNumber;
    DateTime DetectedAt;
}
```

### 2. License Validator

**Purpose**: Validate license authenticity and expiration

**Interface: `ILicenseValidator`**
```csharp
interface ILicenseValidator
{
    ValidationResult Validate(LicenseData license);
    bool VerifySignature(byte[] data, byte[] signature);
    bool CheckExpiration(DateTime expirationDate);
}
```

**Implementation: `RsaLicenseValidator`**
- Uses RSA-SHA256 for signature verification (based on [https://kb3hha.com/ArticleDetails?id=5](https://kb3hha.com/ArticleDetails?id=5))
- Embeds public key in application for signature verification
- Validates license structure and required fields
- Checks expiration against system clock

**Data Structure: `LicenseData`**
```csharp
class LicenseData
{
    string LicenseKey;           // Unique identifier
    DateTime ExpirationDate;     // License validity period
    FeatureFlags EnabledFeatures; // Bitmask of enabled features
    byte[] Signature;            // RSA signature of license data
    string IssuedTo;             // Licensee name
    DateTime IssuedDate;         // Issue timestamp
}
```

**Data Structure: `ValidationResult`**
```csharp
class ValidationResult
{
    bool IsValid;
    List<string> Errors;         // Validation failure reasons
    LicenseStatus Status;        // Valid, Expired, InvalidSignature, Corrupted
}
```

**Enum: `LicenseStatus`**
```csharp
enum LicenseStatus
{
    Valid,
    Expired,
    InvalidSignature,
    Corrupted,
    NotFound
}
```

### 3. SDK Manager

**Purpose**: Control access to SDK features based on license

**Interface: `ISdkManager`**
```csharp
interface ISdkManager
{
    void Initialize(ValidationResult validationResult);
    bool IsFeatureEnabled(SdkFeature feature);
    void ExecuteFeature(SdkFeature feature);
    List<SdkFeature> GetAvailableFeatures();
}
```

**Implementation: `SdkManager`**
- Maintains current license state
- Checks feature flags before allowing execution
- Provides demo implementations for unlicensed mode
- Responds to dongle disconnection events by disabling features

**Enum: `FeatureFlags`** (Bitmask)
```csharp
[Flags]
enum FeatureFlags
{
    None = 0,
    BasicFeature = 1,      // Always available (demo)
    AdvancedAnalytics = 2, // Requires license
    DataExport = 4,        // Requires license
    ApiAccess = 8          // Requires license
}
```

**Enum: `SdkFeature`**
```csharp
enum SdkFeature
{
    BasicFeature,
    AdvancedAnalytics,
    DataExport,
    ApiAccess
}
```

### 4. Console Application

**Purpose**: Provide user interface and orchestrate components

**Class: `Program`**
- Main entry point
- Initializes all components with dependency injection pattern
- Handles command-line arguments for test mode
- Manages application lifecycle

**Class: `ConsoleUI`**
- Displays welcome banner and licensing status
- Formats license information for console output
- Provides command menu and help text
- Handles user input and command routing
- Displays error messages with color coding

**Commands:**
- `status` - Display current licensing status
- `features` - List available features
- `run <feature>` - Execute a specific SDK feature
- `help` - Display command help
- `exit` - Exit application

### 5. Test Data Generator

**Purpose**: Create test license files for simulation mode

**Class: `TestDataGenerator`**
- Generates RSA key pairs for testing
- Creates sample license files with various scenarios:
  - Valid license with all features
  - Valid license with partial features
  - Expired license
  - License with invalid signature
  - Corrupted license data
- Saves test licenses as JSON files
- Provides utility to sign license data with private key

## Data Models

### License File Format (JSON)

```json
{
  "licenseKey": "XXXX-XXXX-XXXX-XXXX",
  "issuedTo": "Test User",
  "issuedDate": "2024-01-01T00:00:00Z",
  "expirationDate": "2025-01-01T00:00:00Z",
  "enabledFeatures": 15,
  "signature": "base64-encoded-rsa-signature"
}
```

### Configuration File Format (JSON)

```json
{
  "mode": "simulation",
  "simulationPath": "./test-licenses",
  "usbVendorId": "0x1234",
  "usbProductId": "0x5678",
  "publicKey": "base64-encoded-public-key"
}
```

### Feature Execution Results

Each SDK feature returns a result object:

```csharp
class FeatureResult
{
    bool Success;
    string Message;
    Dictionary<string, object> Data;
}
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: USB Dongle Identification
*For any* set of connected USB devices with various vendor and product IDs, the detection logic should identify only devices matching the configured licensing dongle vendor/product ID pair.
**Validates: Requirements 1.1, 1.5**

### Property 2: Device Event Handling
*For any* sequence of dongle connection and disconnection events, the application should correctly detect and respond to each event by updating the connection status.
**Validates: Requirements 1.2, 1.3**

### Property 3: License Data Extraction Completeness
*For any* valid license data read from a dongle, all required fields (license key, expiration date, feature flags, signature, issued to, issued date) should be successfully extracted and populated.
**Validates: Requirements 2.2**

### Property 4: Read Error Handling
*For any* unreadable or corrupted dongle data, the read operation should return an error result without crashing the application.
**Validates: Requirements 2.3, 6.3**

### Property 5: Encryption Round-Trip
*For any* valid license data, encrypting then decrypting should produce equivalent license information with all fields preserved.
**Validates: Requirements 2.5**

### Property 6: Signature Verification
*For any* license data and signature pair, the validator should accept licenses with valid RSA signatures and reject licenses with invalid or tampered signatures.
**Validates: Requirements 3.1, 3.4, 6.1**

### Property 7: Expiration Validation
*For any* license with an expiration date, the validator should mark licenses as invalid when the expiration date is before the current system date, and valid when the expiration date is in the future.
**Validates: Requirements 3.2, 3.3, 6.2**

### Property 8: Complete Validation
*For any* license data, when all validation checks (signature and expiration) pass, the validator should mark the license as valid; when any check fails, it should mark the license as invalid.
**Validates: Requirements 3.5**

### Property 9: Comprehensive Error Reporting
*For any* license with multiple validation failures (e.g., both expired and invalid signature), the validation result should include all detected issues in the error list.
**Validates: Requirements 3.6, 6.4, 6.5**

### Property 10: Feature Enablement Based on License
*For any* valid license with a specific feature flags bitmask, the SDK manager should enable exactly the features specified in the bitmask and disable all others.
**Validates: Requirements 4.1, 4.3**

### Property 11: Feature Access Control
*For any* SDK feature and license state, attempting to execute a feature should succeed only when that feature is enabled in the current license, and should fail with an error message when the feature is disabled.
**Validates: Requirements 4.4, 4.5, 8.3**

### Property 12: Disconnection Disables Features
*For any* application state with enabled features, when a dongle disconnection event occurs, all premium features should be immediately disabled.
**Validates: Requirements 5.2**

### Property 13: Reconnection Restores Features
*For any* valid license, when a dongle is disconnected and then reconnected, the application should re-validate the license and restore the previously enabled features.
**Validates: Requirements 5.4**

### Property 14: Licensed Feature Execution
*For any* licensed feature, when invoked with a valid license, the feature should execute successfully and return a result.
**Validates: Requirements 8.2**

### Property 15: Test Data Generation Configurability
*For any* specified expiration date and feature flags combination, the test data generator should create a license file with exactly those parameters.
**Validates: Requirements 9.2, 9.4**

### Property 16: Test Data Scenario Coverage
*For any* test data generation request, the generator should be capable of producing both valid licenses (with valid signatures and future expiration) and invalid licenses (with invalid signatures or past expiration).
**Validates: Requirements 9.3**

### Property 17: License Display Completeness
*For any* license information displayed to the console, the output should contain all key fields: license key, expiration date, issued to, and enabled features.
**Validates: Requirements 2.4, 7.2**

### Property 18: Feature List Accuracy
*For any* set of enabled features, when the application lists available features, the list should include all and only the currently enabled features.
**Validates: Requirements 7.3**

### Property 19: Error Message Distinctiveness
*For any* error condition, the error message displayed should be visually distinct from normal output (e.g., different color or prefix) and contain a description of the error.
**Validates: Requirements 7.5**

### Property 20: Feature Licensing Indication
*For any* feature displayed in the feature list, the output should clearly indicate whether the feature requires a license or is available in demo mode.
**Validates: Requirements 8.5**

## Error Handling

The application implements comprehensive error handling at each layer:

### USB Layer Errors
- **Device not found**: Display clear message, continue in demo mode
- **Read failure**: Log error, attempt retry with exponential backoff (max 3 attempts)
- **Connection loss during read**: Abort operation, transition to demo mode
- **Invalid device data**: Catch parsing exceptions, return corrupted data error

### Validation Layer Errors
- **Invalid signature**: Return validation failure with "Invalid signature" reason
- **Expired license**: Return validation failure with expiration date in message
- **Missing required fields**: Return validation failure with list of missing fields
- **Corrupted data structure**: Catch deserialization exceptions, return corrupted error

### SDK Layer Errors
- **Feature not licensed**: Display error message, prevent execution, suggest upgrade
- **Feature execution failure**: Catch exceptions, log error, return failure result
- **Invalid feature request**: Display available features, prompt for valid selection

### Application Layer Errors
- **Invalid command**: Display help text with available commands
- **Configuration file missing**: Use default configuration, log warning
- **Unhandled exceptions**: Display user-friendly error, log stack trace, exit gracefully

**Error Logging Strategy:**
- All errors logged to console with timestamp
- Critical errors (crashes, security failures) logged to file
- Validation failures logged with license key (for audit trail)
- Test mode errors clearly marked as test-related

## Testing Strategy

The application will use a dual testing approach combining unit tests and property-based tests for comprehensive coverage.

### Property-Based Testing

Property-based tests will validate universal correctness properties across many generated inputs. We will use **FsCheck** ([https://fscheck.github.io/FsCheck/](https://fscheck.github.io/FsCheck/)), a mature property-based testing library for .NET that integrates with xUnit and NUnit.

**Configuration:**
- Minimum 100 iterations per property test
- Each test tagged with format: **Feature: usb-dongle-licensing, Property N: [property text]**
- Custom generators for license data, feature flags, and USB device info
- Shrinking enabled to find minimal failing cases

**Property Test Coverage:**
- Properties 1-20 from Correctness Properties section
- Each property implemented as a single FsCheck test
- Generators create random but valid test data
- Edge cases (empty strings, null values, boundary dates) included in generators

### Unit Testing

Unit tests will verify specific examples, edge cases, and integration points.

**Focus Areas:**
- Specific example scenarios (no dongle at startup, help command, demo mode)
- Edge cases (license expires today, empty feature flags, maximum feature flags)
- Error conditions (corrupted JSON, missing signature, invalid base64)
- Integration between components (validator → SDK manager, USB provider → validator)
- Configuration loading and parsing

**Test Organization:**
- Separate test projects for each layer
- Mock implementations of interfaces for isolation
- Integration tests for end-to-end scenarios
- Test fixtures for common test data

**Balance:**
- Property tests handle comprehensive input coverage
- Unit tests focus on specific scenarios and integration
- Avoid redundant unit tests for cases covered by properties
- Use unit tests for UI output verification (property tests less suitable)

### Test Data

The test suite will include:
- Sample license files (valid, expired, invalid signature, partial features)
- RSA key pairs for testing (separate from production keys)
- Mock USB device configurations
- Configuration files for various scenarios

### Continuous Validation

- All tests run on every build
- Property tests catch regression across input space
- Unit tests validate specific requirements
- Code coverage target: 85% for core logic (excluding UI formatting)
