namespace Factory
{
    /// <summary>
    /// Implementação concreta de um carro
    /// </summary>
    public class Carro : IVeiculo
    {
        public string Modelo { get; set; }
        public int NumeroPortas { get; set; }

        public Carro(string modelo = "Sedan", int numeroPortas = 4)
        {
            Modelo = modelo;
            NumeroPortas = numeroPortas;
        }

        public void ExibirInfo()
        {
            Console.WriteLine($"Carro: {Modelo} com {NumeroPortas} portas");
        }

        public void Acelerar()
        {
            Console.WriteLine("Carro acelerando com motor a combustão... Vrum!");
        }
    }

    /// <summary>
    /// Implementação concreta de uma moto
    /// </summary>
    public class Moto : IVeiculo
    {
        public string Tipo { get; set; }
        public int Cilindradas { get; set; }

        public Moto(string tipo = "Street", int cilindradas = 250)
        {
            Tipo = tipo;
            Cilindradas = cilindradas;
        }

        public void ExibirInfo()
        {
            Console.WriteLine($"Moto: {Tipo} de {Cilindradas}cc");
        }

        public void Acelerar()
        {
            Console.WriteLine("Moto acelerando... Vroooom!");
        }
    }

    /// <summary>
    /// Implementação concreta de uma bicicleta
    /// </summary>
    public class Bicicleta : IVeiculo
    {
        public string Tipo { get; set; }
        public int NumeroMarchas { get; set; }

        public Bicicleta(string tipo = "Mountain Bike", int numeroMarchas = 21)
        {
            Tipo = tipo;
            NumeroMarchas = numeroMarchas;
        }

        public void ExibirInfo()
        {
            Console.WriteLine($"Bicicleta: {Tipo} com {NumeroMarchas} marchas");
        }

        public void Acelerar()
        {
            Console.WriteLine("Pedalando mais rápido... Fush!");
        }
    }

    /// <summary>
    /// Implementação especializada de carro elétrico
    /// </summary>
    public class CarroEletrico : IVeiculo
    {
        public string Modelo { get; set; }
        public int AutonomiaKm { get; set; }

        public CarroEletrico(string modelo = "Tesla Model S", int autonomiaKm = 500)
        {
            Modelo = modelo;
            AutonomiaKm = autonomiaKm;
        }

        public void ExibirInfo()
        {
            Console.WriteLine($"Carro Elétrico: {Modelo} com autonomia de {AutonomiaKm}km");
        }

        public void Acelerar()
        {
            Console.WriteLine("Carro elétrico acelerando silenciosamente... Whoosh!");
        }
    }
}
