namespace ParameterObject;

// Parameter Object: agrupa os dados que antes eram passados como uma longa lista de argumentos.
public sealed class ContratoParametros
{
    public string NomeCliente { get; }
    public string Email { get; }
    public string Telefone { get; }
    public DateTime DataInicio { get; }
    public DateTime DataFim { get; }
    public decimal ValorMensal { get; }
    public string Moeda { get; }
    public bool RenovacaoAutomatica { get; }

    public ContratoParametros(
        string nomeCliente,
        string email,
        string telefone,
        DateTime dataInicio,
        DateTime dataFim,
        decimal valorMensal,
        string moeda,
        bool renovacaoAutomatica)
    {
        NomeCliente = nomeCliente;
        Email = email;
        Telefone = telefone;
        DataInicio = dataInicio;
        DataFim = dataFim;
        ValorMensal = valorMensal;
        Moeda = moeda;
        RenovacaoAutomatica = renovacaoAutomatica;
    }
}
