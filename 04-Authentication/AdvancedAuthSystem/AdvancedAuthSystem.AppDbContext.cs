using Microsoft.EntityFrameworkCore;
using AdvancedAuthSystem.Models;

namespace AdvancedAuthSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserClaim> UserClaims => Set<UserClaim>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Resource> Resources => Set<Resource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
        });

        // UserRole - Many-to-Many
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId);
        });

        // RolePermission - Many-to-Many
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId);
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId);
        });

        // UserClaim configuration
        modelBuilder.Entity<UserClaim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserClaims)
                .HasForeignKey(e => e.UserId);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId);
        });

        // Resource configuration
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedResources)
                .HasForeignKey(e => e.OwnerId);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Roles
        var adminRole = new Role { Id = 1, Name = "Admin", Description = "Administrator with full access" };
        var managerRole = new Role { Id = 2, Name = "Manager", Description = "Department manager" };
        var userRole = new Role { Id = 3, Name = "User", Description = "Standard user" };
        var guestRole = new Role { Id = 4, Name = "Guest", Description = "Guest with read-only access" };

        modelBuilder.Entity<Role>().HasData(adminRole, managerRole, userRole, guestRole);

        // Seed Permissions
        var permissions = new List<Permission>
        {
            new() { Id = 1, Resource = "Users", Action = "Read", Name = "users.read" },
            new() { Id = 2, Resource = "Users", Action = "Write", Name = "users.write" },
            new() { Id = 3, Resource = "Users", Action = "Delete", Name = "users.delete" },
            new() { Id = 4, Resource = "Roles", Action = "Manage", Name = "roles.manage" },
            new() { Id = 5, Resource = "Resources", Action = "Read", Name = "resources.read" },
            new() { Id = 6, Resource = "Resources", Action = "Write", Name = "resources.write" },
            new() { Id = 7, Resource = "Resources", Action = "Delete", Name = "resources.delete" },
            new() { Id = 8, Resource = "Resources", Action = "Manage", Name = "resources.manage" },
            new() { Id = 9, Resource = "Reports", Action = "View", Name = "reports.view" }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);

        // Seed RolePermissions
        var rolePermissions = new List<RolePermission>
        {
            // Admin - all permissions
            new() { RoleId = 1, PermissionId = 1 },
            new() { RoleId = 1, PermissionId = 2 },
            new() { RoleId = 1, PermissionId = 3 },
            new() { RoleId = 1, PermissionId = 4 },
            new() { RoleId = 1, PermissionId = 5 },
            new() { RoleId = 1, PermissionId = 6 },
            new() { RoleId = 1, PermissionId = 7 },
            new() { RoleId = 1, PermissionId = 8 },
            new() { RoleId = 1, PermissionId = 9 },
            
            // Manager
            new() { RoleId = 2, PermissionId = 1 },
            new() { RoleId = 2, PermissionId = 5 },
            new() { RoleId = 2, PermissionId = 6 },
            new() { RoleId = 2, PermissionId = 9 },
            
            // User
            new() { RoleId = 3, PermissionId = 5 },
            new() { RoleId = 3, PermissionId = 6 },
            
            // Guest
            new() { RoleId = 4, PermissionId = 5 }
        };

        modelBuilder.Entity<RolePermission>().HasData(rolePermissions);

        // Seed Users (passwords: Admin123!, Manager123!, User123!)
        var users = new List<User>
        {
            new()
            {
                Id = 1,
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "System",
                LastName = "Administrator",
                Department = "IT",
                DateOfBirth = new DateTime(1985, 1, 1),
                IsActive = true,
                TwoFactorEnabled = false
            },
            new()
            {
                Id = 2,
                Username = "manager",
                Email = "manager@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                FirstName = "John",
                LastName = "Manager",
                Department = "IT",
                DateOfBirth = new DateTime(1988, 5, 15),
                IsActive = true,
                TwoFactorEnabled = false
            },
            new()
            {
                Id = 3,
                Username = "user",
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                FirstName = "Jane",
                LastName = "User",
                Department = "Sales",
                DateOfBirth = new DateTime(1995, 8, 20),
                IsActive = true,
                TwoFactorEnabled = false
            }
        };

        modelBuilder.Entity<User>().HasData(users);

        // Seed UserRoles
        var userRoles = new List<UserRole>
        {
            new() { UserId = 1, RoleId = 1 }, // admin -> Admin
            new() { UserId = 2, RoleId = 2 }, // manager -> Manager
            new() { UserId = 3, RoleId = 3 }  // user -> User
        };

        modelBuilder.Entity<UserRole>().HasData(userRoles);

        // Seed UserClaims
        var userClaims = new List<UserClaim>
        {
            new() { Id = 1, UserId = 2, ClaimType = "Department", ClaimValue = "IT" },
            new() { Id = 2, UserId = 3, ClaimType = "Department", ClaimValue = "Sales" },
            new() { Id = 3, UserId = 1, ClaimType = "Level", ClaimValue = "Senior" },
            new() { Id = 4, UserId = 2, ClaimType = "Level", ClaimValue = "Senior" }
        };

        modelBuilder.Entity<UserClaim>().HasData(userClaims);
    }
}
