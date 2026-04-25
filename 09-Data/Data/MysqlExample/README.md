# MySQL com Entity Framework Core

## Visão geral

Projeto didático do CSharp-101 dedicado a MySQL com Entity Framework Core, com foco em persistência, bancos de dados e acesso a dados.

## Conceitos abordados

Este projeto demonstra a integração do MySQL com .NET usando Entity Framework Core:

- **Entity Framework Core**: ORM para .NET
- **MySQL Provider**: Conectividade com MySQL
- **DbContext**: Contexto de banco de dados
- **Code First**: Criação de banco via código
- **Migrations**: Versionamento de schema
- **Connection Strings**: Configuração de conexão
- **Dependency Injection**: Injeção do contexto

## Objetivos de aprendizagem

- Configurar MySQL com Entity Framework Core
- Criar modelos de dados (entities)
- Implementar DbContext personalizado
- Gerenciar migrations e schema
- Configurar connection strings
- Realizar operações CRUD básicas

### O que Você Aprenderá

1. **Setup MySQL**:
   - Instalação do provider MySQL
   - Configuração de connection string
   - Configuração do DbContext

2. **Code First Approach**:
   - Definição de entities
   - Relacionamentos entre tabelas
   - Data annotations vs Fluent API

3. **Migrations**:
   - Criação de migrations
   - Aplicação de mudanças no banco
   - Rollback de migrations

4. **CRUD Operations**:
   - Create: Inserção de dados
   - Read: Consultas e filtros
   - Update: Atualização de registros
   - Delete: Remoção de dados

## Estrutura do projeto

```text
MysqlExample/
+-- Migrations/
|   +-- 20250609230719_Inicial.cs
|   +-- 20250609230719_Inicial.Designer.cs
|   \-- MeuDbContextModelSnapshot.cs
+-- Properties/
|   \-- launchSettings.json
+-- src/
|   \-- DbContext.cs
+-- .gitignore
+-- appsettings.Development.json
+-- appsettings.json
+-- MysqlExample.csproj
+-- MysqlExample.csproj.user
\-- ...
```

## Como executar

```bash
dotnet run --project 09-Data/Data/MysqlExample/MysqlExample.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Instalação de Pacotes

```bash
dotnet add package MySql.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
```

##### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=meubanco;User=root;Password=minhasenha;"
  }
}
```

##### DbContext Configuration

```csharp
public class MeuDbContext : DbContext
{
    public MeuDbContext(DbContextOptions<MeuDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
        });
    }
}
```

##### Entity Model

```csharp
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public DateTime DataCriacao { get; set; }
}
```

##### 1. Migrations

```bash
dotnet ef migrations add Inicial
dotnet ef database update
```

##### Create (Inserir)

```csharp
[HttpPost]
public async Task<IActionResult> CriarUsuario(Usuario usuario)
{
    _context.Usuarios.Add(usuario);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(ObterUsuario), new { id = usuario.Id }, usuario);
}
```

##### Read (Consultar)

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Usuario>>> ObterUsuarios()
{
    return await _context.Usuarios.ToListAsync();
}

[HttpGet("{id}")]
public async Task<ActionResult<Usuario>> ObterUsuario(int id)
{
    var usuario = await _context.Usuarios.FindAsync(id);
    return usuario == null ? NotFound() : usuario;
}
```

##### Update (Atualizar)

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> AtualizarUsuario(int id, Usuario usuario)
{
    if (id != usuario.Id)
        return BadRequest();

    _context.Entry(usuario).State = EntityState.Modified;
    
    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!UsuarioExists(id))
            return NotFound();
        throw;
    }

    return NoContent();
}
```

##### Delete (Remover)

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> RemoverUsuario(int id)
{
    var usuario = await _context.Usuarios.FindAsync(id);
    if (usuario == null)
        return NotFound();

    _context.Usuarios.Remove(usuario);
    await _context.SaveChangesAsync();

    return NoContent();
}
```

##### 1. Connection Pooling

```csharp
builder.Services.AddDbContext<MeuDbContext>(options =>
    options.UseMySQL(connectionString, mysqlOptions =>
    {
        mysqlOptions.CommandTimeout(30);
    }));

// Connection pooling é automático no EF Core
```

##### 2. Logging

```csharp
builder.Services.AddDbContext<MeuDbContext>(options =>
    options.UseMySQL(connectionString)
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging()); // Apenas em desenvolvimento
```

##### 3. Retry Policy

```csharp
builder.Services.AddDbContext<MeuDbContext>(options =>
    options.UseMySQL(connectionString, mysqlOptions =>
    {
        mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    }));
```

##### Performance

```csharp
// ✅ Use async para operações de banco
var usuarios = await _context.Usuarios.ToListAsync();

// ✅ Use projeções para reduzir dados transferidos
var nomes = await _context.Usuarios
    .Select(u => new { u.Id, u.Nome })
    .ToListAsync();

// ✅ Use Include para eager loading
var usuariosComPosts = await _context.Usuarios
    .Include(u => u.Posts)
    .ToListAsync();
```

##### Security

```csharp
// ✅ Use parâmetros para evitar SQL injection
var usuarios = await _context.Usuarios
    .Where(u => u.Nome.Contains(filtro))
    .ToListAsync();

// ✅ Valide dados antes de salvar
[Required]
[EmailAddress]
public string Email { get; set; }
```

##### Migrations

```csharp
// ✅ Sempre revise migrations antes de aplicar
// ✅ Faça backup antes de migrations em produção
// ✅ Teste migrations em ambiente de desenvolvimento

// Comandos úteis:
// dotnet ef migrations list
// dotnet ef migrations remove
// dotnet ef database drop
```

## Referências

- [MySQL EntityFrameworkCore Provider](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Code First Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
dotnet ef database update
