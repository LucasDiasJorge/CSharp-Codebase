using System;

namespace AbstractClassVsInterfaceDemo.Models;

public sealed class SalaEstudo : IReservavel
{
    public SalaEstudo(
        string nome,
        int capacidadeMaxima)
    {
        Nome = nome;
        CapacidadeMaxima = capacidadeMaxima;
    }

    public string Nome { get; }

    public int CapacidadeMaxima { get; }

    public string? ReservadoPara { get; private set; }

    public bool EstaReservado => ReservadoPara is not null;

    public void ReservarPara(string responsavel)
    {
        if (string.IsNullOrWhiteSpace(responsavel))
        {
            throw new ArgumentException("O responsavel da reserva nao pode ficar vazio.", nameof(responsavel));
        }

        if (EstaReservado)
        {
            throw new InvalidOperationException($"A sala '{Nome}' ja esta reservada para {ReservadoPara}.");
        }

        ReservadoPara = responsavel;
    }

    public string ObterStatusReserva()
    {
        if (EstaReservado)
        {
            return $"reservada para {ReservadoPara} | capacidade {CapacidadeMaxima} pessoas";
        }

        return $"disponivel para reserva | capacidade {CapacidadeMaxima} pessoas";
    }
}
