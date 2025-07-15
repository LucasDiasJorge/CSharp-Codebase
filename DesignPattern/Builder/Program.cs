namespace Builder;

class Program
{
    static void Main(string[] args)
    {
        var pedido = new Pedido.Builder()
            .ComCliente("Lucas Jorge")
            .ComProduto("Notebook")
            .ComQuantidade(1)
            .ComObservacoes("Entregar após às 18h")
            .ComEntregaExpressa()
            .Build();

        Console.WriteLine($"Pedido de {pedido.Produto} para {pedido.Cliente}, entrega expressa: {pedido.EntregaExpressa}");
    }
}
