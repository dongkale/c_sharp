@echo off
set WIX_PATH=C:\Program Files\WiX Toolset v5.0\bin
set PROJECT_PATH=%cd%
cd %PROJECT_PATH%\Installer
"%WIX_PATH%\candle.exe" Product_v3.wxs -ext WixUIExtension -ext WixUtilExtension
"%WIX_PATH%\light.exe" -out output\MyAppInstaller.msi Product.wixobj -cultures:ko-KR -ext WixUIExtension -ext WixUtilExtension -spdb
cd %PROJECT_PATH%
