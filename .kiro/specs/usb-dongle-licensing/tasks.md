# Implementation Plan: USB Dongle Licensing Demo

## Overview

This implementation plan breaks down the USB dongle licensing demonstration application into discrete coding tasks. The approach follows a bottom-up strategy: building core components first (USB interface, validation, SDK management), then integrating them into the console application, and finally adding test data generation utilities.

## Tasks

- [x] 1. Set up project structure and dependencies
  - Create C# console application project targeting .NET 6 or later
  - Add NuGet packages: HIDSharp (for USB HID communication), Newtonsoft.Json (for JSON serialization), FsCheck (for property-based testing), xUnit (for test framework)
  - Create folder structure: `/Core` (interfaces and models), `/Providers` (USB implementations), `/Validation` (license validation), `/SDK` (feature management), `/UI` (console interface), `/Tests` (test projects)
  - Create configuration file template (appsettings.json) with default values
  - _Requirements: All requirements depend on this foundation_

- [ ] 2. Implement core data models and interfaces
  - [x] 2.1 Create data model classes
    - Implement `DongleInfo` class with VendorId, ProductId, SerialNumber, DetectedAt properties
    - Implement `LicenseData` class with LicenseKey, ExpirationDate, EnabledFeatures, Signature, IssuedTo, IssuedDate properties
    - Implement `ValidationResult` class with IsValid, Errors list, Status properties
    - Implement `FeatureResult` class with Success, Message, Data dictionary properties
    - Create enums: `LicenseStatus`, `FeatureFlags` (with Flags attribute), `SdkFeature`
    - _Requirements: 2.2, 3.1, 4.1_
  
  - [ ]* 2.2 Write property test for license data serialization
    - **Property 5: Encryption Round-Trip**
    - **Validates: Requirements 2.5**
    - Test that serializing and deserializing license data preserves all fields
    - Use FsCheck generators for random license data

- [ ] 3. Implement USB dongle interface and providers
  - [x] 3.1 Create IUsbDongleProvider interface
    - Define interface with IsConnected(), DetectDongle(), ReadLicenseData() methods
    - Define DongleConnected and DongleDisconnected events with DongleEventArgs
    - _Requirements: 1.1, 1.2, 1.3, 2.1_
  
  - [x] 3.2 Implement SimulatedDongleProvider
    - Implement IUsbDongleProvider using file system for simulation
    - Read license JSON files from configured directory
    - Use FileSystemWatcher to detect file creation/deletion as connection events
    - Implement IsConnected() by checking for license file existence
    - Implement ReadLicenseData() by reading and deserializing JSON file
    - _Requirements: 1.1, 2.1, 9.1, 9.5_
  
  - [ ]* 3.3 Write property tests for USB provider
    - **Property 1: USB Dongle Identification**
    - **Validates: Requirements 1.1, 1.5**
    - Test that detection correctly filters devices by vendor/product ID
    - **Property 2: Device Event Handling**
    - **Validates: Requirements 1.2, 1.3**
    - Test that connection/disconnection events are properly raised
  
  - [x] 3.4 Implement HidDongleProvider
    - Implement IUsbDongleProvider using HIDSharp library
    - Filter HID devices by configured VendorId and ProductId
    - Implement device enumeration in DetectDongle()
    - Read license data from HID feature reports
    - Set up device arrival/removal monitoring using HIDSharp events
    - Handle device read errors with retry logic (max 3 attempts with exponential backoff)
    - _Requirements: 1.1, 1.2, 1.3, 1.5, 2.1, 2.3_
  
  - [ ]* 3.5 Write unit tests for HID provider error handling
    - Test read failure scenarios
    - Test device disconnection during read
    - Test retry logic with exponential backoff
    - _Requirements: 2.3, 6.3_

- [x] 4. Checkpoint - Ensure USB provider tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 5. Implement license validation
  - [x] 5.1 Create ILicenseValidator interface
    - Define interface with Validate(LicenseData), VerifySignature(byte[], byte[]), CheckExpiration(DateTime) methods
    - _Requirements: 3.1, 3.2_
  
  - [x] 5.2 Implement RsaLicenseValidator
    - Implement RSA-SHA256 signature verification using System.Security.Cryptography
    - Load public key from configuration (base64-encoded XML format)
    - Implement VerifySignature() using RSACryptoServiceProvider
    - Implement CheckExpiration() by comparing with DateTime.UtcNow
    - Implement Validate() to perform all checks and return ValidationResult
    - Collect all validation errors (signature, expiration, missing fields) in result
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 6.1, 6.2, 6.4, 6.5_
  
  - [ ]* 5.3 Write property tests for license validation
    - **Property 6: Signature Verification**
    - **Validates: Requirements 3.1, 3.4, 6.1**
    - Test that valid signatures pass and invalid signatures fail
    - **Property 7: Expiration Validation**
    - **Validates: Requirements 3.2, 3.3, 6.2**
    - Test that expired licenses are rejected and future licenses are accepted
    - **Property 8: Complete Validation**
    - **Validates: Requirements 3.5**
    - Test that licenses passing all checks are marked valid
    - **Property 9: Comprehensive Error Reporting**
    - **Validates: Requirements 3.6, 6.4, 6.5**
    - Test that multiple validation failures are all reported
  
  - [ ]* 5.4 Write unit tests for validation edge cases
    - Test license expiring today (boundary case)
    - Test missing required fields
    - Test corrupted signature data
    - _Requirements: 3.2, 6.3_

- [ ] 6. Implement SDK feature management
  - [x] 6.1 Create ISdkManager interface
    - Define interface with Initialize(ValidationResult), IsFeatureEnabled(SdkFeature), ExecuteFeature(SdkFeature), GetAvailableFeatures() methods
    - _Requirements: 4.1, 4.4_
  
  - [x] 6.2 Implement SdkManager
    - Maintain current license state and enabled features
    - Implement Initialize() to set enabled features based on ValidationResult
    - Implement IsFeatureEnabled() to check feature flags bitmask
    - Implement GetAvailableFeatures() to return list of enabled features
    - Implement ExecuteFeature() with feature-specific logic:
      - BasicFeature: Always available, displays simple message
      - AdvancedAnalytics: Requires license, performs mock data analysis
      - DataExport: Requires license, generates mock export file
      - ApiAccess: Requires license, simulates API call
    - Return FeatureResult with success status and message
    - Handle unlicensed feature access by returning error result
    - Subscribe to dongle disconnection events to disable features
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 5.2, 8.2, 8.3_
  
  - [ ]* 6.3 Write property tests for SDK manager
    - **Property 10: Feature Enablement Based on License**
    - **Validates: Requirements 4.1, 4.3**
    - Test that feature flags correctly enable/disable features
    - **Property 11: Feature Access Control**
    - **Validates: Requirements 4.4, 4.5, 8.3**
    - Test that only enabled features can be executed
    - **Property 12: Disconnection Disables Features**
    - **Validates: Requirements 5.2**
    - Test that disconnection events disable premium features
    - **Property 13: Reconnection Restores Features**
    - **Validates: Requirements 5.4**
    - Test that reconnection with valid license restores features
  
  - [ ]* 6.4 Write unit tests for feature execution
    - Test each SDK feature execution with valid license
    - Test feature execution with invalid license
    - Test demo mode feature execution
    - _Requirements: 8.2, 8.3, 8.4_

- [x] 7. Checkpoint - Ensure core component tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 8. Implement console user interface
  - [x] 8.1 Create ConsoleUI class
    - Implement DisplayWelcomeBanner() to show application title and version
    - Implement DisplayLicenseStatus() to format and display license information
    - Implement DisplayFeatureList() to show available features with licensing indicators
    - Implement DisplayHelp() to show available commands
    - Implement DisplayError() with color coding (red text for errors)
    - Implement FormatLicenseInfo() to create readable license output
    - Use Console.ForegroundColor for color-coded output
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 8.5_
  
  - [ ]* 8.2 Write property tests for UI output
    - **Property 17: License Display Completeness**
    - **Validates: Requirements 2.4, 7.2**
    - Test that license output contains all required fields
    - **Property 18: Feature List Accuracy**
    - **Validates: Requirements 7.3**
    - Test that feature list includes all and only enabled features
    - **Property 19: Error Message Distinctiveness**
    - **Validates: Requirements 7.5**
    - Test that error messages are visually distinct
    - **Property 20: Feature Licensing Indication**
    - **Validates: Requirements 8.5**
    - Test that features are labeled with licensing requirements
  
  - [ ]* 8.3 Write unit tests for UI formatting
    - Test welcome banner display
    - Test help command output
    - Test error message formatting
    - _Requirements: 7.1, 7.4, 7.5_

- [ ] 9. Implement main application orchestration
  - [x] 9.1 Create Program class with Main entry point
    - Parse command-line arguments for mode selection (--simulation or --hid)
    - Load configuration from appsettings.json
    - Initialize appropriate USB provider based on mode
    - Initialize validator with public key from configuration
    - Initialize SDK manager
    - Create ConsoleUI instance
    - Implement main application loop for command processing
    - _Requirements: 1.1, 5.1, 9.5_
  
  - [~] 9.2 Implement command processing
    - Parse user input into commands: status, features, run <feature>, help, exit
    - Implement status command to display current licensing state
    - Implement features command to list available features
    - Implement run command to execute specified feature
    - Implement help command to display command list
    - Implement exit command to gracefully shut down
    - Handle invalid commands with error message and help prompt
    - _Requirements: 7.1, 7.3, 7.4, 8.2, 8.3_
  
  - [~] 9.3 Implement startup sequence
    - Display welcome banner
    - Detect USB dongle (or check for simulation file)
    - If no dongle found, display warning and enter demo mode
    - If dongle found, read license data
    - Validate license
    - Initialize SDK manager with validation result
    - Display licensing status
    - Enter command loop
    - _Requirements: 1.1, 1.4, 2.1, 3.1, 4.1, 5.1, 7.1_
  
  - [ ]* 9.4 Write integration tests for application flow
    - Test startup with valid license
    - Test startup without dongle (demo mode)
    - Test command processing
    - Test feature execution flow
    - _Requirements: 1.4, 5.1, 5.3, 8.4_

- [ ] 10. Implement test data generator utility
  - [~] 10.1 Create TestDataGenerator class
    - Implement GenerateKeyPair() to create RSA key pair for testing
    - Implement SaveKeyPair() to save keys to files (XML format)
    - Implement CreateLicense() with parameters for all license fields
    - Implement SignLicense() to generate RSA signature for license data
    - Implement SaveLicense() to write license as JSON file
    - _Requirements: 9.2, 9.3, 9.4_
  
  - [~] 10.2 Create sample test data scenarios
    - Generate test RSA key pair and save to files
    - Create valid license with all features enabled (expires in 1 year)
    - Create valid license with partial features (only BasicFeature and AdvancedAnalytics)
    - Create expired license (expired 1 month ago)
    - Create license with invalid signature (tampered data)
    - Create corrupted license (malformed JSON)
    - Save all sample licenses to test-licenses directory
    - _Requirements: 9.3, 9.4, 9.6_
  
  - [ ]* 10.3 Write property tests for test data generator
    - **Property 15: Test Data Generation Configurability**
    - **Validates: Requirements 9.2, 9.4**
    - Test that generated licenses have specified parameters
    - **Property 16: Test Data Scenario Coverage**
    - **Validates: Requirements 9.3**
    - Test that both valid and invalid scenarios can be generated
  
  - [ ]* 10.4 Write unit tests for test data generator
    - Test key pair generation
    - Test license signing
    - Test license file creation
    - _Requirements: 9.2, 9.3_

- [ ] 11. Create configuration and documentation
  - [~] 11.1 Create appsettings.json configuration file
    - Add mode setting (simulation or hid)
    - Add simulation path for test license files
    - Add USB vendor ID and product ID for HID mode
    - Add public key (base64-encoded XML) for signature verification
    - Add logging settings
    - _Requirements: 1.5, 3.1, 9.1_
  
  - [~] 11.2 Create README.md with usage instructions
    - Document how to run in simulation mode
    - Document how to run with physical USB dongle
    - Document command-line arguments
    - Document available commands
    - Document test data generation process
    - Include sample configuration
    - _Requirements: All requirements_

- [~] 12. Final checkpoint - Integration testing and validation
  - Run all property-based tests (minimum 100 iterations each)
  - Run all unit tests
  - Test complete application flow in simulation mode
  - Verify all features work correctly with valid license
  - Verify demo mode works without license
  - Verify error handling for invalid licenses
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Property tests use FsCheck with minimum 100 iterations
- Each property test is tagged with: **Feature: usb-dongle-licensing, Property N: [property text]**
- Simulation mode allows testing without physical USB dongles
- HID mode requires physical dongle hardware for testing
- Test data generator creates sample licenses for various scenarios
