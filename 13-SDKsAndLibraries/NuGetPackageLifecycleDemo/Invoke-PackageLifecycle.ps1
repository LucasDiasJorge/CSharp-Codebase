<#
.SYNOPSIS
    Executa o ciclo de vida completo de um pacote NuGet: empacotar, publicar em feed
    local, consumir de outro projeto, evoluir a versao e reprovar uma quebra de contrato.

.DESCRIPTION
    Seis cenarios, cada um imprimindo o que aconteceu:
      1. dotnet pack 1.0.0 e o conteudo do .nupkg/.snupkg
      2. publicacao no feed local
      3. o consumidor restaurando e rodando contra o PACOTE
      4. versao 1.1.0 no feed e resolucao de versao flutuante
      5. PackageValidation reprovando a remocao de um metodo publico
      6. limpeza opcional

.PARAMETER SkipBreakingChange
    Pula o cenario 5, que e o mais lento (dois packs adicionais).

.PARAMETER Clean
    Apaga o feed local e os artefatos no fim.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\Invoke-PackageLifecycle.ps1
#>
[CmdletBinding()]
param(
    [switch]$SkipBreakingChange,
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

$library = 'src/Acme.TextKit/Acme.TextKit.csproj'
$consumer = 'src/Acme.TextKit.Consumer/Acme.TextKit.Consumer.csproj'
$feed = Join-Path $PSScriptRoot 'local-feed'

function Write-Section([string]$Title) {
    Write-Host ''
    Write-Host "=== $Title ===" -ForegroundColor Cyan
}

function Invoke-Step([string]$Description, [scriptblock]$Action) {
    Write-Host "  $Description" -ForegroundColor DarkGray
    & $Action
}

# ---------------------------------------------------------------------------
Write-Section '1. Empacotar a versao 1.0.0'

New-Item -ItemType Directory -Force -Path $feed | Out-Null

Invoke-Step 'dotnet pack -c Release -p:Version=1.0.0' {
    dotnet pack $library -c Release -p:Version=1.0.0 -o $feed --nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw 'pack 1.0.0 falhou' }
}

$package = Join-Path $feed 'Acme.TextKit.1.0.0.nupkg'
$symbols = Join-Path $feed 'Acme.TextKit.1.0.0.snupkg'

Write-Host "  .nupkg  : $([math]::Round((Get-Item $package).Length / 1KB, 1)) KB"
Write-Host "  .snupkg : $([math]::Round((Get-Item $symbols).Length / 1KB, 1)) KB  <- simbolos em pacote separado"

Write-Section '2. O que foi de fato para dentro do pacote'

# Um .nupkg e um zip: da para abrir e conferir, e vale conferir.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($package)
try {
    $archive.Entries |
        Where-Object { $_.FullName -notmatch '^(_rels|package)/' -and $_.FullName -ne '[Content_Types].xml' } |
        ForEach-Object { Write-Host "  $($_.FullName)" }

    $hasXmlDoc = $archive.Entries | Where-Object { $_.FullName -like '*.xml' }
    $hasReadme = $archive.Entries | Where-Object { $_.FullName -eq 'README.md' }

    Write-Host ''
    Write-Host "  documentacao XML no pacote: $([bool]$hasXmlDoc)  <- alimenta o IntelliSense de quem consome"
    Write-Host "  README no pacote          : $([bool]$hasReadme)  <- aparece na pagina do NuGet"
}
finally {
    $archive.Dispose()
}

Write-Section '3. O consumidor usa o PACOTE, nao o projeto'

Invoke-Step 'dotnet restore (resolve Acme.TextKit 1.0.0 do feed local)' {
    dotnet restore $consumer --nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw 'restore do consumidor falhou' }
}

Invoke-Step 'dotnet run' {
    dotnet run --project $consumer -c Release --no-restore --nologo
    if ($LASTEXITCODE -ne 0) { throw 'execucao do consumidor falhou' }
}

Write-Section '4. SemVer: 1.1.0 no feed e resolucao de versao'

Invoke-Step 'dotnet pack -p:Version=1.1.0 (mudanca compativel: so adiciona)' {
    dotnet pack $library -c Release -p:Version=1.1.0 -o $feed --nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw 'pack 1.1.0 falhou' }
}

Get-ChildItem $feed -Filter '*.nupkg' | ForEach-Object { Write-Host "  no feed: $($_.Name)" }

Write-Host ''
Write-Host '  versao fixa "1.0.0" no csproj -> NuGet resolve exatamente 1.0.0'

$floatingProbe = Join-Path $env:TEMP "acme-floating-$(Get-Random)"
New-Item -ItemType Directory -Force -Path $floatingProbe | Out-Null
try {
    Copy-Item 'nuget.config' $floatingProbe
    Copy-Item -Recurse 'src/Acme.TextKit.Consumer/*' $floatingProbe
    Remove-Item (Join-Path $floatingProbe 'obj') -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $floatingProbe 'bin') -Recurse -Force -ErrorAction SilentlyContinue

    # O feed e relativo ao nuget.config, que agora esta em outra pasta.
    (Get-Content (Join-Path $floatingProbe 'nuget.config')) -replace 'value="local-feed"', "value=`"$feed`"" |
        Set-Content (Join-Path $floatingProbe 'nuget.config')

    $probeProject = Join-Path $floatingProbe 'Acme.TextKit.Consumer.csproj'
    (Get-Content $probeProject) -replace 'Version="1.0.0"', 'Version="1.*"' | Set-Content $probeProject

    Write-Host '  versao flutuante "1.*" em uma copia do consumidor:'
    dotnet run --project $probeProject -c Release --nologo 2>&1 |
        Select-String 'InformationalVersion|AssemblyVersion' |
        ForEach-Object { Write-Host "   $($_.Line.Trim())" }
}
finally {
    Remove-Item $floatingProbe -Recurse -Force -ErrorAction SilentlyContinue
}

if (-not $SkipBreakingChange) {
    Write-Section '5. PackageValidation reprova a quebra de contrato'

    Write-Host '  compilando 2.0.0 com DefineConstants=BREAKING (remove CountWords)' -ForegroundColor DarkGray
    Write-Host '  e comparando com a baseline 1.0.0 do feed local...' -ForegroundColor DarkGray
    Write-Host ''

    $output = dotnet pack $library -c Release `
        -p:Version=2.0.0 `
        -p:DefineConstants=BREAKING `
        -p:EnablePackageValidation=true `
        -p:PackageValidationBaselineVersion=1.0.0 `
        -o $feed --nologo -v quiet 2>&1

    $violations = $output | Select-String 'PKV\d+|CP\d+' | Select-Object -First 6

    if ($violations) {
        $violations | ForEach-Object { Write-Host "  $($_.Line.Trim())" -ForegroundColor Yellow }
        Write-Host ''
        Write-Host '  O pack FALHOU, e e isso que se quer: remover um metodo publico e' -ForegroundColor Green
        Write-Host '  mudanca MAJOR, e a ferramenta nao deixa publicar como se nao fosse.' -ForegroundColor Green
    }
    else {
        Write-Host '  ATENCAO: o pack nao reportou violacao de compatibilidade.' -ForegroundColor Red
        Write-Host '  Saida completa:' -ForegroundColor Red
        $output | Select-Object -First 15 | ForEach-Object { Write-Host "  $_" }
    }
}

Write-Section 'Resumo'

Write-Host '  1. dotnet pack produz .nupkg e .snupkg com metadados, README e doc XML'
Write-Host '  2. o conteudo do pacote e verificavel: e um zip'
Write-Host '  3. PackageReference prova o pacote; ProjectReference nao prova nada'
Write-Host '  4. versao fixa resolve exata; "1.*" acompanha a maior 1.x do feed'
Write-Host '  5. PackageValidation transforma SemVer em regra de build, nao em promessa'

if ($Clean) {
    Write-Section 'Limpeza'

    Remove-Item $feed -Recurse -Force -ErrorAction SilentlyContinue
    dotnet nuget locals http-cache --clear | Out-Null

    Write-Host '  feed local removido.'
    Write-Host '  ATENCAO: o pacote tambem ficou no cache global (~/.nuget/packages/acme.textkit).'
    Write-Host '  Para removê-lo: Remove-Item ~/.nuget/packages/acme.textkit -Recurse -Force'
}
