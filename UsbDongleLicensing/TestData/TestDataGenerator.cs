namespace UsbDongleLicensing.TestData;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UsbDongleLicensing.Core;

/// <summary>
/// Generates test data for license validation testing, including RSA key pairs and signed licenses.
/// </summary>
public class TestDataGenerator
{
    private RSA? _privateKey;
    private RSA? _publicKey;

    /// <summary>
    /// Generates a new RSA key pair for testing purposes.
    /// </summary>
    /// <param name="keySize">The size of the RSA key in bits (default: 2048).</param>
    public void GenerateKeyPair(int keySize = 2048)
    {
        _privateKey = RSA.Create(keySize);
        _publicKey = RSA.Create();
        
        // Export and import to create separate public key instance
        var publicKeyXml = _privateKey.ToXmlString(false);
        _publicKey.FromXmlString(publicKeyXml);
    }

    /// <summary>
    /// Saves the generated RSA key pair to files in XML format.
    /// </summary>
    /// <param name="privateKeyPath">Path to save the private key file.</param>
    /// <param name="publicKeyPath">Path to save the public key file.</param>
    /// <exception cref="InvalidOperationException">Thrown when no key pair has been generated.</exception>
    public void SaveKeyPair(string privateKeyPath, string publicKeyPath)
    {
        if (_privateKey == null || _publicKey == null)
        {
            throw new InvalidOperationException("No key pair has been generated. Call GenerateKeyPair() first.");
        }

        // Create directories if they don't exist
        var privateKeyDir = Path.GetDirectoryName(privateKeyPath);
        if (!string.IsNullOrEmpty(privateKeyDir) && !Directory.Exists(privateKeyDir))
        {
            Directory.CreateDirectory(privateKeyDir);
        }

        var publicKeyDir = Path.GetDirectoryName(publicKeyPath);
        if (!string.IsNullOrEmpty(publicKeyDir) && !Directory.Exists(publicKeyDir))
        {
            Directory.CreateDirectory(publicKeyDir);
        }

        // Save private key (includes both private and public parameters)
        var privateKeyXml = _privateKey.ToXmlString(true);
        File.WriteAllText(privateKeyPath, privateKeyXml);

        // Save public key (only public parameters)
        var publicKeyXml = _publicKey.ToXmlString(false);
        File.WriteAllText(publicKeyPath, publicKeyXml);

        Console.WriteLine($"Private key saved to: {privateKeyPath}");
        Console.WriteLine($"Public key saved to: {publicKeyPath}");
    }

    /// <summary>
    /// Creates a license with the specified parameters.
    /// </summary>
    /// <param name="licenseKey">The unique license key identifier.</param>
    /// <param name="issuedTo">The name of the licensee.</param>
    /// <param name="issuedDate">The date when the license was issued.</param>
    /// <param name="expirationDate">The license expiration date.</param>
    /// <param name="enabledFeatures">The bitmask of enabled features.</param>
    /// <returns>A LicenseData object with the specified parameters (signature not yet generated).</returns>
    public LicenseData CreateLicense(
        string licenseKey,
        string issuedTo,
        DateTime issuedDate,
        DateTime expirationDate,
        FeatureFlags enabledFeatures)
    {
        return new LicenseData
        {
            LicenseKey = licenseKey,
            IssuedTo = issuedTo,
            IssuedDate = issuedDate,
            ExpirationDate = expirationDate,
            EnabledFeatures = enabledFeatures,
            Signature = Array.Empty<byte>() // Will be set by SignLicense
        };
    }

    /// <summary>
    /// Generates an RSA signature for the provided license data.
    /// </summary>
    /// <param name="license">The license data to sign.</param>
    /// <returns>The RSA signature as a byte array.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no private key is available.</exception>
    public byte[] SignLicense(LicenseData license)
    {
        if (_privateKey == null)
        {
            throw new InvalidOperationException("No private key available. Call GenerateKeyPair() first.");
        }

        // Serialize license data for signing (excluding the signature field)
        // IMPORTANT: This must match the serialization in RsaLicenseValidator.SerializeLicenseForSigning
        var dataForSigning = new
        {
            LicenseKey = license.LicenseKey,
            IssuedTo = license.IssuedTo,
            IssuedDate = license.IssuedDate.ToString("o"), // ISO 8601 format
            ExpirationDate = license.ExpirationDate.ToString("o"),
            EnabledFeatures = (int)license.EnabledFeatures
        };

        // Use default serialization options to match RsaLicenseValidator
        var json = JsonSerializer.Serialize(dataForSigning);
        var dataBytes = Encoding.UTF8.GetBytes(json);

        // Sign the data using RSA-SHA256
        var signature = _privateKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        
        return signature;
    }

    /// <summary>
    /// Saves a license to a JSON file.
    /// </summary>
    /// <param name="license">The license data to save.</param>
    /// <param name="filePath">The path where the license file should be saved.</param>
    public void SaveLicense(LicenseData license, string filePath)
    {
        // Create the directory if it doesn't exist
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Serialize license to JSON
        var licenseJson = new
        {
            licenseKey = license.LicenseKey,
            issuedTo = license.IssuedTo,
            issuedDate = license.IssuedDate.ToString("o"),
            expirationDate = license.ExpirationDate.ToString("o"),
            enabledFeatures = (int)license.EnabledFeatures,
            signature = Convert.ToBase64String(license.Signature)
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(licenseJson, options);
        File.WriteAllText(filePath, json);

        Console.WriteLine($"License saved to: {filePath}");
    }

    /// <summary>
    /// Gets the public key in base64-encoded XML format for use in configuration.
    /// </summary>
    /// <returns>The base64-encoded public key XML string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no key pair has been generated.</exception>
    public string GetPublicKeyBase64()
    {
        if (_publicKey == null)
        {
            throw new InvalidOperationException("No key pair has been generated. Call GenerateKeyPair() first.");
        }

        var publicKeyXml = _publicKey.ToXmlString(false);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKeyXml));
    }

    /// <summary>
    /// Loads an existing RSA key pair from XML files.
    /// </summary>
    /// <param name="privateKeyPath">Path to the private key file.</param>
    /// <param name="publicKeyPath">Path to the public key file.</param>
    public void LoadKeyPair(string privateKeyPath, string publicKeyPath)
    {
        if (!File.Exists(privateKeyPath))
        {
            throw new FileNotFoundException($"Private key file not found: {privateKeyPath}");
        }

        if (!File.Exists(publicKeyPath))
        {
            throw new FileNotFoundException($"Public key file not found: {publicKeyPath}");
        }

        var privateKeyXml = File.ReadAllText(privateKeyPath);
        var publicKeyXml = File.ReadAllText(publicKeyPath);

        _privateKey = RSA.Create();
        _privateKey.FromXmlString(privateKeyXml);

        _publicKey = RSA.Create();
        _publicKey.FromXmlString(publicKeyXml);

        Console.WriteLine("Key pair loaded successfully.");
    }
}
