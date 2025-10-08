Set-Location -Path $PSScriptRoot

$dllPath = ".\Build\BepInEx\plugins\SPT-Leaderboard\SPTLeaderboard.dll"
$dllFullPath = Join-Path $PSScriptRoot $dllPath
$globalDataPath = "..\Data\GlobalData.cs"

Write-Host "Checking DLL in path: $dllFullPath"
if (-Not (Test-Path $dllFullPath)) {
    Write-Error "DLL file not found: $dllFullPath"
    exit 1
}

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllFullPath).ProductVersion
$versionClean = $version -replace '[^\d\.]', ''

$subVersionLine = Get-Content $globalDataPath | Where-Object { $_ -match 'SubVersion\s*=\s*"(\d+)"' }
if ($subVersionLine -match 'SubVersion\s*=\s*"(\d+)"') {
    $subVersion = $matches[1]
} else {
    Write-Error "SubVersion not found in GlobalData.cs"
    exit 1
}

$sha256 = Get-FileHash -Algorithm SHA256 -Path $dllFullPath
Write-Host "SHA256 Hash of DLL: $($sha256.Hash.ToLower())"

$sourceFolder = ".\Build\*"
$destination7z = ".\SPT_Leaderboard_DEBUG_v${versionClean}-${subVersion}.7z"

# Path 7z
$sevenZipPath = "C:\Program Files\7-Zip\7z.exe"

# Create 7z
& $sevenZipPath a -t7z -mx=9 $destination7z $sourceFolder

Write-Host "DEBUG Mod packed: $destination7z"
