@echo off
cd /d "%~dp0"
echo Criando estrutura do projeto InvoiceThrottlingApi...

if not exist "InvoiceThrottlingApi" mkdir "InvoiceThrottlingApi"
if not exist "InvoiceThrottlingApi\Models" mkdir "InvoiceThrottlingApi\Models"
if not exist "InvoiceThrottlingApi\Services" mkdir "InvoiceThrottlingApi\Services"
if not exist "InvoiceThrottlingApi\Controllers" mkdir "InvoiceThrottlingApi\Controllers"
if not exist "InvoiceThrottlingApi\Properties" mkdir "InvoiceThrottlingApi\Properties"

echo.
echo Estrutura criada com sucesso!
echo.
echo Proximos passos:
echo 1. cd InvoiceThrottlingApi
echo 2. dotnet build
echo 3. dotnet run
echo 4. Acesse: https://localhost:5001/swagger
echo.
