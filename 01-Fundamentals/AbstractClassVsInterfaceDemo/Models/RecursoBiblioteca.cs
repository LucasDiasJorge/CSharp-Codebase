namespace AbstractClassVsInterfaceDemo.Models;

public abstract class RecursoBiblioteca
{
    protected RecursoBiblioteca(
        string codigo,
        string titulo,
        string setor)
    {
        Codigo = codigo;
        Titulo = titulo;
        Setor = setor;
    }

    public string Codigo { get; }

    public string Titulo { get; }

    public string Setor { get; }

    public abstract string ObterCategoria();

    public virtual string ObterResumo()
    {
        return $"{ObterCategoria()} | {Titulo} | Codigo {Codigo} | Setor {Setor}";
    }
}
