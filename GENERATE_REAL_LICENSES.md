# Generate Real RSA-Signed Licenses

This guide shows you how to generate properly signed licenses with real RSA keys for testing.

## Quick Start

### Step 1: Run the Test Data Generator

```cmd
dotnet run --project UsbDongleLicensing -- generate-test-data
```

Or compile and run the standalone generator:

```cmd
csc /reference:UsbDongleLicensing/bin/Debug/net8.0/UsbDongleLicensing.dll /out:GenerateTestLicenses.exe GenerateTestLicenses.cs
GenerateTestLicenses.exe
```

Or use the simpler approach - add this to your Program.cs temporarily:

```cmd
dotnet run --project UsbDongleLicensing -- --generate-keys
```

### Step 2: What Gets Generated

The generator creates:

1. **RSA Key Pair**:
   - `keys/private_key.xml` - Private key (keep secure!)
   - `keys/public_key.xml` - Public key

2. **Test License Files** in `test-licenses/`:
   - `valid_all_features.json` - All features enabled (expires in 2 years)
   - `valid_partial_features.json` - Basic + Analytics only
   - `valid_basic_only.json` - Basic feature only
   - `expired_license.json` - Expired license (for testing)
   - `invalid_signature.json` - Tampered license (for testing)

### Step 3: Configure the Application

The generator will output a **Public Key (Base64)** string. Copy this and paste it into `UsbDongleLicensing/appsettings.json`:

```json
{
  "Mode": "simulation",
  "SimulationPath": "./test-licenses",
  "UsbVendorId": "0x1234",
  "UsbProductId": "0x5678",
  "PublicKey": "PASTE_THE_BASE64_PUBLIC_KEY_HERE",
  "Logging": {
    ...
  }
}
```

### Step 4: Test with Different Licenses

To test a specific license scenario, copy it to `license.json`:

```cmd
# Test with all features
copy test-licenses\valid_all_features.json test-licenses\license.json

# Test with partial features
copy test-licenses\valid_partial_features.json test-licenses\license.json

# Test with expired license
copy test-licenses\expired_license.json test-licenses\license.json

# Test with invalid signature
copy test-licenses\invalid_signature.json test-licenses\license.json
```

### Step 5: Run the Application

```cmd
dotnet run --project UsbDongleLicensing
```

Now the application will properly validate RSA signatures!

---

## Manual Method (Using C# Code Directly)

If you prefer to generate keys programmatically:

```csharp
using UsbDongleLicensing.Core;
using UsbDongleLicensing.TestData;

var generator = new TestDataGenerator();

// Generate keys
generator.GenerateKeyPair(2048);
generator.SaveKeyPair("keys/private_key.xml", "keys/public_key.xml");

// Get public key for config
string publicKeyBase64 = generator.GetPublicKeyBase64();
Console.WriteLine($"Public Key: {publicKeyBase64}");

// Create and sign a license
var license = generator.CreateLicense(
    "DEMO-1234-5678-ABCD",
    "Test User",
    DateTime.UtcNow,
    DateTime.UtcNow.AddYears(2),
    FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | 
    FeatureFlags.DataExport | FeatureFlags.ApiAccess
);

license.Signature = generator.SignLicense(license);
generator.SaveLicense(license, "test-licenses/license.json");
```

---

## Understanding the Signature Process

### How RSA Signing Works

1. **Key Generation**: Creates a public/private key pair
   - Private key: Used to SIGN licenses (keep secret!)
   - Public key: Used to VERIFY signatures (embed in app)

2. **License Signing**:
   ```
   License Data → Serialize to JSON → Hash (SHA256) → Sign with Private Key → Signature
   ```

3. **License Verification**:
   ```
   License Data → Serialize to JSON → Hash (SHA256) → Verify with Public Key + Signature → Valid/Invalid
   ```

### Why Signatures Matter

- **Prevents Tampering**: If someone changes the license data, the signature won't match
- **Authenticity**: Only you (with the private key) can create valid licenses
- **Security**: Public key can be distributed safely; only private key can sign

### Example: What Happens When You Tamper

```json
// Original signed license
{
  "licenseKey": "DEMO-1234",
  "issuedTo": "Test User",
  "expirationDate": "2027-12-31",
  "enabledFeatures": 15,
  "signature": "valid_signature_here"
}

// Someone tries to change it
{
  "licenseKey": "DEMO-1234",
  "issuedTo": "Hacker",  // ← Changed!
  "expirationDate": "2027-12-31",
  "enabledFeatures": 15,
  "signature": "valid_signature_here"  // ← Signature no longer matches!
}
```

The validator will detect this and reject the license with "Invalid signature" error.

---

## Troubleshooting

### "Invalid signature" error

**Cause**: The public key in `appsettings.json` doesn't match the private key used to sign the license.

**Solution**: 
1. Regenerate keys and licenses together
2. Copy the new public key to `appsettings.json`
3. Use the newly generated license files

### "No public key loaded" warning

**Cause**: The `PublicKey` field in `appsettings.json` is empty or invalid.

**Solution**: Copy the base64 public key from the generator output into `appsettings.json`

### License still shows as invalid

**Possible causes**:
1. License is expired (check `expirationDate`)
2. Public key mismatch
3. License file is corrupted

**Debug steps**:
```cmd
# Check the license file is valid JSON
type test-licenses\license.json

# Verify expiration date is in the future
# Check the console output for specific validation errors
```

---

## Security Best Practices

### For Testing/Demo:
- ✅ Use the generated test keys
- ✅ Keep keys in the `keys/` directory
- ✅ Add `keys/` to `.gitignore`

### For Production:
- ⚠️ Generate production keys separately
- ⚠️ Store private key in secure location (HSM, key vault)
- ⚠️ Never commit private keys to version control
- ⚠️ Use different keys for different environments
- ⚠️ Implement key rotation strategy
- ⚠️ Consider using hardware security modules (HSM)

---

## Feature Flags Reference

When creating licenses, use these feature flag values:

| Feature | Value | Binary |
|---------|-------|--------|
| None | 0 | 0000 |
| BasicFeature | 1 | 0001 |
| AdvancedAnalytics | 2 | 0010 |
| DataExport | 4 | 0100 |
| ApiAccess | 8 | 1000 |

**Combine features using bitwise OR:**
- All features: `15` (1+2+4+8)
- Basic + Analytics: `3` (1+2)
- Basic + Export: `5` (1+4)
- Analytics + API: `10` (2+8)

---

## Next Steps

After generating real licenses:

1. Update the **MANUAL_TESTING_GUIDE.md** with the new license files
2. Test all scenarios with properly signed licenses
3. Verify signature validation works correctly
4. Test with tampered licenses to ensure security

Enjoy testing with real RSA-signed licenses! 🔐
