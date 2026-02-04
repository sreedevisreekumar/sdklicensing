# 🚀 START HERE - USB Dongle Licensing Demo

## You're Getting "Invalid Signature" Errors?

**This is normal!** You need to generate real RSA-signed licenses first.

## Quick Fix (3 Steps)

### 1. Build
```powershell
dotnet build UsbDongleLicensing.sln
```

### 2. Generate Licenses
```powershell
dotnet run --project LicenseGenerator
```

Or use the batch file:
```cmd
.\generate-licenses.cmd
```

### 3. Copy Public Key

The generator will show a long string like:
```
Public Key (Base64) for appsettings.json:
PFJTQUtleVZhbHVl... (very long)
```

Copy this ENTIRE string and paste it into `UsbDongleLicensing/appsettings.json`:

```json
{
  "PublicKey": "PASTE_HERE"
}
```

## Test It Works

```powershell
copy test-licenses\valid_all_features.json test-licenses\license.json
dotnet run --project UsbDongleLicensing
```

You should see **"✓ License is VALID"** in green! ✅

---

## Documentation

Choose your guide based on your needs:

### 📖 Quick Guides
- **[SIMPLE_SETUP_GUIDE.md](SIMPLE_SETUP_GUIDE.md)** ⭐ **START HERE** - Simplest instructions
- **[TESTING_SCENARIO_2_GUIDE.md](TESTING_SCENARIO_2_GUIDE.md)** - Detailed Scenario 2 walkthrough
- **[QUICK_START_REAL_LICENSES.md](QUICK_START_REAL_LICENSES.md)** - 3-step quick start

### 📚 Complete Guides
- **[MANUAL_TESTING_GUIDE.md](MANUAL_TESTING_GUIDE.md)** - All 8 test scenarios
- **[GENERATE_REAL_LICENSES.md](GENERATE_REAL_LICENSES.md)** - Technical details & theory
- **[README.md](README.md)** - Project overview & architecture

### 🔧 Technical Documentation
- **[TestDataGenerator_Usage_Example.md](TestDataGenerator_Usage_Example.md)** - API reference
- **[PROJECT_SETUP.md](PROJECT_SETUP.md)** - Project structure
- **Design & Requirements** - See `.kiro/specs/usb-dongle-licensing/`

---

## Common Issues

### "Invalid signature"
→ Regenerate keys: `dotnet run --project LicenseGenerator`
→ Copy NEW public key to `appsettings.json`

### "No public key loaded"
→ Make sure you copied the ENTIRE public key (300+ characters)

### "License expired"
→ Regenerate: `dotnet run --project LicenseGenerator`

### PowerShell says "command not found"
→ Use `.\generate-licenses.cmd` (with `.\` prefix)

---

## What You Get

After running the generator:

### Keys (in `keys/`)
- `private_key.xml` - Signs licenses
- `public_key.xml` - Verifies signatures

### Test Licenses (in `test-licenses/`)
1. `valid_all_features.json` - All features ✅
2. `valid_partial_features.json` - Limited features
3. `valid_basic_only.json` - Basic only
4. `expired_license.json` - For testing expiration
5. `invalid_signature.json` - For testing security

---

## Need Help?

1. **Read**: [SIMPLE_SETUP_GUIDE.md](SIMPLE_SETUP_GUIDE.md)
2. **Still stuck?**: Check [MANUAL_TESTING_GUIDE.md](MANUAL_TESTING_GUIDE.md) troubleshooting section
3. **Want details?**: See [GENERATE_REAL_LICENSES.md](GENERATE_REAL_LICENSES.md)

---

## Success Checklist

- ✅ Built project
- ✅ Ran license generator
- ✅ Copied public key to `appsettings.json`
- ✅ Copied a license to `license.json`
- ✅ Application shows "✓ License is VALID"

**You're ready to test!** 🎉
