# MoneyStorageApi

API minimalista em C# que demonstra como armazenar valores monetários no MySQL com segurança usando Entity Framework Core.

## 🔍 O que você aprende
- Configurar MySQL como banco de dados para uma API .NET 9
- Aplicar `decimal(18,2)` para armazenar dinheiro sem perda de precisão
- Registrar depósitos e saques com histórico (`MoneyMovement`)
- Usar `RowVersion` para proteger contra concorrência otimista
- Organizar operações financeiras em um serviço de domínio dedicado

## 🏗️ Estrutura
```
Data/
  MoneyStorageApi/
    Program.cs
    MoneyStorageApi.csproj
    appsettings*.json
    src/
      Data/MoneyStorageContext.cs
      Domain/(Account.cs, MoneyMovement.cs)
      DTOs/AccountDtos.cs
      Services/AccountService.cs
```

### Modelagem
- `Account` (Guid, OwnerName, Balance, CreatedAtUtc, RowVersion)
- `MoneyMovement` (Id, AccountId, Type, Amount, Description, OccurredAtUtc)
- Movimentos são gerados automaticamente a cada depósito/saque, inclusive no saldo inicial.

## ⚙️ Pré-requisitos
1. MySQL executando localmente (porta 3306)
2. Usuário com permissão de criação de schema
3. .NET 9 SDK instalado

## 🚀 Como rodar
```bash
cd Data/MoneyStorageApi
# Ajuste appsettings*.json com sua senha/porta

# 1. Criar primeira migration
 dotnet ef migrations add InitialMoneySchema

# 2. Aplicar schema no MySQL
 dotnet ef database update

# 3. Executar a API
 dotnet run
```
> Observação: o `dotnet ef` utiliza os pacotes já declarados no `csproj`. Instale a ferramenta global se ainda não tiver (`dotnet tool install --global dotnet-ef`).

## 📡 Endpoints principais
| Verbo | Rota | Corpo | Descrição |
|-------|------|-------|-----------|
| POST | `/accounts` | `{ "ownerName": "João", "initialBalance": 1000.00 }` | Cria uma conta e registra o depósito inicial |
| GET | `/accounts` | - | Lista contas com movimentos |
| GET | `/accounts/{id}` | - | Detalhe da conta |
| POST | `/accounts/{id}/deposit` | `{ "amount": 150.25, "description": "Bonus" }` | Deposita valor |
| POST | `/accounts/{id}/withdraw` | `{ "amount": 50.00, "description": "Compra" }` | Realiza saque (com validação de saldo) |

Todas as operações retornam `AccountResponse`, contendo saldo atual e o histórico ordenado.

## 🔐 Boas práticas aplicadas
- `decimal` para valores monetários e `HasColumnType("decimal(18,2)")`
- `RowVersion` para detectar gravações concorrentes
- Serviço `AccountService` concentra regras de negócio e garante `SaveChanges` único por operação
- `EnableRetryOnFailure` e `CommandTimeout` para resiliência nas conexões MySQL

## 🧪 Testando rapidamente
Arquivo `MoneyStorageApi.http` possui exemplos de requisições HTTP para VS Code/REST Client.

## 📎 Configuração adicional
- Altere a connection string de `appsettings.Development.json` para apontar para seu banco local.
- Se desejar publicar em produção, crie um usuário específico apenas com permissões necessárias e troque a credential em `appsettings.json`.

Bom estudo! 🎯
