# 🔐 SecurityAndAuthentication - Complete User Authentication System

A production-ready, full-stack user authentication system built with ASP.NET Core, JWT tokens, Entity Framework, and modern frontend technologies.

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![JWT](https://img.shields.io/badge/JWT-Authentication-green.svg)](https://jwt.io/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-blue.svg)](https://docs.microsoft.com/en-us/ef/core/)

## ⚡ Quick Start

### 1. Run the Application
```bash
# Clone and navigate to the project
cd SecurityAndAuthentication

# Install dependencies and run
dotnet restore
dotnet build
dotnet run
```

### 2. Access the Application
- **Backend API**: `http://localhost:5150`
- **Frontend**: Open `Front/index.html` in your browser

### 3. Default Admin User
The system automatically creates a master admin user on first run:
- **Email**: `admin@system.com`
- **Username**: `admin`
- **Password**: `Admin123!`

## 🚀 Key Features

### 🔧 Backend (ASP.NET Core Web API)
- ✅ **JWT Authentication** with role-based authorization
- ✅ **Entity Framework Core** with InMemory database
- ✅ **BCrypt password hashing** for security
- ✅ **Role management system** (Admin, User roles)
- ✅ **Master admin user** auto-creation
- ✅ **Token validation & refresh** endpoints
- ✅ **Password change** functionality
- ✅ **CORS configuration** for frontend communication
- ✅ **Comprehensive error handling** and validation

### 🎨 Frontend (HTML/CSS/JavaScript)
- ✅ **Modern, responsive design** with CSS Grid/Flexbox
- ✅ **Registration & Login pages** with real-time validation
- ✅ **Protected dashboard** with user information
- ✅ **Admin panel** for user and role management
- ✅ **Password change interface**
- ✅ **Secure token storage** with auto-refresh
- ✅ **Role-based UI elements** (admin features only visible to admins)
- ✅ **Error handling** with user-friendly messages

### 🔒 Security Features
- ✅ **JWT tokens** with expiration and refresh mechanism
- ✅ **Role-based authorization** with [Authorize(Roles = "Admin")]
- ✅ **Password complexity** requirements
- ✅ **Secure password storage** with BCrypt hashing
- ✅ **Input validation** on both frontend and backend
- ✅ **CORS policy** configuration
- ✅ **Environment variable support** for sensitive data

## 📁 Project Structure

```
SecurityAndAuthentication/
├── 📂 Controllers/
│   └── AuthController.cs              # API endpoints (register, login, admin functions)
├── 📂 Data/
│   ├── ApplicationDbContext.cs        # Entity Framework context
│   └── 📂 Models/
│       └── User.cs                    # User model + DTOs
├── 📂 Services/
│   └── AuthService.cs                 # JWT generation, validation, role management
├── 📂 Front/                          # Frontend application
│   ├── index.html                     # User registration page
│   ├── login.html                     # User login page
│   ├── dashboard.html                 # Protected user dashboard
│   ├── admin.html                     # Admin panel (role management)
│   ├── change-password.html           # Password change interface
│   ├── styles.css                     # Modern CSS styling
│   ├── script.js                      # Registration functionality
│   └── login.js                       # Login and auth utilities
├── 📂 Properties/
│   └── launchSettings.json            # Development server configuration
├── 📄 Program.cs                      # Application startup and configuration
├── 📄 appsettings.json                # JWT settings and configuration
├── 📄 *.csproj                        # Project dependencies
└── � Documentation/
    ├── README.md                      # This file (Quick Start Guide)
    ├── ULTIMATE_GUIDE.md              # Ultra-comprehensive implementation guide
    ├── ROLE_SYSTEM.md                 # Role management system guide
    ├── AUTHORIZATION_GUIDE.md         # Endpoint protection and testing
    └── MASTER_USER_GUIDE.md           # Master user system documentation
```

## 🎯 User Journey

### 1. 📝 New User Registration
1. Open `Front/index.html`
2. Fill in: Email, Username, Password, Confirm Password
3. System validates password complexity and uniqueness
4. User is automatically assigned "User" role
5. Redirect to login page upon success

### 2. 🔐 User Login
1. Open `Front/login.html`
2. Enter Email/Username + Password
3. System validates credentials and generates JWT token
4. Token stored securely in localStorage
5. Redirect to dashboard upon success

### 3. 🏠 Protected Dashboard
1. View personal user information
2. Access role-based features:
   - **Users**: Change password, view profile
   - **Admins**: Access admin panel, manage users and roles
3. Automatic token validation and refresh
4. Secure logout with token cleanup

### 4. 👑 Admin Features (Admin Role Only)
1. Access admin panel via dashboard
2. View all registered users
3. Assign roles to users (User ↔ Admin)
4. Monitor system usage

## 🔌 API Endpoints

### 🔓 Public Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/register` | Register new user |
| `POST` | `/api/auth/login` | User authentication |
| `POST` | `/api/auth/validate-token` | Validate JWT token |
| `POST` | `/api/auth/refresh-token` | Refresh JWT token |

### 🔒 Protected Endpoints (Require Authentication)
| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| `GET` | `/api/auth/user/{id}` | Get user details | Any authenticated user |
| `POST` | `/api/auth/change-password` | Change user password | Any authenticated user |

### 👑 Admin Endpoints (Require Admin Role)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/auth/users` | Get all users |
| `POST` | `/api/auth/assign-role` | Assign role to user |
| `GET` | `/api/auth/roles` | Get available roles |

## 🧪 Testing the System

### Quick Test Scenarios

#### 1. 🆕 Test User Registration
```bash
# Open Front/index.html and register with:
Email: test@example.com
Username: testuser
Password: Test123!
Confirm Password: Test123!
```

#### 2. 🔐 Test User Login
```bash
# Use the credentials you just created or the default admin:
Email: admin@system.com
Password: Admin123!
```

#### 3. 👑 Test Admin Features
```bash
# Login as admin, then:
1. Access Admin Panel from dashboard
2. View all users
3. Assign roles to other users
4. Test role-based access control
```

#### 4. 🔧 Test API Directly
```bash
# Register via API
curl -X POST http://localhost:5150/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "api@test.com",
    "username": "apiuser",
    "password": "Test123!"
  }'

# Login via API
curl -X POST http://localhost:5150/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "api@test.com",
    "password": "Test123!"
  }'
```

## 🛡️ Security Implementation

### JWT Token Structure
```json
{
  "sub": "username",
  "userId": "user-id",
  "username": "actual-username",
  "role": "User",
  "jti": "unique-token-id",
  "exp": 1640995200,
  "iss": "localhost:5150",
  "aud": "myfront.service.com"
}
```

### Role-Based Authorization
- **Public Access**: Registration, Login, Token validation
- **User Access**: Dashboard, Profile, Password change
- **Admin Access**: User management, Role assignment, System configuration

### Password Security
- **Minimum Requirements**: 8 characters, 1 uppercase, 1 lowercase, 1 number, 1 special character
- **Hashing**: BCrypt with automatic salt generation
- **Storage**: Only hashed passwords stored in database

## 📚 Documentation

This project includes comprehensive documentation:

| Document | Purpose | Target Audience |
|----------|---------|-----------------|
| **README.md** | Quick start and overview | Developers getting started |
| **[ULTIMATE_GUIDE.md](ULTIMATE_GUIDE.md)** | Complete implementation guide | Developers implementing from scratch |
| **[ROLE_SYSTEM.md](ROLE_SYSTEM.md)** | Role management system | Developers working with permissions |
| **[AUTHORIZATION_GUIDE.md](AUTHORIZATION_GUIDE.md)** | Endpoint security and testing | QA and security testing |
| **[MASTER_USER_GUIDE.md](MASTER_USER_GUIDE.md)** | Master user system | DevOps and deployment |

### 🎓 For Learning and Implementation

#### Beginners: Start Here 👇
1. **Read this README** for quick start
2. **Run the application** and test all features
3. **Review the code structure** to understand the architecture

#### Intermediate: Deep Dive 👇
1. **Read [ULTIMATE_GUIDE.md](ULTIMATE_GUIDE.md)** for step-by-step implementation
2. **Study role modeling** examples for different business domains
3. **Implement your own role hierarchy**

#### Advanced: Production Ready 👇
1. **Follow deployment preparation** in ULTIMATE_GUIDE.md
2. **Review security best practices** and hardening steps
3. **Implement additional features** like 2FA, rate limiting, etc.

## 🚀 Production Deployment

### Prerequisites Checklist
- [ ] Replace InMemory database with persistent storage (SQL Server, PostgreSQL)
- [ ] Configure environment variables for JWT secrets
- [ ] Set up HTTPS with SSL certificates
- [ ] Configure rate limiting and request throttling
- [ ] Set up proper logging and monitoring
- [ ] Configure CORS for production domains
- [ ] Set up backup and recovery procedures

### Quick Production Setup
```bash
# Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export JWT_SECRET_KEY="your-256-bit-secret-key"
export CONNECTION_STRING="your-production-db-connection"

# Build and run
dotnet publish -c Release
dotnet run --configuration Release
```

For detailed deployment instructions, see **[ULTIMATE_GUIDE.md](ULTIMATE_GUIDE.md)** sections 9-10.

## 🔧 Troubleshooting

### Common Issues

#### 🚫 CORS Errors
```bash
# Verify backend is running on correct port
# Check frontend API URLs match backend port
# Ensure CORS is configured in Program.cs
```

#### 🔑 Authentication Failures
```bash
# Check JWT secret key configuration
# Verify token hasn't expired
# Ensure proper Bearer token format in requests
```

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
