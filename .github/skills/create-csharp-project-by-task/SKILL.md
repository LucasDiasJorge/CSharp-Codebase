---
name: create-csharp-project-by-task
description: 'Criar projeto C# no padrao do repositorio CSharp-101 a partir do input da tarefa. Use quando pedir novo projeto/sample, escolher sub-repo 01-13, atualizar README local e README raiz, proibir var e concluir somente apos build de sucesso do csproj criado.'
argument-hint: 'Descreva a tarefa/projeto a implementar (tema, tipo de app e objetivo didatico).'
user-invocable: true
---

# Criar Projeto C# no Padrao do Repositorio

## Quando Usar
- Solicitacoes como "crie um projeto C#", "novo sample" ou "adicionar projeto no CSharp-101".
- Tarefas que exigem alocacao correta em sub-repo numerado (01 a 13).
- Demandas com documentacao obrigatoria e criterio de done por build.

## Entradas Obrigatorias
- Descricao da tarefa/projeto a implementar.
- Tipo de projeto (console, webapi, classlib, worker, library, etc.).
- Resultado didatico esperado (conceitos que o exemplo deve ensinar).

## Regras Obrigatorias
1. Nunca usar var, exceto em casos de LINQ anonimo inevitavel.
2. Seguir convencoes de codigo e estrutura do repositorio.
3. Documentar sempre:
   - README.md do projeto criado/alterado.
   - README.md raiz com o novo projeto na categoria correta.
4. Considerar done somente apos build bem-sucedido do csproj alvo.

## Decisao de Categoria (Sub-repo)
Para tarefas de base, priorize a alocacao correta entre 01-Fundamentals, 02-AsyncAndConcurrency e 03-WebAPIs antes de avaliar as demais categorias.

Mapeie a tarefa para a pasta tematica correta:
- 01-Fundamentals: sintaxe, OOP base, LINQ basico, delegates, reflection.
- 02-AsyncAndConcurrency: async/await, tasks, threads, sincronizacao.
- 03-WebAPIs: APIs REST, Minimal API, gRPC, middleware/filtros web.
- 04-Authentication: authn/authz, JWT, OAuth, seguranca.
- 05-Messaging: Kafka, RabbitMQ, filas/eventos.
- 06-Caching: Redis, cache patterns, cache distribuido.
- 07-DesignPatterns: GoF, SOLID, code smells, DDD tatico.
- 08-ArchitecturalPatterns: CQRS, Saga, circuit breaker, use cases.
- 09-Data: EF, Dapper, SQL, NoSQL, persistencia.
- 10-Algorithms: algoritmos e estruturas de dados.
- 11-Utilities: serializacao, arquivos, tooling utilitario.
- 12-Testing: testes automatizados, benchmark, validacao de regras.
- 13-SDKsAndLibraries: SDKs, packages e bibliotecas reutilizaveis.

Se houver ambiguidade, priorize a categoria que melhor representa o conceito principal e registre a justificativa.

## Procedimento
1. Ler as referencias:
   - [Convencoes](../../../docs/CONVENCOES.md)
   - [Template de README](../../../docs/README_TEMPLATE.md)
   - [Instrucoes Globais](../../copilot-instructions.md)
2. Definir categoria e caminho final do projeto dentro da pasta NN-NomeCategoria.
3. Criar o projeto no formato adequado ao objetivo (dotnet new ...), mantendo estrutura didatica e nomes claros.
4. Adicionar o projeto a solucao quando aplicavel (dotnet sln add ...).
5. Implementar codigo com tipos explicitos e sem uso indevido de var.
6. Validar uso de var por busca textual e revisar excecoes inevitaveis.
7. Criar/atualizar o README.md local do projeto usando o template oficial.
8. Atualizar o README.md raiz, incluindo o projeto na tabela da categoria correta com descricao curta.
9. Executar build do projeto criado/alterado:
   - dotnet build <caminho-do-csproj>
   - Nao usar build completo da solucao como criterio de done.
10. Marcar como concluido apenas apos passar em todas as verificacoes.

## Checklist de Conclusao
- [ ] Projeto criado na categoria correta (01 a 13).
- [ ] Sub-repo 01, 02 ou 03 escolhido corretamente quando aplicavel.
- [ ] Sem uso indevido de var.
- [ ] README.md local atualizado.
- [ ] README.md raiz atualizado.
- [ ] dotnet build do csproj alvo executado com sucesso.
- [ ] Relatorio final inclui caminhos alterados e comando de build utilizado.

## Saida Esperada
Relato final objetivo contendo:
- Categoria escolhida e justificativa.
- Arquivos criados/alterados.
- Evidencia do build de sucesso do projeto alvo.
- Pendencias restantes (se houver).

## Boas Praticas
- Preferir alteracoes minimas e alinhadas ao estilo da pasta-alvo.
- Em caso de incerteza entre categorias, confirmar antes de mover/adicionar muitos arquivos.
- Se houver erro de build, corrigir e reexecutar build antes de declarar done.
