namespace DictionaryMaster.Models;

public class PerguntaQuiz
{
    public PerguntaQuiz(
        string enunciado,
        Dictionary<char, string> opcoes,
        char alternativaCorreta,
        string explicacao)
    {
        Enunciado = enunciado;
        Opcoes = opcoes;
        AlternativaCorreta = char.ToUpperInvariant(alternativaCorreta);
        Explicacao = explicacao;
    }

    public string Enunciado { get; }

    public Dictionary<char, string> Opcoes { get; }

    public char AlternativaCorreta { get; }

    public string Explicacao { get; }
}
