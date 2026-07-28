@echo off
REM  Extract-Rar.bat - extracts a RAR archive with unrar.exe
REM
REM  Requires unrar.exe (https://www.rarlab.com/rar_add.htm) - either on PATH
REM  or adjust the UNRAR variable below.
REM
REM  Post-Action setup:
REM    Type:      Batch Script
REM    File:      C:\...\scripts\postactions\Extract-Rar.bat
REM    Arguments: "{ImageFile}" "D:\Extracted"
REM
set "UNRAR=unrar.exe"

if "%~1"=="" (
    echo Usage: Extract-Rar.bat archive.rar [destination]
    exit /b 2
)

set "DEST=%~2"
if "%DEST%"=="" set "DEST=%~dp1%~n1"
if not exist "%DEST%" mkdir "%DEST%"

echo === Listing %~nx1 ===
"%UNRAR%" l "%~1"

echo === Extracting to %DEST% ===
"%UNRAR%" x -y -o+ "%~1" "%DEST%\"
if errorlevel 1 (
    echo [ERROR] unrar failed with exit code %errorlevel%
    exit /b 1
)
echo Done.
exit /b 0
