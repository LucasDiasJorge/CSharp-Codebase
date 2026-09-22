# EfCoreRelationshipsDemo

Console que mapeia os três tipos de relacionamento do EF Core, demonstra owned types e mede o custo do carregamento em partes contra o `Include`.

## Visão geral

O exemplo modela um blog com posts e tags, mais autores com perfil e endereço. Cada relacionamento existe para mostrar uma decisão diferente de mapeamento.

No um-para-muitos, a chave estrangeira fica sempre no lado "muitos" — o post sabe de qual blog é, não o contrário. No um-para-um, escolher qual dos dois lados carrega a FK é uma decisão de modelagem, não um detalhe: é ela que define quem pode existir sem o outro. No muitos-para-muitos, o EF Core 5+ dispensa declarar a entidade de junção no código, embora a tabela continue existindo no banco.

O owned type é o caso que costuma surpreender. `Address` não tem `Id` e não vira tabela: seus campos aparecem como colunas em `Authors`, prefixados. O exemplo prova isso consultando o `sqlite_master` — as colunas `Address_Street` e `Address_City` estão lá, e nenhuma tabela `Address` existe.

O ponto mais prático é o N+1, medido com um interceptor que conta as consultas. Carregar três posts e depois as tags de cada um custa **4 consultas**; o mesmo resultado com `Include` custa **1**. Com trinta posts seriam 31 contra 1, e é assim que uma tela fica lenta sem que nenhuma linha de código pareça errada.

## Conceitos abordados

- Um-para-muitos e a posição da chave estrangeira.
- Um-para-um e a escolha do lado dependente.
- Muitos-para-muitos com skip navigation, sem entidade de junção no modelo.
- Owned types como colunas do dono, sem identidade própria.
- `DeleteBehavior` e o efeito do comportamento padrão.
- Carregamento eager (`Include`) versus explícito (`Entry().Collection().Load()`).
- N+1 medido por interceptor de comandos.
- Tracking versus `AsNoTracking`.

## Objetivos de aprendizagem

- Mapear os três tipos de relacionamento sabendo onde cada chave vai parar.
- Reconhecer quando um tipo deve ser owned em vez de entidade.
- Detectar N+1 contando consultas, em vez de adivinhando.
- Escolher entre tracking e `AsNoTracking` a partir do que a consulta faz.

## Estrutura do projeto

```text
EfCoreRelationshipsDemo/
|-- Data/
|   |-- BlogDbContext.cs
|   `-- QueryCounter.cs
|-- Demo/
|   `-- RelationshipsDemoRunner.cs
|-- Model/
|   `-- Entities.cs
|-- EfCoreRelationshipsDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 09-Data/Data/EfCoreRelationshipsDemo/EfCoreRelationshipsDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 09-Data/Data/EfCoreRelationshipsDemo/EfCoreRelationshipsDemo.csproj
```

Não exige serviço externo — usa SQLite em arquivo local, recriado a cada execução e não versionado.

## Boas práticas e pontos de atenção

- A chave estrangeira mora no lado "muitos". Tentar colocá-la no lado "um" é sintoma de que o relacionamento foi entendido ao contrário.
- No um-para-um, decida o dependente de propósito. Quem tem a FK é quem não existe sozinho; inverter muda a semântica do modelo, não só o SQL.
- Declare a entidade de junção só quando ela tiver dados próprios. Para um muitos-para-muitos puro, a skip navigation basta; com data de associação ou quantidade, a entidade precisa existir.
- Use owned type para valor sem identidade. Endereço, dinheiro, intervalo de datas: dois iguais são o mesmo. Se o objeto precisa ser referenciado ou compartilhado, é entidade.
- Conheça o `DeleteBehavior` padrão. Relação obrigatória é `Cascade`; opcional é `SetNull`. A diferença costuma aparecer no primeiro delete em produção.
- Meça o N+1, não confie na intuição. Um interceptor de comandos custa poucas linhas e transforma o problema em número.
- `Include` não é sempre a resposta. Vários `Include` em coleções geram produto cartesiano e trazem linhas repetidas; `AsSplitQuery` resolve, ao custo de mais idas ao banco.
- Use `AsNoTracking` em leitura pura. O change tracker guarda um snapshot de cada entidade para detectar mudanças — trabalho inútil quando nada será gravado.
- Cuidado com navegação preguiçosa (lazy loading). Ela torna o N+1 invisível: o código parece só acessar uma propriedade.

## Conteúdo complementar

Relacionamentos mapeados:

| Tipo | Entidades | Onde fica a FK | Tabela extra |
|---|---|---|---|
| Um-para-muitos | `Blog` → `Post` | `Post.BlogId` | Não |
| Muitos-para-muitos | `Post` ↔ `Tag` | Tabela de junção | `PostTags` |
| Um-para-um | `Author` → `AuthorProfile` | `AuthorProfile.AuthorId` | Não |
| Owned | `Author` possui `Address` | — | Não, vira coluna |

Prova do owned type, lida do próprio banco:

```text
Colunas da tabela Authors: [Id, Name, Address_Street, Address_City]
Tabelas no banco: [AuthorProfiles, Authors, Blogs, PostTags, Posts, Tags]
```

Não existe tabela `Address`. O endereço virou duas colunas prefixadas, e a tabela de junção `PostTags` existe mesmo sem aparecer no modelo.

N+1 medido:

| Estratégia | Consultas | Posts |
|---|---|---|
| Carregamento explícito, um por vez | **4** | 3 |
| `Include` | **1** | 3 |

A fórmula é 1 + N: com 3 posts são 4 consultas, com 30 seriam 31. O tempo cresce linearmente com os dados, e o código não muda de aparência.

Tracking:

```text
com tracking:      1 entidade modificada detectada automaticamente
com AsNoTracking:  0 entidades rastreadas — a alteracao existe no objeto,
                   mas SaveChanges nao gravaria nada
```

Estratégias de carregamento:

| Estratégia | Como | Quando |
|---|---|---|
| Eager | `Include` | Você sabe que vai precisar do relacionado |
| Split | `Include` + `AsSplitQuery` | Vários `Include` de coleção, para evitar produto cartesiano |
| Explícito | `Entry().Collection().Load()` | Decide carregar depois, caso a caso |
| Lazy | Proxies | Raramente; esconde N+1 atrás de um acesso a propriedade |

Relação com os vizinhos da trilha: `sqlite-sample-api` e `MoneyStorageApi` usam EF Core em contexto de API. `Dapper` e `DapperExample` mostram a alternativa sem ORM completo, onde o mapeamento de relacionamento é manual e o N+1 é explícito no código.

## Referências e documentação complementar

- https://learn.microsoft.com/ef/core/modeling/relationships
- https://learn.microsoft.com/ef/core/modeling/owned-entities
- https://learn.microsoft.com/ef/core/querying/related-data/
- https://learn.microsoft.com/ef/core/querying/tracking
