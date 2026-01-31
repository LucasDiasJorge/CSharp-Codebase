# 🔐 Advanced Authentication System

Sistema completo de autenticação e autorização em .NET 9 demonstrando práticas avançadas de segurança.

---

## 📚 Conceitos Abordados

- **JWT Authentication**: Access tokens e refresh tokens
- **2FA (TOTP)**: Autenticação de dois fatores
- **RBAC**: Controle de acesso baseado em papéis
- **ABAC**: Controle de acesso baseado em atributos
- **Custom Policies**: Políticas de autorização customizadas
- **Password Hashing**: BCrypt para armazenamento seguro

---

## 🎯 Objetivos de Aprendizado

- Implementar autenticação JWT completa
- Configurar 2FA com TOTP
- Criar políticas de autorização customizadas
- Entender RBAC vs ABAC
- Aplicar boas práticas de segurança

---

## 📂 Estrutura do Projeto

```
AdvancedAuthSystem/
├── Controllers/
│   ├── AuthController.cs        # Endpoints de autenticação
│   └── ResourceController.cs    # Endpoints protegidos
├── Services/
│   ├── AuthService.cs           # Lógica de autenticação
│   ├── TokenService.cs          # Geração de tokens
│   └── PasswordHasher.cs        # Hash de senhas
├── Handlers/
│   └── Handlers.cs              # Authorization handlers
├── Models/
│   ├── User.cs                  # Entidade de usuário
│   └── DTOs.cs                  # Data transfer objects
├── Requirements.cs              # Authorization requirements
├── PolicyNames.cs               # Constantes de políticas
├── AppDbContext.cs              # Entity Framework context
└── Program.cs                   # Configuração da aplicação
```

---

## 🚀 Como Executar

```bash
cd AdvancedAuthSystem
dotnet restore
dotnet run
```

**Swagger**: http://localhost:5000 ou https://localhost:5001

---

## 👥 Usuários de Teste

| Usuário | Senha | Roles | Departamento |
|---------|-------|-------|--------------|
| `admin` | `Admin123!` | Admin | IT |
| `manager` | `Manager123!` | Manager | IT |
| `user` | `User123!` | User | Sales |

---

## 📋 Endpoints Principais

### Autenticação

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/auth/register` | Registrar usuário |
| `POST` | `/api/auth/login` | Login |
| `POST` | `/api/auth/verify-2fa` | Verificar código 2FA |
| `POST` | `/api/auth/refresh` | Renovar token |
| `POST` | `/api/auth/enable-2fa` | Habilitar 2FA |
| `GET` | `/api/auth/me` | Dados do usuário atual |

### Recursos (Demonstração de Autorização)

| Método | Endpoint | Política |
|--------|----------|----------|
| `GET` | `/api/resource` | Público |
| `POST` | `/api/resource` | Idade >= 18 |
| `PUT` | `/api/resource/{id}` | Resource Owner |
| `DELETE` | `/api/resource/{id}` | Admin Only |
| `GET` | `/api/resource/it-resources` | Departamento IT |

---

## 💡 Exemplos de Uso

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin123!"
}
```

**Resposta:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "expiresIn": 3600,
  "requiresTwoFactor": false
}
```

### Habilitar 2FA

```http
POST /api/auth/enable-2fa
Authorization: Bearer {accessToken}
```

---

## 🛡️ Políticas de Autorização

### Age-Based
- `MinimumAge18` / `MinimumAge21`

### Role-Based
- `Admin` / `AdminOrManager`

### Department-Based
- `ITDepartment` / `SalesDepartment`

### Time-Based
- `WorkingHours` (9h-17h)
- `WeekdaysOnly`

### Resource-Based
- `ResourceOwner` - Dono do recurso ou Admin

---

## ✅ Boas Práticas Demonstradas

- Separation of Concerns
- Dependency Injection
- JWT com tokens de curta duração
- Refresh tokens com revogação
- BCrypt para hash de senhas
- Claims-based authorization
- Resource-based authorization

---

## 🔜 Extensões Futuras

- OAuth 2.0 / OpenID Connect
- External providers (Google, GitHub)
- Rate limiting
- Audit logging
- Account lockout
- Email verification

---

## 🔗 Referências

- [ASP.NET Core Security](https://docs.microsoft.com/aspnet/core/security/)
- [JWT.io](https://jwt.io/)
- [TOTP RFC 6238](https://tools.ietf.org/html/rfc6238)
