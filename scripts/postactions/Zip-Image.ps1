<#  Zip-Image.ps1
    Compresses the finished disk image into a .zip placed next to it.

    Post-Action setup:
      Type:      PowerShell Script
      File:      C:\...\scripts\postactions\Zip-Image.ps1
      Arguments: -ImageFile "{ImageFile}"
      Optional:  add  -DeleteOriginal  to remove the uncompressed image afterwards.
#>
param(
    [Parameter(Mandatory = $true)][string]$ImageFile,
    [switch]$DeleteOriginal
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $ImageFile)) { throw "Image not found: $ImageFile" }

$zip = [IO.Path]::ChangeExtension($ImageFile, ".zip")
Compress-Archive -Path $ImageFile -DestinationPath $zip -CompressionLevel Optimal -Force

$src = (Get-Item $ImageFile).Length
$dst = (Get-Item $zip).Length
Write-Output ("Zipped {0} -> {1}  ({2:N0} -> {3:N0} bytes, {4:P0} of original)" -f `
    [IO.Path]::GetFileName($ImageFile), [IO.Path]::GetFileName($zip), $src, $dst, ($dst / $src))

if ($DeleteOriginal) {
    Remove-Item $ImageFile
    Write-Output "Original image deleted."
}
exit 0
