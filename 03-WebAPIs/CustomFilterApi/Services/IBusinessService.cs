using System;

namespace CustomFilterApi.Services;

public interface IBusinessService
{
    /// <summary>
    /// Executa a lógica de negócio para o payload atual e retorna um identificador/resultado simples para demonstração.
    /// </summary>
    /// <param name="payload">Objeto recebido pela action (ex.: UserDto)</param>
    /// <returns>Resultado simples da execução do serviço</returns>
    string Execute(object payload);
}
