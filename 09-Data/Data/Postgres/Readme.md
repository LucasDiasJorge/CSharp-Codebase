# PostgreSQL com Entity Framework Core

## Visão geral

Projeto didático do CSharp-101 dedicado a PostgreSQL com Entity Framework Core, com foco em persistência, bancos de dados e acesso a dados.

## Conceitos abordados

Este projeto demonstra a integração do PostgreSQL com .NET usando Entity Framework Core:

- **PostgreSQL**: Banco de dados relacional avançado
- **Npgsql Provider**: Driver oficial para PostgreSQL
- **Entity Framework Core**: ORM para .NET
- **Migrations**: Versionamento de schema
- **Connection Pooling**: Otimização de conexões
- **JSON Support**: Suporte nativo a tipos JSON
- **Full-Text Search**: Busca textual avançada

## Objetivos de aprendizagem

- Configurar PostgreSQL com Entity Framework Core
- Usar recursos específicos do PostgreSQL
- Implementar full-text search
- Trabalhar com tipos de dados JSON
- Otimizar performance com índices
- Gerenciar migrações complexas

## Estrutura do projeto

```text
Postgres/
+-- Migrations/
|   +-- 20250409184037_InitialCreate.cs
|   +-- 20250409184037_InitialCreate.Designer.cs
|   \-- AppDbContextModelSnapshot.cs
+-- Models/
|   +-- Roles.cs
|   \-- Users.cs
+-- .gitignore
+-- AppDbContext.cs
+-- Postgres.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 09-Data/Data/Postgres/Postgres.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Instalação de Pacotes

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design

dotnet tool install --global dotnet-ef
```

##### Criação de Migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

##### 1. Instalar PostgreSQL

```bash
# Docker
docker run --name postgres -e POSTGRES_PASSWORD=password -p 5432:5432 -d postgres
```

##### 2. Configurar Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=myapp;Username=postgres;Password=password"
  }
}
```

##### 1. DbContext Configuration

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PostgreSQL-specific configurations
        modelBuilder.HasPostgresExtension("uuid-ossp");
        
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Id)
                  .HasDefaultValueSql("uuid_generate_v4()");
                  
            entity.Property(e => e.Metadata)
                  .HasColumnType("jsonb");
        });
    }
}
```

##### 2. JSON Support

```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, object> Metadata { get; set; } // JSONB
    public string[] Tags { get; set; } // Array
}

// Queries
var products = await context.Products
    .Where(p => EF.Functions.JsonContains(p.Metadata, "\"electronics\""))
    .ToListAsync();
```

##### 3. Full-Text Search

```csharp
// Configuration
modelBuilder.Entity<Product>()
    .HasGeneratedTsVectorColumn(
        p => p.SearchVector,
        "english",
        p => new { p.Name, p.Description })
    .HasIndex(p => p.SearchVector)
    .HasMethod("GIN");

// Query
var products = await context.Products
    .Where(p => p.SearchVector.Matches("search term"))
    .ToListAsync();
```

##### Índices Especializados

```csharp
// GIN index for JSON
modelBuilder.Entity<Product>()
    .HasIndex(p => p.Metadata)
    .HasMethod("gin");

// Trigram index for fuzzy search
modelBuilder.Entity<Product>()
    .HasIndex(p => p.Name)
    .HasMethod("gin")
    .HasOperators("gin_trgm_ops");
```

##### Connection Pooling

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.EnableRetryOnFailure(3);
    }));
```

## Referências

- [Npgsql Documentation](https://www.npgsql.org/efcore/)
- [PostgreSQL Features in EF Core](https://docs.microsoft.com/en-us/ef/core/providers/npgsql/)
- [PostgreSQL Performance Tuning](https://wiki.postgresql.org/wiki/Performance_Optimization)
dotnet ef database update
```

### Pacotes, libs e ferramentas usadas:

https://learn.microsoft.com/pt-br/dotnet/api/system.guid.newguid?view=net-9.0
https://learn.microsoft.com/pt-br/dotnet/csharp/linq/get-started/introduction-to-linq-queries
