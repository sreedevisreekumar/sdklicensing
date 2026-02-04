@echo off
echo Testing USB Dongle Licensing Application
echo.
echo Starting application...
echo.
timeout /t 2 /nobreak >nul
dotnet run --project UsbDongleLicensing
