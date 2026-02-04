# Quick Start: Generate Real RSA-Signed Licenses

## Problem You're Facing

You're getting "Invalid signature" errors because the test licenses use dummy signatures. You need **real RSA-signed licenses** for proper testing.

## Solution: 3 Simple Steps

### Step 1: Build the Project

```cmd
dotnet build UsbDongleLicensing.sln
```

### Step 2: Run the License Generator

**Option A - Using dotnet (recommended):**
```powershell
dotnet run --project LicenseGenerator
```

**Option B - Using batch file:**
```cmd
.\generate-licenses.cmd
```

**Option C - Manual compilation (if needed):**
```cmd
csc /reference:UsbDongleLicensing\bin\Debug\net8.0\UsbDongleLicensing.dll /out:GenerateTestLicenses.exe GenerateTestLicenses.cs
.\GenerateTestLicenses.exe
```

### Step 3: Copy the Public Key

The generator will output something like:

```
Public Key (Base64) for appsettings.json:
PFJTQUtleVZhbHVlPjxNb2R1bHVzPnhZ... (long base64 string)
```

**Copy this entire string** and paste it into `UsbDongleLicensing/appsettings.json`:

```json
{
  "Mode": "simulation",
  "SimulationPath": "./test-licenses",
  "UsbVendorId": "0x1234",
  "UsbProductId": "0x5678",
  "PublicKey": "PASTE_HERE",  ← Paste the base64 string here
  "Logging": {
    ...
  }
}
```

## What Gets Created

The generator creates these files:

### Keys (in `keys/` directory):
- `private_key.xml` - Used to sign licenses (keep secret!)
- `public_key.xml` - Used to verify signatures

### Test Licenses (in `test-licenses/` directory):
1. **valid_all_features.json** - All 4 features enabled, expires in 2 years
2. **valid_partial_features.json** - Only Basic + Analytics
3. **valid_basic_only.json** - Only Basic feature
4. **expired_license.json** - Expired license (for testing error handling)
5. **invalid_signature.json** - Tampered license (for testing security)

## Testing Different Scenarios

To test a specific scenario, copy the license file to `license.json`:

```cmd
# Test with all features (this is what you want for Scenario 2)
copy test-licenses\valid_all_features.json test-licenses\license.json

# Test with partial features (for Scenario 3)
copy test-licenses\valid_partial_features.json test-licenses\license.json

# Test expired license (for Scenario 4)
copy test-licenses\expired_license.json test-licenses\license.json
```

Then run the application:

```cmd
dotnet run --project UsbDongleLicensing
```

## Verify It Works

After setting up the public key and copying a valid license:

1. Run the application
2. You should see: **"✓ License is VALID"** (in green)
3. Type `features` - you should see all 4 features enabled with ✓ marks
4. Type `run analytics` - it should execute successfully

## Troubleshooting

### "Invalid signature" still appears

**Cause**: Public key in `appsettings.json` doesn't match the private key used to sign.

**Fix**: 
1. Delete the `keys/` directory
2. Run the generator again
3. Copy the NEW public key to `appsettings.json`
4. Use the NEW license files

### "No public key loaded" warning

**Cause**: The `PublicKey` field is empty or has invalid base64.

**Fix**: Make sure you copied the ENTIRE base64 string (it's very long, 300+ characters)

### Generator fails to compile

**Cause**: Project not built yet.

**Fix**: Run `dotnet build UsbDongleLicensing.sln` first

## Why This Works

1. **RSA Key Pair**: Creates a public/private key pair
   - Private key signs the licenses
   - Public key verifies the signatures

2. **Signature Process**:
   ```
   License Data → Hash → Sign with Private Key → Signature
   ```

3. **Verification Process**:
   ```
   License Data → Hash → Verify with Public Key + Signature → ✓ Valid
   ```

4. **Security**: If anyone changes the license data, the signature won't match and validation fails

## Next Steps

Once you have real signed licenses working:

1. Test all scenarios in the Manual Testing Guide
2. Try tampering with a license file to see signature validation in action
3. Test with expired licenses
4. Test dongle disconnection (delete license.json while app is running)

That's it! You now have properly signed licenses with real RSA cryptography. 🔐
