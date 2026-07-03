namespace ParameterObject;

// Antes do padrao: lista longa de parametros posicionais, facil de trocar por engano
// (ex: inverter Email e Telefone) e dificil de estender sem quebrar todas as chamadas.
public static class GeradorContratoSemPattern
{
    public static string CriarContrato(
        string nomeCliente,
        string email,
        string telefone,
        DateTime dataInicio,
        DateTime dataFim,
        decimal valorMensal,
        string moeda,
        bool renovacaoAutomatica)
    {
        string renovacao = renovacaoAutomatica ? "com renovacao automatica" : "sem renovacao automatica";

        return $"Contrato de {nomeCliente} ({email}, {telefone}) - " +
               $"vigencia de {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}, " +
               $"valor mensal de {valorMensal:0.00} {moeda}, {renovacao}.";
    }
}
