Para integrar o **Entity Framework Core** com o PostgreSQL, você precisará seguir algumas etapas, incluindo a instalação de pacotes necessários, a configuração da conexão com o banco de dados e a criação de migrações.

Aqui estão os passos completos:

### 1. **Instalar os Pacotes Necessários**

Você precisa instalar o provedor do Entity Framework Core para o PostgreSQL. Execute o seguinte comando para instalar o pacote NuGet:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### 2. **Configurar o `DbContext`**

No seu projeto, você precisa configurar o `DbContext` para usar o PostgreSQL. O `DbContext` é a classe que representa o banco de dados e permite realizar operações CRUD.

Crie um arquivo `AppDbContext.cs` (ou use o nome que preferir):

```csharp
using Microsoft.EntityFrameworkCore;
using TaskManagement.Models; // Ajuste para o namespace correto

namespace TaskManagement
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskModel> Tasks { get; set; }

        // Configuração da string de conexão
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Host=localhost;Port=5432;Database=TaskDB;Username=yourusername;Password=yourpassword";
            optionsBuilder.UseNpgsql(connectionString);
        }

        // Configuração adicional, se necessário
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskModel>()
                .Property(t => t.DueDate)
                .HasColumnType("timestamp with time zone"); // Garantir que a coluna seja do tipo correto
        }
    }
}
```

- **Connection String**: Substitua `yourusername` e `yourpassword` pelos valores corretos de conexão com seu banco de dados PostgreSQL.
- O método `OnModelCreating` define o tipo de coluna para `DueDate` como `timestamp with time zone` para garantir que os valores de data sejam salvos corretamente no banco.

### 3. **Criar a Classe de Modelo `TaskModel`**

A classe de modelo representa as tabelas no banco de dados. Crie o arquivo `TaskModel.cs` com o seguinte conteúdo:

```csharp
using System;

namespace TaskManagement.Models
{
    public class TaskModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime DueDate { get; set; }
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }
}
```

### 4. **Configurar as Migrations**

Para criar e aplicar as migrações ao banco de dados, você precisa primeiro gerar a migração usando o comando do **Entity Framework Core**. Execute o seguinte comando:

```bash
dotnet ef migrations add InitialCreate
```

Esse comando cria um arquivo de migração que representa a estrutura do banco de dados. Depois, aplique a migração com:

```bash
dotnet ef database update
```

Esses dois comandos irão criar a tabela `Tasks` no banco de dados PostgreSQL conforme a estrutura definida no `DbContext` e no modelo `TaskModel`.

### 5. **Conectar e Usar o Banco de Dados**

Depois de ter configurado o `DbContext`, você pode realizar operações de CRUD no banco de dados. Aqui está um exemplo de como adicionar e listar tarefas no banco de dados:

```csharp
using System;
using TaskManagement;
using TaskManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

class Program
{
    static void Main()
    {
        // Exemplo de inserção de dados
        using (var db = new AppDbContext())
        {
            var task = new TaskModel
            {
                Title = "Design homepage",
                Category = "Design",
                Status = TaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(5) // Certifique-se de usar UTC
            };

            db.Tasks.Add(task);
            db.SaveChanges();
        }

        // Exemplo de consulta de dados
        using (var db = new AppDbContext())
        {
            var tasks = db.Tasks.ToList();
            foreach (var task in tasks)
            {
                Console.WriteLine($"{task.Title} - {task.Status} - {task.DueDate}");
            }
        }
    }
}
```

### 6. **Alternativa: Modo de Migração Manual**

Se você não quiser usar as ferramentas de migração diretamente no código, você pode criar o banco de dados manualmente com um script SQL, mas usar o **Entity Framework Core** para manipular as operações de dados.

No caso de migrações manuais, seria necessário criar a tabela e suas colunas usando um script SQL diretamente no PostgreSQL, mas isso é menos flexível em termos de desenvolvimento contínuo e manutenção do banco de dados.

### Resumo:

1. Instale o pacote `Npgsql.EntityFrameworkCore.PostgreSQL`.
2. Configure o `DbContext` para usar PostgreSQL com uma string de conexão adequada.
3. Crie modelos de entidades que representam as tabelas do banco de dados.
4. Gere e aplique migrações usando `dotnet ef migrations add` e `dotnet ef database update`.
5. Execute operações de CRUD usando o `DbContext`.