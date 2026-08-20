<#
.SYNOPSIS
    Audita a consistência do catálogo do CSharp-Codebase.

.DESCRIPTION
    Compara o que existe em disco com o que está declarado no README raiz, na solução e nos
    arquivos de documentação por projeto. Não altera nada — apenas relata.

    Verificações:
      A. .csproj em disco sem entrada no Índice Completo de Projetos do README raiz
      B. Entradas do índice sem pasta correspondente em disco
      C. Contadores declarados por categoria vs. contagem real
      D. .csproj ausentes da CSharp-Codebase.sln
      E. .csproj sem elemento <TargetFramework>
      F. Pastas de projeto sem README.md ou sem CLAUDE.md
      G. Ocorrências de "var" por projeto (heurística)

.PARAMETER RepoRoot
    Raiz do repositório. Padrão: dois níveis acima da pasta do script.

.PARAMETER SkipVarScan
    Pula a verificação G, que é a mais lenta.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\.claude\skills\auditar-catalogo\scripts\Audit-Catalog.ps1

.NOTES
    Compatível com Windows PowerShell 5.1.
    Código de saída 1 quando há achados acionáveis nas seções A, D ou E.
#>
[CmdletBinding()]
param(
    [string] $RepoRoot,
    [switch] $SkipVarScan
)

$ErrorActionPreference = 'Stop'

if (-not $RepoRoot) {
    $RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')
}
$RepoRoot = (Resolve-Path $RepoRoot).Path

$readmePath = Join-Path $RepoRoot 'README.md'
$slnPath = Join-Path $RepoRoot 'CSharp-Codebase.sln'

if (-not (Test-Path $readmePath)) { throw "README raiz não encontrado em $readmePath" }
if (-not (Test-Path $slnPath))   { throw "Solução não encontrada em $slnPath" }

function Write-Section {
    param([string] $Title)
    Write-Host ''
    Write-Host ('=' * 78) -ForegroundColor DarkGray
    Write-Host $Title -ForegroundColor Cyan
    Write-Host ('=' * 78) -ForegroundColor DarkGray
}

function Write-Finding {
    param([string] $Text)
    Write-Host "  ! $Text" -ForegroundColor Yellow
}

function Write-Ok {
    param([string] $Text)
    Write-Host "  ok $Text" -ForegroundColor Green
}

# ---------------------------------------------------------------- disco ----
Write-Host "Auditando: $RepoRoot" -ForegroundColor White

$allCsproj = Get-ChildItem -Path $RepoRoot -Recurse -Filter '*.csproj' -File |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

$projects = foreach ($proj in $allCsproj) {
    $relative = $proj.FullName.Substring($RepoRoot.Length + 1).Replace('\', '/')
    $track = ($relative -split '/')[0]
    [pscustomobject]@{
        Name       = [System.IO.Path]::GetFileNameWithoutExtension($proj.Name)
        FolderName = $proj.Directory.Name
        Directory  = $proj.Directory.FullName
        Relative   = $relative
        Track      = $track
        FullName   = $proj.FullName
    }
}

Write-Host ("  {0} arquivos .csproj encontrados em {1} trilhas" -f `
    $projects.Count, ($projects.Track | Sort-Object -Unique).Count)

# --------------------------------------------------------- README raiz ----
$readmeLines = Get-Content -Path $readmePath -Encoding UTF8

$inIndex = $false
$currentCategory = $null
$categories = @{}
$indexEntries = New-Object System.Collections.ArrayList

# Regex tolerante a acento: o '.' cobre o 'Í' independente de como o arquivo foi decodificado.
$indexHeaderPattern = '^###\s+.ndice\s+Completo\s+de\s+Projetos'

foreach ($line in $readmeLines) {
    if ($line -match $indexHeaderPattern) { $inIndex = $true; continue }
    if ($inIndex -and $line -match '^###\s+' -and $line -notmatch $indexHeaderPattern) { break }
    if (-not $inIndex) { continue }

    if ($line -match '^####\s+(.+?)\s*\((\d+)\s+projetos?\)\s*$') {
        $currentCategory = $Matches[1].Trim()
        $categories[$currentCategory] = [pscustomobject]@{
            Declared = [int]$Matches[2]
            Entries  = New-Object System.Collections.ArrayList
        }
        continue
    }

    if ($currentCategory -and $line -match '^\s*-\s+`([^`]+)`') {
        $entryName = $Matches[1].Trim()
        [void]$categories[$currentCategory].Entries.Add($entryName)
        [void]$indexEntries.Add($entryName)
    }
}

Write-Host ("  {0} categorias e {1} entradas lidas do índice do README raiz" -f `
    $categories.Count, $indexEntries.Count)

# Nomes citados no índice, normalizados (aceita "Caching/RedisMetaData")
$indexNames = @{}
foreach ($entry in $indexEntries) {
    $leaf = ($entry -split '/')[-1]
    $indexNames[$leaf.ToLowerInvariant()] = $true
    $indexNames[$entry.ToLowerInvariant()] = $true
}

# ------------------------------------------------------------- solução ----
$slnContent = Get-Content -Path $slnPath -Raw
$slnPaths = @{}
foreach ($match in [regex]::Matches($slnContent, '"([^"]+\.csproj)"')) {
    $slnPaths[$match.Groups[1].Value.Replace('\', '/').ToLowerInvariant()] = $true
}

# ================================================================ A =========
Write-Section 'A. Projetos em disco ausentes do índice do README raiz'
$missingFromIndex = @()
foreach ($project in $projects) {
    $byName = $indexNames.ContainsKey($project.Name.ToLowerInvariant())
    $byFolder = $indexNames.ContainsKey($project.FolderName.ToLowerInvariant())
    if (-not $byName -and -not $byFolder) {
        $missingFromIndex += $project
    }
}
if ($missingFromIndex.Count -eq 0) {
    Write-Ok 'todo .csproj tem entrada correspondente no índice'
} else {
    foreach ($project in ($missingFromIndex | Sort-Object Relative)) {
        Write-Finding $project.Relative
    }
    Write-Host ("  -> {0} projeto(s) sem entrada. Adicione em '#### {1}' do README raiz." -f `
        $missingFromIndex.Count, 'NN-Categoria')
}

# ================================================================ B =========
Write-Section 'B. Entradas do índice sem projeto correspondente em disco'
$diskNames = @{}
foreach ($project in $projects) {
    $diskNames[$project.Name.ToLowerInvariant()] = $true
    $diskNames[$project.FolderName.ToLowerInvariant()] = $true
}
$orphanEntries = @()
foreach ($entry in ($indexEntries | Sort-Object -Unique)) {
    $leaf = ($entry -split '/')[-1]
    if (-not $diskNames.ContainsKey($leaf.ToLowerInvariant())) {
        # pode ser pasta agrupadora (ex.: `Kafka`, `Caching`) — só reportar se nem pasta existir
        $asFolder = Get-ChildItem -Path $RepoRoot -Recurse -Directory -Filter $leaf -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } | Select-Object -First 1
        if (-not $asFolder) { $orphanEntries += $entry }
    }
}
if ($orphanEntries.Count -eq 0) {
    Write-Ok 'toda entrada do índice tem pasta ou projeto em disco'
} else {
    foreach ($entry in $orphanEntries) {
        Write-Finding "'$entry' está no índice mas não existe em disco"
    }
}

# ================================================================ C =========
Write-Section 'C. Contadores declarados por categoria'
Write-Host '  (declarado = titulo do README | entradas = itens listados | csproj = arquivos em disco)'
Write-Host ''
foreach ($categoryName in ($categories.Keys | Sort-Object)) {
    $category = $categories[$categoryName]
    $trackKey = ($categoryName -split ' ')[0]
    # @() e obrigatorio: em PS 5.1 um unico objeto retornado por Where-Object nao expoe .Count.
    $csprojCount = @($projects | Where-Object { $_.Track -ieq $trackKey }).Count
    $entryCount = $category.Entries.Count
    $flag = ' '
    if ($category.Declared -ne $entryCount) { $flag = '!' }
    $color = 'Gray'
    if ($flag -eq '!') { $color = 'Yellow' }
    Write-Host ("  {0} {1,-28} declarado={2,-4} entradas={3,-4} csproj={4}" -f `
        $flag, $categoryName, $category.Declared, $entryCount, $csprojCount) -ForegroundColor $color
}
Write-Host ''
Write-Host '  Divergencia declarado/entradas e erro de contador — corrija o titulo.' -ForegroundColor DarkGray
Write-Host '  Divergencia entradas/csproj costuma ser legitima (pasta agrupadora,' -ForegroundColor DarkGray
Write-Host '  subprojetos aninhados, item sem .csproj). Confira antes de "corrigir".' -ForegroundColor DarkGray

# ================================================================ D =========
Write-Section 'D. Projetos ausentes da CSharp-Codebase.sln'
$missingFromSln = @()
foreach ($project in $projects) {
    if (-not $slnPaths.ContainsKey($project.Relative.ToLowerInvariant())) {
        $missingFromSln += $project
    }
}
if ($missingFromSln.Count -eq 0) {
    Write-Ok 'todo .csproj esta registrado na solucao'
} else {
    foreach ($project in ($missingFromSln | Sort-Object Relative)) {
        Write-Finding $project.Relative
    }
    Write-Host ''
    Write-Host '  Registrar com:' -ForegroundColor DarkGray
    foreach ($project in ($missingFromSln | Sort-Object Relative)) {
        Write-Host ("    dotnet sln CSharp-Codebase.sln add {0}" -f $project.Relative) -ForegroundColor DarkGray
    }
}

# ================================================================ E =========
Write-Section 'E. Projetos sem <TargetFramework> (NETSDK1013)'
$missingTfm = @()
foreach ($project in $projects) {
    $content = Get-Content -Path $project.FullName -Raw
    if ($content -notmatch '<TargetFramework(s)?>') {
        $missingTfm += $project
    }
}
if ($missingTfm.Count -eq 0) {
    Write-Ok 'todo .csproj declara TargetFramework'
} else {
    foreach ($project in ($missingTfm | Sort-Object Relative)) {
        Write-Finding ("{0}  (build falha com NETSDK1013)" -f $project.Relative)
    }
    Write-Host '  -> ver .claude/skills/consertar-build/SKILL.md' -ForegroundColor DarkGray
}

# ================================================================ F =========
Write-Section 'F. Documentacao por projeto'
$noReadme = @()
$noClaude = @()
foreach ($project in $projects) {
    if (-not (Test-Path (Join-Path $project.Directory 'README.md'))) { $noReadme += $project }
    if (-not (Test-Path (Join-Path $project.Directory 'CLAUDE.md'))) { $noClaude += $project }
}

Write-Host ("  Sem README.md local: {0}" -f $noReadme.Count)
foreach ($project in ($noReadme | Sort-Object Relative)) {
    # Sobe ate a raiz da trilha procurando um README que cubra este subprojeto.
    $note = ''
    $ancestor = Split-Path $project.Directory -Parent
    while ($ancestor -and $ancestor.Length -gt $RepoRoot.Length) {
        if (Test-Path (Join-Path $ancestor 'README.md')) {
            $coveredBy = $ancestor.Substring($RepoRoot.Length + 1).Replace('\', '/')
            $note = "  [coberto por $coveredBy/README.md]"
            break
        }
        $ancestor = Split-Path $ancestor -Parent
    }
    Write-Host ("    - {0}{1}" -f $project.Relative, $note) -ForegroundColor Yellow
}
Write-Host ''
Write-Host ("  Sem CLAUDE.md local: {0}" -f $noClaude.Count)
foreach ($project in ($noClaude | Sort-Object Relative)) {
    Write-Host ("    - {0}" -f $project.Relative) -ForegroundColor Yellow
}

# ================================================================ G =========
if (-not $SkipVarScan) {
    Write-Section 'G. Ocorrencias de "var" (heuristica — revisar manualmente)'
    $varPattern = '(^|[^A-Za-z0-9_])var\s+[a-zA-Z_]'
    $varByProject = @{}
    $totalVar = 0
    foreach ($project in $projects) {
        $csFiles = Get-ChildItem -Path $project.Directory -Recurse -Filter '*.cs' -File -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
        if (-not $csFiles) { continue }
        $hits = $csFiles | Select-String -Pattern $varPattern -AllMatches -ErrorAction SilentlyContinue
        $count = 0
        if ($hits) { $count = @($hits).Count }
        if ($count -gt 0) {
            $varByProject[$project.Relative] = $count
            $totalVar += $count
        }
    }
    Write-Host ("  {0} ocorrencias em {1} projeto(s)" -f $totalVar, $varByProject.Count)
    Write-Host '  Top 15:'
    $varByProject.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 15 | ForEach-Object {
        Write-Host ("    {0,5}  {1}" -f $_.Value, $_.Key) -ForegroundColor Yellow
    }
    Write-Host ''
    Write-Host '  Tipo anonimo de LINQ e a unica excecao permitida; comentarios e strings' -ForegroundColor DarkGray
    Write-Host '  sao falsos positivos desta heuristica.' -ForegroundColor DarkGray
} else {
    Write-Section 'G. Ocorrencias de "var" — pulado (-SkipVarScan)'
}

# ================================================================ resumo ===
Write-Section 'Resumo'
Write-Host ("  csproj em disco ................ {0}" -f $projects.Count)
Write-Host ("  ausentes do indice (A) ......... {0}" -f $missingFromIndex.Count)
Write-Host ("  entradas orfas do indice (B) ... {0}" -f $orphanEntries.Count)
Write-Host ("  ausentes da solucao (D) ........ {0}" -f $missingFromSln.Count)
Write-Host ("  sem TargetFramework (E) ........ {0}" -f $missingTfm.Count)
Write-Host ("  sem README local (F) ........... {0}" -f $noReadme.Count)
Write-Host ("  sem CLAUDE.md local (F) ........ {0}" -f $noClaude.Count)
Write-Host ''

$hardFindings = $missingFromIndex.Count + $missingFromSln.Count + $missingTfm.Count
if ($hardFindings -gt 0) {
    Write-Host ("Achados acionaveis: {0}" -f $hardFindings) -ForegroundColor Yellow
    exit 1
}
Write-Host 'Catalogo consistente nas verificacoes A, D e E.' -ForegroundColor Green
exit 0
