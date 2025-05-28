# **Entity Framework Core (EF Core)**

O **Entity Framework Core (EF Core)** possui várias particularidades importantes que afetam como ele **interage com o banco de dados**, **gerencia o estado das entidades**, **executa comandos** e **otimiza performance**. Aqui vai uma lista completa (com explicações claras) das principais **particularidades e comportamentos internos do EF Core**:

---

## ✅ 1. **Change Tracking (Rastreamento de Mudanças)**

* **Default:** Ativo por padrão.
* Monitora todas as entidades carregadas pelo contexto (`DbContext`) para detectar alterações.
* Permite o uso do `SaveChanges()` sem necessidade de SQL manual.
* Pode ser desativado com `.AsNoTracking()`, o que melhora performance em consultas de leitura.

📌 Ferramenta: `context.ChangeTracker`

---

## 🔁 2. **States das Entidades**

Cada entidade tem um `EntityState`:

* `Detached`: Fora do rastreamento.
* `Unchanged`: Sem alterações.
* `Modified`: Propriedades alteradas.
* `Added`: A ser inserida.
* `Deleted`: A ser removida.

Você pode definir manualmente:

```csharp
context.Entry(entidade).State = EntityState.Modified;
```

---

## 🔌 3. **Detaching and Reattaching**

Usado para gerenciar entidades fora do ciclo de vida do `DbContext` (ex: aplicações web/API):

* **Detach**: remove do contexto

  ```csharp
  context.Entry(entidade).State = EntityState.Detached;
  ```

* **Attach**: reanexa sem alterações

  ```csharp
  context.Attach(entidade);
  ```

* **Atualizar uma entidade desconectada**

  ```csharp
  context.Entry(entidade).State = EntityState.Modified;
  ```

---

## ⚙️ 4. **Automatic Change Detection**

Por padrão, o EF verifica mudanças automaticamente antes de salvar:

```csharp
context.ChangeTracker.AutoDetectChangesEnabled = false;
```

Desativar melhora performance, mas exige que você chame manualmente:

```csharp
context.ChangeTracker.DetectChanges();
```

---

## 🔄 5. **Identity Resolution**

Quando você carrega múltiplas entidades que referenciam o mesmo objeto (ex: mesmo `Author` em dois `Book`), o EF Core tenta **evitar duplicações** de instâncias, se o rastreamento estiver ativado.

Sem isso, você pode acabar com várias cópias do mesmo objeto.

---

## ⚡ 6. **AsNoTracking e AsNoTrackingWithIdentityResolution**

* `AsNoTracking()`: desativa completamente o rastreamento (ideal para leitura).
* `AsNoTrackingWithIdentityResolution()`: desativa o tracking **mas ainda garante identidade** (mesmo objeto em várias relações continua sendo uma única instância).

---

## 🔗 7. **Lazy Loading, Eager Loading e Explicit Loading**

### a) **Lazy Loading** (preguiçoso)

Carrega relacionamentos apenas quando acessados.
Necessário:

* Pacote `Microsoft.EntityFrameworkCore.Proxies`
* `UseLazyLoadingProxies()`
* Propriedades de navegação devem ser `virtual`

### b) **Eager Loading** (antecipado)

Carrega os relacionamentos junto na consulta:

```csharp
context.Pedidos.Include(p => p.Cliente).ToList();
```

### c) **Explicit Loading**

Você carrega manualmente após a entidade principal:

```csharp
context.Entry(pedido).Reference(p => p.Cliente).Load();
```

---

## 🧩 8. **Shadow Properties**

Propriedades não definidas na classe, mas mapeadas no EF:

```csharp
modelBuilder.Entity<Pedido>().Property<DateTime>("DataCriacao");
```

Útil para audit logs.

---

## 🧠 9. **Value Conversions**

Transforma tipos .NET em formatos compatíveis com o banco:

```csharp
builder.Property(p => p.Status)
    .HasConversion<string>();
```

---

## 🧱 10. **Owned Types**

Tipos que não possuem ID próprio, fazem parte da entidade "dona":

```csharp
[Owned]
public class Endereco { ... }
```

---

## 🕵️ 11. **Concurrency Tokens**

Detecção de concorrência otimista com `RowVersion`:

```csharp
[Timestamp]
public byte[] RowVersion { get; set; }
```

---

## ⏱️ 12. **Migrations (Migrações)**

* Gerencia a evolução do schema do banco de dados.
* Comando comum:

  ```bash
  dotnet ef migrations add NomeMigracao
  dotnet ef database update
  ```

---

## 🧪 13. **Seed Data (Dados Iniciais)**

Definido no `OnModelCreating()`:

```csharp
modelBuilder.Entity<Pessoa>().HasData(new Pessoa { Id = 1, Nome = "Fulano" });
```

---

## 💾 14. **Tracking de Performance e Logs**

EF Core usa `ILogger` do .NET. Pode configurar logs SQL com:

```csharp
options.UseSqlServer(connectionString)
       .LogTo(Console.WriteLine, LogLevel.Information);
```

---

## 🧹 15. **Pooling de DbContext**

Reutiliza instâncias do `DbContext` para reduzir overhead:

```csharp
services.AddDbContextPool<MeuContexto>(options =>
    options.UseSqlServer(connectionString));
```

---

## 💡 16. **Interceptors e Events**

Permite interceptar comandos SQL, mudanças em entidades, conexões abertas, etc.

---

## 📚 17. **Relational Mapping (Fluent API vs Data Annotations)**

Você pode configurar o modelo com:

* **Anotações**

  ```csharp
  [Key], [Required], [MaxLength(50)]
  ```

* **Fluent API**

  ```csharp
  modelBuilder.Entity<Pessoa>().HasKey(p => p.Id);
  ```

---
