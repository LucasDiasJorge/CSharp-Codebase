using CsCheck;

namespace PropertyBasedTestingDemo.Tests;

/// <summary>
/// Ajudante para tratar a falha de uma propriedade como <b>dado</b>, e não como teste
/// vermelho.
///
/// Os exemplos deste projeto precisam mostrar uma propriedade reprovando uma
/// implementação errada. Rodar isso como teste falhando deixaria a suíte vermelha de
/// propósito; capturar a exceção e afirmar o contraexemplo mínimo mantém a suíte verde
/// e documenta o achado.
/// </summary>
public static class PropertyFailure
{
    /// <summary>
    /// Executa a propriedade esperando que ela reprove, e devolve a falha.
    /// </summary>
    public static CsCheckException Expect(Action property) =>
        Assert.Throws<CsCheckException>(property);

    /// <summary>
    /// O contraexemplo mínimo, que o CsCheck imprime na segunda linha da mensagem
    /// depois de encolher o caso original.
    /// </summary>
    public static string Counterexample(CsCheckException failure)
    {
        string[] lines = failure.Message.Split('\n');

        return lines.Length > 1 ? lines[1].Trim() : failure.Message;
    }

    /// <summary>
    /// Quantos passos de encolhimento foram necessários. Varia entre execuções — é o
    /// caminho, não o destino.
    /// </summary>
    public static int ShrinkCount(CsCheckException failure)
    {
        string message = failure.Message;
        int open = message.IndexOf('(');
        int shrinks = message.IndexOf(" shrinks", StringComparison.Ordinal);

        if (open < 0 || shrinks < 0 || shrinks < open)
        {
            return -1;
        }

        return int.Parse(message.AsSpan(open + 1, shrinks - open - 1));
    }
}
