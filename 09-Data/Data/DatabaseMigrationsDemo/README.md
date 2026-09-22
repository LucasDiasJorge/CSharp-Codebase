# DatabaseMigrationsDemo

Console com três migrations reais que evoluem o schema, incluindo seed declarativo, backfill escrito à mão e a reversão — mais o padrão expand/contract para mudança sem downtime.

## Visão geral

Migration é uma alteração de schema versionada e ordenada. O EF Core gera cada uma comparando o modelo atual com o snapshot da anterior, e registra o que já rodou na tabela `__EFMigrationsHistory` — é por isso que aplicar duas vezes não repete nada.

O projeto tem três migrations, e cada uma representa um tipo diferente de mudança. `InitialCreate` cria a tabela e insere o seed, que é declarado com `HasData` e vira `INSERT` dentro da própria migration. `AddCustomerPhone` acrescenta uma coluna nula — mudança aditiva, que a versão anterior da aplicação ignora sem quebrar. `SplitCustomerNameExpand` é a interessante: divide `Name` em `FirstName` e `LastName`.

Essa terceira é a que ensina o padrão. Renomear ou dividir uma coluna em uma tacada só quebra qualquer instância da aplicação que ainda esteja rodando a versão antiga. O caminho seguro é **expand/contract**: primeiro acrescentar as colunas novas e preencher a partir das antigas, deixando as duas formas coexistirem; depois, quando todo o parque tiver migrado, remover a antiga em uma migration separada.

O backfill é a parte que o scaffolding não faz. O EF gera a mudança de **schema** a partir do diff do modelo; mover **dados** de uma coluna para outra é decisão de negócio e precisa ser escrita à mão com `migrationBuilder.Sql`.

## Conceitos abordados

- Migration como alteração de schema versionada e ordenada.
- `__EFMigrationsHistory` e idempotência da aplicação.
- Seed declarativo com `HasData`, versionado junto do schema.
- Backfill de dados com `migrationBuilder.Sql`.
- Expand/contract para mudança compatível.
- `Down()` e o que a reversão não consegue recuperar.
- Aviso de possível perda de dados no scaffolding.
- Migração programática com `IMigrator` e alvo explícito.

## Objetivos de aprendizagem

- Criar, aplicar e reverter migrations com segurança.
- Distinguir mudança aditiva de mudança que quebra compatibilidade.
- Escrever o backfill que o scaffolding não gera.
- Entender o que um `Down()` consegue e o que não consegue desfazer.

## Estrutura do projeto

```text
DatabaseMigrationsDemo/
|-- Data/
|   `-- ShopDbContext.cs
|-- Demo/
|   `-- MigrationsDemoRunner.cs
|-- Migrations/
|   |-- 20260922020729_InitialCreate.cs
|   |-- 20260922020740_AddCustomerPhone.cs
|   |-- 20260922020755_SplitCustomerNameExpand.cs
|   `-- ShopDbContextModelSnapshot.cs
|-- Model/
|   `-- Customer.cs
|-- DatabaseMigrationsDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj
```

Não exige serviço externo — SQLite em arquivo local, recriado a cada execução e não versionado.

Comandos da CLI de migrations (exigem `dotnet-ef`, instalável com `dotnet tool install --global dotnet-ef`):

```bash
# criar uma migration nova a partir do modelo atual
dotnet ef migrations add NomeDaMigration \
  --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj \
  --output-dir Migrations

# aplicar tudo o que estiver pendente
dotnet ef database update \
  --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj

# voltar para uma migration anterior (executa os Down)
dotnet ef database update AddCustomerPhone \
  --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj

# descartar a ultima migration AINDA NAO aplicada
dotnet ef migrations remove \
  --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj

# gerar o script SQL, para revisao ou para aplicar em producao
dotnet ef migrations script \
  --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj
```

## Boas práticas e pontos de atenção

- Nunca edite uma migration já aplicada em qualquer ambiente. O histórico registra que ela rodou; alterar o arquivo faz o schema real divergir do que o código diz ter sido feito, e ninguém percebe até quebrar.
- Revise o que o scaffolding gerou antes de commitar. O EF acerta a maior parte, mas o diff do modelo não conhece a intenção — especialmente em renomeações, que ele costuma interpretar como remover e criar.
- Leia o aviso de possível perda de dados. Ele aparece inclusive quando só o `Down()` é destrutivo, como no `AddCustomerPhone` deste projeto: o `Up` só acrescenta, mas o `Down` derruba a coluna.
- Backfill é escrito à mão. Migration move schema; mover dados é decisão sua, com `migrationBuilder.Sql`.
- Use expand/contract para mudança incompatível. Adicione o novo, preencha, deixe conviver, migre a aplicação e só então remova o antigo — em migrations separadas, liberadas em momentos diferentes.
- Coluna nova obrigatória precisa de valor padrão ou de três passos. Adicionar `NOT NULL` sem default falha se já houver linhas; o caminho é nula → backfill → tornar obrigatória.
- `Down()` desfaz schema, não dados. Recriar uma coluna removida é fácil; recuperar o que havia nela, não. Depois da etapa contract, a reversão deixa de ser reversível de verdade.
- Em produção, prefira gerar o script e revisá-lo. `dotnet ef migrations script --idempotent` produz SQL aplicável sem depender da aplicação subir com permissão de DDL.
- Cuidado com `Database.Migrate()` no startup de várias instâncias. Duas subindo juntas tentam migrar ao mesmo tempo; migração costuma ser passo separado do deploy.
- `HasData` serve para dado de referência, não para massa de teste. Ele é versionado junto do schema, e mudá-lo gera uma migration com o `UPDATE` correspondente.

## Conteúdo complementar

As três migrations:

| Migration | Tipo de mudança | Compatível com a versão anterior? |
|---|---|---|
| `InitialCreate` | Cria tabela + seed | — |
| `AddCustomerPhone` | Coluna nova, nula | Sim |
| `SplitCustomerNameExpand` | Colunas novas + backfill | Sim, `Name` continua existindo |
| *(contract, não incluída)* | Remover `Name` | **Não** — só depois de todos migrarem |

Saída dos cenários:

**1 e 2. Pendentes e aplicação**

```text
pendentes: InitialCreate, AddCustomerPhone, SplitCustomerNameExpand
aplicadas: 3
```

**3. Seed e backfill**

```text
#1 Name="Ana Souza"  -> FirstName="Ana"   LastName="Souza"  Phone=(nulo)
#2 Name="Bruno Lima" -> FirstName="Bruno" LastName="Lima"   Phone=(nulo)
```

O seed veio do `HasData`, dentro da migration. `FirstName` e `LastName` foram preenchidos pelo `migrationBuilder.Sql` — sem ele, nasceriam vazios.

**4. Reversão**

```text
aplicadas agora: 2
pendente de novo: SplitCustomerNameExpand
colunas de Customers: [Id, Email, Name, Phone]
```

O `Down()` removeu as duas colunas, e com elas o que o backfill havia escrito.

**5. Reaplicação**

```text
#1 FirstName="Ana"   LastName="Souza"
#2 FirstName="Bruno" LastName="Lima"
```

Os dados voltaram **porque a origem ainda existia**. Se a etapa contract já tivesse removido `Name`, o `Down` não teria de onde reconstruir — e é exatamente por isso que contract se faz por último, e com calma.

Expand/contract, passo a passo:

```text
1. expand    adiciona FirstName/LastName, faz o backfill, mantem Name
2. deploy    a aplicacao passa a escrever nos dois e ler dos novos
3. espera    ate nenhuma instancia antiga estar rodando
4. contract  remove Name, em uma migration separada
```

Cada passo é compatível com o anterior, e em nenhum momento existe uma combinação de código e schema que não funcione.

Relação com os vizinhos da trilha: `EfCoreRelationshipsDemo` cobre modelagem e `EfCoreOptimisticConcurrencyDemo` trata de conflitos de gravação. `sqlite-sample-api` usa EF Core em uma API, sem o ciclo de migrations.

## Referências e documentação complementar

- https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying
- https://learn.microsoft.com/ef/core/modeling/data-seeding
- https://martinfowler.com/articles/evodb.html
