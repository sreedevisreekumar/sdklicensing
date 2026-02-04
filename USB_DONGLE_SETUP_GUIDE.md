# USB Dongle Setup and Testing Guide

This guide explains how to set up and test the USB Dongle Licensing system with a physical USB device.

## Overview

The application can read license data from a physical USB HID device. The license data must be stored on the USB device and readable via HID feature reports.

---

## Prerequisites

1. **USB HID Device**: A USB device that supports HID (Human Interface Device) protocol
2. **Device Configuration**: The USB device must be configured to store and return license data
3. **Device IDs**: Know your USB device's Vendor ID and Product ID

---

## Step 1: Identify Your USB Device

### Find Vendor ID and Product ID

**On Windows:**

1. **Using Device Manager:**
   - Press `Win + X` and select "Device Manager"
   - Expand "Human Interface Devices" or "Universal Serial Bus devices"
   - Right-click your USB device → Properties
   - Go to "Details" tab
   - Select "Hardware Ids" from the dropdown
   - Look for: `VID_XXXX` (Vendor ID) and `PID_XXXX` (Product ID)
   - Example: `USB\VID_1234&PID_5678` means VendorId=0x1234, ProductId=0x5678

2. **Using PowerShell:**
   ```powershell
   Get-PnpDevice -PresentOnly | Where-Object { $_.InstanceId -match '^USB' } | Format-Table -AutoSize
   ```

---

## Step 2: Configure the USB Device

### Option A: Using a Programmable USB Device

If you have a programmable USB HID device (like Arduino, Teensy, etc.):

1. **Program the device** to respond to HID feature report requests
2. **Store license JSON** in the device's memory
3. **Return license data** when the application requests it via HID feature report

**Example License Data Format:**
```json
{
  "licenseKey": "DEMO-1234-5678-ABCD",
  "issuedTo": "Test User",
  "issuedDate": "2026-02-04T18:58:47.9573186Z",
  "expirationDate": "2028-02-04T18:58:47.9574492Z",
  "enabledFeatures": 15,
  "signature": "oNaHVTPmkMAmwV6gosLr9CaWU40pAtHSolgdcIUxfJLnq7TuOUY1UJcQp32fIP5W4pHE2eOyfI8+fmCu6e0c0itPsFGipVeaTHUiqV+FSLYFUedlJcSRQMPC+pWXEl/wIh5uDSdnstygGVofUf0mlbFGRhCfE2fDaZ5eRNhoo2w2YwfB9VHsDcjMtr7VbQ5iqcCFKHGvN8+HIXG/DIcH165/vVJMawefmGEWZkX00Hug3N8qCi7WTVGPZCgXEqLWqYXxFQzDCO4Lvq4Hw+6d6zT5rIEpkWjq4lfgHHDnxi+zc1/vJ8QBcXzZTetMwZGDVv7M9HPV/wCFao97ePwg/g=="
}
```

### Option B: Using a USB Flash Drive (Simulation)

If you don't have a programmable HID device, you can test with a regular USB flash drive:

1. **Use simulation mode** (the application will read from the file system)
2. **Copy a license file** to the USB drive
3. **Configure the path** in appsettings.json

---

## Step 3: Configure appsettings.json

Update the configuration file with your USB device information:

```json
{
  "Mode": "hid",
  "SimulationPath": "./test-licenses",
  "UsbVendorId": "0x1234",
  "UsbProductId": "0x5678",
  "PublicKey": "PFJTQUtleVZhbHVlPjxNb2R1bHVzPnVtMVZkWTdic1Y1b3dObWtPY2VoUEVDZlA2a0xNYk1icXR0OHhzWWlQY280L3N5SzhQU2c3U0ZISlRrUEl3VDNJSmM1V3ZmMUJSemdLdHN3K29hUzVnREx0a2VXalBhcFQySU03cVZQNm5BNVp6MWtZTDNURDluWWFBeE5EaW9NUkVqbURTZ05ZYVFCUzJ1L0FUbHpONlZqZWFMUXdxcmVoM2tiLzEwNE9mSUdZRG82OU5rSUZPNDFyV2RPVUxrQlYxQWlZcWxvOFdKUEVJa3hjQVkzc3NzeXBxNGdleXJpNm11UlloVnJxR3VTa1B5bktDVG9mdmd2M2pzNDdnQmg1KzAybXZRa2RXYm4vQkw1cWhodDdaWnUvVWYxOW8xT2QvSENvOUN6ZXB1WElSbHhxeFNialAwV0RNbThwR1IycDJQWHBzK1FQRVNYRGZEUmJRdVZJUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48L1JTQUtleVZhbHVlPg==",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**Configuration Fields:**

- **Mode**: Set to `"hid"` for physical USB device
- **UsbVendorId**: Your device's Vendor ID in hex format (e.g., `"0x1234"`)
- **UsbProductId**: Your device's Product ID in hex format (e.g., `"0x5678"`)
- **PublicKey**: The base64-encoded public key (already configured from license generation)

---

## Step 4: Prepare License Data on USB Device

### Generate a Valid License

Use one of the pre-generated test licenses:

```cmd
type test-licenses\valid_all_features.json
```

**Copy this JSON content** to your USB device's HID feature report storage.

### Available Test Licenses

1. **valid_all_features.json** - All features enabled
2. **valid_partial_features.json** - Only Basic + Analytics
3. **valid_basic_only.json** - Only Basic feature
4. **expired_license.json** - Expired license (for testing)
5. **invalid_signature.json** - Tampered license (for testing)

---

## Step 5: Run the Application in HID Mode

### Method 1: Using Command Line Argument

```cmd
dotnet run --project UsbDongleLicensing --hid
```

### Method 2: Using appsettings.json

1. Set `"Mode": "hid"` in appsettings.json
2. Run:
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

---

## Step 6: Test the Application

### Expected Startup Behavior

When you run the application with a USB dongle connected:

```
╔════════════════════════════════════════════════════════════╗
║  USB Dongle Licensing Demo                               ║
║  Version: 1.0.0                                            ║
╚════════════════════════════════════════════════════════════╝

Mode: HID
Using physical USB dongle (HID mode)

Initializing licensing system...

✓ USB dongle detected: 0x1234:0x5678

License Status:
═══════════════════════════════════════════════════════════
✓ License is VALID
Status: Valid
License Information:
License Key:     DEMO-1234-5678-ABCD
Issued To:       Test User
Issued Date:     2026-02-04
Expiration Date: 2028-02-04
Enabled Features: Basic, Analytics, Export, API
═══════════════════════════════════════════════════════════

Type 'help' for available commands or 'exit' to quit.

>
```

### Test Commands

Try these commands:

1. **Check Status:**
   ```
   > status
   ```

2. **List Features:**
   ```
   > features
   ```

3. **Run Features:**
   ```
   > run basic
   > run analytics
   > run export
   > run api
   ```

4. **Get Help:**
   ```
   > help
   ```

5. **Exit:**
   ```
   > exit
   ```

---

## Step 7: Test Dongle Connection/Disconnection

### Test Hot-Plug Detection

1. **Start the application** with the dongle connected
2. **Unplug the dongle** while the application is running
3. **Observe the behavior:**
   ```
   ⚠ USB dongle disconnected
   Premium features have been disabled. Running in demo mode.
   ```

4. **Plug the dongle back in**
5. **Observe reconnection:**
   ```
   ✓ USB dongle connected: 0x1234:0x5678
   Re-validating license...
   ```

---

## Troubleshooting

### Issue: "No USB dongle detected"

**Possible Causes:**
1. USB device not connected
2. Wrong Vendor ID or Product ID in appsettings.json
3. Device is not a HID device
4. Insufficient permissions to access USB device

**Solutions:**
- Verify device is connected: Check Device Manager
- Double-check VID/PID in appsettings.json
- Try running as Administrator
- Verify device appears in HID devices list

### Issue: "Error reading license data"

**Possible Causes:**
1. Device doesn't support HID feature reports
2. License data not properly stored on device
3. Device returns invalid data format

**Solutions:**
- Verify device supports HID feature reports
- Check device firmware/programming
- Test with simulation mode first to verify license format

### Issue: "Invalid license signature"

**Possible Causes:**
1. License data was modified after signing
2. Wrong public key in appsettings.json
3. License was not signed with the correct private key

**Solutions:**
- Regenerate licenses using: `dotnet run --project LicenseGenerator`
- Verify public key matches the one shown during license generation
- Don't manually edit license JSON files

### Issue: "Device read timeout"

**Possible Causes:**
1. Device is slow to respond
2. Device firmware issue
3. USB communication problem

**Solutions:**
- Check device firmware
- Try a different USB port
- Restart the device
- Check USB cable quality

---

## Alternative: Testing Without Physical USB Dongle

If you don't have a programmable USB HID device, use **simulation mode**:

### Quick Simulation Mode Test

1. **Keep Mode as "simulation"** in appsettings.json:
   ```json
   {
     "Mode": "simulation",
     "SimulationPath": "./test-licenses"
   }
   ```

2. **Run the application:**
   ```cmd
   dotnet run --project UsbDongleLicensing
   ```

3. **Test different scenarios** by copying different license files:
   ```cmd
   copy test-licenses\valid_all_features.json test-licenses\license.json
   ```

---

## Hardware Recommendations

### Suitable USB Devices for Testing

1. **Arduino Leonardo / Micro** - Native USB HID support
2. **Teensy 3.x / 4.x** - Excellent HID support
3. **STM32 with USB** - Programmable HID device
4. **Custom USB HID Device** - Purpose-built licensing dongle

### Programming Your USB Device

**Example Arduino Code Structure:**
```cpp
#include <HID.h>

// Store license JSON in EEPROM or flash
const char licenseData[] = "{\"licenseKey\":\"...\"}";

void setup() {
  // Initialize HID
}

void loop() {
  // Respond to HID feature report requests
  // Return license JSON data
}
```

---

## Security Considerations

### For Production Use

1. **Encrypt License Data**: Store encrypted license data on the USB device
2. **Secure Storage**: Use secure elements or encrypted flash
3. **Tamper Detection**: Implement tamper detection in device firmware
4. **Unique Device IDs**: Use unique serial numbers for each dongle
5. **Challenge-Response**: Implement challenge-response authentication

### Best Practices

- Never store private keys on the USB device
- Use hardware-based security when possible
- Implement rate limiting for license checks
- Log all license validation attempts
- Use secure communication protocols

---

## Next Steps

1. **Test in Simulation Mode First**: Verify the application works correctly
2. **Prepare USB Device**: Program your USB HID device with license data
3. **Configure Device IDs**: Update appsettings.json with correct VID/PID
4. **Test HID Mode**: Run application with physical USB device
5. **Test Scenarios**: Try different license types and connection states

---

## Support

For issues or questions:
- Check the MANUAL_TESTING_GUIDE.md for simulation mode testing
- Review the README.md for general application information
- Verify your USB device supports HID feature reports
- Test with simulation mode to isolate USB-specific issues
