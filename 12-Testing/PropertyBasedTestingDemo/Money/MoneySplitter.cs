namespace PropertyBasedTestingDemo.Money;

/// <summary>
/// Divide um valor em partes iguais. Duas implementações: a que qualquer um escreve
/// primeiro e a que sobrevive às propriedades.
///
/// O requisito é simples de enunciar e é exatamente uma propriedade: <b>dinheiro não
/// aparece nem desaparece</b> — a soma das partes tem de ser igual ao total.
/// </summary>
public static class MoneySplitter
{
    /// <summary>
    /// Versão ingênua: divide e arredonda cada parte. Passa nos exemplos redondos que
    /// alguém escolheria à mão (100/4, 10/2) e perde centavos no resto dos casos.
    /// </summary>
    public static IReadOnlyList<decimal> SplitEvenlyNaive(decimal total, int parts)
    {
        decimal each = Math.Round(total / parts, 2, MidpointRounding.ToEven);

        return Enumerable.Repeat(each, parts).ToList();
    }

    /// <summary>
    /// Versão correta: trabalha na menor unidade (centavos) e distribui o resto entre
    /// as primeiras partes. Nenhum centavo se perde, e nenhuma parte difere de outra
    /// em mais de um centavo.
    /// </summary>
    public static IReadOnlyList<decimal> SplitEvenly(decimal total, int parts)
    {
        if (parts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parts), "o numero de partes precisa ser positivo");
        }

        long cents = (long)Math.Round(total * 100m, MidpointRounding.ToEven);
        long baseCents = cents / parts;
        long remainder = cents - (baseCents * parts);

        List<decimal> result = new List<decimal>(parts);

        for (int index = 0; index < parts; index++)
        {
            // O resto e distribuido um centavo por parte, do inicio para o fim.
            long value = baseCents + (index < remainder ? 1 : 0);

            result.Add(value / 100m);
        }

        return result;
    }
}
