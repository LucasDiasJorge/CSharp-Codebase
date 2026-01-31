namespace UseCases.Examples.AuthenticateUser.Interfaces;

/// <summary>
/// Serviço de verificação de senha
/// </summary>
public interface IPasswordVerifier
{
    bool Verify(string password, string hash);
}
