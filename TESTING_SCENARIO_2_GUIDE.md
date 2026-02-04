# Quick Guide: Running Test Scenario 2 (Valid License)

This is a simplified guide specifically for running **Test Scenario 2** with a properly signed license.

## What You Need to Do

### One-Time Setup (Do This First!)

#### 1. Build the project
```cmd
dotnet build UsbDongleLicensing.sln
```

#### 2. Generate RSA keys and licenses

**Option A - Using dotnet (recommended):**
```powershell
dotnet run --project LicenseGenerator
```

**Option B - Using batch file:**
```cmd
.\generate-licenses.cmd
```

**What this does:**
- Creates RSA key pair in `keys/` folder
- Generates 5 test license files in `test-licenses/` folder
- Shows you a **Public Key (Base64)** in the console

#### 3. Copy the Public Key

The console will show something like:
```
Public Key (Base64) for appsettings.json:
PFJTQUtleVZhbHVlPjxNb2R1bHVzPnhZeU... (very long string)
```

**Copy this entire string** (it's 300+ characters long)

#### 4. Paste into appsettings.json

Open `UsbDongleLicensing/appsettings.json` and paste the public key:

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

Save the file.

---

### Running Scenario 2 (Every Time)

#### 1. Copy the valid license
```cmd
copy test-licenses\valid_all_features.json test-licenses\license.json
```

#### 2. Run the application
```cmd
dotnet run --project UsbDongleLicensing
```

#### 3. What You Should See

```
╔════════════════════════════════════════════════════════════╗
║  USB Dongle Licensing Demo                                 ║
║  Version: 1.0.0                                            ║
╔════════════════════════════════════════════════════════════╗

License Status:
═══════════════════════════════════════════════════════════

✓ License is VALID                    ← Should be GREEN

License Key:     DEMO-1234-5678-ABCD
Issued To:       Test User
Issued Date:     2024-01-01
Expiration Date: 2027-12-31           ← Should be in the future
Enabled Features: Basic, Analytics, Export, API

═══════════════════════════════════════════════════════════
```

**NO "Invalid signature" errors!** ✅

#### 4. Test the Commands

Type these commands to test:

```
> features
  ✓ Basic Feature              [Always Available]
  ✓ Advanced Analytics         [Licensed]
  ✓ Data Export                [Licensed]
  ✓ API Access                 [Licensed]

> run analytics
Success: true
Message: Advanced analytics completed successfully. Analysis of 10,000 records performed.
Data: { totalRecords: 10000, averageValue: 42.5, ... }

> run export
Success: true
Message: Data export completed successfully. File generated: export_20240204_123456.csv
Data: { fileName: "export_20240204_123456.csv", fileSize: "2.5 MB", ... }

> run api
Success: true
Message: API access successful. Data retrieved from remote endpoint.
Data: { endpoint: "https://api.example.com/v1/data", statusCode: 200, ... }

> exit
```

---

## Troubleshooting

### Problem: "Invalid signature" error

**Cause:** Public key doesn't match the private key used to sign the license.

**Solution:**
1. Delete the keys and licenses:
   ```cmd
   rmdir /s /q keys
   rmdir /s /q test-licenses
   ```

2. Regenerate everything:
   ```powershell
   dotnet run --project LicenseGenerator
   ```
   
   Or:
   ```cmd
   .\generate-licenses.cmd
   ```

3. Copy the NEW public key to `appsettings.json`

4. Copy the NEW license:
   ```cmd
   copy test-licenses\valid_all_features.json test-licenses\license.json
   ```

5. Try again

### Problem: "License expired" error

**Cause:** The license was generated more than 2 years ago.

**Solution:** Regenerate the licenses:
```powershell
dotnet run --project LicenseGenerator
copy test-licenses\valid_all_features.json test-licenses\license.json
```

### Problem: "No public key loaded" warning

**Cause:** The `PublicKey` field in `appsettings.json` is empty or incomplete.

**Solution:** 
1. Make sure you copied the ENTIRE public key string (it's very long)
2. Check there are no extra spaces or line breaks
3. The string should start with something like `PFJTQUtleVZhbHVl...`

### Problem: Generator fails to compile

**Cause:** Project not built yet.

**Solution:**
```cmd
dotnet build UsbDongleLicensing.sln
```

---

## Why This Works

1. **RSA Key Pair**: The generator creates a public/private key pair
   - **Private key** (in `keys/private_key.xml`) - Signs the licenses
   - **Public key** (in `appsettings.json`) - Verifies the signatures

2. **Signing Process**:
   ```
   License Data → Hash (SHA256) → Sign with Private Key → Signature
   ```

3. **Verification Process**:
   ```
   License Data → Hash (SHA256) → Verify with Public Key + Signature → ✓ Valid
   ```

4. **Security**: If anyone changes the license data, the signature won't match and validation fails

---

## Success Checklist

- ✅ Built the project
- ✅ Ran `dotnet run --project LicenseGenerator`
- ✅ Copied public key to `appsettings.json`
- ✅ Copied `valid_all_features.json` to `license.json`
- ✅ Application shows "✓ License is VALID" in green
- ✅ All 4 features show as enabled
- ✅ Can run all features successfully
- ✅ No "Invalid signature" errors

If all checkboxes are checked, you're done! 🎉

---

## Next Steps

Once Scenario 2 works, try other scenarios:

- **Scenario 3**: Partial license
  ```cmd
  copy test-licenses\valid_partial_features.json test-licenses\license.json
  ```

- **Scenario 4**: Expired license
  ```cmd
  copy test-licenses\expired_license.json test-licenses\license.json
  ```

- **Scenario 5**: Invalid signature (security test)
  ```cmd
  copy test-licenses\invalid_signature.json test-licenses\license.json
  ```

See **MANUAL_TESTING_GUIDE.md** for complete details on all scenarios.
