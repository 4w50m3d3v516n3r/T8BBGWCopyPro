@echo off
REM  Convert-Image.bat - converts a flux image (e.g. .scp) to a sector image
REM  using "gw.exe convert".
REM
REM  Usage: Convert-Image.bat "image.scp" <format> <target-extension>
REM  Example arguments in the Post-Action editor:
REM      "{ImageFile}" amiga.amigados adf        -> image.adf
REM      "{ImageFile}" ibm.1440 img              -> image.img
REM
REM  Adjust GW below if gw.exe is not on your PATH.
set "GW=gw.exe"

if "%~3"=="" (
    echo Usage: Convert-Image.bat image.scp format target-extension
    exit /b 2
)

echo Converting %~nx1 to %~n1.%3 (format %2) ...
"%GW%" convert --format %2 "%~1" "%~dpn1.%3"
if errorlevel 1 (
    echo [ERROR] gw convert failed with exit code %errorlevel%
    exit /b 1
)
echo Done: %~dpn1.%3
exit /b 0
