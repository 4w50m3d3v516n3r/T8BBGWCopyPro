<#  Copy-ToBackup.ps1
    Copies the finished image (and its .sha256 sidecar, if present) to a backup
    folder or NAS share, preserving the file name.

    Post-Action setup:
      Type:      PowerShell Script
      File:      C:\...\scripts\postactions\Copy-ToBackup.ps1
      Arguments: -ImageFile "{ImageFile}" -Destination "\\NAS\FloppyArchive"
#>
param(
    [Parameter(Mandatory = $true)][string]$ImageFile,
    [Parameter(Mandatory = $true)][string]$Destination
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $ImageFile))   { throw "Image not found: $ImageFile" }
New-Item -ItemType Directory -Force -Path $Destination | Out-Null

Copy-Item $ImageFile -Destination $Destination -Force
Write-Output "Copied $([IO.Path]::GetFileName($ImageFile)) -> $Destination"

$sidecar = "$ImageFile.sha256"
if (Test-Path $sidecar) {
    Copy-Item $sidecar -Destination $Destination -Force
    Write-Output "Copied checksum sidecar as well."
}
exit 0
