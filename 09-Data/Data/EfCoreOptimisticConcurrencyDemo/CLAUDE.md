# CLAUDE.md — EfCoreOptimisticConcurrencyDemo

Console com atualização perdida, detecção por concurrency token e as três estratégias de resolução, mais retry. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 09-Data/Data/EfCoreOptimisticConcurrencyDemo/EfCoreOptimisticConcurrencyDemo.csproj
dotnet run --project 09-Data/Data/EfCoreOptimisticConcurrencyDemo/EfCoreOptimisticConcurrencyDemo.csproj
```

Roda sete cenários (0 a 6) e termina. Sem serviço externo — SQLite em arquivo.

## Estrutura interna

Duas entidades de propósito: `Product` (com `IsConcurrencyToken` em `Version`) e `UnguardedProduct` (sem). O par é o exemplo; **não remover a versão desprotegida**.

`ShopDbContext.SaveChangesAsync` **incrementa o token à mão**. No SQL Server o `rowversion` seria mantido pelo banco; o SQLite não tem equivalente. Isso precisa valer para **toda** gravação — um caminho de escrita que não passe por aqui desliga a detecção sem erro nenhum.

Cada cenário de resolução usa `exception.Entries[0]` e `GetDatabaseValuesAsync`. A diferença entre eles é o que se faz com `OriginalValues` e `CurrentValues`:

- **Banco vence**: `SetValues(databaseValues)` nos dois.
- **Cliente vence**: `OriginalValues.SetValues(databaseValues)` e regrava.
- **Merge**: percorre as propriedades e só mantém o valor local onde `proposed != original`.

`ResetProductAsync` roda antes dos cenários 3 a 6, para que cada um comece do mesmo estado.

## Pontos de atenção

- TFM `net10.0`. Pacotes: `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.Extensions.Logging.Console`.
- SQLite em `concurrency-demo.db`, ignorado pelo `.gitignore` local. Não versionar.
- **Armadilha já corrigida, não reintroduzir:** o cenário 1 precisa que os dois usuários alterem o **mesmo campo**. Uma versão anterior tinha A mexendo no preço e B no estoque, e afirmava que o preço se perdia — mas **o EF Core gera `UPDATE` apenas das colunas alteradas**, então nada se perdia e a saída contradizia o texto. Só o read-modify-write sobre o mesmo campo produz a atualização perdida. Esse fato está explicado no README e numa linha de log do próprio cenário.
- O cenário 6 depende de a interferência acontecer **só na primeira tentativa** (`if (attempt == 1)`). Sem isso o retry nunca convergiria.
- O retry reaplica a **intenção** (`product.Stock -= 3`), não um valor absoluto. Trocar por `product.Stock = 7` traria de volta a atualização perdida — é o erro mais comum ao implementar retry.
- Os números do README (10 → 8 → 7, esperado 5; preço 199/275; estoque 77) vêm do seed e dos cenários. Alterar qualquer valor invalida as tabelas.
- `PropertyValues` e `EntityEntry` vêm de `Microsoft.EntityFrameworkCore.ChangeTracking`. O `using` está no runner e no contexto.
- **Fronteira com os vizinhos**: [EfCoreRelationshipsDemo](../EfCoreRelationshipsDemo/CLAUDE.md) cobre modelagem e carregamento. `MySqlNamedAdvisoryLockReservation` mostra a alternativa pessimista. `EventSourcingBankAccountDemo` (trilha 08) aplica versão esperada a um fluxo de eventos — mesma ideia, outro contexto.
