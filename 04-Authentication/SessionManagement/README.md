# 🔑 Session Management

Sistema de gerenciamento de sessão em .NET 9 com JWT, Refresh Token Rotation e controle multi-dispositivo.

---

## 📚 Conceitos Abordados

- **Session Management**: Rastreamento de sessões por dispositivo com metadados (IP, UserAgent)
- **JWT Access Token**: Tokens de curta duração (15 min) com `SessionId` embutido
- **Refresh Token Rotation**: Cada uso do refresh token gera um novo par de tokens (previne replay attacks)
- **Refresh Token Hash**: Armazenado como SHA-256 hash no banco (nunca em texto plano)
- **Session Middleware**: Valida sessão no banco a cada requisição autenticada
- **Max Sessions per User**: Limite configurável de sessões simultâneas (revoga a mais antiga)
- **Gerenciamento remoto**: Listar, revogar e encerrar sessões de outros dispositivos

---

## 🎯 Objetivos de Aprendizado

- Entender a diferença entre JWT stateless e sessões com estado no servidor
- Implementar Refresh Token Rotation segura
- Rastrear sessões por dispositivo/IP para auditoria
- Usar middleware customizado para validação de estado de sessão
- Aplicar boas práticas de segurança: hash de tokens, limite de sessões

---

## 📂 Estrutura do Projeto

```
SessionManagement/
├── Controllers/
│   └── SessionController.cs        # Endpoints REST
├── Data/
│   └── AppDbContext.cs             # EF Core InMemory context
├── DTOs/
│   └── DTOs.cs                     # Records de entrada/saída
├── Middleware/
│   └── SessionValidationMiddleware.cs # Validação de sessão por request
├── Models/
│   └── Models.cs                   # Entidades: User, Session
├── Services/
│   ├── TokenService.cs             # Geração JWT e hash de tokens
│   └── SessionService.cs           # Lógica de negócio das sessões
├── Properties/
│   └── launchSettings.json
├── Program.cs                      # Configuração da aplicação
├── appsettings.json                # Configurações JWT e Session
├── SessionManagement.http          # Testes HTTP
└── SessionManagement.csproj
```

---

## 🚀 Como Executar

### Pré-requisitos

- .NET 9.0 SDK
- Redis (local ou via Docker)

```bash
# Subir Redis com Docker
docker run -d --name redis-session -p 6379:6379 redis:alpine
```

```bash
cd 04-Authentication/SessionManagement
dotnet restore
dotnet run
```

**Swagger**: http://localhost:5210/swagger

---

## 📋 Endpoints

| Método | Endpoint | Auth | Descrição |
|--------|----------|------|-----------|
| `POST` | `/api/session/register` | ❌ | Registrar novo usuário |
| `POST` | `/api/session/login` | ❌ | Login — cria sessão e retorna tokens |
| `POST` | `/api/session/refresh` | ❌ | Renovar tokens via refresh token |
| `POST` | `/api/session/logout` | ✅ | Encerrar sessão atual |
| `GET`  | `/api/session` | ✅ | Listar sessões ativas do usuário |
| `DELETE` | `/api/session/{id}` | ✅ | Revogar sessão específica |
| `DELETE` | `/api/session?keepCurrent=true` | ✅ | Revogar todas (exceto atual) |

---

## ⚙️ Configuração

```json
"Jwt": {
  "AccessTokenExpiryMinutes": "15"
},
"Session": {
  "MaxSessionsPerUser": "5",
  "RefreshTokenExpiryDays": "7"
},
"Redis": {
  "ConnectionString": "localhost:6379"
}
```

---

## 🔐 Fluxo de Autenticação

```
Login → AccessToken (15 min) + RefreshToken (7 dias) + SessionId no Redis
         ↓ expirou?
         POST /refresh com RefreshToken
         → Novo AccessToken + Novo RefreshToken (rotação)
         ↓ logout / revogação?
         Session marcada como IsRevoked = true no DB
         → Middleware bloqueia próximas requisições com esse SessionId
```
