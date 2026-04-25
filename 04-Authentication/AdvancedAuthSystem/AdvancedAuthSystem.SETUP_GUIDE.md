# Advanced Authentication & Authorization System - Setup Guide

## Estrutura de Arquivos Criada

Os seguintes arquivos foram criados na raiz do repositório com o prefixo `AdvancedAuthSystem`:

### Arquivos Principais
- `AdvancedAuthSystem.csproj` - Projeto
- `AdvancedAuthSystem.Program.cs` - Entry point e configuração
- `AdvancedAuthSystem.appsettings.json` - Configurações
- `AdvancedAuthSystem.appsettings.Development.json` - Configurações de desenvolvimento
- `AdvancedAuthSystem.launchSettings.json` - Perfis de execução
- `AdvancedAuthSystem.README.md` - Documentação completa
- `AdvancedAuthSystem.http` - Testes HTTP

### Modelos e DTOs
- `AdvancedAuthSystem.User.cs` - Modelos de dados
- `AdvancedAuthSystem.DTOs.cs` - DTOs e Records

### Data
- `AdvancedAuthSystem.AppDbContext.cs` - Contexto EF Core com seed data

### Serviços
- `AdvancedAuthSystem.PasswordHasher.cs` - Hash de senhas e 2FA/TOTP
- `AdvancedAuthSystem.TokenService.cs` - Geração e validação de JWT
- `AdvancedAuthSystem.AuthService.cs` - Lógica de autenticação

### Autorização
- `AdvancedAuthSystem.PolicyNames.cs` - Constantes de políticas
- `AdvancedAuthSystem.Requirements.cs` - Requirements customizados
- `AdvancedAuthSystem.Handlers.cs` - Handlers de autorização

### Controllers
- `AdvancedAuthSystem.AuthController.cs` - Endpoints de autenticação
- `AdvancedAuthSystem.ResourceController.cs` - Demonstração de autorização

## Opção 1: Organização Manual (Recomendado)

### Passo 1: Criar Estrutura de Diretórios
```batch
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101"
mkdir AdvancedAuthSystem
cd AdvancedAuthSystem
mkdir Models DTOs Services Authorization Controllers Data Properties
mkdir DTOs\Auth DTOs\User DTOs\Resource
mkdir Authorization\Policies Authorization\Requirements Authorization\Handlers
```

### Passo 2: Mover Arquivos
Mova os arquivos da raiz para suas respectivas pastas:

**Raiz do projeto (AdvancedAuthSystem/):**
- `AdvancedAuthSystem.csproj` → `AdvancedAuthSystem.csproj`
- `AdvancedAuthSystem.Program.cs` → `Program.cs`
- `AdvancedAuthSystem.appsettings.json` → `appsettings.json`
- `AdvancedAuthSystem.appsettings.Development.json` → `appsettings.Development.json`
- `AdvancedAuthSystem.README.md` → `README.md`
- `AdvancedAuthSystem.http` → `AdvancedAuthSystem.http`

**Properties/:**
- `AdvancedAuthSystem.launchSettings.json` → `Properties/launchSettings.json`

**Models/:**
- `AdvancedAuthSystem.User.cs` → `Models/Models.cs`

**DTOs/:**
- `AdvancedAuthSystem.DTOs.cs` → `DTOs/DTOs.cs`

**Data/:**
- `AdvancedAuthSystem.AppDbContext.cs` → `Data/AppDbContext.cs`

**Services/:**
- `AdvancedAuthSystem.PasswordHasher.cs` → `Services/PasswordHasher.cs`
- `AdvancedAuthSystem.TokenService.cs` → `Services/TokenService.cs`
- `AdvancedAuthSystem.AuthService.cs` → `Services/AuthService.cs`

**Authorization/Policies/:**
- `AdvancedAuthSystem.PolicyNames.cs` → `Authorization/Policies/PolicyNames.cs`

**Authorization/Requirements/:**
- `AdvancedAuthSystem.Requirements.cs` → `Authorization/Requirements/Requirements.cs`

**Authorization/Handlers/:**
- `AdvancedAuthSystem.Handlers.cs` → `Authorization/Handlers/Handlers.cs`

**Controllers/:**
- `AdvancedAuthSystem.AuthController.cs` → `Controllers/AuthController.cs`
- `AdvancedAuthSystem.ResourceController.cs` → `Controllers/ResourceController.cs`

### Passo 3: Executar o Projeto
```bash
cd AdvancedAuthSystem
dotnet restore
dotnet build
dotnet run
```

Acesse: http://localhost:5000

## Opção 2: Usar Script de Setup

Execute o script de setup criado:
```batch
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101"
setup-advancedauth.bat
```

## Opção 3: Executar Diretamente da Raiz (Mais Rápido para Testar)

Como todos os arquivos estão na raiz com prefixo, você pode executar diretamente:

```bash
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101"
dotnet restore AdvancedAuthSystem.csproj
dotnet build AdvancedAuthSystem.csproj
dotnet run --project AdvancedAuthSystem.csproj
```

**Observação:** Esta opção funciona, mas não segue a estrutura de pastas convencional.

## Verificar Instalação

Após executar, você deve ver:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started
```

Acesse o Swagger em: **http://localhost:5000**

## Testar Rapidamente

### 1. Login como Admin
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin123!"
}
```

### 2. Usar o Token Retornado
Copie o `accessToken` da resposta e use em requisições subsequentes:
```http
GET http://localhost:5000/api/auth/me
Authorization: Bearer {accessToken}
```

## Funcionalidades Principais

✅ **JWT Authentication** - Access e Refresh Tokens
✅ **2FA/TOTP** - Autenticação de dois fatores
✅ **RBAC** - Role-Based Access Control
✅ **ABAC** - Attribute-Based Access Control
✅ **Custom Policies** - Políticas customizadas (idade, departamento, horário)
✅ **Resource-Based Authorization** - Autorização baseada em propriedade
✅ **BCrypt** - Hash seguro de senhas
✅ **Swagger/OpenAPI** - Documentação interativa

## Usuários de Teste

| Username | Password | Role | Department |
|----------|----------|------|------------|
| admin | Admin123! | Admin | IT |
| manager | Manager123! | Manager | IT |
| user | User123! | User | Sales |

## Próximos Passos

1. Explore a documentação no README.md principal
2. Teste os endpoints no Swagger
3. Use o arquivo .http para testes manuais
4. Experimente habilitar 2FA em um usuário
5. Teste diferentes políticas de autorização

## Adicionar ao Solution

Para adicionar ao CSharp-101.sln (após organizar a estrutura):
```bash
dotnet sln CSharp-101.sln add AdvancedAuthSystem/AdvancedAuthSystem.csproj
```

## Troubleshooting

### Porta em uso
Se a porta 5000 estiver em uso, modifique em `launchSettings.json` ou use:
```bash
dotnet run --urls "http://localhost:5500"
```

### Build errors
Certifique-se de ter o .NET 9 SDK instalado:
```bash
dotnet --version
```

### Database issues
O projeto usa InMemory database. Se houver problemas, o banco é recriado a cada execução.
