namespace ShortCircuitEvaluationDemo;

public static class Program
{
    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }

    // Helper methods with visible side effects (Console.WriteLine) used to prove,
    // by observation, whether the right-hand operand of an operator was evaluated.
    public static bool ReturnFalseAndLog()
    {
        Console.WriteLine("  -> ReturnFalseAndLog() foi executado");
        return false;
    }

    public static bool ReturnTrueAndLog()
    {
        Console.WriteLine("  -> ReturnTrueAndLog() foi executado");
        return true;
    }

    public static bool CheckRightSideAndLog()
    {
        Console.WriteLine("  -> CheckRightSideAndLog() foi executado");
        return true;
    }

    public static string GetDefaultNameAndLog()
    {
        Console.WriteLine("  -> GetDefaultNameAndLog() foi executado");
        return "Anonimo";
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Short-Circuit Evaluation Demo (tipos explicitos, sem 'var')");

        // 1) && (AND com short-circuit): se o lado esquerdo eh false, o direito nao eh avaliado.
        PrintHeader("&& com short-circuit: lado esquerdo false");
        Console.WriteLine("Expressao: ReturnFalseAndLog() && CheckRightSideAndLog()");
        bool andShortCircuitResult = ReturnFalseAndLog() && CheckRightSideAndLog();
        Console.WriteLine($"Resultado: {andShortCircuitResult} (CheckRightSideAndLog nao deveria aparecer acima)");

        // 2) & (AND sem short-circuit): ambos os lados sao sempre avaliados.
        PrintHeader("& sem short-circuit: lado esquerdo false");
        Console.WriteLine("Expressao: ReturnFalseAndLog() & CheckRightSideAndLog()");
        bool andBitwiseResult = ReturnFalseAndLog() & CheckRightSideAndLog();
        Console.WriteLine($"Resultado: {andBitwiseResult} (CheckRightSideAndLog aparece mesmo o resultado ja estando decidido)");

        // 3) || (OR com short-circuit): se o lado esquerdo eh true, o direito nao eh avaliado.
        PrintHeader("|| com short-circuit: lado esquerdo true");
        Console.WriteLine("Expressao: ReturnTrueAndLog() || CheckRightSideAndLog()");
        bool orShortCircuitResult = ReturnTrueAndLog() || CheckRightSideAndLog();
        Console.WriteLine($"Resultado: {orShortCircuitResult} (CheckRightSideAndLog nao deveria aparecer acima)");

        // 4) | (OR sem short-circuit): ambos os lados sao sempre avaliados.
        PrintHeader("| sem short-circuit: lado esquerdo true");
        Console.WriteLine("Expressao: ReturnTrueAndLog() | CheckRightSideAndLog()");
        bool orBitwiseResult = ReturnTrueAndLog() | CheckRightSideAndLog();
        Console.WriteLine($"Resultado: {orBitwiseResult} (CheckRightSideAndLog aparece mesmo o resultado ja estando decidido)");

        // 5) Guard clause com null: o short-circuit evita o NullReferenceException.
        PrintHeader("Guard clause com null (evitando NullReferenceException)");
        string? nullableText = null;
        if (nullableText != null && nullableText.Length > 0)
        {
            Console.WriteLine($"Texto valido, tamanho: {nullableText.Length}");
        }
        else
        {
            Console.WriteLine("nullableText != null && nullableText.Length > 0 => false (segundo lado nao foi avaliado, sem excecao)");
        }

        Console.WriteLine("Agora forcando a mesma checagem com '&' (avalia os dois lados sempre):");
        try
        {
#pragma warning disable CS8602 // dereference intencional de referencia possivelmente nula para demonstrar o risco do '&'
            bool unsafeNullCheck = nullableText != null & nullableText.Length > 0;
#pragma warning restore CS8602
            Console.WriteLine($"Resultado inesperado: {unsafeNullCheck}");
        }
        catch (NullReferenceException exception)
        {
            Console.WriteLine($"Excecao capturada: {exception.GetType().Name} (o '&' avaliou nullableText.Length mesmo com nullableText nulo)");
        }

        // 6) Guard clause com divisao: o short-circuit evita o DivideByZeroException.
        PrintHeader("Guard clause com divisao (evitando DivideByZeroException)");
        int numerator = 10;
        int divisor = 0;
        if (divisor != 0 && numerator / divisor > 1)
        {
            Console.WriteLine("Divisao realizada com seguranca");
        }
        else
        {
            Console.WriteLine("divisor != 0 && numerator / divisor > 1 => false (segundo lado nao foi avaliado, sem excecao)");
        }

        Console.WriteLine("Agora forcando a mesma checagem com '&' (avalia os dois lados sempre):");
        try
        {
            bool unsafeDivisionCheck = divisor != 0 & numerator / divisor > 1;
            Console.WriteLine($"Resultado inesperado: {unsafeDivisionCheck}");
        }
        catch (DivideByZeroException exception)
        {
            Console.WriteLine($"Excecao capturada: {exception.GetType().Name} (o '&' avaliou numerator / divisor mesmo com divisor igual a zero)");
        }

        // 7) Guard clause com indice de array: o short-circuit evita o IndexOutOfRangeException.
        PrintHeader("Guard clause com indice de array (evitando IndexOutOfRangeException)");
        int[] numbers = [10, 20, 30];
        int index = 5;
        if (index >= 0 && index < numbers.Length && numbers[index] > 0)
        {
            Console.WriteLine($"numbers[{index}] = {numbers[index]}");
        }
        else
        {
            Console.WriteLine("index >= 0 && index < numbers.Length && numbers[index] > 0 => false (numbers[index] nao foi avaliado, sem excecao)");
        }

        Console.WriteLine("Agora forcando a mesma checagem com '&' (avalia todos os lados sempre):");
        try
        {
            bool unsafeIndexCheck = index >= 0 & index < numbers.Length & numbers[index] > 0;
            Console.WriteLine($"Resultado inesperado: {unsafeIndexCheck}");
        }
        catch (IndexOutOfRangeException exception)
        {
            Console.WriteLine($"Excecao capturada: {exception.GetType().Name} (o '&' avaliou numbers[index] mesmo fora dos limites do array)");
        }

        // 8) Operadores relacionados que tambem fazem short-circuit: ?? e ?.
        PrintHeader("Operadores relacionados: ?? (null-coalescing) e ?. (null-conditional)");

        string? providedName = "Lucas";
        Console.WriteLine("Expressao: providedName ?? GetDefaultNameAndLog() (providedName != null)");
        string finalName = providedName ?? GetDefaultNameAndLog();
        Console.WriteLine($"Resultado: {finalName} (GetDefaultNameAndLog nao deveria aparecer acima)");

        string? missingName = null;
        Console.WriteLine("Expressao: missingName ?? GetDefaultNameAndLog() (missingName == null)");
        string fallbackName = missingName ?? GetDefaultNameAndLog();
        Console.WriteLine($"Resultado: {fallbackName} (GetDefaultNameAndLog deveria aparecer acima)");

        string? maybeNullText = null;
        int? maybeLength = maybeNullText?.Length;
        Console.WriteLine($"maybeNullText?.Length => {(maybeLength.HasValue ? maybeLength.Value.ToString() : "null")} (acesso a .Length foi ignorado pelo '?.')");

        Console.WriteLine();
        Console.WriteLine("Demo finalizada.");
    }
}
