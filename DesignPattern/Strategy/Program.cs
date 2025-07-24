// Interface da estratégia
public interface IPagamentoStrategy
{
    void Pagar(decimal valor);
}

// Estratégia 1: pagamento com cartão de crédito
public class CartaoCreditoStrategy : IPagamentoStrategy
{
    public void Pagar(decimal valor)
    {
        Console.WriteLine($"Pagando R${valor} com Cartão de Crédito.");
    }
}

// Estratégia 2: pagamento com Pix
public class PixStrategy : IPagamentoStrategy
{
    public void Pagar(decimal valor)
    {
        Console.WriteLine($"Pagando R${valor} com Pix.");
    }
}

// Estratégia 3: pagamento com boleto
public class BoletoStrategy : IPagamentoStrategy
{
    public void Pagar(decimal valor)
    {
        Console.WriteLine($"Gerando boleto para R${valor}.");
    }
}

// Contexto que usa a estratégia
public class ProcessadorPagamento
{
    private IPagamentoStrategy _estrategia;

    public ProcessadorPagamento(IPagamentoStrategy estrategia)
    {
        _estrategia = estrategia;
    }

    public void SetEstrategia(IPagamentoStrategy estrategia)
    {
        _estrategia = estrategia;
    }

    public void ProcessarPagamento(decimal valor)
    {
        _estrategia.Pagar(valor);
    }
}

// Exemplo de uso
class Program
{
    static void Main(string[] args)
    {
        ProcessadorPagamento processador = new ProcessadorPagamento(new CartaoCreditoStrategy());
        processador.ProcessarPagamento(150.00m);

        // Troca a estratégia em tempo de execução
        processador.SetEstrategia(new PixStrategy());
        processador.ProcessarPagamento(75.00m);
    }
}
