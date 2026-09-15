namespace RefreshTokenRotationDemo.Tokens;

/// <summary>
/// Ciclo de vida de um refresh token sob rotação. Um token usado não é apagado: ele
/// fica registrado como <see cref="Used"/> justamente para que a reapresentação seja
/// detectável.
/// </summary>
public enum RefreshTokenStatus
{
    /// <summary>Vale uma única troca.</summary>
    Active,

    /// <summary>Já foi trocado por outro. Reapresentá-lo é sinal de vazamento.</summary>
    Used,

    /// <summary>Invalidado por logout ou por detecção de reuso na família.</summary>
    Revoked
}
