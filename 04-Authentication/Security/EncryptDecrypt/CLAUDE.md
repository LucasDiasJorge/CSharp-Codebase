# CLAUDE.md — EncryptDecrypt

Console de criptografia simétrica com as APIs de `System.Security.Cryptography`. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/Security/EncryptDecrypt/EncryptDecrypt.csproj
dotnet run --project 04-Authentication/Security/EncryptDecrypt/EncryptDecrypt.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): ciclo completo de cifrar e decifrar, com chave e IV explícitos para que o papel de cada um fique visível. Sem abstrações intermediárias — a intenção é ver as chamadas de criptografia diretamente.

## Pontos de atenção

- TFM `net9.0`, sem pacotes externos.
- Chave e IV **fixos no código**: é material didático. Em código real, IV é por mensagem e a chave vem de um cofre.
- Criptografia é diferente de hashing de senha: para senha, ver `PasswordHasher` em `SafeVault` e `AdvancedAuthSystem` (BCrypt).
