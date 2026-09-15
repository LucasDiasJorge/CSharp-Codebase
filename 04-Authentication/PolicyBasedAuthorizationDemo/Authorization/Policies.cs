namespace PolicyBasedAuthorizationDemo.Authorization;

/// <summary>
/// Nomes das policies em um lugar só. Policy é identificada por string, e string solta
/// espalhada por controllers é erro de digitação esperando para acontecer — o pedido
/// simplesmente passa a falhar sem que nada acuse o motivo.
/// </summary>
public static class Policies
{
    /// <summary>Exige nível de acesso mínimo, via requirement próprio.</summary>
    public const string ConfidentialAccess = "ConfidentialAccess";

    /// <summary>Exige uma claim de valor exato — o caso mais simples.</summary>
    public const string EngineeringOnly = "EngineeringOnly";

    /// <summary>Regra inline com <c>RequireAssertion</c>.</summary>
    public const string WeekdayOnly = "WeekdayOnly";

    /// <summary>Duas exigências na mesma policy: valem em conjunto (AND).</summary>
    public const string SeniorEngineering = "SeniorEngineering";
}
