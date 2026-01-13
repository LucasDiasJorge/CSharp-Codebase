using System;

namespace CircuitBreakerDemo;

/// <summary>
/// Simula um serviço externo instável (apenas para demonstração)
/// </summary>
public class ServicoInstavel
{
    private int _chamadas = 0;
    
    public string ChamarAPI()
    {
        _chamadas++;
        
        // Falha nas primeiras 3 chamadas, depois funciona
        if (_chamadas <= 3)
        {
            throw new Exception("Serviço temporariamente indisponível");
        }
        
        return $"Dados recebidos com sucesso! (chamada #{_chamadas})";
    }
}
