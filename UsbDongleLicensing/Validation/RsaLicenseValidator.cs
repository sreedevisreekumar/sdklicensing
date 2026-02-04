namespace UsbDongleLicensing.Validation;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UsbDongleLicensing.Core;

/// <summary>
/// Validates license data using RSA-SHA256 signature verification.
/// </summary>
public class RsaLicenseValidator : ILicenseValidator
{
    private readonly RSA? _rsa;
    private readonly string _publicKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="RsaLicenseValidator"/> class.
    /// </summary>
    /// <param name="publicKey">The RSA public key in base64-encoded XML format.</param>
    public RsaLicenseValidator(string publicKey)
    {
        _publicKey = publicKey;
        
        if (!string.IsNullOrWhiteSpace(publicKey))
        {
            try
            {
                _rsa = RSA.Create();
                // Decode base64 to get XML string
                var xmlKey = Encoding.UTF8.GetString(Convert.FromBase64String(publicKey));
                _rsa.FromXmlString(xmlKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load public key: {ex.Message}");
                _rsa = null;
            }
        }
    }

    /// <summary>
    /// Validates the provided license data by checking signature, expiration, and required fields.
    /// </summary>
    /// <param name="license">The license data to validate.</param>
    /// <returns>A ValidationResult containing the validation status and any errors.</returns>
    public ValidationResult Validate(LicenseData license)
    {
        var result = new ValidationResult
        {
            IsValid = true,
            Status = LicenseStatus.Valid,
            Errors = new List<string>(),
            LicenseData = license
        };

        // Check for missing required fields
        var missingFields = new List<string>();
        
        if (string.IsNullOrWhiteSpace(license.LicenseKey))
        {
            missingFields.Add("LicenseKey");
        }
        
        if (string.IsNullOrWhiteSpace(license.IssuedTo))
        {
            missingFields.Add("IssuedTo");
        }
        
        if (license.Signature == null || license.Signature.Length == 0)
        {
            missingFields.Add("Signature");
        }
        
        if (license.ExpirationDate == default(DateTime))
        {
            missingFields.Add("ExpirationDate");
        }
        
        if (license.IssuedDate == default(DateTime))
        {
            missingFields.Add("IssuedDate");
        }

        if (missingFields.Count > 0)
        {
            result.IsValid = false;
            result.Status = LicenseStatus.Corrupted;
            result.Errors.Add($"Missing required fields: {string.Join(", ", missingFields)}");
            // Don't check expiration or signature if the license is corrupted
            return result;
        }

        // Check expiration
        if (!CheckExpiration(license.ExpirationDate))
        {
            result.IsValid = false;
            result.Status = LicenseStatus.Expired;
            result.Errors.Add($"License expired on {license.ExpirationDate:yyyy-MM-dd HH:mm:ss} UTC");
        }

        // Check signature
        if (license.Signature != null && license.Signature.Length > 0)
        {
            var licenseDataBytes = SerializeLicenseForSigning(license);
            if (!VerifySignature(licenseDataBytes, license.Signature))
            {
                result.IsValid = false;
                // Only set status to InvalidSignature if it's not already Expired
                if (result.Status != LicenseStatus.Expired)
                {
                    result.Status = LicenseStatus.InvalidSignature;
                }
                result.Errors.Add("Invalid license signature");
            }
        }

        return result;
    }

    /// <summary>
    /// Verifies the RSA signature of the provided data.
    /// </summary>
    /// <param name="data">The data that was signed.</param>
    /// <param name="signature">The RSA signature to verify.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    public bool VerifySignature(byte[] data, byte[] signature)
    {
        if (_rsa == null)
        {
            Console.WriteLine("Warning: No public key loaded, signature verification skipped");
            return false;
        }

        if (data == null || data.Length == 0)
        {
            return false;
        }

        if (signature == null || signature.Length == 0)
        {
            return false;
        }

        try
        {
            return _rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine($"Signature verification failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if the provided expiration date is still valid.
    /// </summary>
    /// <param name="expirationDate">The expiration date to check.</param>
    /// <returns>True if the expiration date is in the future; otherwise, false.</returns>
    public bool CheckExpiration(DateTime expirationDate)
    {
        return expirationDate > DateTime.UtcNow;
    }

    /// <summary>
    /// Serializes license data for signing (excludes the signature field itself).
    /// </summary>
    /// <param name="license">The license data to serialize.</param>
    /// <returns>Byte array of the serialized license data.</returns>
    private byte[] SerializeLicenseForSigning(LicenseData license)
    {
        // Create a copy of the license data without the signature for verification
        var dataForSigning = new
        {
            LicenseKey = license.LicenseKey,
            IssuedTo = license.IssuedTo,
            IssuedDate = license.IssuedDate.ToString("o"), // ISO 8601 format
            ExpirationDate = license.ExpirationDate.ToString("o"),
            EnabledFeatures = (int)license.EnabledFeatures
        };

        var json = JsonSerializer.Serialize(dataForSigning);
        return Encoding.UTF8.GetBytes(json);
    }
}
