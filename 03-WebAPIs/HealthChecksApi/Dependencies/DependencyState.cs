namespace HealthChecksApi.Dependencies;

/// <summary>
/// Estado simulado de uma dependência externa. Existe para que os cenários de falha
/// sejam alternáveis em tempo de execução, sem derrubar banco nem broker de verdade.
/// Os três modos de falha são diferentes de propósito — cada um exercita um caminho
/// distinto do health check.
/// </summary>
public enum DependencyState
{
    /// <summary>Respondendo rápido.</summary>
    Healthy,

    /// <summary>Responde, mas acima do orçamento de latência do cache (1s).</summary>
    Slow,

    /// <summary>Demora mais que o timeout configurado no registro (5s).</summary>
    Hanging,

    /// <summary>Recusa a conexão.</summary>
    Down
}
