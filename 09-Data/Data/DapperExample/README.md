# DapperExample

## Visão geral

Exemplo de uso do Dapper como micro-ORM para operações com MySQL, incluindo transações e criptografia de senhas.

## Conceitos abordados

- **Dapper**: Micro-ORM de alta performance para .NET
- **TransactionScope**: Gerenciamento de transações distribuídas
- **BCrypt**: Hashing seguro de senhas
- **MySQL**: Conexão e operações com MySQL
- **Minimal API**: Estrutura de API minimalista do .NET

## Objetivos de aprendizagem

- Executar queries SQL com Dapper de forma eficiente
- Gerenciar transações para garantir consistência de dados
- Implementar hashing seguro de senhas com BCrypt
- Estruturar modelos de dados com relacionamentos

## Estrutura do projeto

```text
DapperExample/
+-- Properties/
|   \-- launchSettings.json
+-- src/
|   +-- Database/
|   \-- Models/
+-- appsettings.Development.json
+-- appsettings.json
+-- DapperExample.csproj
+-- DapperExample.csproj.user
+-- DapperExample.http
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 09-Data/Data/DapperExample/DapperExample.csproj
```

## Boas práticas e pontos de atenção

- Usar TransactionScope para operações que envolvem múltiplas tabelas
- Nunca armazenar senhas em texto plano
- Usar salt único por senha com BCrypt
- Parametrizar queries para evitar SQL Injection

### Pontos de Atenção

- Configurar connection string em `appsettings.json` (não hardcoded)
- BCrypt é intencionalmente lento - ideal para senhas
- TransactionScope requer DTC em cenários distribuídos

## Conteúdo complementar

##### Estrutura do Projeto

```
DapperExample/
├── src/
│   ├── Models/
│   │   ├── BaseModel.cs
│   │   ├── User.cs
│   │   └── Company.cs
│   └── Database/
│       └── Scripts/
│           └── CreateTables.sql
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── Program.cs
└── README.md
```

##### Pré-requisitos

- .NET 9.0 SDK
- MySQL Server rodando
- Banco de dados `my-db` criado

##### Configuração do Banco

```sql
CREATE DATABASE IF NOT EXISTS `my-db`;

CREATE TABLE companies (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL
);

CREATE TABLE users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    Password VARCHAR(255) NOT NULL,
    company_id INT,
    FOREIGN KEY (company_id) REFERENCES companies(Id)
);
```

##### Conexão e Insert com Transação

```csharp
using (MySqlConnection connection = new MySqlConnection(connectionString))
{
    connection.Open();

    using (TransactionScope transaction = new TransactionScope())
    {
        // Inserir empresa
        string insertCompanyQuery = @"INSERT INTO companies (Name) VALUES (@Name);";
        connection.Execute(insertCompanyQuery, new { Name = "Minha Empresa" });

        // Obter ID recém-inserido
        int companyId = connection.QuerySingle<int>("SELECT LAST_INSERT_ID();");

        // Inserir usuário vinculado
        string insertUserQuery = @"INSERT INTO users (Name, Email, Password, company_id) 
                                   VALUES (@Name, @Email, @Password, @CompanyId);";

        string salt = BCryptHelper.GenerateSalt(12);
        User newUser = new User
        {
            Name = "Lucas",
            Email = "lucas@example.com",
            Password = BCryptHelper.HashPassword("123456", salt),
            CompanyId = companyId
        };

        connection.Execute(insertUserQuery, newUser);
        
        // Commit
        transaction.Complete();
    }
}
```

##### Select com Dapper

```csharp
IEnumerable<User> users = connection.Query<User>("SELECT * FROM users");
foreach (User user in users)
{
    Console.WriteLine($"{user.Id}: {user.Name} - {user.Email}");
}
```

##### Dependências

| Pacote | Descrição |
|--------|-----------|
| `Dapper` | Micro-ORM |
| `MySqlConnector` | Driver MySQL |
| `DevOne.Security.Cryptography.BCrypt` | Hashing BCrypt |

## Referências

- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [MySqlConnector](https://mysqlconnector.net/)
- [BCrypt.Net](https://github.com/BcryptNet/bcrypt.net)
