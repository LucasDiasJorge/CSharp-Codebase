# Authenticate User Use Case

## Descrição

Use Case de autenticação segura que implementa verificação de credenciais, proteção contra brute-force, geração de tokens JWT e auditoria de logins.

## Fluxo de Execução

```
┌─────────────────────────────────────────────────────────────────┐
│                  AuthenticateUserUseCase                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Validar entrada (email e senha não vazios)                  │
│                          ↓                                       │
│  2. Buscar usuário por email                                    │
│                          ↓                                       │
│  3. Verificar se conta pode fazer login                         │
│      - Conta ativa?                                             │
│      - Conta bloqueada?                                         │
│      - Período de lockout?                                      │
│                          ↓                                       │
│  4. Verificar senha                                             │
│                          ↓                                       │
│  5. Se falha: incrementar tentativas e possível lockout         │
│                          ↓                                       │
│  6. Se sucesso: resetar tentativas                              │
│                          ↓                                       │
│  7. Gerar Access Token (JWT)                                    │
│                          ↓                                       │
│  8. Revogar refresh tokens antigos                              │
│                          ↓                                       │
│  9. Gerar novo Refresh Token                                    │
│                          ↓                                       │
│  10. Registrar auditoria                                        │
│                          ↓                                       │
│  11. Retornar tokens e dados do usuário                         │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Mecanismo de Proteção contra Brute-Force

| Parâmetro | Valor |
|-----------|-------|
| Máximo de tentativas | 5 |
| Duração do lockout | 15 minutos |
| Auto-unlock | Sim, após período |

### Mensagens de Erro Progressivas

```
Tentativa 1: "Credenciais inválidas. Tentativas restantes: 4"
Tentativa 2: "Credenciais inválidas. Tentativas restantes: 3"
Tentativa 3: "Credenciais inválidas. Tentativas restantes: 2"
Tentativa 4: "Credenciais inválidas. Tentativas restantes: 1"
Tentativa 5: "Conta bloqueada por 15 minutos após 5 tentativas falhas"
```

## Segurança Implementada

✅ **Mensagens genéricas** - Não revela se email existe no sistema  
✅ **Proteção brute-force** - Lockout após múltiplas falhas  
✅ **Auditoria completa** - Logs de todas as tentativas  
✅ **Refresh token rotation** - Tokens antigos são revogados  
✅ **IP e User-Agent** - Registrados para análise de segurança  

## Dependências

| Interface | Responsabilidade |
|-----------|------------------|
| `IAuthUserRepository` | Acesso a dados de usuários |
| `IRefreshTokenRepository` | Gerenciamento de refresh tokens |
| `ILoginAuditRepository` | Registro de tentativas de login |
| `IPasswordVerifier` | Verificação de hash de senha |
| `IJwtTokenGenerator` | Geração de tokens JWT |

## Exemplo de Uso

```csharp
var input = new AuthenticateUserInput(
    Email: "usuario@email.com",
    Password: "senha123",
    IpAddress: "192.168.1.1",
    UserAgent: "Mozilla/5.0..."
);

var result = await useCase.ExecuteAsync(input);

if (result.IsSuccess)
{
    Console.WriteLine($"Bem-vindo, {result.Value.Name}!");
    Console.WriteLine($"Access Token: {result.Value.AccessToken}");
    Console.WriteLine($"Expira em: {result.Value.ExpiresAt}");
    Console.WriteLine($"Roles: {string.Join(", ", result.Value.Roles)}");
}
else
{
    Console.WriteLine($"Falha: {result.Error}");
}
```

## Auditoria

Todos os eventos de login são registrados:

```json
{
  "id": "guid",
  "email": "usuario@email.com",
  "userId": "guid-ou-null",
  "success": false,
  "failureReason": "Senha incorreta",
  "ipAddress": "192.168.1.1",
  "userAgent": "Mozilla/5.0...",
  "timestamp": "2025-01-15T10:30:00Z"
}
```

## Cenários de Teste

- ✅ Login com credenciais válidas
- ✅ Retorno de tokens JWT e refresh
- ❌ Email não encontrado (mensagem genérica)
- ❌ Senha incorreta com contador
- ❌ Conta bloqueada após 5 tentativas
- ❌ Conta desativada
- ✅ Auto-unlock após 15 minutos
