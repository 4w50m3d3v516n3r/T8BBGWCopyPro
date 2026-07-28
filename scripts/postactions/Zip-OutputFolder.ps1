<#  Zip-OutputFolder.ps1
    Compresses ALL disk images in a folder into one timestamped zip archive.

    Post-Action setup:
      Type:      PowerShell Script
      File:      C:\...\scripts\postactions\Zip-OutputFolder.ps1
      Arguments: -Folder "D:\FloppyImages"
      (or:       -Folder "{ImageFile}"  to use the folder the image lives in)
#>
param(
    [Parameter(Mandatory = $true)][string]$Folder,
    [string]$ZipPath = ""
)

$ErrorActionPreference = "Stop"

# Accept either a folder or a file (then its parent folder is used)
if (Test-Path $Folder -PathType Leaf) { $Folder = Split-Path $Folder -Parent }
if (-not (Test-Path $Folder)) { throw "Folder not found: $Folder" }

if (-not $ZipPath) {
    $stamp   = Get-Date -Format "yyyyMMdd_HHmmss"
    $ZipPath = Join-Path $Folder ("Images_{0}.zip" -f $stamp)
}

$imageExt = ".adf", ".adz", ".scp", ".img", ".ima", ".st", ".hfe", ".ipf", ".d64", ".dsk"
$files = Get-ChildItem $Folder -File |
         Where-Object { $imageExt -contains $_.Extension.ToLower() }

if ($files.Count -eq 0) { Write-Output "No disk images found in $Folder - nothing to do."; exit 0 }

Compress-Archive -Path $files.FullName -DestinationPath $ZipPath -CompressionLevel Optimal -Force
Write-Output ("Archived {0} image(s) into {1}" -f $files.Count, $ZipPath)
exit 0
