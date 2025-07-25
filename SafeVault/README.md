# SafeVault - Secure C# Web API

SafeVault is a highly secure .NET 7.0 Web API that implements best practices for authentication, authorization, input validation, and prevention of common security vulnerabilities including SQL injection and XSS attacks.

## 🔒 Security Features

### Authentication & Authorization
- **JWT Authentication** with secure token generation and validation
- **Role-Based Access Control (RBAC)** with Admin and User roles
- **Password hashing** using BCrypt with high work factor
- **Token validation** endpoints for client-side auth checks

### SQL Injection Prevention
- **Parameterized queries** throughout using Dapper
- **Input validation and sanitization** before database operations
- **Stored procedures** for complex database operations

### XSS Protection
- **Input sanitization** for all user-provided content
- **Content Security Policy (CSP)** headers
- **X-XSS-Protection headers** for browsers

### Other Security Measures
- **Rate limiting middleware** to prevent brute-force attacks
- **Security headers** to prevent various attacks (clickjacking, MIME sniffing, etc.)
- **HTTPS enforcement** with HSTS headers
- **Global exception handling** to prevent information leakage
- **Request logging** with sensitive data redaction

## 📚 Project Structure

```
SafeVault/
├── Controllers/             # API endpoints
├── Data/                    # Database repositories and connection management
├── Middleware/              # Security middleware components
├── Models/                  # Domain models and DTOs
├── Security/                # Security-specific components
├── Services/                # Business logic services
├── Program.cs               # Application entry point
└── appsettings.json         # Application configuration
```

## 🚀 Getting Started

### Prerequisites
- .NET 7.0 SDK
- SQL Server (local or remote)

### Database Setup
1. Update the connection string in `appsettings.json`
2. Run the SQL setup script:
```
sqlcmd -S YourServer -i setup-database.sql
```

### Running the Application
1. Clone the repository
2. Navigate to the project directory
3. Run the application:
```
dotnet run
```
4. Access the Swagger UI at `https://localhost:5001/swagger`

## 🔐 Security Best Practices Implemented

### Input Validation
- All user inputs are validated using data annotations and custom validators
- Input validation occurs before any processing
- Custom regex patterns ensure proper format and prevent injection

### Authentication
- JWT tokens with appropriate expiry times
- Secure password storage using BCrypt hashing
- Token validation with proper signature verification

### Authorization
- Role-based access control on endpoints
- Fine-grained permissions for user resources
- Resource ownership verification for all operations

### SQL Injection Prevention
- Parameterized queries with Dapper
- No string concatenation for SQL queries
- SQL inputs sanitized and validated

### XSS Prevention
- HTML sanitization for user-generated content
- Proper Content-Security-Policy headers
- Encoded output where needed

### Rate Limiting
- Different limits for sensitive endpoints
- IP-based rate limiting
- Prevention of brute-force attacks

## 📋 API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get JWT token
- `POST /api/auth/validate-token` - Validate a JWT token
- `PUT /api/auth/change-password` - Change user password
- `PUT /api/auth/assign-role` - Assign role to a user (Admin only)

### Secrets Management
- `GET /api/secret` - Get all secrets for current user
- `GET /api/secret/{id}` - Get a specific secret
- `POST /api/secret` - Create a new secret
- `PUT /api/secret/{id}` - Update an existing secret
- `DELETE /api/secret/{id}` - Delete a secret

## 🧪 Security Testing

The project includes tests for:
- Authentication workflows
- Authorization enforcement
- Input validation
- SQL injection prevention
- XSS vulnerability prevention

## 📜 License

MIT License
