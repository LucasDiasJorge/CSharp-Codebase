# CLAUDE.md — ReadmeStandardizer

Utilitário que padroniza os READMEs do repositório em lote. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
# via wrapper (caminho normal de uso):
powershell -ExecutionPolicy Bypass -File .\tools\Standardize-Readmes.ps1

# incluindo o README raiz:
powershell -ExecutionPolicy Bypass -File .\tools\Standardize-Readmes.ps1 -IncludeRootReadme

# direto:
dotnet run --project tools/ReadmeStandardizer/ReadmeStandardizer.csproj -- --root <caminho> [--include-root]
```

## Estrutura interna

`Program.cs` (arquivo único) percorre o repositório a partir de `--root` e aplica as convenções de `docs/CONVENCOES.md` e `docs/README_TEMPLATE.md` aos READMEs encontrados.

`tools/Standardize-Readmes.ps1` é um wrapper fino: resolve a raiz do repositório a partir da própria localização e repassa `--root` (e `--include-root` quando `-IncludeRootReadme` é passado). O README raiz fica **de fora por padrão** — ele tem estrutura própria, com o índice completo de projetos, que a padronização não deve sobrescrever.

## Pontos de atenção

- **Ferramenta destrutiva: reescreve READMEs em lote.** Rode com a árvore de trabalho limpa e revise o diff antes de comitar. Um `git status` limpo antes da execução é o que permite reverter.
- TFM `net9.0`, sem dependências externas.
- **Não faz parte do catálogo didático** — é infraestrutura do repositório. Fica fora das trilhas 01-13 e não entra na contagem de projetos.
- **Sem README local.**
