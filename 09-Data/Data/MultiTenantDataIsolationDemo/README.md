# MultiTenantDataIsolationDemo

Console que isola dados por tenant com query filters globais e índice composto — e demonstra as quatro formas de furar esse isolamento sem que nada acuse o problema.

## Visão geral

Em um sistema multi-tenant com banco compartilhado, cada linha pertence a um cliente e ninguém pode ver o dado de ninguém. Fazer isso com um `WHERE TenantId = ...` em cada consulta funciona até a primeira vez que alguém esquece — e aí um cliente vê a fatura de outro.

O query filter global resolve a parte fácil: declarado uma vez no `OnModelCreating`, ele entra em toda consulta LINQ sobre a entidade, inclusive em agregações. As duas mesmas consultas deste exemplo, sem nenhuma menção a `TenantId`, devolvem conjuntos diferentes conforme o tenant ativo.

O índice composto resolve a parte de desempenho, e a ordem das colunas importa. Como toda consulta filtra por tenant, `TenantId` precisa ser a primeira coluna. O exemplo mostra o plano do SQLite: com `TenantId` na frente, `SEARCH ... USING INDEX`; sem ele, `SCAN` da tabela inteira.

A parte mais importante é a última. Filtro global **não é** uma garantia de segurança — é um padrão conveniente com quatro buracos conhecidos, e o projeto exercita todos: SQL escrito à mão, `IgnoreQueryFilters`, o cache do `Find()` e a troca de tenant em um contexto já em uso.

## Conceitos abordados

- Query filter global com `HasQueryFilter`.
- Resolução do tenant em tempo de consulta.
- Atribuição automática de `TenantId` na inclusão.
- Bloqueio de gravação cruzada no `SaveChanges`.
- Índice composto e por que `TenantId` lidera.
- Vazamento por SQL cru.
- Vazamento por `IgnoreQueryFilters`.
- Vazamento pelo cache do `Find()`.
- Ciclo de vida do `DbContext` por requisição.

## Objetivos de aprendizagem

- Implementar isolamento por tenant que funcione por padrão, não por disciplina.
- Ordenar índices compostos a partir do padrão de consulta real.
- Reconhecer os caminhos que contornam o filtro global.
- Entender por que o `DbContext` não pode ser reutilizado entre tenants.

## Estrutura do projeto

```text
MultiTenantDataIsolationDemo/
|-- Data/
|   |-- BillingDbContext.cs
|   `-- TenantContext.cs
|-- Demo/
|   `-- MultiTenantDemoRunner.cs
|-- Model/
|   `-- Invoice.cs
|-- MultiTenantDataIsolationDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 09-Data/Data/MultiTenantDataIsolationDemo/MultiTenantDataIsolationDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 09-Data/Data/MultiTenantDataIsolationDemo/MultiTenantDataIsolationDemo.csproj
```

Não exige serviço externo — SQLite em arquivo local, recriado a cada execução e não versionado.

## Boas práticas e pontos de atenção

- Filtro global é conveniência, não fronteira de segurança. Ele cobre o caminho comum do LINQ e nada além disso; tratar como garantia é o erro que produz vazamento.
- Um `DbContext` por requisição, com o tenant fixado na criação. Reaproveitar o contexto entre tenants é o caminho mais direto para o cache devolver dado alheio.
- `Find()` consulta o change tracker antes do banco, e o cache local **não** passa pelo filtro. Se a entidade já foi carregada sob outro tenant, `Find()` a devolve.
- Todo SQL escrito à mão precisa filtrar por tenant explicitamente. `FromSqlRaw`, procedure, view, script de manutenção — o EF não injeta nada ali.
- `IgnoreQueryFilters` deveria doer. Ele tem usos legítimos — relatório administrativo, rotina de suporte —, mas em código de aplicação cada ocorrência merece justificativa, e vale barrar por revisão ou analisador.
- Preencha `TenantId` automaticamente na inclusão. Depender de cada `Add` lembrar produz linhas órfãs, que ninguém enxerga e ninguém apaga.
- Valide o tenant também na gravação. Carregar sob um tenant e salvar sob outro é bloqueável no `SaveChanges`, e é barato fazê-lo.
- `TenantId` primeiro no índice composto. Toda consulta filtra por ele; invertida a ordem, o índice deixa de ser aproveitado no caso que importa.
- Considere isolamento mais forte quando o risco justificar. Schema por tenant, ou banco por tenant, custam mais para operar mas eliminam a classe inteira de vazamento por consulta.
- Teste o isolamento. Um teste que consulta como o tenant A e verifica que não vê nada do B vale mais que qualquer revisão de código.

## Conteúdo complementar

Resultados observados:

**1. O filtro funcionando**

```text
tenant 'acme':   2 faturas, total 350.00   [Ana, Bruno]
tenant 'globex': 2 faturas, total 2100.00  [Carla, Diego]
```

Nenhuma das consultas mencionou `TenantId` — nem a listagem, nem o `SUM`.

**2. Atribuição automática**

```text
fatura de Elena gravada com TenantId='globex', sem ninguem atribuir
```

**3. Índice composto e o plano real do SQLite**

```text
WHERE TenantId = 'acme' AND Customer = 'Ana'
  -> SEARCH Invoices USING INDEX IX_Invoices_TenantId_Customer (TenantId=? AND Customer=?)

WHERE Customer = 'Ana'
  -> SCAN Invoices
```

O índice só é aproveitado quando a consulta começa pela primeira coluna dele. Como **toda** consulta multi-tenant filtra por tenant, é ele que precisa liderar.

**As quatro formas de furar o isolamento**

| # | Caminho | O que acontece | Como se protege |
|---|---|---|---|
| 4 | SQL cru | Estando em `acme`, trouxe **5 faturas de 2 tenants** | Filtrar explicitamente em todo SQL manual |
| 5 | `IgnoreQueryFilters` | 2 faturas com filtro, **5 sem** | Restringir por revisão; proibir em código de aplicação |
| 6 | `Find()` | Devolveu a fatura de `globex` estando em `acme` | Um contexto por requisição, tenant fixo |
| 7 | Gravação cruzada | **Bloqueada** pelo `SaveChanges` | Validar o tenant também na escrita |

O cenário 6 merece atenção porque é o menos óbvio:

```text
em 'globex':  carregada a fatura #3 de Carla
troca para 'acme':
  consulta LINQ -> null            (correto)
  Find(3)       -> fatura de Carla (VAZOU)
```

A consulta LINQ passou pelo filtro e não achou nada. O `Find()` encontrou a entidade no change tracker e a devolveu sem consultar o banco — e, portanto, sem passar pelo filtro.

Estratégias de isolamento, em ordem de força:

| Estratégia | Isolamento | Custo operacional |
|---|---|---|
| Coluna `TenantId` + query filter | Lógico, no nível da aplicação | Baixo |
| Schema por tenant | Lógico, no nível do banco | Médio |
| Banco por tenant | Físico | Alto |

O exemplo implementa a primeira, que é a mais comum — e por isso mesmo a que mais precisa que seus limites sejam conhecidos.

Relação com os vizinhos da trilha: `EfCoreRelationshipsDemo` cobre modelagem e `EfCoreOptimisticConcurrencyDemo` trata de gravações concorrentes. `PolicyBasedAuthorizationDemo` (trilha 04) trata de autorização por recurso, que é a camada acima — decidir se o usuário pode agir sobre um registro que ele já tem direito de enxergar.

## Referências e documentação complementar

- https://learn.microsoft.com/ef/core/querying/filters
- https://learn.microsoft.com/ef/core/miscellaneous/multitenancy
- https://learn.microsoft.com/azure/architecture/guide/multitenant/considerations/data-isolation
- https://www.sqlite.org/eqp.html
