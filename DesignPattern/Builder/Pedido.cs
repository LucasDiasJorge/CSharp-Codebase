public class Pedido
{
    public string Cliente { get; private set; } = string.Empty;
    public string Produto { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public string Observacoes { get; private set; } = string.Empty;
    public bool EntregaExpressa { get; private set; }

    private Pedido() { }

    public class Builder
    {
        private readonly Pedido _pedido = new Pedido();

        public Builder ComCliente(string cliente)
        {
            _pedido.Cliente = cliente;
            return this;
        }

        public Builder ComProduto(string produto)
        {
            _pedido.Produto = produto;
            return this;
        }

        public Builder ComQuantidade(int quantidade)
        {
            _pedido.Quantidade = quantidade;
            return this;
        }

        public Builder ComObservacoes(string observacoes)
        {
            _pedido.Observacoes = observacoes;
            return this;
        }

        public Builder ComEntregaExpressa()
        {
            _pedido.EntregaExpressa = true;
            return this;
        }

        public Pedido Build()
        {
            if (string.IsNullOrEmpty(_pedido.Cliente))
                throw new InvalidOperationException("Cliente é obrigatório.");
            if (string.IsNullOrEmpty(_pedido.Produto))
                throw new InvalidOperationException("Produto é obrigatório.");
            if (_pedido.Quantidade <= 0)
                throw new InvalidOperationException("Quantidade deve ser maior que zero.");

            return _pedido;
        }
    }
}
