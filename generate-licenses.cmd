@echo off
echo ========================================
echo USB Dongle Licensing - License Generator
echo ========================================
echo.

echo Running license generator...
echo.
dotnet run --project LicenseGenerator

echo.
echo ========================================
echo Done! Check the output above for the Public Key.
echo ========================================
pause
