$dllPath = "F:\GitHub\Tarkov\SPT-Leaderboard\client\bin\Release\SPTLeaderboard.dll"

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllPath).ProductVersion

$versionClean = $version -replace '[^\d\.]', ''

$sourceFolder = "C:\Users\Katrin0522\Documents\1 - DATA\SPT_ROOT\*"

$destinationZip = "C:\Users\Katrin0522\Documents\1 - DATA\SPT_Leaderboard_RELEASE_v$versionClean.zip"

Compress-Archive -Path $sourceFolder -DestinationPath $destinationZip -Force

Write-Host "RELEASE Mod packed: $destinationZip"