using Factory;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Demonstração do Design Pattern Factory ===\n");

        // Criando veículos usando a Factory
        IVeiculo carro = VeiculoFactory.CriarVeiculo(TipoVeiculo.Carro);
        carro.ExibirInfo();
        carro.Acelerar();
        Console.WriteLine();

        IVeiculo moto = VeiculoFactory.CriarVeiculo(TipoVeiculo.Moto);
        moto.ExibirInfo();
        moto.Acelerar();
        Console.WriteLine();

        IVeiculo bicicleta = VeiculoFactory.CriarVeiculo(TipoVeiculo.Bicicleta);
        bicicleta.ExibirInfo();
        bicicleta.Acelerar();
        Console.WriteLine();

        // Exemplo usando método Factory estático
        Console.WriteLine("=== Usando Factory Method direto ===");
        var carroEletrico = CarroFactory.CriarCarroEletrico();
        carroEletrico.ExibirInfo();
        carroEletrico.Acelerar();
    }
}
