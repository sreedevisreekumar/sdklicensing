# USB Stick Licensing Guide (Simple & Reliable)

This guide shows you how to use a regular USB flash drive (like Lexar) for licensing - no HID programming required!

---

## Overview

Your application already supports reading license files from any directory, including USB drives. This is the **simplest and most reliable** method for USB-based licensing.

**How it works:**
1. Put a signed license file on your USB stick
2. Configure the app to check that USB drive location
3. App runs only when USB stick is plugged in with valid license

---

## Step 1: Prepare Your USB Stick

### Insert Your USB Drive

1. Plug in your Lexar (or any) USB stick
2. Note the drive letter (e.g., `E:\`, `F:\`, `G:\`)

### Create License Directory (Optional)

You can put the license file directly on the root, or create a folder:

**Option A - Root of drive:**
```
E:\license.json
```

**Option B - In a folder:**
```
E:\licenses\license.json
```

---

## Step 2: Copy License File to USB Stick

### Copy a Valid License

Use one of your pre-generated test licenses:

```cmd
copy test-licenses\valid_all_features.json E:\license.json
```

**Or copy to a specific folder:**
```cmd
mkdir E:\licenses
copy test-licenses\valid_all_features.json E:\licenses\license.json
```

### Verify the File

Check that the license file is on the USB stick:

```cmd
type E:\license.json
```

You should see the license JSON content with the signature.

---

## Step 3: Configure appsettings.json

Update your configuration to point to the USB drive:

### For Root of Drive:

```json
{
  "Mode": "simulation",
  "SimulationPath": "E:",
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

### For Folder on Drive:

```json
{
  "Mode": "simulation",
  "SimulationPath": "E:\\licenses",
  "PublicKey": "PFJTQUtleVZhbHVlPjxNb2R1bHVzPnVtMVZkWTdic1Y1b3dObWtPY2VoUEVDZlA2a0xNYk1icXR0OHhzWWlQY280L3N5SzhQU2c3U0ZISlRrUEl3VDNJSmM1V3ZmMUJSemdLdHN3K29hUzVnREx0a2VXalBhcFQySU03cVZQNm5BNVp6MWtZTDNURDluWWFBeE5EaW9NUkVqbURTZ05ZYVFCUzJ1L0FUbHpONlZqZWFMUXdxcmVoM2tiLzEwNE9mSUdZRG82OU5rSUZPNDFyV2RPVUxrQlYxQWlZcWxvOFdKUEVJa3hjQVkzc3NzeXBxNGdleXJpNm11UlloVnJxR3VTa1B5bktDVG9mdmd2M2pzNDdnQmg1KzAybXZRa2RXYm4vQkw1cWhodDdaWnUvVWYxOW8xT2QvSENvOUN6ZXB1WElSbHhxeFNialAwV0RNbThwR1IycDJQWHBzK1FQRVNYRGZEUmJRdVZJUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48L1JTQUtleVZhbHVlPg=="
}
```

**Important Notes:**
- Use double backslashes `\\` in JSON for Windows paths
- Or use forward slashes: `"E:/licenses"`
- The app looks for a file named `license.json` in this directory

---

## Step 4: Run the Application

### Start the Application

```cmd
dotnet run --project UsbDongleLicensing
```

### Expected Output (USB Stick Connected)

```
╔════════════════════════════════════════════════════════════╗
║  USB Dongle Licensing Demo                               ║
║  Version: 1.0.0                                            ║
╚════════════════════════════════════════════════════════════╝

Mode: SIMULATION
⚠ Running in SIMULATION mode - using file-based license

Initializing licensing system...

✓ USB dongle detected: SIM:0001

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

### Expected Output (USB Stick NOT Connected)

```
╔════════════════════════════════════════════════════════════╗
║  USB Dongle Licensing Demo                               ║
║  Version: 1.0.0                                            ║
╚════════════════════════════════════════════════════════════╝

Mode: SIMULATION
⚠ Running in SIMULATION mode - using file-based license

Initializing licensing system...

⚠ No USB dongle detected

License Status:
═══════════════════════════════════════════════════════════
✗ License is INVALID
Status: NotFound
Validation Errors:
  ✗ No USB dongle detected
═══════════════════════════════════════════════════════════

Type 'help' for available commands or 'exit' to quit.

> run analytics
✗ Feature not available: This feature requires a valid license
```

---

## Step 5: Test Hot-Plug Detection

The application automatically detects when you plug/unplug the USB stick!

### Test Unplugging

1. **Start the app** with USB stick connected
2. **Unplug the USB stick** while app is running
3. **Observe:**
   ```
   ⚠ USB dongle disconnected
   Premium features have been disabled. Running in demo mode.
   ```

### Test Plugging Back In

1. **Plug the USB stick back in**
2. **Observe:**
   ```
   ✓ USB dongle connected: SIM:0001
   Re-validating license...
   
   License Status:
   ═══════════════════════════════════════════════════════════
   ✓ License is VALID
   ```

---

## How It Works

### The SimulatedDongleProvider

Your application uses `SimulatedDongleProvider` which:

1. **Monitors the directory** specified in `SimulationPath`
2. **Watches for `license.json`** file
3. **Detects connection** when file appears
4. **Detects disconnection** when file disappears (USB unplugged)
5. **Reads and validates** the license using RSA signature

### File System Watcher

The provider uses `FileSystemWatcher` to detect:
- File creation (USB plugged in)
- File deletion (USB unplugged)
- File changes (license updated)

This happens **automatically in real-time** - no polling needed!

---

## Testing Different Scenarios

### Scenario 1: Valid License with All Features

```cmd
copy test-licenses\valid_all_features.json E:\license.json
dotnet run --project UsbDongleLicensing
```

**Result:** All features enabled (Basic, Analytics, Export, API)

### Scenario 2: Partial Features License

```cmd
copy test-licenses\valid_partial_features.json E:\license.json
dotnet run --project UsbDongleLicensing
```

**Result:** Only Basic and Analytics features enabled

### Scenario 3: Expired License

```cmd
copy test-licenses\expired_license.json E:\license.json
dotnet run --project UsbDongleLicensing
```

**Result:** License rejected, demo mode only

### Scenario 4: Invalid Signature (Tampered)

```cmd
copy test-licenses\invalid_signature.json E:\license.json
dotnet run --project UsbDongleLicensing
```

**Result:** Signature verification fails, demo mode only

### Scenario 5: No USB Stick

```cmd
# Unplug USB stick or delete license.json
dotnet run --project UsbDongleLicensing
```

**Result:** No license found, demo mode only

---

## Production Deployment

### For End Users

1. **Distribute USB sticks** with valid license files
2. **Configure the application** to check the USB drive
3. **Users plug in USB stick** to run the application
4. **Application validates** license signature

### Configuration Options

#### Option A: Fixed Drive Letter

```json
{
  "SimulationPath": "E:"
}
```

**Pros:** Simple, predictable
**Cons:** Drive letter may vary on different computers

#### Option B: Search All Drives

Modify the application to search all removable drives for license file.

#### Option C: Specific Folder Name

```json
{
  "SimulationPath": "E:\\MyAppLicense"
}
```

**Pros:** More secure, less likely to conflict
**Cons:** Requires specific folder structure

---

## Security Enhancements

### Current Security Features

✅ **RSA Signature Verification** - License can't be tampered with
✅ **Expiration Date Check** - Licenses can expire
✅ **Feature Flags** - Control which features are enabled
✅ **Real-time Monitoring** - Detects USB removal immediately

### Additional Security (Optional)

1. **Check USB Serial Number:**
   - Bind license to specific USB stick
   - Prevents copying license to another USB

2. **Encrypt License File:**
   - Encrypt the license.json file
   - Decrypt in application

3. **Hardware Binding:**
   - Bind license to computer hardware ID
   - Prevents using same USB on multiple computers

4. **Online Validation:**
   - Periodically check license with server
   - Revoke licenses remotely

---

## Advantages of USB Stick Method

✅ **Simple** - No HID programming required
✅ **Reliable** - Works with any USB flash drive
✅ **Cheap** - Use standard USB sticks
✅ **User-Friendly** - Plug and play
✅ **Secure** - RSA signature prevents tampering
✅ **Hot-Plug** - Automatic detection
✅ **Cross-Platform** - Works on Windows, Linux, Mac

---

## Troubleshooting

### Issue: "No USB dongle detected"

**Check:**
1. Is USB stick plugged in?
2. Is the drive letter correct in appsettings.json?
3. Does `license.json` exist on the USB stick?
4. Is the path correct (check backslashes)?

**Test:**
```cmd
dir E:\license.json
```

### Issue: "Invalid license signature"

**Check:**
1. Is the license file properly signed?
2. Is the public key correct in appsettings.json?
3. Was the license file modified after signing?

**Solution:**
Regenerate the license:
```cmd
dotnet run --project LicenseGenerator
copy test-licenses\valid_all_features.json E:\license.json
```

### Issue: "Drive letter changes"

**Problem:** USB drive gets different letter on different computers

**Solutions:**
1. Use a consistent drive letter (assign in Windows)
2. Modify app to search all removable drives
3. Use a specific folder name to identify the correct drive

### Issue: "Hot-plug not working"

**Check:**
1. Is FileSystemWatcher working?
2. Are you testing in the same directory?
3. Try restarting the application

---

## Quick Start Commands

### Setup USB Stick:
```cmd
# Copy license to USB stick (replace E: with your drive letter)
copy test-licenses\valid_all_features.json E:\license.json
```

### Update appsettings.json:
```json
{
  "Mode": "simulation",
  "SimulationPath": "E:"
}
```

### Run Application:
```cmd
dotnet run --project UsbDongleLicensing
```

### Test Commands:
```cmd
> status      # Check license status
> features    # List available features
> run basic   # Run basic feature
> run analytics  # Run analytics (requires license)
> exit        # Quit application
```

---

## Summary

**This method is perfect because:**

1. ✅ **No HID programming** - Works with any USB stick
2. ✅ **Already implemented** - SimulatedDongleProvider does everything
3. ✅ **Secure** - RSA signatures prevent tampering
4. ✅ **Simple** - Just copy license file to USB
5. ✅ **Reliable** - File system monitoring is very stable
6. ✅ **User-friendly** - Plug and play experience

**You're ready to go!** Just:
1. Copy a license file to your Lexar USB stick
2. Update the drive letter in appsettings.json
3. Run the application

That's it! No HID complexity needed. 🎉
