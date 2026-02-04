# USB Dongle Licensing Demo - Manual Testing Guide

This guide provides step-by-step instructions for manually testing the USB Dongle Licensing Demo application.

## Prerequisites

1. Ensure you have .NET 8.0 SDK installed
2. Navigate to the project directory: `c:\Users\svayalilprab\SDKUSBLocking`

## Initial Setup: Generate Real RSA-Signed Licenses

**IMPORTANT**: Before running any test scenarios, you need to generate properly signed licenses with real RSA keys.

### Step 1: Build the Application

```cmd
dotnet build UsbDongleLicensing.sln
```

### Step 2: Generate RSA Keys and Test Licenses

Run the license generator:

**Option A - Using dotnet (recommended):**
```powershell
dotnet run --project LicenseGenerator
```

**Option B - Using batch file:**
```cmd
.\generate-licenses.cmd
```

This will:
- Generate RSA key pair in `keys/` directory
- Create 5 test license files in `test-licenses/` directory
- Display the **Public Key (Base64)** in the console

### Step 3: Configure the Public Key

Copy the **Public Key (Base64)** from the console output (it's a long string starting with something like `PFJTQUtleVZhbHVl...`)

Paste it into `UsbDongleLicensing/appsettings.json`:

```json
{
  "Mode": "simulation",
  "SimulationPath": "./test-licenses",
  "UsbVendorId": "0x1234",
  "UsbProductId": "0x5678",
  "PublicKey": "PASTE_THE_ENTIRE_BASE64_STRING_HERE",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    },
    "Console": {
      "Enabled": true
    },
    "File": {
      "Enabled": true,
      "Path": "logs/app.log"
    }
  }
}
```

**Note**: The public key is very long (300+ characters). Make sure you copy the entire string.

### Generated Test License Files

The generator creates these files in `test-licenses/`:

1. **valid_all_features.json** - All features enabled, expires in 2 years
2. **valid_partial_features.json** - Basic + Analytics only
3. **valid_basic_only.json** - Basic feature only
4. **expired_license.json** - Expired license (for testing)
5. **invalid_signature.json** - Tampered license (for security testing)

---

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

**Purpose:** Test with a valid license that has all features enabled and proper RSA signature

### Steps:

1. **Copy the valid license file:**
   ```cmd
   copy test-licenses\valid_all_features.json test-licenses\license.json
   ```

2. **Verify the public key is configured:**
   - Open `UsbDongleLicensing/appsettings.json`
   - Ensure the `PublicKey` field contains the base64 string from the initial setup
   - If it's empty, go back to "Initial Setup" section above

3. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

4. **Expected behavior:**
   - Welcome banner displays
   - **"✓ License is VALID"** message (in green)
   - License details displayed:
     - License Key: DEMO-1234-5678-ABCD
     - Issued To: Test User
     - Expiration Date: (2 years from generation date)
     - Enabled Features: Basic, Analytics, Export, API
   - **No "Invalid signature" errors**

5. **Test commands:**
   - Type `status` - Should show valid license with all details
   - Type `features` - Should show all 4 features as enabled with ✓ marks:
     ```
     ✓ Basic Feature              [Always Available]
     ✓ Advanced Analytics         [Licensed]
     ✓ Data Export                [Licensed]
     ✓ API Access                 [Licensed]
     ```
   - Type `run basic` - Should execute successfully
   - Type `run analytics` - Should execute and show mock analysis data:
     ```
     Success: true
     Message: Advanced analytics completed successfully. Analysis of 10,000 records performed.
     Data: totalRecords, averageValue, maxValue, minValue, etc.
     ```
   - Type `run export` - Should execute and show mock export file details:
     ```
     Success: true
     Message: Data export completed successfully. File generated: export_YYYYMMDD_HHMMSS.csv
     Data: fileName, filePath, fileSize, recordCount, etc.
     ```
   - Type `run api` - Should execute and show mock API response:
     ```
     Success: true
     Message: API access successful. Data retrieved from remote endpoint.
     Data: endpoint, method, statusCode, responseTime, etc.
     ```
   - Type `exit` - Should close the application

### Troubleshooting Scenario 2:

**If you see "Invalid signature" error:**

1. **Check public key configuration:**
   ```cmd
   type UsbDongleLicensing\appsettings.json
   ```
   - Verify `PublicKey` field is not empty
   - Verify it contains a long base64 string (300+ characters)

2. **Regenerate keys and licenses:**
   ```powershell
   rmdir /s /q keys
   rmdir /s /q test-licenses
   dotnet run --project LicenseGenerator
   ```
   - Copy the NEW public key to `appsettings.json`
   - Copy the NEW license file: `copy test-licenses\valid_all_features.json test-licenses\license.json`

3. **Verify license file format:**
   ```cmd
   type test-licenses\license.json
   ```
   - Should be valid JSON
   - Should have all required fields: licenseKey, issuedTo, issuedDate, expirationDate, enabledFeatures, signature

**If you see "License expired" error:**

The license was generated more than 2 years ago. Regenerate:
```powershell
dotnet run --project LicenseGenerator
copy test-licenses\valid_all_features.json test-licenses\license.json
```

---

## Test Scenario 3: Partial License (Limited Features)

**Purpose:** Test with a license that only enables some features

### Steps:

1. **Copy the partial license file:**
   ```cmd
   copy test-licenses\valid_partial_features.json test-licenses\license.json
   ```

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **Expected behavior:**
   - "✓ License is VALID" message
   - License shows: Enabled Features: Basic, Analytics (only 2 features)

4. **Test commands:**
   - Type `features` - Should show:
     ```
     ✓ Basic Feature              [Always Available]
     ✓ Advanced Analytics         [Licensed]
     ✗ Data Export                [Requires License]
     ✗ API Access                 [Requires License]
     ```
   - Type `run basic` - Should work ✓
   - Type `run analytics` - Should work ✓
   - Type `run export` - Should show "not licensed" error ✗
   - Type `run api` - Should show "not licensed" error ✗

---

## Test Scenario 4: Expired License

**Purpose:** Test with an expired license

### Steps:

1. **Copy the expired license file:**
   ```cmd
   copy test-licenses\expired_license.json test-licenses\license.json
   ```

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **Expected behavior:**
   - "✗ License is INVALID" message (in red)
   - Status shows "Expired"
   - Error message shows: "License expired on [date]"
   - Only Basic Feature should be available (demo mode)

4. **Test commands:**
   - Type `features` - Should show only Basic Feature enabled
   - Type `run basic` - Should work (demo mode)
   - Type `run analytics` - Should show "not licensed" error

---

## Test Scenario 5: Invalid Signature (Tampered License)

**Purpose:** Test security - what happens when someone tampers with a license

### Steps:

1. **Copy the tampered license file:**
   ```cmd
   copy test-licenses\invalid_signature.json test-licenses\license.json
   ```
   
   **Note:** This license was signed, then the `issuedTo` field was changed from "Original User" to "Hacker". The signature no longer matches the data.

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **Expected behavior:**
   - "✗ License is INVALID" message (in red)
   - Status shows "InvalidSignature"
   - Error message: "Invalid license signature"
   - Only Basic Feature available (demo mode)
   - **This proves the RSA signature security works!**

4. **Understanding what happened:**
   - Original license was properly signed
   - Someone changed the license data after signing
   - The signature no longer matches the modified data
   - Validator detects tampering and rejects the license

---

## Test Scenario 6: Corrupted License File

**Purpose:** Test error handling with malformed JSON

### Steps:

1. **Create a corrupted license file:**
   ```cmd
   echo { invalid json content } > test-licenses\license.json
   ```

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **Expected behavior:**
   - Application should handle the error gracefully
   - Should show error message about corrupted/invalid license data
   - Should fall back to demo mode
   - Only Basic Feature available

---

## Test Scenario 7: Simulating Dongle Disconnection

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

## Test Scenario 7: Simulating Dongle Disconnection

**Purpose:** Test runtime disconnection behavior

### Steps:

1. **Start with a valid license:**
   ```cmd
   copy test-licenses\valid_all_features.json test-licenses\license.json
   ```

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **While the application is running:**
   - Type `features` - Note all 4 features are enabled
   - **In another terminal/window, delete the license file:**
     ```cmd
     del test-licenses\license.json
     ```
   - Wait 1-2 seconds for the file watcher to detect the change
   - Type `features` again - Should now show only Basic Feature enabled
   - Type `run analytics` - Should show "not licensed" error
   - Type `run basic` - Should still work (demo mode)

4. **Test reconnection:**
   - **In the other terminal, restore the license:**
     ```cmd
     copy test-licenses\valid_all_features.json test-licenses\license.json
     ```
   - Wait 1-2 seconds
   - Type `features` - Should show all 4 features enabled again
   - Type `run analytics` - Should work again

**Note:** This simulates unplugging and re-plugging a USB dongle in real-world usage.

---

## Test Scenario 8: Command Validation

**Purpose:** Test invalid command handling

### Steps:

1. **Run the application with any license state**

2. **Test invalid commands:**
   - Type `invalid` - Should show error and suggest help
   - Type `run` - Should show error (missing feature name)
   - Type `run unknown` - Should show error (unknown feature)
   - Type `help` - Should display command list

---

## Test Scenario 8: Command Validation

**Purpose:** Test invalid command handling

### Steps:

1. **Run the application with any license state**

2. **Test invalid commands:**
   - Type `invalid` - Should show error and suggest help
   - Type `run` - Should show error (missing feature name)
   - Type `run unknown` - Should show error (unknown feature)
   - Type `help` - Should display command list

---

## Quick Reference: Test License Files

After running the initial setup, you have these license files available:

| File | Purpose | Features | Expiration | Signature |
|------|---------|----------|------------|-----------|
| `valid_all_features.json` | Full license | All 4 features | 2 years | ✓ Valid |
| `valid_partial_features.json` | Limited license | Basic + Analytics | 2 years | ✓ Valid |
| `valid_basic_only.json` | Minimal license | Basic only | 2 years | ✓ Valid |
| `expired_license.json` | Test expiration | All features | Past | ✓ Valid but expired |
| `invalid_signature.json` | Test security | Basic | 2 years | ✗ Tampered |

**To switch between scenarios:**
```cmd
copy test-licenses\[filename].json test-licenses\license.json
```

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
