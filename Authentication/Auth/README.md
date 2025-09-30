# Auth

## Visão Geral
Este projeto demonstra autenticação via JWT em uma API ASP.NET Core minimal, incluindo emissão de tokens e validação de credenciais. Serve como base para outros exemplos de segurança.

## Objetivos Didáticos
- Gerar e assinar JWTs.
- Configurar autenticação e autorização.
- Aplicar melhores práticas básicas de segurança.

## Estrutura do Projeto
```
Auth/
   Program.cs
   Controllers/
   Services/
   Middlewares/ (se aplicável)
   appsettings.json
```

## Tecnologias e Pacotes
| Categoria | Pacote | Observação |
|-----------|--------|-----------|
| Framework | ASP.NET Core 9 | API e middleware |
| Auth | Microsoft.AspNetCore.Authentication.JwtBearer | Validação do token |
| JWT | System.IdentityModel.Tokens.Jwt | Criação/validação de tokens |

## Como Executar
```powershell
dotnet restore
dotnet run --project ./Auth/Auth.csproj
```
A API subirá (porta definida pelo Kestrel/launchSettings). Verifique o endpoint de documentação (Swagger/OpenAPI) se configurado.

## Fluxo Principal
1. Usuário envia credenciais para endpoint de login.
2. Serviço de autenticação valida dados.
3. Token JWT é emitido com claims essenciais.
4. Endpoints protegidos exigem header `Authorization: Bearer <token>`.

## Boas Práticas Demonstradas
- Injeção de dependência para serviços de autenticação.
- Configuração explícita de JwtBearerOptions.
- Uso de tipos explícitos (sem `var`).
- Separação de responsabilidades (controller vs serviço).

## Pontos de Atenção
- Este exemplo não cobre refresh tokens.
- Armazenamento de usuários simplificado (não usar em produção sem reforço de segurança).

## Próximos Passos Sugeridos
- Adicionar refresh token.
- Integrar IdentityServer ou OpenIddict.
- Adicionar roles/policies avançadas.

---
README gerado a partir do template comum em `docs/README-TEMPLATE.md`.
# Autenticação JWT em ASP.NET Core

## 📚 Conceitos Abordados

Este projeto demonstra a implementação de autenticação JWT (JSON Web Token) em ASP.NET Core:

- **JWT (JSON Web Token)**: Padrão para transmissão segura de informações
- **Bearer Authentication**: Esquema de autenticação baseado em tokens
- **Middleware customizado**: Interceptação e processamento de requisições
- **Claims**: Informações sobre o usuário contidas no token
- **Authorization**: Controle de acesso baseado em roles/policies

## 🎯 Objetivos de Aprendizado

- Implementar autenticação stateless com JWT
- Configurar middleware de autenticação/autorização
- Entender o ciclo de vida de tokens JWT
- Proteger endpoints de API
- Validar e decodificar tokens JWT

## 💡 Conceitos Importantes

### Estrutura do JWT
Um JWT consiste em três partes separadas por pontos:
```
header.payload.signature
```

### Configuração do JWT
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
```

### Proteção de Endpoints
```csharp
[Authorize] // Requer autenticação
[Authorize(Roles = "Admin")] // Requer role específica
```

## 🚀 Como Executar

```bash
cd Auth
dotnet run
```

## 📋 Endpoints Disponíveis

- `POST /auth/login` - Obter token JWT
- `GET /auth/protected` - Endpoint protegido (requer token)
- `GET /auth/admin` - Endpoint admin (requer role específica)

## 📖 O que Você Aprenderá

1. **Diferença entre Autenticação e Autorização**:
   - Autenticação: "Quem você é?"
   - Autorização: "O que você pode fazer?"

2. **Vantagens do JWT**:
   - Stateless (não armazena estado no servidor)
   - Escalável para microserviços
   - Self-contained (contém informações do usuário)

3. **Segurança**:
   - Assinatura digital para integridade
   - Expiração automática de tokens
   - Validação de claims

4. **Pipeline de Middleware**:
   - Ordem de execução dos middlewares
   - Como interceptar requisições
   - Adição de informações ao contexto

## 🔐 Fluxo de Autenticação

1. **Login**: Cliente envia credenciais
2. **Validação**: Servidor valida credenciais
3. **Geração**: Servidor cria e assina JWT
4. **Retorno**: Token é enviado ao cliente
5. **Uso**: Cliente inclui token em requisições subsequentes
6. **Validação**: Servidor valida token em cada requisição

## 🔍 Pontos de Atenção

- **Chave secreta**: Mantenha a chave de assinatura segura
- **Expiração**: Configure tempo apropriado para expiração
- **HTTPS**: Sempre use HTTPS em produção
- **Refresh tokens**: Implemente para melhor experiência do usuário
- **Claims mínimos**: Inclua apenas informações necessárias no token

## 📚 Recursos Adicionais

- [JWT.io](https://jwt.io/) - Decodificador de tokens JWT
- [RFC 7519 - JWT Specification](https://tools.ietf.org/html/rfc7519)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
