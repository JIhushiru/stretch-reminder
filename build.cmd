@echo off
rem Publish a single-file exe (framework-dependent: uses the installed .NET 8 runtime) to .\dist
dotnet publish StretchReminder -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o dist
if %errorlevel% neq 0 exit /b %errorlevel%
echo.
echo Done. Run dist\StretchReminder.exe
