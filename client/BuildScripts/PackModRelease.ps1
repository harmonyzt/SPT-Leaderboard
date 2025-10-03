$dllPath = "C:\GitHub\Tarkov\SPT-Leaderboard\client\bin\Release\SPTLeaderboard.dll"

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllPath).ProductVersion
$versionClean = $version -replace '[^\d\.]', ''

$sha256 = Get-FileHash -Algorithm SHA256 -Path $dllPath
Write-Host "SHA256 Hash of DLL: $($sha256.Hash.ToLower())"

$sourceFolder = "C:\Users\Katrin0522\Documents\1 - DATA\SPT_ROOT\*"
$destination7z = "C:\Users\Katrin0522\Documents\1 - DATA\SPT_Leaderboard_RELEASE_v$versionClean.7z"

# Path 7z
$sevenZipPath = "C:\Program Files\7-Zip\7z.exe"

# Create 7z
& $sevenZipPath a -t7z -mx=9 $destination7z $sourceFolder

Write-Host "RELEASE Mod packed: $destination7z"
