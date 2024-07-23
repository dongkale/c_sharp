@echo off
set WIX_PATH=C:\Program Files\WiX Toolset v5.0\bin\wix
set PROJECT_PATH=%cd%
cd %PROJECT_PATH%\Installer
"%WIX_PATH%\candle.exe" Product.wxs
"%WIX_PATH%\light.exe" -out output\MyAppInstaller.msi Product.wixobj
cd %PROJECT_PATH%
