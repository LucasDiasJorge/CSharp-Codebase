# MongoUserApi

API didática em .NET 8 + MongoDB mostrando CRUD de Usuários, autenticação JWT e autorização com Policies.

## Objetivo
Aprender:
- Como estruturar uma API simples com MongoDB (Driver oficial)
- Diferenças conceituais entre MongoDB (NoSQL orientado a documentos) e bancos relacionais (SQL)
- CRUD completo + Login
- Uso de JWT + Claims + Policies simples (`AdminOnly`, `ActiveUser`)
- Index único para email
- Boas práticas básicas (tipos explícitos, DTOs, hash de senha SHA256 para fins educativos)

> Observação: Hash de senha com SHA256 puro **não** é recomendado em produção. Use BCrypt / PBKDF2 / Argon2.

## Estrutura de Pastas
```
MongoUserApi/
  Program.cs
  appsettings.json
  Configuration/
  Controllers/
  Models/
  Repositories/
  Services/
```

## Entidade `User`
Representa um documento na coleção `users`:
```json
{
  "_id": "64f...",
  "email": "john@acme.com",
  "displayName": "John",
  "passwordHash": "...",
  "role": "user", // ou "admin"
  "active": true,
  "createdAt": "2025-09-04T12:00:00Z"
}
```

## Fluxo de Autenticação
1. `POST /api/users/register` cria usuário (role padrão = `user`) e gera hash da senha
2. `POST /api/users/login` valida credenciais e retorna JWT com claims:
   - `sub` (Id do usuário)
   - `email`
   - `role`
   - `active`
3. Endpoints protegidos usam `[Authorize]` + Policies:
   - `AdminOnly` exige claim `role=admin`
   - `ActiveUser` exige claim `active=true`

## Endpoints Principais
| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/api/users/register` | Anônimo | Cria novo usuário |
| POST | `/api/users/login` | Anônimo | Retorna JWT |
| GET | `/api/users` | AdminOnly | Lista todos os usuários |
| GET | `/api/users/{id}` | Autenticado | Busca por Id |
| PUT | `/api/users/{id}` | ActiveUser | Atualiza displayName/role/active |
| DELETE | `/api/users/{id}` | AdminOnly | Remove usuário |

## Como Rodar
Pré-requisitos:
- .NET 8 SDK
- Docker (opcional) OU MongoDB local em `mongodb://localhost:27017`

Subir MongoDB rápido via Docker:
```bash
docker run -d --name mongo -p 27017:27017 mongo:6
```

Adicionar o projeto à solution (se ainda não estiver):
```bash
dotnet sln add MongoUserApi/MongoUserApi.csproj
```

Restaurar e subir API:
```bash
dotnet restore
dotnet run --project MongoUserApi/MongoUserApi.csproj
```
Abrir Swagger: `https://localhost:5001/swagger` (ou porta configurada). Aceite certificado de dev.

## Testando Fluxo
1. Registrar:
```json
POST /api/users/register
{
  "email": "admin@acme.com",
  "displayName": "Admin",
  "password": "Pass123!"
}
```
2. Promover no banco (manualmente) para `role=admin` (ex: via Mongo Shell / Compass) ou adicione lógica extra.
3. Login:
```json
POST /api/users/login
{
  "email": "admin@acme.com",
  "password": "Pass123!"
}
```
4. Copiar token e usar no botão Authorize do Swagger: `Bearer <token>`
5. Chamar endpoints protegidos.

## Policies (Autorização)
- `AdminOnly`: filtro administrativo baseado em claim `role`.
- `ActiveUser`: garante que usuário não foi desativado (claim `active=true`).

Essas policies leem claims existentes no token, logo se o usuário mudar de estado depois do login (ex: `active=false`), é preciso renovar o token para refletir.

## Index e Unicidade
O repositório cria um índice único em `email`. Vantagens:
- Busca rápida por email
- Garantia de não duplicação

Se tentar registrar email existente: retorna 409 (nesse código lança `InvalidOperationException`, que você poderia tratar com middleware global).

## MongoDB vs Bancos Relacionais
### Vantagens MongoDB
- Esquema flexível (evolução de campos sem migrations complexas)
- Documentos aninhados evitam muitos JOINs
- Escalabilidade horizontal (sharding) nativa
- Modelo natural para dados orientados a agregados
- Escrita rápida para grandes volumes de dados semi-estruturados

### Desvantagens / Cuidados
- Ausência de transações complexas multi-documento (existe transação ACID, mas custo maior e limites)
- Consistência eventual em alguns cenários distribuídos
- Difícil garantir integridade referencial sem lógica de aplicação (não há foreign keys nativas)
- Consultas analíticas complexas às vezes mais simples em SQL
- Risco de crescimento desordenado de documentos (schema drift) se governança fraca

### Quando Usar
- Catálogos de produtos, perfis de usuário, conteúdo variando em campos
- Logs, eventos, telemetria

### Quando Preferir SQL
- Relacionamentos complexos altamente normalizados
- Relatórios transacionais e agregações fortes ad-hoc
- Regras fortes de integridade referencial

## Padrões Didáticos Demonstrados
- Repository + Service separando responsabilidades
- DTOs de Entrada para não expor hash
- Tipos explícitos (sem `var`)
- Hashing determinístico (apenas educacional)
- Index único criado ao inicializar repositório
- Policies baseadas em claims
- Config centralizada em `appsettings.json`

## Possíveis Extensões
- Paginação em `GetAll`
- Atualização de senha com salt + BCrypt
- Refresh Tokens
- Soft Delete (flag ao invés de remover)
- Versionamento de API
- Logs estruturados + Observabilidade

## Observações Sobre Performance
- Cada request ao Mongo usa driver async não bloqueante
- Reutilização de `MongoClient` via DI (singleton) seria possível; aqui cada contexto cria client a partir das settings (poderia otimizar)

## Segurança
- Troque a chave JWT (`Jwt:Key`) por algo longo e seguro
- Armazene segredo em `User Secrets` / Vault em produção
- Habilite HTTPS obrigatório e SameSite Cookie se usar cookies

## Aprendizado Chave
Mongo facilita evoluir campos sem migrations, mas exige disciplina para não degradar consistência. A escolha entre Mongo e SQL depende do modelo de acesso, não só de performance.

---
Bons estudos! Ajuste, brinque e experimente cenários de alteração de schema para sentir a flexibilidade.
