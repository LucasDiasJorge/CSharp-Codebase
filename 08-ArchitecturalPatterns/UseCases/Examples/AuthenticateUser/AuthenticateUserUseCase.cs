using UseCases.Core;
using UseCases.Examples.AuthenticateUser.DTOs;
using UseCases.Examples.AuthenticateUser.Entities;
using UseCases.Examples.AuthenticateUser.Interfaces;

namespace UseCases.Examples.AuthenticateUser;

/// <summary>
/// Use Case: Autenticação de Usuário
/// 
/// Implementa:
/// - Verificação de credenciais
/// - Proteção contra brute-force (lockout)
/// - Geração de tokens JWT
/// - Refresh tokens
/// - Auditoria de logins
/// </summary>
public class AuthenticateUserUseCase : IUseCase<AuthenticateUserInput, Result<AuthenticateUserOutput>>
{
    private readonly IAuthUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILoginAuditRepository _auditRepository;
    private readonly IPasswordVerifier _passwordVerifier;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthenticateUserUseCase(
        IAuthUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ILoginAuditRepository auditRepository,
        IPasswordVerifier passwordVerifier,
        IJwtTokenGenerator jwtGenerator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _auditRepository = auditRepository;
        _passwordVerifier = passwordVerifier;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<Result<AuthenticateUserOutput>> ExecuteAsync(
        AuthenticateUserInput input,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar entrada
        if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
        {
            await LogFailedAttempt(input.Email, null, "Credenciais vazias", input.IpAddress, input.UserAgent, cancellationToken);
            return Result<AuthenticateUserOutput>.Failure("Email e senha são obrigatórios");
        }

        // 2. Buscar usuário
        var user = await _userRepository.GetByEmailAsync(input.Email, cancellationToken);
        if (user is null)
        {
            await LogFailedAttempt(input.Email, null, "Usuário não encontrado", input.IpAddress, input.UserAgent, cancellationToken);
            // Mensagem genérica para não revelar se email existe
            return Result<AuthenticateUserOutput>.Failure("Credenciais inválidas");
        }

        // 3. Verificar se pode fazer login
        var canLoginResult = user.CanLogin();
        if (canLoginResult.IsFailure)
        {
            await LogFailedAttempt(input.Email, user.Id, canLoginResult.Error, input.IpAddress, input.UserAgent, cancellationToken);
            return Result<AuthenticateUserOutput>.Failure(canLoginResult.Error);
        }

        // 4. Verificar senha
        if (!_passwordVerifier.Verify(input.Password, user.PasswordHash))
        {
            var failResult = user.RecordFailedLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await LogFailedAttempt(input.Email, user.Id, "Senha incorreta", input.IpAddress, input.UserAgent, cancellationToken);
            return Result<AuthenticateUserOutput>.Failure(failResult.Error);
        }

        // 5. Registrar login bem-sucedido
        user.RecordSuccessfulLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // 6. Gerar tokens
        var (accessToken, expiresAt) = _jwtGenerator.GenerateAccessToken(user.Id, user.Email, user.Roles);

        // Revogar tokens antigos e criar novo refresh token
        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, cancellationToken);
        var refreshToken = RefreshToken.Create(user.Id, input.IpAddress, input.UserAgent);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        // 7. Registrar auditoria de sucesso
        var successLog = LoginAuditLog.Create(input.Email, user.Id, true, null, input.IpAddress, input.UserAgent);
        await _auditRepository.AddAsync(successLog, cancellationToken);

        // 8. Retornar resultado
        return Result<AuthenticateUserOutput>.Success(new AuthenticateUserOutput(
            user.Id,
            user.Email,
            user.Name,
            accessToken,
            refreshToken.Token,
            expiresAt,
            user.Roles
        ));
    }

    private async Task LogFailedAttempt(string email, Guid? userId, string reason, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        var log = LoginAuditLog.Create(email, userId, false, reason, ipAddress, userAgent);
        await _auditRepository.AddAsync(log, cancellationToken);
    }
}
