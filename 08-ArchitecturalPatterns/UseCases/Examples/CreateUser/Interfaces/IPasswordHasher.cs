namespace UseCases.Examples.CreateUser.Interfaces;

/// <summary>
/// Interface do serviço de hash de senha
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
