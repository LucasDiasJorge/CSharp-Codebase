using System.Text.RegularExpressions;

namespace SafeVault.Security;

public interface IInputValidator
{
    bool ValidateEmail(string email);
    bool ValidateUsername(string username);
    bool ValidatePassword(string password);
    string SanitizeHtml(string input);
    bool ContainsSqlInjection(string input);
}

public class InputValidator : IInputValidator
{
    // Regular expressions for validation
    private static readonly Regex EmailRegex = new Regex(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex UsernameRegex = new Regex(
        @"^[a-zA-Z0-9_-]{3,50}$",
        RegexOptions.Compiled);
    
    private static readonly Regex PasswordRegex = new Regex(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{12,}$",
        RegexOptions.Compiled);
    
    // SQL Injection patterns to check for
    private static readonly Regex SqlInjectionRegex = new Regex(
        @"(\b(select|insert|update|delete|drop|alter|create|rename|truncate|exec|declare|union|--)\b)|(/\*.*\*/)|(\b(xp_cmdshell|sp_executesql|sp_)|(\%27))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        return EmailRegex.IsMatch(email);
    }
    
    public bool ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;
        
        return UsernameRegex.IsMatch(username);
    }
    
    public bool ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
        
        return PasswordRegex.IsMatch(password);
    }
    
    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        
        // Replace HTML tags with encoded entities
        input = input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
        
        return input;
    }
    
    public bool ContainsSqlInjection(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        
        return SqlInjectionRegex.IsMatch(input);
    }
}
