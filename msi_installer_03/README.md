"# c_sharp"

-- build
wix build Product_v4.wxs -o Product_v4.msi -ext WixToolset.UI.wixext -ext WixToolset.UTIL.wixext -culture "ko-KR" -dcl "mszip

-- Understand Wix V4 Project
https://thecadcoder.com/knowledge-base/wix-understand-project/#understand-packagewxs

-- 오류

- https://github.com/orgs/wixtoolset/discussions/6516

  wix build -ext WixUIExtension installer.wxs
  wix.exe : error WIX0144: The extension 'WixUIExtension' could not be found. Checked paths: WixUIExtension

- 해결

1. wix extension add WixToolset.UI.wixext // .wix 폴더 생성
2. wix extension add WixToolset.UTIL.wixext // .wix 폴더 생성

3. wix extension list
   > WixToolset.UI.wixext 5.0.1
   > WixToolset.UTIL.wixext 5.0.1

-- WiX v4 for WiX v3 users

https://wixtoolset.org/docs/fourthree/

-- Wix v3 -> Wix v4
wix convert Product.wxs (overwrite)

-- Wix Download
https://github.com/wixtoolset/wix/releases

-- license file added

"%WIX_PATH%\light.exe" -out output\MyAppInstaller.msi Product.wixobj -cultures:ko-KR -ext WixUIExtension -ext WixUtilExtension -spdb
