# 🎯 C# 101 - Guia Completo de Desenvolvimento .NET

![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)

> 📚 **Repositório educacional** com exemplos práticos, conceitos fundamentais e guias completos para desenvolvimento C# e .NET

## 📋 Índice

- [🎯 C# 101 - Guia Completo de Desenvolvimento .NET](#-c-101---guia-completo-de-desenvolvimento-net)
  - [📋 Índice](#-índice)
  - [📖 Sobre o Projeto](#-sobre-o-projeto)
  - [�️ Estrutura do Projeto](#️-estrutura-do-projeto)
  - [🚀 Começando](#-começando)
    - [📋 Pré-requisitos](#-pré-requisitos)
    - [⚡ Instalação e Configuração](#-instalação-e-configuração)
  - [🛠️ Comandos Essenciais do .NET CLI](#️-comandos-essenciais-do-net-cli)
    - [📌 Criando Projetos e Arquivos](#-criando-projetos-e-arquivos)
    - [✅ Templates Disponíveis](#-templates-disponíveis)
    - [📁 Criar Arquivos Individuais](#-criar-arquivos-individuais)
    - [🔧 Manipulação de Projetos](#-manipulação-de-projetos)
  - [📦 Gerenciamento de Dependências](#-gerenciamento-de-dependências)
  - [🏗️ Gerenciar Soluções e Projetos](#️-gerenciar-soluções-e-projetos)
  - [💾 Entity Framework Core](#-entity-framework-core)
  - [🧪 Testes Unitários](#-testes-unitários)
  - [🚀 Publicação e Deploy](#-publicação-e-deploy)
  - [ℹ️ Informações do Sistema](#ℹ️-informações-do-sistema)
  - [🧹 Limpeza e Manutenção](#-limpeza-e-manutenção)
  - [🏗️ Princípios SOLID](#️-princípios-solid)
  - [🤝 Como Contribuir](#-como-contribuir)
  - [📄 Licença](#-licença)

## 📖 Sobre o Projeto

Este repositório contém uma coleção abrangente de exemplos práticos, padrões de design e conceitos fundamentais para desenvolvimento em **C#** e **.NET**. Ideal para desenvolvedores iniciantes e intermediários que desejam aprimorar suas habilidades.

### 🎯 Objetivos
- Fornecer exemplos práticos de conceitos C#/.NET
- Demonstrar implementações de padrões de design
- Apresentar boas práticas de desenvolvimento
- Servir como referência rápida para comandos e configurações

## 🗂️ Estrutura do Projeto

```
CSharp-101/
├── 🔄 Asynchronous/           # Programação assíncrona
├── 🔐 Auth/                   # Autenticação e autorização
├── ⚙️  BackgroundWorker/       # Workers em background
├── 📋 ClassToDTO/             # Mapeamento de objetos
├── 🗜️  CompressDecompress/     # Compressão de dados
├── 📚 Course/                 # Exemplos básicos
├── 🔗 CustomMiddleware/       # Middlewares personalizados
├── 💾 Dapper/                 # Micro ORM Dapper
├── 🏗️  DesignPattern/          # Padrões de design
├── 📊 EF/                     # Entity Framework
├── 🔒 EncryptDecrypt/         # Criptografia
├── 📡 Events/                 # Sistema de eventos
├── 🔄 Kafka/                  # Apache Kafka
├── 🔍 Linq/                   # Language Integrated Query
├── 🌐 MinimalApiDemo/         # APIs mínimas
├── 🗄️  Postgres/               # PostgreSQL
├── 📬 QueueExample/           # Filas de mensagem
├── 🐰 RabbitMQ/               # Message broker
├── ⚡ Redis/                  # Cache Redis
├── 🔍 Reflection/             # Reflexão em C#
├── 🔐 SafeVault/              # Armazenamento seguro
├── 🧪 Tests/                  # Testes unitários
└── 🌐 WebApplication/         # Aplicações web
```

## 🚀 Começando

### 📋 Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) ou superior
- [Visual Studio Code](https://code.visualstudio.com/) ou [Visual Studio](https://visualstudio.microsoft.com/)
- [Git](https://git-scm.com/)

### ⚡ Instalação e Configuração

1. **Clone o repositório:**
```bash
git clone https://github.com/LucasDiasJorge/CSharp-101.git
cd CSharp-101
```

2. **Restaure as dependências:**
```bash
dotnet restore
```

3. **Compile o projeto:**
```bash
dotnet build
```

## �️ Comandos Essenciais do .NET CLI

### 📌 Criando Projetos e Arquivos

**Sintaxe básica:**
```bash
dotnet new <TEMPLATE> -n <NOME_DO_PROJETO> --use-program-main
```

**Exemplo prático:**
```bash
dotnet new console -n MinhaApp --use-program-main
```

### ✅ Templates Disponíveis
| **Template** | **Comando** | **Descrição** |
|--------------|-------------|---------------|
| Console | `dotnet new console` | �️ Aplicação console básica |
| Web API | `dotnet new webapi` | 🌐 API REST ASP.NET Core |
| MVC | `dotnet new mvc` | 🏗️ Aplicação MVC completa |
| Blazor Server | `dotnet new blazorserver` | ⚡ App Blazor server-side |
| Blazor WASM | `dotnet new blazorwasm` | 🌐 App Blazor client-side |
| Class Library | `dotnet new classlib` | 📚 Biblioteca de classes (.dll) |
| Worker Service | `dotnet new worker` | ⚙️ Serviço background |
| gRPC | `dotnet new grpc` | 🔄 Serviço gRPC |
| xUnit Test | `dotnet new xunit` | 🧪 Projeto de testes xUnit |
| Razor Pages | `dotnet new razor` | 📄 Aplicação Razor Pages |

### 📁 Criar Arquivos Individuais

| **Tipo** | **Comando** | **Exemplo** |
|----------|-------------|-------------|
| Solução | `dotnet new sln` | `dotnet new sln -n MinhaSolucao` |
| Classe | `dotnet new class` | `dotnet new class -n MinhaClasse` |
| Interface | `dotnet new interface` | `dotnet new interface -n IServico` |
| Enum | `dotnet new enum` | `dotnet new enum -n StatusEnum` |
| Record | `dotnet new record` | `dotnet new record -n Pessoa` |

### 🔧 Manipulação de Projetos

**Criar solução e adicionar projetos:**
```bash
# Criar uma solução
dotnet new sln -n MinhaSolucao

# Criar projeto console
dotnet new console -n MinhaApp

# Adicionar projeto à solução
dotnet sln add MinhaApp/MinhaApp.csproj
```

**Comandos básicos:**
```bash
# Restaurar dependências
dotnet restore

# Compilar projeto
dotnet build

# Executar projeto
dotnet run

# Executar com configuração específica
dotnet run --configuration Release
```

## 📦 Gerenciamento de Dependências

### ➕ Adicionar Pacotes NuGet
```bash
# Sintaxe básica
dotnet add package <NOME_DO_PACOTE>

# Versão específica
dotnet add package <PACOTE> --version <VERSAO>

# Exemplos comuns
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Newtonsoft.Json
dotnet add package AutoMapper
dotnet add package Serilog
```

### ➖ Remover e Gerenciar Pacotes
```bash
# Remover pacote
dotnet remove package <NOME_DO_PACOTE>

# Listar pacotes instalados
dotnet list package

# Listar pacotes desatualizados
dotnet list package --outdated

# Atualizar pacotes
dotnet add package <PACOTE> --version <NOVA_VERSAO>
```

## 🏗️ Gerenciar Soluções e Projetos

```bash
# Adicionar projeto à solução
dotnet sln add <CAMINHO_DO_PROJETO>

# Remover projeto da solução
dotnet sln remove <CAMINHO_DO_PROJETO>

# Listar projetos na solução
dotnet sln list

# Compilar solução inteira
dotnet build <NOME_DA_SOLUCAO>.sln
```

## 💾 Entity Framework Core

### � Instalação
```bash
# Pacotes essenciais
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design

# Provedores de banco de dados
dotnet add package Microsoft.EntityFrameworkCore.SqlServer    # SQL Server
dotnet add package Microsoft.EntityFrameworkCore.Sqlite       # SQLite
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL      # PostgreSQL
dotnet add package Pomelo.EntityFrameworkCore.MySql           # MySQL
```

### � Migrations
```bash
# Criar uma migration
dotnet ef migrations add <NomeDaMigration>

# Aplicar migrations
dotnet ef database update

# Reverter para migration específica
dotnet ef database update <NomeDaMigration>

# Remover última migration
dotnet ef migrations remove

# Listar migrations
dotnet ef migrations list

# Script SQL da migration
dotnet ef migrations script
```

### 🗄️ Banco de Dados
```bash
# Criar banco de dados
dotnet ef database update

# Remover banco de dados
dotnet ef database drop

# Informações do banco
dotnet ef dbcontext info

# Scaffold de banco existente
dotnet ef dbcontext scaffold "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer
```

## 🧪 Testes Unitários

### � Criar Projetos de Teste
```bash
# xUnit (recomendado)
dotnet new xunit -n Testes.MinhaApp

# MSTest
dotnet new mstest -n Testes.MinhaApp

# NUnit
dotnet new nunit -n Testes.MinhaApp
```

### ▶️ Executar Testes
```bash
# Executar todos os testes
dotnet test

# Executar com relatório detalhado
dotnet test --verbosity normal

# Executar testes específicos
dotnet test --filter "TestCategory=Unit"

# Executar com cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

## 🚀 Publicação e Deploy

### � Publicação Básica
```bash
# Publicação simples
dotnet publish -c Release -o ./publicado

# Publicação com runtime específico
dotnet publish -c Release -r win-x64 --self-contained true

# Publicação para Linux
dotnet publish -c Release -r linux-x64 --self-contained true

# Arquivo único
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### 🐳 Docker
```bash
# Publicação para container
dotnet publish -c Release --runtime linux-x64 --self-contained true -o ./publicado

# Criar imagem Docker (requer Dockerfile)
docker build -t minha-app .
```

## ℹ️ Informações do Sistema

```bash
# Versão do .NET
dotnet --version

# Informações detalhadas
dotnet --info

# Listar SDKs instalados
dotnet --list-sdks

# Listar runtimes instalados
dotnet --list-runtimes

# Verificar templates disponíveis
dotnet new --list
```

## 🧹 Limpeza e Manutenção

```bash
# Limpar build temporário
dotnet clean

# Limpar cache NuGet
dotnet nuget locals all --clear

# Limpar cache específico
dotnet nuget locals global-packages --clear

# Verificar e restaurar projetos
dotnet restore --force
```

---

## 🏗️ Princípios SOLID

Os princípios **SOLID** são diretrizes fundamentais para escrever código limpo, manutenível e extensível:

### 🔸 S — Single Responsibility Principle (SRP)

> **"Uma classe deve ter apenas uma razão para mudar"**

❌ **Violando o SRP:**
```csharp
public class UserManager
{
    public void CreateUser(User user) { /* lógica de criação */ }
    public void SendEmailNotification(User user) { /* lógica de email */ }
    public void LogActivity(string message) { /* lógica de log */ }
}
```

✅ **Aplicando o SRP:**
```csharp
public class UserService
{
    public void CreateUser(User user) { /* lógica de criação */ }
}

public class EmailService
{
    public void SendNotification(User user) { /* lógica de email */ }
}

public class LoggingService
{
    public void LogActivity(string message) { /* lógica de log */ }
}
```

### 🔸 O — Open/Closed Principle (OCP)

> **"Aberto para extensão, fechado para modificação"**

❌ **Violando o OCP:**
```csharp
public class PaymentProcessor
{
    public void ProcessPayment(string paymentType, decimal amount)
    {
        if (paymentType == "CreditCard")
            ProcessCreditCard(amount);
        else if (paymentType == "PayPal")
            ProcessPayPal(amount);
        // Adicionar novo tipo requer modificar esta classe
    }
}
```

✅ **Aplicando o OCP:**
```csharp
public interface IPaymentMethod
{
    void ProcessPayment(decimal amount);
}

public class CreditCardPayment : IPaymentMethod
{
    public void ProcessPayment(decimal amount) { /* implementação */ }
}

public class PayPalPayment : IPaymentMethod
{
    public void ProcessPayment(decimal amount) { /* implementação */ }
}

public class PaymentProcessor
{
    public void ProcessPayment(IPaymentMethod paymentMethod, decimal amount)
    {
        paymentMethod.ProcessPayment(amount);
    }
}
```

### 🔸 L — Liskov Substitution Principle (LSP)

> **"Subclasses devem ser substituíveis por suas superclasses"**

❌ **Violando o LSP:**
```csharp
public abstract class Bird
{
    public abstract void Fly();
}

public class Eagle : Bird
{
    public override void Fly() => Console.WriteLine("Flying high!");
}

public class Penguin : Bird
{
    public override void Fly() => throw new NotSupportedException();
}
```

✅ **Aplicando o LSP:**
```csharp
public abstract class Bird
{
    public abstract void Move();
}

public interface IFlyingBird
{
    void Fly();
}

public class Eagle : Bird, IFlyingBird
{
    public override void Move() => Fly();
    public void Fly() => Console.WriteLine("Flying high!");
}

public class Penguin : Bird
{
    public override void Move() => Console.WriteLine("Swimming!");
}
```

### 🔸 I — Interface Segregation Principle (ISP)

> **"Clientes não devem depender de interfaces que não utilizam"**

❌ **Violando o ISP:**
```csharp
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
}

public class Robot : IWorker
{
    public void Work() { /* implementa */ }
    public void Eat() { throw new NotImplementedException(); }
    public void Sleep() { throw new NotImplementedException(); }
}
```

✅ **Aplicando o ISP:**
```csharp
public interface IWorkable
{
    void Work();
}

public interface IEatable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

public class Human : IWorkable, IEatable, ISleepable
{
    public void Work() { /* implementa */ }
    public void Eat() { /* implementa */ }
    public void Sleep() { /* implementa */ }
}

public class Robot : IWorkable
{
    public void Work() { /* implementa */ }
}
```

### 🔸 D — Dependency Inversion Principle (DIP)

> **"Dependa de abstrações, não de implementações concretas"**

❌ **Violando o DIP:**
```csharp
public class OrderService
{
    private readonly SqlServerRepository repository;
    private readonly EmailService emailService;

    public OrderService()
    {
        repository = new SqlServerRepository();
        emailService = new EmailService();
    }
}
```

✅ **Aplicando o DIP:**
```csharp
public interface IRepository
{
    void Save(Order order);
}

public interface INotificationService
{
    void SendNotification(string message);
}

public class OrderService
{
    private readonly IRepository repository;
    private readonly INotificationService notificationService;

    public OrderService(IRepository repository, INotificationService notificationService)
    {
        this.repository = repository;
        this.notificationService = notificationService;
    }
}
```

---

## 🤝 Como Contribuir

1. **Fork** o repositório
2. Crie uma **branch** para sua feature (`git checkout -b feature/MinhaFeature`)
3. **Commit** suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. **Push** para a branch (`git push origin feature/MinhaFeature`)
5. Abra um **Pull Request**

### 📝 Diretrizes de Contribuição

- Siga as convenções de código C#
- Adicione documentação adequada
- Inclua testes para novas funcionalidades
- Mantenha o README atualizado

---

<div align="center">

**🚀 Feito com ❤️ para a comunidade de desenvolvedores C#/.NET**

[⭐ Deixe uma estrela](https://github.com/LucasDiasJorge/CSharp-101) • [🐛 Reporte um bug](https://github.com/LucasDiasJorge/CSharp-101/issues) • [💡 Solicite uma feature](https://github.com/LucasDiasJorge/CSharp-101/issues)

</div>