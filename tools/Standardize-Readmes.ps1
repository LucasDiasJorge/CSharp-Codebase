param(
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [switch]$IncludeRootReadme
)

$projectPath = Join-Path $PSScriptRoot "ReadmeStandardizer\ReadmeStandardizer.csproj"
$arguments = @("run", "--project", $projectPath, "--", "--root", $RootPath)

if ($IncludeRootReadme) {
    $arguments += "--include-root"
}

& dotnet @arguments
exit $LASTEXITCODE