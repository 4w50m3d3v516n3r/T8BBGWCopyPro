<#  Validate-Image.ps1
    Validates a freshly created disk image:
      1. File exists and is not zero bytes.
      2. File size matches the expected size for known image types (warning only).
      3. Writes a SHA-256 checksum to "<image>.sha256".
    Exit code 0 = OK, 1 = validation failed (visible in gw_output.log).

    Post-Action setup:
      Type:      PowerShell Script
      File:      C:\...\scripts\postactions\Validate-Image.ps1
      Arguments: -ImageFile "{ImageFile}"
#>
param(
    [Parameter(Mandatory = $true)][string]$ImageFile
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ImageFile)) {
    Write-Output "[FAIL] Image file does not exist: $ImageFile"
    exit 1
}

$item = Get-Item $ImageFile
if ($item.Length -eq 0) {
    Write-Output "[FAIL] Image file is empty (0 bytes): $ImageFile"
    exit 1
}

# Expected sizes (bytes) for common sector-image types. Flux formats (.scp/.hfe)
# have variable sizes and are only checked for non-emptiness.
$expected = @{
    ".adf" = @(901120, 1802240)                              # Amiga DD / HD
    ".img" = @(184320, 327680, 368640, 737280, 819200,
               1228800, 1474560, 2949120)                    # common PC sizes
    ".ima" = @(737280, 1474560)
    ".st"  = @(368640, 409600, 737280, 819200)               # Atari ST
    ".d64" = @(174848, 175531)                               # C64 1541 (w/o + with error info)
}

$ext = $item.Extension.ToLower()
if ($expected.ContainsKey($ext)) {
    if ($expected[$ext] -contains $item.Length) {
        Write-Output ("[OK]   Size check passed: {0:N0} bytes is valid for {1}" -f $item.Length, $ext)
    } else {
        Write-Output ("[WARN] Unusual size for {0}: {1:N0} bytes (expected one of: {2})" -f `
            $ext, $item.Length, ($expected[$ext] -join ", "))
    }
} else {
    Write-Output ("[INFO] No size table for {0} - skipping size check ({1:N0} bytes)." -f $ext, $item.Length)
}

$hash = (Get-FileHash $ImageFile -Algorithm SHA256).Hash
$sidecar = "$ImageFile.sha256"
"$hash *$([IO.Path]::GetFileName($ImageFile))" | Out-File -FilePath $sidecar -Encoding ascii
Write-Output "[OK]   SHA-256: $hash"
Write-Output "[OK]   Checksum written to $sidecar"
exit 0
