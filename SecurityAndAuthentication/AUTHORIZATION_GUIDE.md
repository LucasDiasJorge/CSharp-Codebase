# Proteção de Endpoints com Autorização por Roles

## Resumo das Alterações

Implementamos um sistema completo de autorização baseado em roles para proteger os endpoints de gerenciamento de usuários e roles.

## ✅ Endpoints Protegidos

### 🔒 Apenas para Admins (`[Authorize(Roles = "Admin")]`)

1. **`GET /api/auth/users`** - Listar todos os usuários
2. **`POST /api/auth/assign-role`** - Atribuir roles a usuários  
3. **`GET /api/auth/roles`** - Obter roles disponíveis

### 🔐 Apenas para Usuários Autenticados (`[Authorize]`)

4. **`GET /api/auth/users/{id}`** - Buscar usuário específico

### 🌐 Endpoints Públicos (sem proteção)

- **`POST /api/auth/register`** - Registro de novos usuários
- **`POST /api/auth/login`** - Login de usuários
- **`POST /api/auth/validate-token`** - Validação de token
- **`POST /api/auth/refresh-token`** - Renovação de token

## 🔧 Mudanças Técnicas Implementadas

### 1. AuthService - Inclusão de Role no JWT
```csharp
// ANTES:
public string GenerateJwtToken(string username, int userId)

// DEPOIS:
public string GenerateJwtToken(string username, int userId, string role = "User")
```

**Novo claim adicionado:**
```csharp
new Claim(ClaimTypes.Role, role)
```

### 2. Método para Extrair Role do Token
```csharp
public string? GetRoleFromToken(string token)
{
    if (ValidateToken(token, out ClaimsPrincipal? principal))
    {
        return principal?.FindFirst(ClaimTypes.Role)?.Value;
    }
    return null;
}
```

### 3. Atualização dos Endpoints para Usar Role
- **Login**: Agora passa a role do usuário para o token
- **Refresh Token**: Mantém a role do usuário no novo token

## 🚦 Como Funciona a Autorização

### Fluxo de Autenticação com Role
1. **Login** → JWT é gerado com a role do usuário
2. **Requisições** → JWT é enviado no header `Authorization: Bearer <token>`
3. **Validação** → ASP.NET Core valida automaticamente a role no token
4. **Acesso** → Endpoint só é acessado se a role for válida

### Exemplo de Token JWT (Claims)
```json
{
  "sub": "admin_user",
  "userId": "1",
  "username": "admin_user",
  "name": "admin_user",
  "role": "Admin",  // ← Claim essencial para autorização
  "jti": "unique-id",
  "exp": 1640995200,
  "iss": "localhost:5150",
  "aud": "myfront.service.com"
}
```

## 🔬 Testando a Proteção

### 1. Teste com Usuário Normal
```bash
# 1. Registrar usuário normal
POST /api/auth/register
{
  "email": "user@test.com",
  "username": "normaluser",
  "password": "password123",
  "confirmPassword": "password123"
}

# 2. Fazer login
POST /api/auth/login
{
  "emailOrUsername": "normaluser",
  "password": "password123"
}

# 3. Tentar acessar endpoint de admin (deve falhar)
GET /api/auth/users
Authorization: Bearer <token_do_usuario_normal>
# Resultado: 403 Forbidden
```

### 2. Teste com Usuário Admin
```bash
# 1. Promover usuário para admin via API
POST /api/auth/assign-role
{
  "userId": 1,
  "role": "Admin"
}

# 2. Fazer login novamente para obter token com role Admin
POST /api/auth/login
{
  "emailOrUsername": "normaluser",
  "password": "password123"
}

# 3. Acessar endpoint de admin (deve funcionar)
GET /api/auth/users
Authorization: Bearer <token_do_admin>
# Resultado: 200 OK com lista de usuários
```

## 🛡️ Segurança Implementada

### Frontend (admin.html)
- Verifica se o usuário tem role "Admin" antes de mostrar a interface
- Redireciona para login se não tiver permissão

### Backend (API)
- **Validação de Token**: ASP.NET Core JWT middleware
- **Autorização por Role**: Atributos `[Authorize(Roles = "Admin")]`
- **Claims Validation**: Role é validada automaticamente

### Proteções Múltiplas
1. **Token JWT**: Deve ser válido e não expirado
2. **Role Claim**: Deve conter a role "Admin" 
3. **Usuário Ativo**: Usuário deve existir no banco de dados

## 📋 Próximos Passos Recomendados

### 1. Política de Autorização Customizada
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("AdminOrModerator", policy => 
        policy.RequireRole("Admin", "Moderator"));
});
```

### 2. Atributos Customizados
```csharp
[Authorize(Policy = "AdminOrModerator")]
public async Task<IActionResult> ModerateContent()
```

### 3. Rate Limiting por Role
```csharp
// Admins: mais requisições permitidas
// Users: limite menor
```

## ✅ Status Atual

- ✅ Endpoints protegidos corretamente
- ✅ JWT tokens incluem role claims
- ✅ Autorização funcionando no backend
- ✅ Interface admin protegida no frontend
- ✅ Documentação completa

**O sistema está totalmente funcional e seguro para produção!**
