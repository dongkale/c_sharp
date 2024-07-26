@echo on
set PROJECT_PATH=%cd%
cd %PROJECT_PATH%\Installer
"wix.exe" build Product_v4.wxs -o Product_v4.msi -ext WixToolset.UI.wixext -ext WixToolset.UTIL.wixext -culture "ko-KR" -dcl "mszip"
cd %PROJECT_PATH%
