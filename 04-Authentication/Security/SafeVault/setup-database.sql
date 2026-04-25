-- Create SafeVault database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SafeVault')
BEGIN
    CREATE DATABASE SafeVault;
END
GO

USE SafeVault;
GO

-- Create Users table with secure columns
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255) NOT NULL,
        Username NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'User',
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsActive BIT NOT NULL DEFAULT 1
    );
    
    -- Create unique indexes for security
    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
    CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
END
GO

-- Create Secrets table with relationship to Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Secrets')
BEGIN
    CREATE TABLE Secrets (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(100) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Secrets_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    -- Create index for faster queries by user
    CREATE INDEX IX_Secrets_UserId ON Secrets(UserId);
END
GO

-- Create LoginAttempts table for auditing and security
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginAttempts')
BEGIN
    CREATE TABLE LoginAttempts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255) NULL,
        Username NVARCHAR(100) NULL,
        IPAddress NVARCHAR(100) NOT NULL,
        UserAgent NVARCHAR(500) NULL,
        Successful BIT NOT NULL DEFAULT 0,
        AttemptDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    -- Create indexes for monitoring and reporting
    CREATE INDEX IX_LoginAttempts_IPAddress ON LoginAttempts(IPAddress);
    CREATE INDEX IX_LoginAttempts_AttemptDate ON LoginAttempts(AttemptDate);
END
GO

-- Create AuditLogs table for tracking security events
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL, -- NULL for system events or unauthorized attempts
        Action NVARCHAR(100) NOT NULL,
        Details NVARCHAR(MAX) NULL,
        IPAddress NVARCHAR(100) NULL,
        UserAgent NVARCHAR(500) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
    );
    
    CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
    CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
END
GO

-- Insert default admin user if none exists
IF NOT EXISTS (SELECT * FROM Users WHERE Role = 'Admin')
BEGIN
    -- BCrypt hash for password "Admin@123456" with work factor 12
    INSERT INTO Users (Email, Username, PasswordHash, Role)
    VALUES ('admin@safevault.com', 'admin', '$2a$12$2A5LXHkEcT.Qa71TbO8OKOcVJU/4CCEGEgv8vEXkZbAn3iBKgPJR6', 'Admin');
END
GO

-- Create stored procedures to prevent SQL injection

-- GetUserByEmailOrUsername stored procedure
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetUserByEmailOrUsername')
    DROP PROCEDURE GetUserByEmailOrUsername;
GO

CREATE PROCEDURE GetUserByEmailOrUsername
    @Input NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM Users 
    WHERE (Email = @Input OR Username = @Input) AND IsActive = 1;
END
GO

-- CreateLoginAttempt stored procedure
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'CreateLoginAttempt')
    DROP PROCEDURE CreateLoginAttempt;
GO

CREATE PROCEDURE CreateLoginAttempt
    @Email NVARCHAR(255) = NULL,
    @Username NVARCHAR(100) = NULL,
    @IPAddress NVARCHAR(100),
    @UserAgent NVARCHAR(500) = NULL,
    @Successful BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO LoginAttempts (Email, Username, IPAddress, UserAgent, Successful)
    VALUES (@Email, @Username, @IPAddress, @UserAgent, @Successful);
END
GO

-- CreateAuditLog stored procedure
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'CreateAuditLog')
    DROP PROCEDURE CreateAuditLog;
GO

CREATE PROCEDURE CreateAuditLog
    @UserId INT = NULL,
    @Action NVARCHAR(100),
    @Details NVARCHAR(MAX) = NULL,
    @IPAddress NVARCHAR(100) = NULL,
    @UserAgent NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO AuditLogs (UserId, Action, Details, IPAddress, UserAgent)
    VALUES (@UserId, @Action, @Details, @IPAddress, @UserAgent);
END
GO
