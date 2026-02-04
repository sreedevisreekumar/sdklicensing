# How to Check if Your USB Device Supports HID

This guide shows you multiple ways to check if your USB device supports the HID (Human Interface Device) protocol on Windows.

---

## Method 1: Using Device Manager (Easiest)

### Steps:

1. **Open Device Manager:**
   - Press `Win + X` and select "Device Manager"
   - OR press `Win + R`, type `devmgmt.msc`, and press Enter

2. **Look for your device in these categories:**
   - **"Human Interface Devices"** - If your device appears here, it supports HID
   - **"Universal Serial Bus devices"** - May also show USB devices

3. **Check device properties:**
   - Right-click on your USB device
   - Select "Properties"
   - Go to the "Details" tab
   - Select "Device class" from the dropdown
   - If it shows **"HIDClass"** or **"USB"**, it may support HID

### What to Look For:

**HID Devices typically show as:**
- USB Input Device
- HID-compliant device
- USB Human Interface Device
- HID Keyboard
- HID Mouse
- Generic HID Device

**Non-HID Devices show as:**
- USB Mass Storage Device (flash drives)
- USB Composite Device
- Disk drives
- Portable Devices

---

## Method 2: Using PowerShell (Most Detailed)

### Check All USB Devices:

```powershell
Get-PnpDevice -PresentOnly | Where-Object { $_.Class -eq 'HIDClass' } | Format-Table -AutoSize
```

**Expected Output for HID devices:**
```
Status Class    FriendlyName
------ -----    ------------
OK     HIDClass USB Input Device
OK     HIDClass HID-compliant mouse
OK     HIDClass HID Keyboard Device
```

### Get Detailed Device Information:

```powershell
Get-PnpDevice -PresentOnly | Where-Object { $_.InstanceId -match '^USB' } | Select-Object Status, Class, FriendlyName, InstanceId | Format-List
```

**Look for:**
- **Class**: Should be "HIDClass" for HID devices
- **InstanceId**: Contains VID (Vendor ID) and PID (Product ID)

### Find Vendor ID and Product ID:

```powershell
Get-PnpDevice -PresentOnly | Where-Object { $_.Class -eq 'HIDClass' } | ForEach-Object {
    $deviceId = $_.InstanceId
    if ($deviceId -match 'VID_([0-9A-F]{4}).*PID_([0-9A-F]{4})') {
        [PSCustomObject]@{
            Name = $_.FriendlyName
            VendorID = "0x$($matches[1])"
            ProductID = "0x$($matches[2])"
            Status = $_.Status
        }
    }
} | Format-Table -AutoSize
```

**Expected Output:**
```
Name                    VendorID ProductID Status
----                    -------- --------- ------
USB Input Device        0x046D   0xC52B    OK
HID Keyboard Device     0x413C   0x2113    OK
```

---

## Method 3: Using Windows Registry

### Steps:

1. **Open Registry Editor:**
   - Press `Win + R`
   - Type `regedit` and press Enter

2. **Navigate to:**
   ```
   HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB
   ```

3. **Look for your device:**
   - Expand the USB key
   - Look for entries like `VID_XXXX&PID_XXXX`
   - Click on the device entry
   - Look for "Class" value
   - If it says **"HIDClass"**, the device supports HID

---

## Method 4: Using Third-Party Tools

### USBDeview (Free Tool)

1. **Download USBDeview:**
   - Visit: https://www.nirsoft.net/utils/usb_devices_view.html
   - Download and run USBDeview.exe

2. **Check Device Information:**
   - Find your USB device in the list
   - Look at the "Device Class" column
   - If it shows **"HID"** or **"HIDClass"**, it supports HID

3. **View Details:**
   - Double-click the device
   - Check "Device Class" field
   - Note the Vendor ID and Product ID

---

## Method 5: Using Command Prompt

### List All HID Devices:

```cmd
wmic path Win32_PnPEntity where "PNPClass='HIDClass'" get Caption, DeviceID, Status
```

**Expected Output:**
```
Caption                  DeviceID                                          Status
HID Keyboard Device      HID\VID_413C&PID_2113\...                        OK
USB Input Device         HID\VID_046D&PID_C52B\...                        OK
```

---

## Method 6: Test with Your Application

### Quick Test:

1. **Plug in your USB device**

2. **Run the application in HID mode:**
   ```cmd
   dotnet run --project UsbDongleLicensing --hid
   ```

3. **Check the output:**
   - If it says **"✓ USB dongle detected"** - Your device supports HID
   - If it says **"⚠ No USB dongle detected"** - Device may not support HID or wrong VID/PID

---

## Common USB Device Types and HID Support

### ✅ Devices That Usually Support HID:

- **Keyboards** - Always HID
- **Mice** - Always HID
- **Game Controllers** - Usually HID
- **Barcode Scanners** - Often HID
- **Smart Card Readers** - Some support HID
- **Arduino Leonardo/Micro** - Programmable HID
- **Teensy Boards** - Programmable HID
- **Custom USB Dongles** - If programmed for HID

### ❌ Devices That Usually DON'T Support HID:

- **USB Flash Drives** - Mass Storage Class (not HID)
- **External Hard Drives** - Mass Storage Class
- **USB Printers** - Printer Class
- **USB Webcams** - Video Class
- **USB Audio Devices** - Audio Class
- **USB Serial Adapters** - CDC/Serial Class
- **Most Smartphones** - MTP/PTP Class

---

## Detailed PowerShell Script

Save this as `Check-USBDevices.ps1`:

```powershell
# Check USB HID Devices Script
Write-Host "=== USB HID Device Checker ===" -ForegroundColor Cyan
Write-Host ""

# Get all HID devices
$hidDevices = Get-PnpDevice -PresentOnly | Where-Object { $_.Class -eq 'HIDClass' }

if ($hidDevices.Count -eq 0) {
    Write-Host "No HID devices found!" -ForegroundColor Red
    exit
}

Write-Host "Found $($hidDevices.Count) HID device(s):" -ForegroundColor Green
Write-Host ""

foreach ($device in $hidDevices) {
    Write-Host "Device: $($device.FriendlyName)" -ForegroundColor Yellow
    Write-Host "  Status: $($device.Status)"
    
    # Extract VID and PID
    if ($device.InstanceId -match 'VID_([0-9A-F]{4}).*PID_([0-9A-F]{4})') {
        $vid = "0x$($matches[1])"
        $pid = "0x$($matches[2])"
        Write-Host "  Vendor ID: $vid" -ForegroundColor Green
        Write-Host "  Product ID: $pid" -ForegroundColor Green
        Write-Host "  Configuration for appsettings.json:"
        Write-Host "    `"UsbVendorId`": `"$vid`","
        Write-Host "    `"UsbProductId`": `"$pid`","
    }
    
    Write-Host "  Instance ID: $($device.InstanceId)"
    Write-Host ""
}

Write-Host "=== End of Report ===" -ForegroundColor Cyan
```

**Run it:**
```powershell
powershell -ExecutionPolicy Bypass -File Check-USBDevices.ps1
```

---

## What If My Device Doesn't Support HID?

### Option 1: Use Simulation Mode

Continue using the file-based simulation mode:

```json
{
  "Mode": "simulation",
  "SimulationPath": "./test-licenses"
}
```

This works perfectly for testing and development!

### Option 2: Get a HID-Compatible Device

**Recommended Devices:**

1. **Arduino Leonardo** (~$20)
   - Native USB HID support
   - Easy to program
   - Perfect for prototyping

2. **Teensy 3.2 or 4.0** (~$20-25)
   - Excellent HID support
   - Fast and reliable
   - Great documentation

3. **Custom USB Dongle**
   - Purpose-built for licensing
   - Can include secure elements
   - Professional solution

### Option 3: Use a Different Protocol

Modify the application to support:
- **USB Serial (CDC)** - Most USB devices support this
- **USB Mass Storage** - Read license from a file on USB drive
- **Network-based licensing** - Check license from a server

---

## Quick Reference Commands

### Check if specific device is HID:

```powershell
# Replace with your device name
Get-PnpDevice -FriendlyName "*USB*" | Select-Object FriendlyName, Class, Status
```

### List all USB devices with their class:

```powershell
Get-PnpDevice -PresentOnly | Where-Object { $_.InstanceId -match '^USB' } | Select-Object FriendlyName, Class | Sort-Object Class
```

### Find devices by Vendor ID:

```powershell
# Replace 046D with your vendor ID
Get-PnpDevice -PresentOnly | Where-Object { $_.InstanceId -match 'VID_046D' }
```

---

## Troubleshooting

### "No HID devices found"

**Possible reasons:**
1. No HID devices are connected
2. Device drivers not installed
3. Device is not recognized by Windows

**Solutions:**
- Connect a USB keyboard or mouse (always HID)
- Install device drivers
- Try a different USB port

### "Device shows in Device Manager but not as HID"

**This means:**
- Your device does NOT support HID protocol
- It uses a different USB class (Mass Storage, Serial, etc.)

**Solutions:**
- Use simulation mode instead
- Get a HID-compatible device
- Reprogram the device firmware to support HID (if possible)

### "Multiple devices with same VID/PID"

**This is normal for:**
- Generic USB devices
- Devices from the same manufacturer

**Solution:**
- Use additional identification (serial number)
- Modify the application to handle multiple devices

---

## Summary

**To check if your USB device supports HID:**

1. ✅ **Easiest**: Check Device Manager → Look under "Human Interface Devices"
2. ✅ **Most Reliable**: Run PowerShell command to list HID devices
3. ✅ **Quick Test**: Run the application and see if it detects the device

**If your device is NOT HID:**
- Use simulation mode (works great for testing!)
- Consider getting an Arduino Leonardo or Teensy for HID support
- Or continue with file-based licensing

---

## Need Help?

- Most USB flash drives are **NOT HID devices**
- Most keyboards and mice **ARE HID devices**
- Arduino Leonardo/Teensy **CAN BE programmed as HID devices**
- When in doubt, use **simulation mode** - it works perfectly!
