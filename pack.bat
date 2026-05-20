@echo off
setlocal

set "ROOT=%~dp0"
set "SOLUTION=%ROOT%CodeWF.EventBus.slnx"
set "CONFIGURATION=Release"
set "PACKAGE_DIR=%ROOT%Output\NuGet"

echo [CodeWF.EventBus] Restore packages...
dotnet restore "%SOLUTION%"
if errorlevel 1 goto :failed

if not exist "%PACKAGE_DIR%" mkdir "%PACKAGE_DIR%"
del /q "%PACKAGE_DIR%\*.nupkg" 2>nul
del /q "%PACKAGE_DIR%\*.snupkg" 2>nul

echo [CodeWF.EventBus] Build and pack %CONFIGURATION% packages...
dotnet build "%SOLUTION%" -c "%CONFIGURATION%" --no-restore -p:PackageOutputPath="%PACKAGE_DIR%"
if errorlevel 1 goto :failed

echo.
echo [CodeWF.EventBus] Packages:
dir /b "%PACKAGE_DIR%\*.nupkg"
echo.
echo [CodeWF.EventBus] Done. Output: %PACKAGE_DIR%
exit /b 0

:failed
echo.
echo [CodeWF.EventBus] Pack failed.
exit /b 1
