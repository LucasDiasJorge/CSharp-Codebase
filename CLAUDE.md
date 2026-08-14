# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visão geral

Catálogo educacional com ~125 projetos `.csproj` independentes, organizados em 13 trilhas temáticas numeradas (`01-Fundamentals` a `13-SDKsAndLibraries`) mais `tools/`. **Não há código compartilhado entre trilhas**: cada projeto é autocontido, tem README próprio e é estudado/executado isoladamente. `CSharp-Codebase.sln` funciona como índice do workspace, não como unidade de build.

Documentação é parte do artefato: um sample sem README local e sem entrada no README raiz é considerado incompleto.

## Comandos

Sempre direcionar ao `.csproj` concreto — **nunca** usar build/test da solução inteira como critério de conclusão (muitos projetos exigem serviços externos ou frameworks fora de suporte, e alguns estão quebrados; ver "Armadilhas").

```bash
dotnet build 01-Fundamentals/YieldReturnDemo/YieldReturnDemo.csproj
dotnet run   --project 03-WebAPIs/MinimalApiDemo/MinimalApiDemo.csproj
dotnet test  12-Testing/OrderRuleConsole/OrderRuleConsole.Tests/OrderRuleConsole.Tests.csproj

# Um único teste / subconjunto
dotnet test <caminho-tests.csproj> --filter "FullyQualifiedName~NomeDoTeste"

# Registrar projeto novo na solução
dotnet sln CSharp-Codebase.sln add <caminho-do-csproj>
dotnet sln CSharp-Codebase.sln list

# Padronizar READMEs em lote (usa tools/ReadmeStandardizer)
powershell -ExecutionPolicy Bypass -File .\tools\Standardize-Readmes.ps1
```

Existem apenas 3 projetos de teste, todos xUnit: `12-Testing/OrderRuleConsole/OrderRuleConsole.Tests` (net9.0), `09-Data/Data/sqlite-sample-api.Tests` (net6.0), `13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk.Tests` (net5.0).

Não há CI, `.editorconfig` nem `global.json` — a validação é local, no escopo alterado.

## Armadilhas conhecidas

- **`Directory.Build.props` não existe mais** (removido no commit `50763d5`), mas `.github/copilot-instructions.md` ainda o cita como fonte central de `TargetFramework`/`Nullable`/`ImplicitUsings`. Consequência real: 6 projetos que omitem `TargetFramework` confiando nessa herança **falham com `NETSDK1013`** — `01-Fundamentals/LogicalOperatorsDemo`, `06-Caching/Caching/{CacheAside,CacheIncrement,CachePatterns}`, `07-DesignPatterns/SOLIDExamples`, `09-Data/Data/MongoUserApi`. Ao criar projeto novo, declarar `TargetFramework`, `Nullable` e `ImplicitUsings` explicitamente no `.csproj`.
- **Target frameworks são heterogêneos**, não uniformes: net9.0 (maioria), net10.0, net8.0, net7.0, net6.0, net5.0, netstandard2.0 e net10.0-windows. O SDK instalado é 10.0.302. Preservar o TFM existente ao editar um projeto; não "modernizar" sem pedido.
- **Layout varia**: alguns projetos usam `src/` (`GrpcSample`, `UnifiedCacheSdk`, `MySimpleSdk`, `AtomicOperationsDemo`, `ClassToDTO`, `DapperExample`, `MoneyStorageApi`, `MysqlExample`). Localizar o `.csproj` real antes de montar comandos.
- **Serviços externos**: projetos de `05-Messaging`, `06-Caching` e `09-Data` exigem Kafka, RabbitMQ, Redis, MySQL, PostgreSQL ou MongoDB ativos. `docker-compose.yml` disponível em `05-Messaging/Kafka/` e `06-Caching/Caching/CacheIncrement/`.
- Ignorar `bin/` e `obj/` ao revisar diffs e ao buscar ocorrências de `var`.

## Convenções de código (obrigatórias)

Definidas em `.github/instructions/csharp-style.instructions.md`:

1. **Não usar `var`** — exceto quando um tipo anônimo de LINQ tornar o tipo explícito impossível.
2. Classes e métodos em PascalCase; parâmetros e variáveis locais em camelCase.
3. `readonly` para dependências injetadas via construtor.
4. Logs estruturados com `ILogger`.
5. Manter o exemplo focado no conceito ensinado — sem infraestrutura ou abstração sem finalidade didática.
6. Concluir tarefa somente após `dotnet build` bem-sucedido do `.csproj` alterado.

## Convenções de documentação

Fontes de verdade: `docs/CONVENCOES.md` e `docs/README_TEMPLATE.md`.

- Idioma padrão Português-BR; termos técnicos em inglês quando mais reconhecíveis. Manter o idioma do arquivo editado.
- README local segue a ordem fixa: Título → Visão geral → Conceitos abordados → Objetivos de aprendizagem → Estrutura do projeto → Como executar → Boas práticas e pontos de atenção → Conteúdo complementar → Referências.
- Árvore de estrutura curta, omitindo `bin/`, `obj/`, `.git/`, `.vs/`. Comandos sempre com o caminho real do `.csproj`, sem placeholders do template.
- Ao adicionar projeto: incluir `- \`NomeProjeto\` - descrição curta` na categoria correta do **Índice Completo de Projetos** do README raiz, e atualizar o contador do título da categoria e o total geral.

## Skill do repositório

`.github/skills/create-csharp-project-by-task/SKILL.md` define o fluxo completo para criar um sample novo (escolha da categoria 01-13 pelo conceito principal ensinado — não pelo template —, criação, registro na `.sln`, README local, atualização do README raiz e validação). Seguir esse arquivo ao criar projetos, em vez de improvisar o processo.
