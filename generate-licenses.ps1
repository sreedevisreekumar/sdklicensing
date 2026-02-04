# PowerShell script to generate RSA keys and test licenses
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "USB Dongle Licensing - License Generator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if project is built
if (-not (Test-Path "UsbDongleLicensing\bin\Debug\net8.0\UsbDongleLicensing.dll")) {
    Write-Host "Building the project first..." -ForegroundColor Yellow
    dotnet build UsbDongleLicensing.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed!" -ForegroundColor Red
        Write-Host "Please fix build errors and try again." -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

Write-Host "Running license generator..." -ForegroundColor Green
Write-Host ""

# Use dotnet script to run the generator
$code = @'
using System;
using UsbDongleLicensing.Core;
using UsbDongleLicensing.TestData;

Console.WriteLine("=== USB Dongle Licensing - Test Data Generator ===");
Console.WriteLine();

var generator = new TestDataGenerator();

// Generate RSA key pair
Console.WriteLine("Generating RSA key pair (2048-bit)...");
generator.GenerateKeyPair(2048);
Console.WriteLine("✓ Key pair generated");
Console.WriteLine();

// Save keys to files
Console.WriteLine("Saving keys to files...");
generator.SaveKeyPair("keys/private_key.xml", "keys/public_key.xml");
Console.WriteLine();

// Get public key for configuration
var publicKeyBase64 = generator.GetPublicKeyBase64();
Console.WriteLine("Public Key (Base64) for appsettings.json:");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine(publicKeyBase64);
Console.ResetColor();
Console.WriteLine();

// Scenario 1: Valid license with all features
Console.WriteLine("Creating test licenses...");
Console.WriteLine();

Console.WriteLine("1. Valid license with ALL features");
var validAllFeatures = generator.CreateLicense(
    "DEMO-1234-5678-ABCD",
    "Test User",
    DateTime.UtcNow,
    DateTime.UtcNow.AddYears(2),
    FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics | 
    FeatureFlags.DataExport | FeatureFlags.ApiAccess
);
validAllFeatures.Signature = generator.SignLicense(validAllFeatures);
generator.SaveLicense(validAllFeatures, "test-licenses/valid_all_features.json");

// Scenario 2: Valid license with partial features
Console.WriteLine("2. Valid license with PARTIAL features (Basic + Analytics)");
var validPartial = generator.CreateLicense(
    "PARTIAL-1234-5678-ABCD",
    "Limited User",
    DateTime.UtcNow,
    DateTime.UtcNow.AddYears(2),
    FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics
);
validPartial.Signature = generator.SignLicense(validPartial);
generator.SaveLicense(validPartial, "test-licenses/valid_partial_features.json");

// Scenario 3: Expired license
Console.WriteLine("3. EXPIRED license");
var expired = generator.CreateLicense(
    "EXPIRED-1234-5678-ABCD",
    "Expired User",
    DateTime.UtcNow.AddYears(-2),
    DateTime.UtcNow.AddMonths(-1),
    FeatureFlags.BasicFeature | FeatureFlags.AdvancedAnalytics
);
expired.Signature = generator.SignLicense(expired);
generator.SaveLicense(expired, "test-licenses/expired_license.json");

// Scenario 4: License with invalid signature (tampered)
Console.WriteLine("4. License with INVALID signature (tampered)");
var tampered = generator.CreateLicense(
    "TAMPERED-1234-5678-ABCD",
    "Original User",
    DateTime.UtcNow,
    DateTime.UtcNow.AddYears(2),
    FeatureFlags.BasicFeature
);
tampered.Signature = generator.SignLicense(tampered);
tampered.IssuedTo = "Hacker";
generator.SaveLicense(tampered, "test-licenses/invalid_signature.json");

// Scenario 5: License with only basic feature
Console.WriteLine("5. Valid license with BASIC feature only");
var basicOnly = generator.CreateLicense(
    "BASIC-1234-5678-ABCD",
    "Basic User",
    DateTime.UtcNow,
    DateTime.UtcNow.AddYears(2),
    FeatureFlags.BasicFeature
);
basicOnly.Signature = generator.SignLicense(basicOnly);
generator.SaveLicense(basicOnly, "test-licenses/valid_basic_only.json");

Console.WriteLine();
Console.WriteLine("=== Test Data Generation Complete ===");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("✓ RSA keys saved to keys/ directory");
Console.WriteLine("✓ 5 test licenses created in test-licenses/ directory");
Console.ResetColor();
Console.WriteLine();
Console.WriteLine("Next Steps:");
Console.WriteLine("1. Copy the Public Key (Base64) shown above");
Console.WriteLine("2. Paste it into UsbDongleLicensing/appsettings.json in the 'PublicKey' field");
Console.WriteLine("3. Run: dotnet run --project UsbDongleLicensing");
Console.WriteLine();
'@

# Save the code to a temporary file
$tempFile = [System.IO.Path]::GetTempFileName() + ".cs"
$code | Out-File -FilePath $tempFile -Encoding UTF8

try {
    # Compile and run using dotnet
    $dllPath = Resolve-Path "UsbDongleLicensing\bin\Debug\net8.0\UsbDongleLicensing.dll"
    
    # Use csc from .NET SDK
    $cscPath = & where.exe csc 2>$null
    
    if ($cscPath) {
        # csc is available
        Write-Host "Compiling generator..." -ForegroundColor Gray
        csc /reference:$dllPath /out:GenerateTestLicenses.exe $tempFile /nologo
        if ($LASTEXITCODE -eq 0) {
            .\GenerateTestLicenses.exe
            Remove-Item GenerateTestLicenses.exe -ErrorAction SilentlyContinue
        }
    } else {
        # Use dotnet-script as fallback
        Write-Host "Note: Using dotnet-script method (csc not in PATH)" -ForegroundColor Gray
        Write-Host ""
        
        # Create a simple console app on the fly
        $projectDir = "TempLicenseGenerator"
        if (Test-Path $projectDir) {
            Remove-Item $projectDir -Recurse -Force
        }
        
        dotnet new console -n $projectDir -o $projectDir | Out-Null
        Copy-Item $tempFile "$projectDir\Program.cs" -Force
        
        # Add reference to main project
        $csprojContent = Get-Content "$projectDir\$projectDir.csproj"
        $csprojContent = $csprojContent -replace '</Project>', @"
  <ItemGroup>
    <ProjectReference Include="..\UsbDongleLicensing\UsbDongleLicensing.csproj" />
  </ItemGroup>
</Project>
"@
        $csprojContent | Set-Content "$projectDir\$projectDir.csproj"
        
        # Run it
        dotnet run --project $projectDir
        
        # Cleanup
        Remove-Item $projectDir -Recurse -Force -ErrorAction SilentlyContinue
    }
} finally {
    # Cleanup temp file
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Done! Check the output above for the Public Key." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
