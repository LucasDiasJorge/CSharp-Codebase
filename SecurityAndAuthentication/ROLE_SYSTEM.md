# Role Assignment System - User Guide

## Overview
This authentication system now includes a comprehensive role assignment feature that allows administrators to manage user roles dynamically.

## Available Roles
- **User**: Default role for new registrations
- **Admin**: Full access including role management
- **Moderator**: Intermediate role (can be extended for specific permissions)

## How It Works

### 1. User Registration
- New users automatically get the "User" role
- Role is stored in the database and returned with all user data

### 2. Role Management (Admin Only)
- Access the admin panel at `admin.html`
- Only users with "Admin" role can access this panel
- View all users with their current roles
- Assign new roles to any user by User ID

### 3. API Endpoints

#### Get Available Roles
```
GET /api/auth/roles
```
Returns: `{ "roles": ["User", "Admin", "Moderator"] }`

#### Assign Role
```
POST /api/auth/assign-role
Content-Type: application/json

{
  "userId": 1,
  "role": "Admin"
}
```

#### All Other Endpoints Now Include Role
- `/api/auth/login` - Returns user role in response
- `/api/auth/users` - Includes role for each user
- `/api/auth/validate-token` - Returns role in user data

### 4. Frontend Features

#### Dashboard Updates
- Shows user's current role in profile section
- Displays roles for all users in the user list
- Admin users see an "Admin Panel" button

#### Admin Panel
- Protected page requiring Admin role
- Form to assign roles to users by ID
- Real-time user list with role information
- Dropdown selection for available roles

### 5. Testing the System

#### Step 1: Create Test Users
1. Open `index.html` and register a new user
2. The user will automatically get "User" role

#### Step 2: Promote to Admin
1. Note the User ID from the dashboard
2. Use an API client (or browser dev tools) to make this request:
```bash
POST http://localhost:5150/api/auth/assign-role
Content-Type: application/json

{
  "userId": 1,
  "role": "Admin"
}
```

#### Step 3: Access Admin Panel
1. Login with the promoted user
2. Click "Admin Panel" button on dashboard
3. Manage other users' roles

### 6. Security Features
- Role validation on backend (only valid roles accepted)
- Admin panel access control (frontend and backend validation)
- JWT tokens include role information
- All API responses include role data for proper frontend handling

### 7. Database Schema
The User model now includes:
```csharp
public string Role { get; set; } = "User"; // Default role
```

This is stored in the database and indexed for performance.

## Usage Examples

### Create an Admin User (via API)
```bash
# 1. Register normally first
POST /api/auth/register
{
  "email": "admin@test.com",
  "username": "admin",
  "password": "password123",
  "confirmPassword": "password123"
}

# 2. Get the userId from response, then assign Admin role
POST /api/auth/assign-role
{
  "userId": 1,
  "role": "Admin"
}
```

### Check User Role
```bash
POST /api/auth/validate-token
{
  "token": "your-jwt-token"
}
```

The response will include the user's role in the user object.

## Development Notes

### Adding New Roles
1. Update the `validRoles` array in `AuthController.AssignRole`
2. Add the role to the dropdown in `admin.html`
3. Update any role-specific UI logic

### Extending Role Permissions
You can add role-based authorization attributes to controllers:
```csharp
[Authorize(Roles = "Admin,Moderator")]
public async Task<IActionResult> SomeProtectedAction()
```

This system provides a solid foundation for role-based access control that can be extended as needed.
