using System.ComponentModel.DataAnnotations;

namespace ResilientHttpSdk;

/// <summary>
/// Configuração do SDK. Ter uma classe de options em vez de parâmetros de construtor é o
/// que permite configurar por <c>appsettings</c>, validar no start e trocar por ambiente
/// sem recompilar quem usa.
/// </summary>
public sealed class ResilientHttpSdkOptions
{
    public const string SectionName = "ResilientHttpSdk";

    /// <summary>Endereço base da API. Obrigatório — sem ele o SDK não tem para onde falar.</summary>
    [Required(ErrorMessage = "BaseAddress e obrigatorio")]
    public Uri? BaseAddress { get; set; }

    /// <summary>Chave enviada em <c>X-Api-Key</c>. Vem de user secrets ou do cofre, nunca do appsettings versionado.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Timeout de cada tentativa, não da operação inteira.</summary>
    [Range(1, 120, ErrorMessage = "TimeoutSeconds deve estar entre 1 e 120")]
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Quantas tentativas ADICIONAIS após a primeira falhar. Zero desliga o retry — e é
    /// o valor certo para operação não idempotente sem chave de idempotência.
    /// </summary>
    [Range(0, 5, ErrorMessage = "MaxRetryAttempts deve estar entre 0 e 5")]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Atraso base do backoff exponencial entre tentativas.</summary>
    [Range(1, 5_000, ErrorMessage = "BaseDelayMilliseconds deve estar entre 1 e 5000")]
    public int BaseDelayMilliseconds { get; set; } = 200;
}
