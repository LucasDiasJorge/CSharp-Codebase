using System;

namespace AbstractClassVsInterfaceDemo.Models;

public sealed class LivroFisico : RecursoBiblioteca, IReservavel
{
    public LivroFisico(
        string codigo,
        string titulo,
        string autor,
        int paginas,
        string setor)
        : base(codigo, titulo, setor)
    {
        Autor = autor;
        Paginas = paginas;
    }

    public string Autor { get; }

    public int Paginas { get; }

    public string? ReservadoPara { get; private set; }

    public bool EstaReservado => ReservadoPara is not null;

    public string Nome => Titulo;

    public override string ObterCategoria()
    {
        return "Livro fisico";
    }

    public override string ObterResumo()
    {
        return base.ObterResumo() + $" | Autor {Autor} | Paginas {Paginas}";
    }

    public void ReservarPara(string responsavel)
    {
        if (string.IsNullOrWhiteSpace(responsavel))
        {
            throw new ArgumentException("O responsavel da reserva nao pode ficar vazio.", nameof(responsavel));
        }

        if (EstaReservado)
        {
            throw new InvalidOperationException($"O livro '{Titulo}' ja esta reservado para {ReservadoPara}.");
        }

        ReservadoPara = responsavel;
    }

    public string ObterStatusReserva()
    {
        if (EstaReservado)
        {
            return $"reservado para {ReservadoPara}";
        }

        return "disponivel para reserva";
    }
}
