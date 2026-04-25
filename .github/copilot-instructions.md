# CSharp-101 AI Guide (Lean)

- Repository with independent .NET 9 samples; cada projeto deve ser autoexplicativo com README próprio.
- Preferir build/teste direcionado por csproj; evitar build completo da solução por padrão.
- `Directory.Build.props` centraliza `TargetFramework=net9.0`, `Nullable` e `ImplicitUsings`; não duplicar em cada projeto.
- Para executar amostras, usar `dotnet run --project <caminho-do-csproj>`.
- Seguir as convenções em `/docs/CONVENCOES.md` e o template em `/docs/README_TEMPLATE.md`.
- Manter idioma do arquivo: se estiver em português, documentar em português.
- Novos samples devem incluir README local e atualização do README raiz.
- Alguns projetos usam `src/`; sempre apontar comandos para o csproj concreto.
- Ignorar `bin/` e `obj/` ao revisar diffs.
- Como não há CI ativo, validar localmente build/teste do escopo alterado.

## Notas de Performance
- Usar busca e leitura por escopo (arquivo/pasta específica), evitando varreduras amplas sem filtro.
- Priorizar mudanças mínimas para reduzir custo de revisão e risco de regressão.
- Regras específicas de domínio devem viver em arquivos `.instructions.md` com `applyTo`.
