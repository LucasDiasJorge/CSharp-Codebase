# 🔐 SecurityAndAuthentication - Complete User Authentication System

A production-ready, full-stack user authentication system built with ASP.NET Core, JWT tokens, Entity Framework, and modern frontend technologies.

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![JWT](https://img.shields.io/badge/JWT-Authentication-green.svg)](https://jwt.io/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-blue.svg)](https://docs.microsoft.com/en-us/ef/core/)
<!-- README padronizado (versão condensada) -->
# SecurityAndAuthentication (JWT + RBAC + OTP)

Implementa autenticação com JWT, controle de acesso por papéis (RBAC), mitigação de força bruta e suporte a OTP (2FA) demonstrativo. Projeto didático e base para evolução.

## 1. Fluxo Resumido
1. Login: valida usuário, senha (hash+salt) e (se exigido) OTP.
2. Mitigação brute force: incrementa tentativas, aplica lockout temporário.
3. Gera JWT com claims: sub, username, role, sessionId, exp.
4. Endpoints protegidos verificam `[Authorize]` e/ou `[Authorize(Roles=...)]`.

## 2. Endpoints Principais
| Endpoint | Método | Proteção | Descrição |
|----------|--------|----------|-----------|
| /api/auth/login | POST | Pública | Autentica e retorna JWT |
| /api/auth/otp | POST | Pública / pré-login | Gera código OTP temporário |
| /api/demo/authenticated | GET | JWT | Requer usuário autenticado |
| /api/demo/admin | GET | JWT + Role=Admin | Exige papel Admin |

## 3. Execução
```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\SecurityAndAuthentication"
dotnet run
```
Teste via REST Client / Postman enviando Authorization: Bearer <token>.

## 4. Request / Response de Login
Request:
```json
{ "username": "admin", "password": "Admin@123", "otp": "123456" }
```
Response (exemplo):
```json
{ "token": "<jwt>", "expiresIn": 1800, "sessionId": "<guid>", "user": { "id": 1, "username": "admin", "role": "Admin" } }
```

## 5. Serviços (Camadas)
- AuthenticationService: Orquestra login, brute force, OTP, token.
- TokenService: Criação do JWT (assinatura + expiry).
- BruteForceProtectionService: Tentativas e lockout.
- OtpService: Geração/validação de códigos temporários.
- PasswordHasher: Hash + salt.
- UserRepository: Armazena / consulta usuários (in-memory demo).

## 6. Segurança Implementada
- Hash + salt para senha (evita texto plano).
- Lockout após N falhas (mitiga brute force).
- JWT curto + claims mínimas (princípio mínimo privilégio).
- RBAC por atributo de Role.
- OTP adiciona segundo fator opcional.

## 7. Melhorias Futuras
- Refresh token + revogação.
- Armazenar usuários em banco (EF Core / Dapper).
- Trocar hash para Argon2 / PBKDF2.
- Rate limiting e audit trail.
- Lista de bloqueio para JWT revogados (jti).
- Testes integrados (login, lockout, OTP, RBAC).

## 8. Checklist Rápido
[ ] HTTPS obrigatório
[ ] Segredo JWT em local seguro (vault)
[ ] Expiração curta + refresh
[ ] Auditoria de eventos críticos
[ ] Cabeçalhos de segurança (HSTS, CSP)
[ ] Limite de requisições / IP throttling

## 9. Estrutura (resumo)
```
Controllers/
Services/
Repositories/
Models/
Helpers/
Configuration/
Program.cs
```

## 10. Aprendizado Chave
Integra vários pilares: validação de credenciais, MFA opcional, mitigação de ataques, emissão de token e autorização declarativa.

---
Versão condensada substitui README longo original.
#### 🗃️ Database Issues
```bash
# InMemory database resets on restart (expected behavior)
# Check Entity Framework configuration
# Verify models are properly mapped
```

#### 🌐 Frontend Issues
```bash
# Ensure frontend files are served over HTTP/HTTPS (not file://)
# Check browser console for JavaScript errors
# Verify localStorage is supported and enabled
```

## 🤝 Contributing

This project serves as a learning resource and production template. Feel free to:
- Fork and modify for your own projects
- Submit issues for bugs or improvement suggestions
- Create pull requests for enhancements
- Use as a foundation for your own authentication systems

## 📄 License

This project is provided as-is for educational and commercial use. Feel free to use, modify, and distribute according to your needs.

---

## 🎯 Next Steps

Ready to build upon this foundation? Consider implementing:

- [ ] **Two-Factor Authentication (2FA)** with SMS/Email
- [ ] **OAuth integration** (Google, GitHub, Microsoft)
- [ ] **Password recovery** via email
- [ ] **User profile management** with photo uploads
- [ ] **Audit logging** for security compliance
- [ ] **Real-time notifications** with SignalR
- [ ] **Advanced role hierarchies** with custom permissions
- [ ] **Multi-tenant support** for SaaS applications

**Happy coding! 🎉**
