<#  Extract-Archive.ps1
    Lists an archive with lsar.exe and extracts it with unar.exe.
    Reports all disk images found after extraction.

    Post-Action setup:
      Type:      PowerShell Script
      File:      C:\...\scripts\postactions\Extract-Archive.ps1
      Arguments: -Archive "{ImageFile}" -Destination "D:\Extracted"
#>
param(
    [Parameter(Mandatory = $true)][string]$Archive,
    [string]$Destination = "",
    [string]$ToolsDir    = ""
)

$ErrorActionPreference = "Stop"

# Locate lsar/unar: explicit -ToolsDir, then tools\ next to this script's
# grandparent (repo layout), then tools\ next to GWCopyPro.exe, then PATH.
function Find-Tool([string]$name) {
    $candidates = @()
    if ($ToolsDir) { $candidates += (Join-Path $ToolsDir $name) }
    $candidates += (Join-Path $PSScriptRoot "..\..\tools\$name")
    $candidates += (Join-Path (Split-Path $PSScriptRoot -Parent) "tools\$name")
    $candidates += (Join-Path (Get-Location) "tools\$name")
    foreach ($c in $candidates) { if (Test-Path $c) { return (Resolve-Path $c).Path } }
    $cmd = Get-Command $name -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    throw "$name not found. Pass -ToolsDir <folder containing lsar.exe/unar.exe>."
}

$lsar = Find-Tool "lsar.exe"
$unar = Find-Tool "unar.exe"

if (-not (Test-Path $Archive)) { throw "Archive not found: $Archive" }

if (-not $Destination) {
    $Destination = Join-Path (Split-Path $Archive -Parent) `
                   ([IO.Path]::GetFileNameWithoutExtension($Archive))
}
New-Item -ItemType Directory -Force -Path $Destination | Out-Null

Write-Output "=== Archive contents ($([IO.Path]::GetFileName($Archive))) ==="
& $lsar $Archive

Write-Output "=== Extracting to $Destination ==="
& $unar -force-overwrite -output-directory $Destination $Archive
if ($LASTEXITCODE -ne 0) { throw "unar.exe failed with exit code $LASTEXITCODE" }

$imageExt = ".adf", ".adz", ".scp", ".img", ".ima", ".st", ".hfe", ".ipf", ".d64", ".dsk"
$images = Get-ChildItem $Destination -Recurse -File |
          Where-Object { $imageExt -contains $_.Extension.ToLower() }

Write-Output "=== Disk images found: $($images.Count) ==="
$images | ForEach-Object { Write-Output "  $($_.FullName)  ($($_.Length) bytes)" }
exit 0
