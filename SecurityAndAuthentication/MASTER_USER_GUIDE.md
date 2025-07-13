# Sistema de Usuário Master - Guia Completo

## 🚀 Funcionalidade Implementada

### Criação Automática do Usuário Master
O sistema agora cria automaticamente um usuário administrador master na primeira execução da aplicação.

## 📋 Credenciais Padrão do Master User

```
📧 Email: admin@system.com
👤 Username: admin
🔑 Password: Admin123!
🛡️ Role: Admin
```

## 🔧 Como Funciona

### 1. Inicialização Automática
- ✅ Executa na inicialização da aplicação (`Program.cs`)
- ✅ Verifica se já existe algum usuário com role "Admin"
- ✅ Cria o usuário master apenas se nenhum admin existir
- ✅ Exibe as credenciais no console para referência

### 2. Configuração via Environment Variables
Você pode personalizar as credenciais do master user usando variáveis de ambiente:

```bash
# Windows PowerShell
$env:MASTER_USER_EMAIL = "meuadmin@empresa.com"
$env:MASTER_USER_USERNAME = "superadmin"
$env:MASTER_USER_PASSWORD = "MinhaSenapSegura123!"

# Linux/Mac
export MASTER_USER_EMAIL="meuadmin@empresa.com"
export MASTER_USER_USERNAME="superadmin"
export MASTER_USER_PASSWORD="MinhaSenapSegura123!"
```

### 3. Segurança Implementada
- 🔒 **Senha criptografada** com BCrypt
- 🔒 **Role Admin** atribuída automaticamente
- 🔒 **Verificação de existência** (não duplica admins)
- 🔒 **Logs detalhados** com recomendações de segurança

## 📝 Endpoints Disponíveis para o Master User

### Login do Master User
```bash
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUsername": "admin",
  "password": "Admin123!"
}
```

### Alterar Senha (Recomendado após primeiro login)
```bash
POST /api/auth/change-password
Authorization: Bearer <token>
Content-Type: application/json

{
  "currentPassword": "Admin123!",
  "newPassword": "NovaSenapSegura123!",
  "confirmNewPassword": "NovaSenapSegura123!"
}
```

### Gerenciar Roles de Outros Usuários
```bash
POST /api/auth/assign-role
Authorization: Bearer <token>
Content-Type: application/json

{
  "userId": 2,
  "role": "Admin"
}
```

## 🎯 Interface Frontend Atualizada

### 1. Dashboard com Troca de Senha
- ✅ Botão **"Change Password"** no header
- ✅ Redirecionamento para `change-password.html`
- ✅ Interface moderna e responsiva

### 2. Página de Troca de Senha (`change-password.html`)
- ✅ Formulário com validação em tempo real
- ✅ Confirmação de senha
- ✅ Feedback visual de sucesso/erro
- ✅ Autenticação obrigatória

### 3. Admin Panel
- ✅ Acesso exclusivo para usuários Admin
- ✅ Gerenciamento completo de roles
- ✅ Listagem de usuários com roles

## 🔄 Fluxo Recomendado de Primeiro Uso

### 1. Primeiro Acesso
```bash
# 1. Executar aplicação
dotnet run

# 2. Observar credenciais no console
# =================================
# 👤 USUÁRIO MASTER CRIADO:
# 📧 Email: admin@system.com
# 👤 Username: admin
# 🔑 Password: Admin123!
# =================================
```

### 2. Login Inicial
1. Abrir `http://localhost:5150` no navegador
2. Navegar para `login.html`
3. Fazer login com: `admin` / `Admin123!`

### 3. Alterar Senha (RECOMENDADO)
1. No dashboard, clicar em **"Change Password"**
2. Inserir senha atual: `Admin123!`
3. Definir nova senha segura
4. Confirmar alteração

### 4. Gerenciar Sistema
1. Acessar **"Admin Panel"** no dashboard
2. Promover outros usuários para admin se necessário
3. Gerenciar roles conforme necessário

## 🛡️ Segurança e Boas Práticas

### ✅ Implementado
- **Hash de senha** com BCrypt + salt automático
- **JWT tokens** com claims de role
- **Autorização por endpoints** (`[Authorize(Roles = "Admin")]`)
- **Validação de token** em todas as requisições protegidas
- **Environment variables** para configuração segura
- **Logs de segurança** com recomendações

### 🔒 Recomendações Adicionais
1. **Altere a senha padrão** imediatamente após primeiro login
2. **Configure environment variables** em produção
3. **Use HTTPS** em produção
4. **Monitore logs** de acesso e alterações
5. **Limite tentativas** de login (considere implementar)

## 📊 Status Atual do Sistema

### ✅ Completo e Funcional
- ✅ Criação automática de master user
- ✅ Autenticação JWT com roles
- ✅ Autorização por endpoints
- ✅ Interface completa (registro, login, dashboard, admin)
- ✅ Gerenciamento de roles
- ✅ Troca de senha
- ✅ Documentação completa

### 🚀 Pronto para Produção
O sistema está completo e seguro para uso em produção, desde que:
1. Configure environment variables apropriadas
2. Use HTTPS em produção
3. Configure um banco de dados persistente (não InMemory)
4. Implemente rate limiting se necessário

## 🎉 Resultado Final

**Um sistema de autenticação completo e profissional com:**
- Usuário master automático
- Autenticação JWT robusta
- Autorização baseada em roles
- Interface moderna e funcional
- Segurança implementada corretamente
- Documentação abrangente

**O sistema está 100% funcional e pronto para uso!** 🎯
