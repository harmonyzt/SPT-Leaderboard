$dllPath = "F:\GitHub\Tarkov\SPT-Leaderboard\client\bin\Beta\SPTLeaderboard.dll"
$globalDataPath = "F:\GitHub\Tarkov\SPT-Leaderboard\client\Data\GlobalData.cs"

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllPath).ProductVersion
$versionClean = $version -replace '[^\d\.]', ''

$subVersionLine = Get-Content $globalDataPath | Where-Object { $_ -match 'SubVersion\s*=\s*"(\d+)"' }

if ($subVersionLine -match 'SubVersion\s*=\s*"(\d+)"') {
    $subVersion = $matches[1]
} else {
    Write-Error "SubVersion not found in GlobalData.cs"
    exit 1
}

$sha256 = Get-FileHash -Algorithm SHA256 -Path $dllPath
Write-Host "SHA256 Hash of DLL: $($sha256.Hash.ToLower())"

$sourceFolder = "C:\Users\Katrin0522\Documents\1 - DATA\SPT_ROOT\*"
$destinationZip = "C:\Users\Katrin0522\Documents\1 - DATA\SPT_Leaderboard_BETA_v${versionClean}-${subVersion}.zip"

Compress-Archive -Path $sourceFolder -DestinationPath $destinationZip -Force

Write-Host "BETA Mod packed: $destinationZip"
