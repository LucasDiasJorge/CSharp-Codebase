# RedisCacheKeyParams

Exemplo didatico de como reduzir o crescimento de sobrecargas ao compor chaves Redis usando params object[] em C#.

## Visao geral

Este projeto demonstra uma abordagem unica para montar chaves de cache com um unico metodo Join(params object[] parts), capaz de receber qualquer quantidade de argumentos.

A proposta e manter a interface estavel, simplificar manutencao e permitir combinacoes de tipos como string, int, Guid e DateOnly sem expandir o contrato.

## Conceitos abordados

- params object[] para variadic arguments em C#.
- Contrato estavel com interface enxuta para composicao de chaves.
- Classe base abstrata para centralizar validacoes e padrao de separador.
- Flexibilidade para combinar tipos heterogeneos sem inflar a interface.

## Objetivos de aprendizagem

- Entender quando trocar sobrecargas por params object[] em APIs internas.
- Estruturar chaves de cache com namespace e segmentos previsiveis.
- Aplicar validacao de entrada para evitar chaves invalidas.
- Manter flexibilidade para alterar separador sem quebrar a interface principal.

## Estrutura do projeto

```text
RedisCacheKeyParams/
+-- CacheKeys/
|   +-- DefaultRedisCacheKeys.cs
|   \-- RedisCacheKeys.cs
+-- Contracts/
|   \-- IRedisCacheKeys.cs
+-- Program.cs
+-- README.md
\-- RedisCacheKeyParams.csproj
```

## Como executar

```bash
dotnet run --project 06-Caching/Caching/RedisCacheKeyParams/RedisCacheKeyParams.csproj
```

## Boas praticas e pontos de atencao

- Normalize e valide segmentos antes de montar a chave final.
- Defina separador consistente por contexto para facilitar observabilidade e busca.
- Use nomes de segmentos semanticos (ex.: cache, status, tenant, batch).

## Conteudo complementar

Exemplos de composicao no Program.cs:

- cache_user_42
- cache_status_active
- cache_batch_7_v2

## Referencias e documentacao complementar

- [Documentacao oficial de params (C#)](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/method-parameters#params-modifier)
- [Documentacao do string.Join](https://learn.microsoft.com/dotnet/api/system.string.join)
