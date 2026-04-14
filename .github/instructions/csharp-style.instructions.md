---
applyTo: '**/*.{cs,csproj,sln}'
description: 'Padrões C# e .NET do repositório. Use para implementar e revisar código C# com foco didático.'
---

# Padrões C# e .NET

## Regras Obrigatórias
1. Não usar `var`, exceto em LINQ anônimo inevitável.
2. Classes e métodos em PascalCase; parâmetros e variáveis locais em camelCase.
3. Usar `readonly` para dependências injetadas via construtor.
4. Preservar `net9.0`, `Nullable` e `ImplicitUsings` definidos em `Directory.Build.props`.

## Execução e Validação
1. Preferir `dotnet build <caminho-do-csproj>` ao build da solução inteira.
2. Usar `dotnet run --project <caminho-do-csproj>` para execução.
3. Encerrar tarefa somente com build bem-sucedido do projeto alterado.

## Qualidade
1. Aplicar refatorações de baixo risco e alto impacto (extrair método/classe, reduzir condicionais complexas).
2. Evitar mudanças estruturais sem necessidade didática.
3. Manter logs estruturados com `ILogger`.
