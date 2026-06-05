using System;
using System.Collections.Generic;
using AbstractClassVsInterfaceDemo.Models;

namespace AbstractClassVsInterfaceDemo;

public static class Program
{
    public static void Main()
    {
        LivroFisico livro = new LivroFisico(
            "LB-101",
            "C# em Profundidade",
            "Jon Skeet",
            920,
            "Biblioteca Central");

        Revista revista = new Revista(
            "RV-202",
            "Arquitetura .NET",
            48,
            "Sala de Leitura");

        SalaEstudo sala = new SalaEstudo("Sala de Estudo 03", 6);

        List<RecursoBiblioteca> acervo = new List<RecursoBiblioteca>
        {
            livro,
            revista
        };

        List<IReservavel> reservaveis = new List<IReservavel>
        {
            livro,
            sala
        };

        PrintTitle("Classe abstrata x interface no dominio da biblioteca");

        Console.WriteLine("1. Classe abstrata: recursos com estado e comportamento compartilhados");
        foreach (RecursoBiblioteca recurso in acervo)
        {
            Console.WriteLine($"- {recurso.ObterResumo()}");
        }

        Console.WriteLine();
        Console.WriteLine("2. Interface: capacidade de reserva aplicada a tipos diferentes");
        foreach (IReservavel reservavel in reservaveis)
        {
            reservavel.ReservarPara("Equipe de estudos");
            Console.WriteLine($"- {reservavel.Nome}: {reservavel.ObterStatusReserva()}");
        }

        Console.WriteLine();
        Console.WriteLine("3. O mesmo objeto pode participar das duas estruturas quando fizer sentido");
        Console.WriteLine($"- {livro.Titulo} aparece no acervo como {livro.ObterCategoria()} e tambem e reservavel.");
        Console.WriteLine($"- {sala.Nome} nao herda de RecursoBiblioteca, mas implementa IReservavel.");

        Console.WriteLine();
        Console.WriteLine("Regra pratica:");
        Console.WriteLine("- Use classe abstrata quando houver identidade comum, estado compartilhado e implementacao base.");
        Console.WriteLine("- Use interface quando quiser um contrato reutilizavel em classes de familias diferentes.");
    }

    private static void PrintTitle(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine(new string('=', title.Length));
        Console.WriteLine();
    }
}
