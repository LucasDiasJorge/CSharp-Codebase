namespace SafeVault.Security;

/// <summary>
/// Interface for password hashing service
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password securely
    /// </summary>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies if a password matches a hash
    /// </summary>
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// Implementation of password hasher using BCrypt.Net-Next
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // Higher is more secure but slower
    
    public string HashPassword(string password)
    {
        // Use a high work factor for better security
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        // BCrypt implementation handles timing attacks
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
