@echo off
echo ===================================
echo Advanced Auth System - Setup
echo ===================================
echo.

REM Create directory structure
mkdir AdvancedAuthSystem 2>nul
cd AdvancedAuthSystem

mkdir Models 2>nul
mkdir DTOs 2>nul
mkdir DTOs\Auth 2>nul
mkdir DTOs\User 2>nul
mkdir DTOs\Resource 2>nul
mkdir Services 2>nul
mkdir Authorization 2>nul
mkdir Authorization\Policies 2>nul
mkdir Authorization\Requirements 2>nul
mkdir Authorization\Handlers 2>nul
mkdir Controllers 2>nul
mkdir Data 2>nul
mkdir Properties 2>nul

echo Directory structure created.
echo.

REM Move files to correct locations
echo Moving files to correct locations...

move /Y ..\AdvancedAuthSystem.csproj AdvancedAuthSystem.csproj >nul 2>&1
move /Y ..\AdvancedAuthSystem.Program.cs Program.cs >nul 2>&1
move /Y ..\AdvancedAuthSystem.appsettings.json appsettings.json >nul 2>&1
move /Y ..\AdvancedAuthSystem.appsettings.Development.json appsettings.Development.json >nul 2>&1
move /Y ..\AdvancedAuthSystem.launchSettings.json Properties\launchSettings.json >nul 2>&1
move /Y ..\AdvancedAuthSystem.README.md README.md >nul 2>&1
move /Y ..\AdvancedAuthSystem.http AdvancedAuthSystem.http >nul 2>&1

move /Y ..\AdvancedAuthSystem.User.cs Models\Models.cs >nul 2>&1
move /Y ..\AdvancedAuthSystem.DTOs.cs DTOs\DTOs.cs >nul 2>&1
move /Y ..\AdvancedAuthSystem.AppDbContext.cs Data\AppDbContext.cs >nul 2>&1

move /Y ..\AdvancedAuthSystem.PasswordHasher.cs Services\PasswordHasher.cs >nul 2>&1
move /Y ..\AdvancedAuthSystem.TokenService.cs Services\TokenService.cs >nul 2>&1
move /Y ..\AdvancedAuthSystem.AuthService.cs Services\AuthService.cs >nul 2>&1

move /Y ..\AdvancedAuthSystem.PolicyNames.cs Authorization\Policies\PolicyNames.cs >nul 2>&1
move /Y ..\AdvancedAuthSystem.Requirements.cs Authorization\Requirements\Requirements.cs >nul 2>&1
move /Y ..\AdvancedAuthSystem.Handlers.cs Authorization\Handlers\Handlers.cs >nul 2>&1

move /Y ..\AdvancedAuthSystem.AuthController.cs Controllers\AuthController.cs >nul 2>&1
move /Y ..\AdvancedAuthSystem.ResourceController.cs Controllers\ResourceController.cs >nul 2>&1

cd ..

echo.
echo ===================================
echo Files organized successfully!
echo ===================================
echo.
echo To build and run the project:
echo   cd AdvancedAuthSystem
echo   dotnet restore
echo   dotnet build
echo   dotnet run
echo.
echo Access Swagger at: http://localhost:5000
echo.
pause
