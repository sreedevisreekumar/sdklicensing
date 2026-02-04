using Xunit;
using System.IO;
using System.Text.Json;
using UsbDongleLicensing.Core;

namespace UsbDongleLicensing.Tests;

/// <summary>
/// Tests for the Program class and application configuration.
/// </summary>
public class ProgramTests
{
    [Fact]
    public void AppConfiguration_CanBeDeserialized_FromJson()
    {
        // Arrange
        var json = @"{
            ""Mode"": ""simulation"",
            ""SimulationPath"": ""./test-licenses"",
            ""UsbVendorId"": ""0x1234"",
            ""UsbProductId"": ""0x5678"",
            ""PublicKey"": ""test-key"",
            ""Logging"": {
                ""LogLevel"": {
                    ""Default"": ""Information"",
                    ""Microsoft"": ""Warning""
                },
                ""Console"": {
                    ""Enabled"": true
                },
                ""File"": {
                    ""Enabled"": true,
                    ""Path"": ""logs/app.log""
                }
            }
        }";

        // Act
        var config = JsonSerializer.Deserialize<AppConfiguration>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(config);
        Assert.Equal("simulation", config.Mode);
        Assert.Equal("./test-licenses", config.SimulationPath);
        Assert.Equal("0x1234", config.UsbVendorId);
        Assert.Equal("0x5678", config.UsbProductId);
        Assert.Equal("test-key", config.PublicKey);
        Assert.NotNull(config.Logging);
        Assert.Equal("Information", config.Logging.LogLevel.Default);
        Assert.True(config.Logging.Console.Enabled);
    }

    [Fact]
    public void AppConfiguration_HasDefaultValues()
    {
        // Arrange & Act
        var config = new AppConfiguration();

        // Assert
        Assert.Equal("simulation", config.Mode);
        Assert.Equal("./test-licenses", config.SimulationPath);
        Assert.Equal("0x1234", config.UsbVendorId);
        Assert.Equal("0x5678", config.UsbProductId);
        Assert.Equal(string.Empty, config.PublicKey);
        Assert.NotNull(config.Logging);
    }

    [Theory]
    [InlineData("0x1234", 0x1234)]
    [InlineData("0x5678", 0x5678)]
    [InlineData("0xABCD", 0xABCD)]
    [InlineData("1234", 0x1234)]
    [InlineData("FFFF", 0xFFFF)]
    public void HexIdParsing_ConvertsCorrectly(string hexString, int expectedValue)
    {
        // Arrange
        var cleanHex = hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? hexString.Substring(2)
            : hexString;

        // Act
        var result = Convert.ToInt32(cleanHex, 16);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void ConfigurationFile_ExistsInProject()
    {
        // Arrange
        var configPath = Path.Combine("..", "..", "..", "..", "UsbDongleLicensing", "appsettings.json");

        // Act
        var exists = File.Exists(configPath);

        // Assert
        Assert.True(exists, "appsettings.json should exist in the UsbDongleLicensing project");
    }
}
