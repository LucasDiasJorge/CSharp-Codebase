@echo off
cd /d "%~dp0"

echo Movendo projeto InvoiceThrottlingApi para pasta propria...

REM Criar estrutura de pastas
if not exist "InvoiceThrottlingApi" mkdir "InvoiceThrottlingApi"
if not exist "InvoiceThrottlingApi\Models" mkdir "InvoiceThrottlingApi\Models"
if not exist "InvoiceThrottlingApi\Services" mkdir "InvoiceThrottlingApi\Services"
if not exist "InvoiceThrottlingApi\Controllers" mkdir "InvoiceThrottlingApi\Controllers"
if not exist "InvoiceThrottlingApi\Properties" mkdir "InvoiceThrottlingApi\Properties"

REM Mover arquivos
if exist "InvoiceThrottlingApi.csproj" move /Y "InvoiceThrottlingApi.csproj" "InvoiceThrottlingApi\"
if exist "InvoiceModels.cs" move /Y "InvoiceModels.cs" "InvoiceThrottlingApi\Models\Invoice.cs"
if exist "InvoiceGenerator.cs" move /Y "InvoiceGenerator.cs" "InvoiceThrottlingApi\Services\InvoiceGenerator.cs"
if exist "InvoiceProcessor.cs" move /Y "InvoiceProcessor.cs" "InvoiceThrottlingApi\Services\InvoiceProcessor.cs"
if exist "InvoiceController.cs" move /Y "InvoiceController.cs" "InvoiceThrottlingApi\Controllers\InvoiceController.cs"
if exist "InvoiceProgram.cs" move /Y "InvoiceProgram.cs" "InvoiceThrottlingApi\Program.cs"
if exist "InvoiceAppSettings.json" move /Y "InvoiceAppSettings.json" "InvoiceThrottlingApi\appsettings.json"
if exist "INVOICE_THROTTLING_README.md" move /Y "INVOICE_THROTTLING_README.md" "InvoiceThrottlingApi\README.md"

REM Criar launchSettings.json
echo {> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo   "$schema": "http://json.schemastore.org/launchsettings.json",>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo   "profiles": {>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo     "http": {>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "commandName": "Project",>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "dotnetRunMessages": true,>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "launchBrowser": true,>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "launchUrl": "swagger",>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "applicationUrl": "http://localhost:5000",>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "environmentVariables": {>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo         "ASPNETCORE_ENVIRONMENT": "Development">> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       }>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo     },>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo     "https": {>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "commandName": "Project",>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "dotnetRunMessages": true,>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "launchBrowser": true,>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "launchUrl": "swagger",>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "applicationUrl": "https://localhost:5001;http://localhost:5000",>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       "environmentVariables": {>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo         "ASPNETCORE_ENVIRONMENT": "Development">> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo       }>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo     }>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo   }>> "InvoiceThrottlingApi\Properties\launchSettings.json"
echo }>> "InvoiceThrottlingApi\Properties\launchSettings.json"

echo.
echo ======================================
echo Projeto organizado com sucesso!
echo ======================================
echo.
echo Para executar:
echo   cd InvoiceThrottlingApi
echo   dotnet run
echo.
echo Acesse: https://localhost:5001/swagger
echo.

del setup_invoice.py 2>nul

pause
