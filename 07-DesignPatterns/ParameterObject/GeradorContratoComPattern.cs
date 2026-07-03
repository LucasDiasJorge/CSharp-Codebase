namespace ParameterObject;

// Depois do padrao: um unico parametro nomeado substitui a lista longa,
// tornando a assinatura estavel mesmo se novos campos forem adicionados a ContratoParametros.
public static class GeradorContratoComPattern
{
    public static string CriarContrato(ContratoParametros parametros)
    {
        string renovacao = parametros.RenovacaoAutomatica ? "com renovacao automatica" : "sem renovacao automatica";

        return $"Contrato de {parametros.NomeCliente} ({parametros.Email}, {parametros.Telefone}) - " +
               $"vigencia de {parametros.DataInicio:dd/MM/yyyy} a {parametros.DataFim:dd/MM/yyyy}, " +
               $"valor mensal de {parametros.ValorMensal:0.00} {parametros.Moeda}, {renovacao}.";
    }
}
