namespace AbstractClassVsInterfaceDemo.Models;

public sealed class Revista : RecursoBiblioteca
{
    public Revista(
        string codigo,
        string titulo,
        int edicao,
        string setor)
        : base(codigo, titulo, setor)
    {
        Edicao = edicao;
    }

    public int Edicao { get; }

    public override string ObterCategoria()
    {
        return "Revista";
    }

    public override string ObterResumo()
    {
        return base.ObterResumo() + $" | Edicao {Edicao}";
    }
}
