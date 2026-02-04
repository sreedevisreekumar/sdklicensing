# USB Dongle Licensing Demo - Manual Testing Guide

This guide provides step-by-step instructions for manually testing the USB Dongle Licensing Demo application.

## Prerequisites

1. Ensure you have .NET 8.0 SDK installed
2. Navigate to the project directory: `c:\Users\svayalilprab\SDKUSBLocking`

## Build the Application

```cmd
dotnet build UsbDongleLicensing.sln
```

## Test Scenario 1: Demo Mode (No License)

**Purpose:** Test the application when no USB dongle/license file is present

### Steps:

1. **Delete any existing license file:**
   ```cmd
   del test-licenses\license.json
   ```

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **Expected behavior:**
   - Welcome banner displays
   - "No license found - Running in DEMO MODE" message appears
   - Only "Basic Feature" should be available

4. **Test commands:**
   - Type `status` - Should show demo mode
   - Type `features` - Should show only Basic Feature as enabled
   - Type `run basic` - Should execute successfully
   - Type `run analytics` - Should show "not licensed" error
   - Type `help` - Should display all available commands
   - Type `exit` - Should close the application

---

## Test Scenario 2: Valid License (All Features)

**Purpose:** Test with a valid license that has all features enabled

### Steps:

1. **Create a valid license file:**
   ```cmd
   mkdir test-licenses
   ```

2. **Create `test-licenses\license.json` with this content:**
   ```json
   {
     "licenseKey": "DEMO-1234-5678-ABCD",
     "issuedTo": "Test User",
     "issuedDate": "2024-01-01T00:00:00Z",
     "expirationDate": "2025-12-31T23:59:59Z",
     "enabledFeatures": 15,
     "signature": "dGVzdHNpZ25hdHVyZWZvcmRlbW9wdXJwb3Nlcw=="
   }
   ```
   
   **Note:** `enabledFeatures: 15` = All features (1+2+4+8 = BasicFeature + AdvancedAnalytics + DataExport + ApiAccess)

3. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

4. **Expected behavior:**
   - Welcome banner displays
   - "License is VALID" message (in green)
   - License details displayed (key, issued to, dates, features)

5. **Test commands:**
   - Type `status` - Should show valid license with all details
   - Type `features` - Should show all 4 features as enabled with ✓ marks
   - Type `run basic` - Should execute successfully
   - Type `run analytics` - Should execute and show mock analysis data
   - Type `run export` - Should execute and show mock export file details
   - Type `run api` - Should execute and show mock API response
   - Type `exit` - Should close the application

---

## Test Scenario 3: Partial License (Limited Features)

**Purpose:** Test with a license that only enables some features

### Steps:

1. **Update `test-licenses\license.json`:**
   ```json
   {
     "licenseKey": "PARTIAL-1234-5678-ABCD",
     "issuedTo": "Limited User",
     "issuedDate": "2024-01-01T00:00:00Z",
     "expirationDate": "2025-12-31T23:59:59Z",
     "enabledFeatures": 3,
     "signature": "dGVzdHNpZ25hdHVyZWZvcmRlbW9wdXJwb3Nlcw=="
   }
   ```
   
   **Note:** `enabledFeatures: 3` = BasicFeature (1) + AdvancedAnalytics (2)

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **Test commands:**
   - Type `features` - Should show Basic and Analytics enabled, Export and API disabled
   - Type `run basic` - Should work
   - Type `run analytics` - Should work
   - Type `run export` - Should show "not licensed" error
   - Type `run api` - Should show "not licensed" error

---

## Test Scenario 4: Expired License

**Purpose:** Test with an expired license

### Steps:

1. **Update `test-licenses\license.json`:**
   ```json
   {
     "licenseKey": "EXPIRED-1234-5678-ABCD",
     "issuedTo": "Expired User",
     "issuedDate": "2023-01-01T00:00:00Z",
     "expirationDate": "2024-01-01T00:00:00Z",
     "enabledFeatures": 15,
     "signature": "dGVzdHNpZ25hdHVyZWZvcmRlbW9wdXJwb3Nlcw=="
   }
   ```

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **Expected behavior:**
   - "License is INVALID" message (in red)
   - Status shows "Expired"
   - Error message shows expiration date
   - Only Basic Feature should be available

---

## Test Scenario 5: Corrupted License File

**Purpose:** Test error handling with malformed JSON

### Steps:

1. **Update `test-licenses\license.json` with invalid JSON:**
   ```
   { invalid json content }
   ```

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **Expected behavior:**
   - Application should handle the error gracefully
   - Should show error message about corrupted license
   - Should fall back to demo mode

---

## Test Scenario 6: Simulating Dongle Disconnection

**Purpose:** Test runtime disconnection behavior

### Steps:

1. **Start with a valid license file**

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **While the application is running:**
   - Type `features` - Note all enabled features
   - **In another terminal/window, delete the license file:**
     ```cmd
     del test-licenses\license.json
     ```
   - Wait a moment for the file watcher to detect the change
   - Type `features` again - Should now show only Basic Feature
   - Type `run analytics` - Should show "not licensed" error

---

## Test Scenario 7: Command Validation

**Purpose:** Test invalid command handling

### Steps:

1. **Run the application with any license state**

2. **Test invalid commands:**
   - Type `invalid` - Should show error and suggest help
   - Type `run` - Should show error (missing feature name)
   - Type `run unknown` - Should show error (unknown feature)
   - Type `help` - Should display command list

---

## Feature Flags Reference

The `enabledFeatures` field in the license JSON uses a bitmask:

| Feature | Value | Binary |
|---------|-------|--------|
| None | 0 | 0000 |
| BasicFeature | 1 | 0001 |
| AdvancedAnalytics | 2 | 0010 |
| DataExport | 4 | 0100 |
| ApiAccess | 8 | 1000 |

**Examples:**
- All features: `15` (1+2+4+8)
- Basic + Analytics: `3` (1+2)
- Basic + Export: `5` (1+4)
- Analytics + API: `10` (2+8)

---

## Verification Checklist

After testing, verify:

- ✅ Welcome banner displays correctly
- ✅ License status shows appropriate colors (green/red/yellow)
- ✅ All license fields display correctly (key, issued to, dates, features)
- ✅ Feature list shows correct enabled/disabled states
- ✅ Feature execution works for enabled features
- ✅ Feature execution blocks disabled features with clear error
- ✅ Help command shows all commands and descriptions
- ✅ Error messages display in red
- ✅ Demo mode works when no license present
- ✅ Expired licenses are rejected
- ✅ Corrupted files are handled gracefully
- ✅ File watcher detects license file changes
- ✅ Exit command closes application cleanly

---

## Troubleshooting

### Application doesn't run:
- Check that .NET 8.0 SDK is installed: `dotnet --version`
- Rebuild the solution: `dotnet clean && dotnet build`
- Check for compilation errors in the build output

### License file isn't detected:
- Verify the file is in `test-licenses\license.json` relative to the executable
- Check the `appsettings.json` file has correct `SimulationPath` setting
- Ensure JSON is valid (use a JSON validator)

### Colors don't display correctly:
- Ensure you're using a terminal that supports ANSI color codes (Windows Terminal, PowerShell, or modern CMD)
- Try running in Windows Terminal for best color support

### File watcher doesn't detect changes:
- Some file systems may have delays in notification
- Try waiting 1-2 seconds after file changes
- Restart the application if changes aren't detected

---

## Running Automated Tests

To run the full test suite:

```cmd
dotnet test UsbDongleLicensing.Tests
```

Expected output:
- Total tests: 91+
- All tests should pass
- No failures or errors

---

## Additional Testing Notes

### Testing with Physical USB Dongles

To test with actual USB hardware (not covered in this guide):

1. Update `appsettings.json`:
   ```json
   {
     "Mode": "hid",
     "UsbVendorId": "0x1234",
     "UsbProductId": "0x5678"
   }
   ```

2. Connect a USB HID device with matching vendor/product IDs
3. The device must store license data in HID feature reports

### Security Testing

Note: The current implementation uses test signatures that are NOT cryptographically secure. For production use:

1. Generate proper RSA key pairs using the TestDataGenerator
2. Sign licenses with the private key
3. Embed only the public key in the application
4. Never distribute the private key

---

## Support

For issues or questions:
- Review the design document: `.kiro/specs/usb-dongle-licensing/design.md`
- Check the requirements: `.kiro/specs/usb-dongle-licensing/requirements.md`
- Review test implementations in `UsbDongleLicensing.Tests/`
