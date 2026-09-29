; Regras ainda nao publicadas em versao. O analisador RS2008 exige este arquivo:
; e ele que permite rastrear em qual versao cada diagnostico foi introduzido ou mudou
; de severidade — informacao que quem consome o analisador precisa.
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|---------------------|----------|-------------------------------------------------
AEE001  | Acme.EnumExtensions | Warning  | Enum inacessivel para geracao de extensoes
