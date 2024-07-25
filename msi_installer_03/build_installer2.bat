@echo off
set WIX_PATH=C:\Program Files\WiX Toolset v5.0__\bin
set PROJECT_PATH=%cd%
cd %PROJECT_PATH%\Installer
"%WIX_PATH%\wix.exe" build -o Product.msi Product.wxs -ext WixUIExtension -ext WixUtilExtension
cd %PROJECT_PATH%
