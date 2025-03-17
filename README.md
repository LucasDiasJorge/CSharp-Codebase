# **📌 `dotnet new` → Criando Projetos e Arquivos**
O comando `dotnet new` é usado para criar novos projetos, soluções e arquivos específicos.  

### **✅ Criar um Novo Projeto**
```sh
dotnet new <TEMPLATE> -n <NOME_DO_PROJETO> --use-program-main
```
🔹 **Exemplo:** Criar um projeto **Console** chamado `MinhaApp`  
```sh
dotnet new console -n MinhaApp --use-program-main
```

### **✅ Templates Disponíveis**
| **Comando**                  | **Descrição**                     |
|------------------------------|---------------------------------|
| `dotnet new console`         | Projeto Console                 |
| `dotnet new web`             | Aplicação Web API (ASP.NET Core) |
| `dotnet new mvc`             | Projeto ASP.NET MVC             |
| `dotnet new webapi`          | API REST ASP.NET Core           |
| `dotnet new classlib`        | Biblioteca de Classes (.dll)    |
| `dotnet new blazorserver`    | Aplicação Blazor Server         |
| `dotnet new blazorwasm`      | Aplicação Blazor WebAssembly    |
| `dotnet new razorclasslib`   | Biblioteca de Componentes Razor |
| `dotnet new worker`          | Serviço Background (Worker)     |
| `dotnet new grpc`            | Serviço gRPC                    |
| `dotnet new xunit`           | Testes Unitários xUnit          |
| `dotnet new mstest`          | Testes Unitários MSTest         |
| `dotnet new nunit`           | Testes Unitários NUnit          |

---

### **✅ Criar Arquivos Individuais**
| **Comando**                    | **Descrição**                   |
|--------------------------------|--------------------------------|
| `dotnet new sln`               | Criar uma solução (.sln)      |
| `dotnet new class`             | Criar uma classe              |
| `dotnet new interface`         | Criar uma interface           |
| `dotnet new enum`              | Criar uma enumeração          |
| `dotnet new record`            | Criar um record (C# 9+)       |
| `dotnet new page`              | Criar uma página Razor        |

🔹 **Exemplo:** Criar uma **interface** chamada `IMeuServico.cs`  
```sh
dotnet new interface -n IMeuServico
```

---

# **📌 Manipulação de Projetos .NET**
## **✅ Criar, Restaurar e Executar Projetos**
### **🔹 Criar uma solução e adicionar projetos**
```sh
dotnet new sln -n MinhaSolucao      # Criar uma solução
dotnet new console -n MinhaApp      # Criar um projeto Console
dotnet sln add MinhaApp/MinhaApp.csproj  # Adicionar o projeto à solução
```

### **🔹 Restaurar dependências do projeto**
```sh
dotnet restore
```

### **🔹 Compilar um projeto**
```sh
dotnet build
```

### **🔹 Executar um projeto**
```sh
dotnet run
```

---

## **✅ Trabalhando com Dependências (NuGet)**
### **🔹 Adicionar um pacote NuGet**
```sh
dotnet add package <NOME_DO_PACOTE>
```
🔹 **Exemplo:** Adicionar **Entity Framework Core**  
```sh
dotnet add package Microsoft.EntityFrameworkCore
```

### **🔹 Remover um pacote NuGet**
```sh
dotnet remove package <NOME_DO_PACOTE>
```

### **🔹 Listar pacotes instalados**
```sh
dotnet list package
```

### **🔹 Atualizar todos os pacotes**
```sh
dotnet nuget update source
```

---

## **✅ Gerenciar Projetos**
### **🔹 Adicionar um Projeto à Solução**
```sh
dotnet sln add <CAMINHO_DO_PROJETO>
```

### **🔹 Remover um Projeto da Solução**
```sh
dotnet sln remove <CAMINHO_DO_PROJETO>
```

### **🔹 Listar Projetos em uma Solução**
```sh
dotnet sln list
```

---

## **✅ Trabalhando com Banco de Dados (Entity Framework Core)**
### **🔹 Instalar Entity Framework Core**
```sh
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer    # Se for usar SQL Server
dotnet add package Microsoft.EntityFrameworkCore.Sqlite       # Se for usar SQLite
dotnet add package Microsoft.EntityFrameworkCore.PostgreSQL   # Se for usar PostgreSQL
```

### **🔹 Criar uma Migration**
```sh
dotnet ef migrations add Inicial
```

### **🔹 Atualizar o Banco de Dados**
```sh
dotnet ef database update
```

### **🔹 Remover Última Migration**
```sh
dotnet ef migrations remove
```

### **🔹 Listar Migrations**
```sh
dotnet ef migrations list
```

---

## **✅ Testes Unitários**
### **🔹 Criar um Projeto de Teste**
```sh
dotnet new xunit -n TestesMinhaApp
dotnet new mstest -n TestesMinhaApp
dotnet new nunit -n TestesMinhaApp
```

### **🔹 Rodar Testes**
```sh
dotnet test
```

---

## **✅ Publicação e Deploy**
### **🔹 Publicar um Projeto**
```sh
dotnet publish -c Release -o ./publicado
```

### **🔹 Publicar para um Container Docker**
```sh
dotnet publish -c Release --runtime linux-x64 --self-contained true -o ./publicado
```

---

## **✅ Informação sobre o Ambiente**
### **🔹 Versão do .NET instalada**
```sh
dotnet --version
```

### **🔹 Listar SDKs Instalados**
```sh
dotnet --list-sdks
```

### **🔹 Listar Runtimes Instaladas**
```sh
dotnet --list-runtimes
```

---

## **✅ Limpeza de Projetos**
### **🔹 Limpar Build Temporário**
```sh
dotnet clean
```

### **🔹 Remover Dependências e Arquivos de Build**
```sh
dotnet nuget locals all --clear
```

---

# **🚀 Conclusão**
Agora você tem um **guia completo** dos comandos essenciais do **.NET CLI**! 🎯  

📌 **Se estiver iniciando um projeto:**  
```sh
dotnet new webapi -n MinhaApi
cd MinhaApi
dotnet run
```

📌 **Se quiser adicionar EF Core:**  
```sh
dotnet add package Microsoft.EntityFrameworkCore
dotnet ef migrations add Inicial
dotnet ef database update
```

📌 **Se quiser compilar e executar:**  
```sh
dotnet build
dotnet run
```
