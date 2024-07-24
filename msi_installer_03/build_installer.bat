@echo off
set WIX_PATH=C:\Program Files\WiX Toolset v5.0\bin
set PROJECT_PATH=%cd%
cd %PROJECT_PATH%\Installer
"%WIX_PATH%\candle.exe" Product.wxs -ext WixUIExtension
"%WIX_PATH%\light.exe" -out output\MyAppInstaller.msi Product.wixobj -cultures:ko-KR -dWixUILicenseRtf=license.rtf -ext WixUIExtension -spdb
cd %PROJECT_PATH%
