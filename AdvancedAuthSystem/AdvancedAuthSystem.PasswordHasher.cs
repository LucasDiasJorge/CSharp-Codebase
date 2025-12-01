using System.Security.Cryptography;
using System.Text;

namespace AdvancedAuthSystem.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

public interface ITwoFactorService
{
    string GenerateSecret();
    string GenerateQrCodeUrl(string username, string secret, string issuer = "AdvancedAuthSystem");
    bool ValidateCode(string secret, string code);
    string GetManualEntryKey(string secret);
}

public class TwoFactorService : ITwoFactorService
{
    public string GenerateSecret()
    {
        var bytes = new byte[20];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base32Encode(bytes);
    }

    public string GenerateQrCodeUrl(string username, string secret, string issuer = "AdvancedAuthSystem")
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedUsername = Uri.EscapeDataString(username);
        return $"otpauth://totp/{encodedIssuer}:{encodedUsername}?secret={secret}&issuer={encodedIssuer}";
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 6)
            return false;

        var window = TimeSpan.FromMinutes(1);
        var currentTime = DateTimeOffset.UtcNow;
        
        // Check current time and ±1 time step for clock skew
        for (int i = -1; i <= 1; i++)
        {
            var timeStep = currentTime.Add(TimeSpan.FromSeconds(i * 30));
            var computedCode = GenerateCode(secret, timeStep);
            if (computedCode == code)
                return true;
        }
        
        return false;
    }

    public string GetManualEntryKey(string secret)
    {
        return secret;
    }

    private string GenerateCode(string secret, DateTimeOffset timeStep)
    {
        var counter = (long)(timeStep.ToUnixTimeSeconds() / 30);
        var key = Base32Decode(secret);
        
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);
        
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24) |
                     ((hash[offset + 1] & 0xFF) << 16) |
                     ((hash[offset + 2] & 0xFF) << 8) |
                     (hash[offset + 3] & 0xFF);
        
        var otp = binary % 1000000;
        return otp.ToString("D6");
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        
        for (int i = 0; i < data.Length; i += 5)
        {
            int byteCount = Math.Min(5, data.Length - i);
            ulong buffer = 0;
            
            for (int j = 0; j < byteCount; j++)
                buffer = (buffer << 8) | data[i + j];
            
            int bitCount = byteCount * 8;
            while (bitCount > 0)
            {
                int index = (int)((buffer >> (bitCount - 5)) & 0x1F);
                result.Append(alphabet[index]);
                bitCount -= 5;
            }
        }
        
        return result.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        input = input.TrimEnd('=').ToUpperInvariant();
        var bytes = new List<byte>();
        
        for (int i = 0; i < input.Length; i += 8)
        {
            ulong buffer = 0;
            int charsToProcess = Math.Min(8, input.Length - i);
            
            for (int j = 0; j < charsToProcess; j++)
            {
                char c = input[i + j];
                int value = c >= 'A' && c <= 'Z' ? c - 'A' :
                           c >= '2' && c <= '7' ? c - '2' + 26 : 0;
                buffer = (buffer << 5) | (ulong)value;
            }
            
            int bitCount = charsToProcess * 5;
            while (bitCount >= 8)
            {
                bytes.Add((byte)((buffer >> (bitCount - 8)) & 0xFF));
                bitCount -= 8;
            }
        }
        
        return bytes.ToArray();
    }
}
