# SecurityAndAuthentication - User Authentication System

Um sistema completo de autenticação de usuários com frontend e backend.

## 🚀 Funcionalidades

### Backend (ASP.NET Core)
- ✅ API RESTful para autenticação
- ✅ Entity Framework com InMemory Database
- ✅ Hash de senhas com BCrypt
- ✅ Validação de dados
- ✅ CORS configurado
- ✅ Endpoints para registro, login, e gerenciamento de usuários

### Frontend (HTML/CSS/JavaScript)
- ✅ Página de registro de usuário
- ✅ Página de login
- ✅ Dashboard protegido
- ✅ Autenticação com localStorage
- ✅ Design responsivo e moderno
- ✅ Validação em tempo real

## 📁 Estrutura do Projeto

```
SecurityAndAuthentication/
├── Controllers/
│   └── AuthController.cs          # API endpoints
├── Data/
│   ├── ApplicationDbContext.cs    # Contexto do banco
│   └── Models/
│       └── User.cs               # Modelos e DTOs
├── Front/
│   ├── index.html               # Página de registro
│   ├── login.html               # Página de login
│   ├── dashboard.html           # Dashboard protegido
│   ├── styles.css               # Estilos CSS
│   ├── script.js                # Script de registro
│   └── login.js                 # Script de autenticação
├── Program.cs                   # Configuração da aplicação
└── *.csproj                     # Configuração do projeto
```

## 🛠️ Como Executar

### Pré-requisitos
- .NET 9.0 SDK
- Um navegador web moderno

### Backend
1. Abra o terminal no diretório do projeto
2. Execute os comandos:
```bash
dotnet restore
dotnet build
dotnet run
```

O backend estará rodando em: `http://localhost:5150` (ou conforme configurado)

### Frontend
1. Abra o arquivo `Front/index.html` em um navegador
2. Ou sirva os arquivos através de um servidor web local

## 📋 Fluxo de Autenticação

### 1. Registro de Usuário
- Acesse `index.html`
- Preencha: Email, Username, Password, Confirm Password
- Opcionalmente marque para receber newsletters
- Após sucesso, será redirecionado para login

### 2. Login
- Acesse `login.html`
- Use Email ou Username + Password
- Após sucesso, será redirecionado para dashboard

### 3. Dashboard
- Página protegida que requer autenticação
- Exibe informações do usuário logado
- Lista todos os usuários registrados
- Opção de logout

## 🔌 API Endpoints

### Autenticação
- `POST /api/auth/register` - Registrar novo usuário
- `POST /api/auth/login` - Fazer login
- `POST /api/auth/validate-token` - Validar token
- `POST /api/auth/refresh-token` - Renovar token

### Usuários
- `GET /api/auth/users` - Listar todos os usuários
- `GET /api/auth/users/{id}` - Buscar usuário por ID

## 📊 Dados de Exemplo

O sistema é inicializado com um banco em memória. Todos os dados são perdidos quando a aplicação é reiniciada.

### Teste Manual
1. Registre um novo usuário através da interface
2. Faça login com as credenciais criadas
3. Acesse o dashboard para ver as informações

## 🔒 Segurança

- **Senhas**: Hash com BCrypt (salt automático)
- **Validação**: Dados validados no frontend e backend
- **Tokens**: Sistema JWT (JSON Web Tokens) com validação e refresh
- **Autenticação**: Bearer token authentication
- **Expiração**: Tokens válidos por 2 horas com auto-refresh
- **CORS**: Configurado para desenvolvimento
- **Claims**: Username, userId e tempo de expiração incluídos

## 🎯 Autenticação JWT

### Como Funciona
1. **Login**: Backend gera token JWT válido por 2 horas
2. **Armazenamento**: Token salvo no localStorage do navegador
3. **Validação**: Cada requisição protegida valida o token
4. **Refresh**: Token pode ser renovado antes da expiração
5. **Logout**: Token removido do cliente

### Estrutura do Token
```json
{
  "sub": "username",
  "userId": "123",
  "username": "user123",
  "jti": "unique-token-id",
  "exp": 1640995200,
  "iss": "localhost:5150",
  "aud": "myfront.service.com"
}
```

## 🎨 Design

- Interface inspirada em designs modernos
- Paleta de cores azul (#1976d2)
- Layout responsivo
- Feedback visual para ações do usuário
- Mensagens de erro e sucesso

## ⚡ Próximos Passos

Para produção, considere:
- [ ] Implementar JWT tokens reais
- [ ] Usar banco de dados persistente (SQL Server, PostgreSQL)
- [ ] Adicionar recuperação de senha
- [ ] Implementar roles e permissões
- [ ] Adicionar testes unitários
- [ ] Configurar HTTPS em produção
- [ ] Implementar rate limiting
- [ ] Adicionar logs de auditoria

## 🐛 Troubleshooting

### Problema: CORS errors
- Verifique se o backend está rodando
- Confirme a URL da API no `script.js` e `login.js`

### Problema: Porta já em uso
- Mude a porta no `launchSettings.json`
- Ou pare outros processos usando a porta

### Problema: Dados não persistem
- Normal, usando InMemory Database
- Dados são perdidos quando a aplicação para
