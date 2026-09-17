namespace CacheStampedeProtectionDemo.Caching;

/// <summary>
/// TTL com variação aleatória. Existe para o caso que passa despercebido: quando muitas
/// chaves são populadas no mesmo instante — logo após um deploy, ou depois de um
/// <c>FLUSHALL</c> — um TTL fixo faz todas expirarem no mesmo instante também, e o
/// stampede volta multiplicado por todas as chaves de uma vez.
/// </summary>
public static class TtlJitter
{
    /// <summary>Fração do TTL usada como amplitude do sorteio.</summary>
    public const double JitterFraction = 0.2;

    /// <summary>
    /// Devolve o TTL base com variação de ±20%. Sorteia por chamada, então duas chaves
    /// populadas no mesmo milissegundo recebem prazos diferentes.
    /// </summary>
    public static TimeSpan Apply(TimeSpan baseTtl)
    {
        double factor = 1.0 + ((Random.Shared.NextDouble() * 2.0 - 1.0) * JitterFraction);

        return TimeSpan.FromMilliseconds(baseTtl.TotalMilliseconds * factor);
    }
}
