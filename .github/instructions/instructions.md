---
description: "Diretrizes globais e leves do CSharp-101 para manter consistencia com baixo custo de contexto."
---

# CSharp-101 - Instruções Globais

## Fonte de Verdade
- /docs/CONVENCOES.md
- /docs/README_TEMPLATE.md

## Regras Obrigatórias
1. Não usar var, exceto em LINQ anônimo inevitável.
2. Usar nomes consistentes: classes e métodos em PascalCase; parâmetros e variáveis locais em camelCase.
3. Usar readonly para dependências injetadas por construtor.
4. Atualizar README local e README raiz quando criar/alterar exemplos didáticos.
5. Concluir a tarefa somente após build bem-sucedido do csproj alvo.

## Refatoração e Entrega
1. Priorizar refatorações de alto impacto e baixo risco: extrair método, extrair classe e reduzir condicionais complexas.
2. Explicar mudanças de forma objetiva: problema, alteração e benefício.
3. Evitar exemplos longos desnecessários quando um diff pequeno resolve.

## Performance Operacional
1. Preferir comandos direcionados (dotnet build <projeto>.csproj) e evitar build da solução inteira por padrão.
2. Ler somente arquivos necessários para a tarefa, com buscas específicas por pasta/arquivo.
3. Reutilizar instruções por escopo (applyTo) para evitar contexto global excessivo.
