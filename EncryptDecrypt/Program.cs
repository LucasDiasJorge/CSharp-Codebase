
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        Console.WriteLine("Escolha uma opção:");
        Console.WriteLine("1 - Encriptar");
        Console.WriteLine("2 - Decriptar");
        Console.Write("Opção: ");
        var opcao = Console.ReadLine();

        if (opcao == "1")
        {
            Console.Write("Digite o texto: ");
            var texto = Console.ReadLine();
            Console.Write("Digite a senha: ");
            var senha = ReadPassword();
            var encrypted = EncryptString(texto, senha);
            Console.WriteLine($"Texto encriptado: {encrypted}");
        }
        else if (opcao == "2")
        {
            Console.Write("Digite o texto encriptado (base64): ");
            var texto = Console.ReadLine();
            Console.Write("Digite a senha: ");
            var senha = ReadPassword();
            try
            {
                var decrypted = DecryptString(texto, senha);
                Console.WriteLine($"Texto decriptado: {decrypted}");
            }
            catch
            {
                Console.WriteLine("Falha ao decriptar. Verifique a senha ou o texto encriptado.");
            }
        }
        else
        {
            Console.WriteLine("Opção inválida.");
        }
    }

    // AES Encryption
    public static string EncryptString(string plainText, string password)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Derive key and IV from password
        var salt = RandomNumberGenerator.GetBytes(16);
        var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        aes.Key = key.GetBytes(32);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Output: [salt][IV][ciphertext] (all base64)
        var result = new byte[salt.Length + aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(aes.IV, 0, result, salt.Length, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, salt.Length + aes.IV.Length, cipherBytes.Length);
        return Convert.ToBase64String(result);
    }

    // AES Decryption
    public static string DecryptString(string encryptedBase64, string password)
    {
        var fullCipher = Convert.FromBase64String(encryptedBase64);
        var salt = new byte[16];
        var iv = new byte[16];
        Buffer.BlockCopy(fullCipher, 0, salt, 0, 16);
        Buffer.BlockCopy(fullCipher, 16, iv, 0, 16);
        var cipher = new byte[fullCipher.Length - 32];
        Buffer.BlockCopy(fullCipher, 32, cipher, 0, cipher.Length);

        var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key.GetBytes(32);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    // Read password without echo
    public static string ReadPassword()
    {
        var pwd = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
            {
                pwd.Length--;
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                pwd.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return pwd.ToString();
    }
}
