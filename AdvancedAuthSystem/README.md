# Advanced Authentication & Authorization System

Sistema completo de autenticação e autorização em .NET 9 demonstrando práticas avançadas de segurança.

## Funcionalidades

### Autenticação
- **JWT (JSON Web Tokens)**: Tokens de acesso e refresh tokens
- **2FA (Two-Factor Authentication)**: Autenticação de dois fatores com TOTP
- **Password Hashing**: BCrypt para armazenamento seguro de senhas
- **Token Refresh**: Renovação automática de tokens expirados

### Autorização
- **RBAC (Role-Based Access Control)**: Controle de acesso baseado em papéis
  - Roles: Admin, Manager, User, Guest
  - Hierarquia de permissões
  
- **ABAC (Attribute-Based Access Control)**: Controle de acesso baseado em atributos
  - Políticas baseadas em claims
  - Políticas baseadas em recursos
  - Políticas baseadas em contexto (horário, localização)
  
- **Custom Policies**: Políticas de autorização customizadas
  - MinimumAgePolicy
  - ResourceOwnerPolicy
  - DepartmentPolicy
  - TimeBasedPolicy

## Como Usar

### 1. Restaurar Dependências
```bash
dotnet restore
```

### 2. Executar o Projeto
```bash
dotnet run
```

### 3. Acessar Swagger
```
http://localhost:5000
ou
https://localhost:5001
```

## Fluxos de Autenticação

### 1. Registro de Usuário
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john.doe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "department": "IT",
  "dateOfBirth": "1990-01-01"
}
```

### 2. Login Básico
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin123!"
}

Response:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "expiresIn": 3600,
  "requiresTwoFactor": false
}
```

### 3. Habilitar 2FA
```http
POST /api/auth/enable-2fa
Authorization: Bearer {accessToken}

Response:
{
  "secret": "JBSWY3DPEHPK3PXP",
  "qrCodeUrl": "otpauth://totp/...",
  "manualEntryKey": "JBSWY3DPEHPK3PXP"
}
```

### 4. Login com 2FA
```http
POST /api/auth/verify-2fa
Content-Type: application/json

{
  "username": "admin",
  "code": "123456",
  "tempToken": "temp_token_from_login"
}
```

### 5. Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "abc123..."
}
```

## Exemplos de Autorização

### 1. RBAC - Apenas Admin
```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("api/resource/{id}")]
```

### 2. ABAC - Idade Mínima
```csharp
[Authorize(Policy = "MinimumAge21")]
[HttpGet("api/resource/restricted-content")]
```

### 3. Resource Owner
```csharp
[HttpPut("api/resource/{id}")]
// Autorização verificada no controller via IAuthorizationService
```

### 4. Departamento Específico
```csharp
[Authorize(Policy = "ITDepartment")]
[HttpGet("api/resource/it-resources")]
```

### 5. Horário Comercial
```csharp
[Authorize(Policy = "WorkingHours")]
[HttpPost("api/resource/sensitive-operation")]
```

## Usuários de Teste (seed data)

### Admin
- **Username**: `admin`
- **Password**: `Admin123!`
- **Roles**: Admin
- **Department**: IT

### Manager
- **Username**: `manager`
- **Password**: `Manager123!`
- **Roles**: Manager
- **Department**: IT

### User
- **Username**: `user`
- **Password**: `User123!`
- **Roles**: User
- **Department**: Sales

## Endpoints Principais

### Autenticação
- `POST /api/auth/register` - Registrar novo usuário
- `POST /api/auth/login` - Login
- `POST /api/auth/verify-2fa` - Verificar código 2FA
- `POST /api/auth/refresh` - Renovar token
- `POST /api/auth/enable-2fa` - Habilitar 2FA (requer autenticação)
- `POST /api/auth/disable-2fa` - Desabilitar 2FA (requer autenticação)
- `POST /api/auth/logout` - Logout (revogar refresh token)
- `GET /api/auth/me` - Obter dados do usuário atual

### Recursos (Demonstração de Autorização)
- `GET /api/resource` - Listar recursos públicos (sem auth)
- `GET /api/resource/my-resources` - Listar meus recursos (requer auth)
- `GET /api/resource/{id}` - Obter recurso (ABAC - owner ou público)
- `POST /api/resource` - Criar recurso (requer idade >= 18)
- `PUT /api/resource/{id}` - Atualizar recurso (ABAC - apenas owner)
- `DELETE /api/resource/{id}` - Deletar recurso (RBAC - apenas Admin)
- `GET /api/resource/it-resources` - Recursos de IT (ABAC - departamento IT)
- `POST /api/resource/sensitive-operation` - Operação sensível (ABAC - horário comercial)
- `GET /api/resource/restricted-content` - Conteúdo restrito (ABAC - idade >= 21)

## Políticas de Autorização

### Age-Based
- `MinimumAge18` - Idade mínima 18 anos
- `MinimumAge21` - Idade mínima 21 anos

### Role-Based
- `Admin` - Apenas administradores
- `AdminOrManager` - Admin ou Manager

### Department-Based
- `ITDepartment` - Apenas departamento IT
- `SalesDepartment` - Apenas departamento Sales
- `HRDepartment` - Apenas departamento HR

### Time-Based
- `WorkingHours` - Apenas horário comercial (9h-17h)
- `WeekdaysOnly` - Apenas dias úteis

### Resource-Based
- `ResourceOwner` - Dono do recurso, Admin ou Manager do departamento

### Claim-Based
- `SeniorLevel` - Nível sênior
- `JuniorLevel` - Nível júnior

### Permission-Based
- `CanManageUsers` - Permissão users.write

## Testando 2FA

1. Registre ou faça login com um usuário existente
2. Habilite 2FA: `POST /api/auth/enable-2fa`
3. Use o `secret` retornado em um app autenticador (Google Authenticator, Authy, etc.)
   - Ou use o `qrCodeUrl` para gerar um QR Code
4. Faça logout e login novamente
5. Você receberá `requiresTwoFactor: true` e um `tempToken`
6. Verifique com: `POST /api/auth/verify-2fa` usando o código do app autenticador

## Testando Políticas de Autorização

### Teste 1: RBAC
```bash
# Login como admin
# DELETE /api/resource/{id} -> Sucesso (200)

# Login como user
# DELETE /api/resource/{id} -> Forbidden (403)
```

### Teste 2: ABAC - Idade
```bash
# Criar usuário com < 18 anos
# POST /api/resource -> Forbidden (403)

# Criar usuário com >= 18 anos
# POST /api/resource -> Success (201)
```

### Teste 3: ABAC - Departamento
```bash
# Login como user do departamento IT
# GET /api/resource/it-resources -> Success (200)

# Login como user do departamento Sales
# GET /api/resource/it-resources -> Forbidden (403)
```

### Teste 4: ABAC - Horário
```bash
# POST /api/resource/sensitive-operation
# Durante horário comercial (9h-17h) -> Success (200)
# Fora do horário comercial -> Forbidden (403)
```

### Teste 5: Resource Owner
```bash
# Criar recurso como user1
# PUT /api/resource/{id} como user1 -> Success (200)
# PUT /api/resource/{id} como user2 -> Forbidden (403)
# PUT /api/resource/{id} como admin -> Success (200)
```

## Arquitetura

O projeto demonstra:

1. **Separation of Concerns**: Separação clara entre autenticação, autorização, serviços e controllers
2. **Dependency Injection**: Uso extensivo de DI para testabilidade
3. **Authorization Handlers**: Handlers customizados para lógica complexa de autorização
4. **Resource-Based Authorization**: Autorização baseada em propriedade/contexto do recurso
5. **Policy-Based Authorization**: Políticas reutilizáveis e componíveis
6. **JWT Best Practices**: Access tokens de curta duração + refresh tokens
7. **Security Best Practices**: BCrypt, TOTP, HTTPS, Claims

## Extensões Futuras

- OAuth 2.0 / OpenID Connect
- External providers (Google, Microsoft, GitHub)
- Rate limiting
- Audit logging
- Password policies
- Account lockout
- Email verification
- Password reset

## Tecnologias Utilizadas

- **.NET 9**
- **ASP.NET Core Web API**
- **Entity Framework Core** (InMemory)
- **JWT Bearer Authentication**
- **BCrypt.Net** (password hashing)
- **Custom Authorization Handlers**
- **Swagger/OpenAPI**

## Segurança

- Senhas hasheadas com BCrypt
- JWT tokens assinados
- 2FA com TOTP (RFC 6238)
- Refresh tokens com revogação
- Claims-based authorization
- Resource-based authorization
- Time-based access control
