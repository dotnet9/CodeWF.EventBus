@echo off
setlocal

set "ROOT=%~dp0"
set "CONFIGURATION=Release"
set "PACKAGE_DIR=%ROOT%artifacts\packages"

if not exist "%PACKAGE_DIR%" mkdir "%PACKAGE_DIR%"
del /q "%PACKAGE_DIR%\*.nupkg" 2>nul
del /q "%PACKAGE_DIR%\*.snupkg" 2>nul

for %%P in (
    "%ROOT%src\CodeWF.EventBus\CodeWF.EventBus.csproj"
    "%ROOT%src\CodeWF.IOC.EventBus\CodeWF.IOC.EventBus.csproj"
    "%ROOT%src\CodeWF.DryIoc.EventBus\CodeWF.DryIoc.EventBus.csproj"
    "%ROOT%src\CodeWF.AspNetCore.EventBus\CodeWF.AspNetCore.EventBus.csproj"
) do (
    echo [CodeWF.EventBus] Restore %%~nxP...
    dotnet restore "%%~P"
    if errorlevel 1 goto :failed
    echo [CodeWF.EventBus] Build %%~nxP...
    dotnet build "%%~P" -c "%CONFIGURATION%" --no-restore -p:GeneratePackageOnBuild=false
    if errorlevel 1 goto :failed
    echo [CodeWF.EventBus] Pack %%~nxP...
    dotnet pack "%%~P" -c "%CONFIGURATION%" --no-build --no-restore -o "%PACKAGE_DIR%"
    if errorlevel 1 goto :failed
)

for /r "%PACKAGE_DIR%" %%F in (*.pdb) do del /q "%%F" 2>nul

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
