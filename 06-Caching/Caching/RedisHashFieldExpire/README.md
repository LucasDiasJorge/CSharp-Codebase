# RedisHashFieldExpire

## Visão geral

Exemplo console focado exclusivamente na API `HashFieldExpire` do StackExchange.Redis para definir TTL em campos específicos dentro de um hash. O objetivo é demonstrar o fluxo mínimo para criar hash, aplicar expiração por campo e observar o retorno da operação.

## Conceitos abordados

- Expiração por campo com `HashFieldExpire`.
- Diferença entre expirar a chave inteira e expirar apenas campos do hash.
- Tratamento de incompatibilidade de versão do Redis para comandos `HEXPIRE`.

## Objetivos de aprendizagem

- Executar um cenário mínimo e reproduzível para `HashFieldExpire`.
- Entender o retorno da API (`ExpireResult`) ao aplicar TTL por campo.
- Validar pré-requisitos de infraestrutura para recursos avançados de hash no Redis.

## Estrutura do projeto

```text
RedisHashFieldExpire/
+-- Program.cs
+-- README.md
\-- RedisHashFieldExpire.csproj
```

## Como executar

```bash
dotnet run --project 06-Caching/Caching/RedisHashFieldExpire/RedisHashFieldExpire.csproj
```

## Boas práticas e pontos de atenção

- Garanta Redis `7.4+` para suporte a `HEXPIRE`.
- Use nomes de chave com contexto de domínio (ex.: `demo:hashfieldexpire:product:1`).
- Para inspeção manual, prefira `redis-cli` com `HGETALL` e `HTTL` após a execução.

## Conteúdo complementar

Comandos úteis para validação manual:

```bash
redis-cli HGETALL demo:hashfieldexpire:product:1
redis-cli HTTL demo:hashfieldexpire:product:1 FIELDS 1 field1
redis-cli HTTL demo:hashfieldexpire:product:1 FIELDS 1 field2
```

## Referências e documentação complementar

- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis Hashes](https://redis.io/docs/latest/develop/data-types/hashes/)
- [Redis Command Reference](https://redis.io/docs/latest/commands/)
