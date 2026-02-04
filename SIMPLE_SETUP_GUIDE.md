# Simple Setup Guide - Generate Real Licenses

## The Easiest Way (3 Commands)

### Step 1: Build the project
```powershell
dotnet build UsbDongleLicensing.sln
```

### Step 2: Generate keys and licenses
```powershell
dotnet run --project LicenseGenerator
```

**This will:**
- Generate RSA key pair in `keys/` folder
- Create 5 test license files in `test-licenses/` folder
- Display a **Public Key (Base64)** - a very long string starting with `PFJTQUtleVZhbHVl...`

### Step 3: Copy the Public Key

Copy the entire public key string from the console output and paste it into `UsbDongleLicensing/appsettings.json`:

```json
{
  "Mode": "simulation",
  "SimulationPath": "./test-licenses",
  "UsbVendorId": "0x1234",
  "UsbProductId": "0x5678",
  "PublicKey": "PASTE_THE_ENTIRE_LONG_STRING_HERE",
  "Logging": {
    ...
  }
}
```

**That's it!** You're done with setup.

---

## Test It Works

### Run Scenario 2 (Valid License with All Features)

```powershell
copy test-licenses\valid_all_features.json test-licenses\license.json
dotnet run --project UsbDongleLicensing
```

### Expected Output

You should see:
```
╔════════════════════════════════════════════════════════════╗
║  USB Dongle Licensing Demo                                 ║
║  Version: 1.0.0                                            ║
╔════════════════════════════════════════════════════════════╗

License Status:
═══════════════════════════════════════════════════════════

✓ License is VALID                    ← GREEN TEXT

License Key:     DEMO-1234-5678-ABCD
Issued To:       Test User
Issued Date:     2024-02-04
Expiration Date: 2026-02-04
Enabled Features: Basic, Analytics, Export, API

═══════════════════════════════════════════════════════════
```

**No "Invalid signature" errors!** ✅

### Test Commands

```
> features
  ✓ Basic Feature              [Always Available]
  ✓ Advanced Analytics         [Licensed]
  ✓ Data Export                [Licensed]
  ✓ API Access                 [Licensed]

> run analytics
Success: true
Message: Advanced analytics completed successfully...

> exit
```

---

## Other Test Scenarios

### Partial License (Basic + Analytics only)
```powershell
copy test-licenses\valid_partial_features.json test-licenses\license.json
dotnet run --project UsbDongleLicensing
```

### Expired License
```powershell
copy test-licenses\expired_license.json test-licenses\license.json
dotnet run --project UsbDongleLicensing
```

### Invalid Signature (Security Test)
```powershell
copy test-licenses\invalid_signature.json test-licenses\license.json
dotnet run --project UsbDongleLicensing
```

---

## Troubleshooting

### "Invalid signature" error

**Fix:** Regenerate everything
```powershell
Remove-Item keys -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item test-licenses -Recurse -Force -ErrorAction SilentlyContinue
dotnet run --project LicenseGenerator
```
Then copy the NEW public key to `appsettings.json`

### "No public key loaded" warning

**Fix:** Make sure you copied the ENTIRE public key (it's 300+ characters)

### License expired

**Fix:** Regenerate (licenses are valid for 2 years from generation)
```powershell
dotnet run --project LicenseGenerator
```

---

## What Was Created

### Keys (in `keys/` directory):
- `private_key.xml` - Signs licenses (keep secret!)
- `public_key.xml` - Verifies signatures

### Licenses (in `test-licenses/` directory):
1. `valid_all_features.json` - All 4 features, expires in 2 years
2. `valid_partial_features.json` - Basic + Analytics only
3. `valid_basic_only.json` - Basic feature only
4. `expired_license.json` - Expired (for testing)
5. `invalid_signature.json` - Tampered (for security testing)

---

## Why This Works

1. **RSA Encryption**: Creates public/private key pair
2. **Signing**: Private key signs the license data
3. **Verification**: Public key verifies the signature
4. **Security**: If anyone changes the license, signature won't match

---

## Success Checklist

- ✅ Ran `dotnet run --project LicenseGenerator`
- ✅ Copied public key to `appsettings.json`
- ✅ Copied a license file to `license.json`
- ✅ Application shows "✓ License is VALID"
- ✅ No "Invalid signature" errors

**You're done!** 🎉

For more details, see:
- **MANUAL_TESTING_GUIDE.md** - All test scenarios
- **TESTING_SCENARIO_2_GUIDE.md** - Detailed Scenario 2 guide
- **GENERATE_REAL_LICENSES.md** - Technical details
