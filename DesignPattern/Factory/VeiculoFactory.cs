namespace Factory
{
    /// <summary>
    /// Factory principal que cria diferentes tipos de veículos
    /// Implementa o padrão Simple Factory
    /// </summary>
    public static class VeiculoFactory
    {
        /// <summary>
        /// Cria um veículo baseado no tipo especificado
        /// </summary>
        /// <param name="tipo">Tipo do veículo a ser criado</param>
        /// <returns>Instância do veículo solicitado</returns>
        /// <exception cref="ArgumentException">Lançada quando o tipo não é suportado</exception>
        public static IVeiculo CriarVeiculo(TipoVeiculo tipo)
        {
            return tipo switch
            {
                TipoVeiculo.Carro => new Carro(),
                TipoVeiculo.Moto => new Moto(),
                TipoVeiculo.Bicicleta => new Bicicleta(),
                _ => throw new ArgumentException($"Tipo de veículo {tipo} não é suportado.")
            };
        }

        /// <summary>
        /// Cria um veículo com parâmetros customizados
        /// </summary>
        /// <param name="tipo">Tipo do veículo</param>
        /// <param name="parametros">Parâmetros específicos do veículo</param>
        /// <returns>Instância do veículo customizado</returns>
        public static IVeiculo CriarVeiculoCustomizado(TipoVeiculo tipo, params object[] parametros)
        {
            return tipo switch
            {
                TipoVeiculo.Carro when parametros.Length >= 2 => 
                    new Carro((string)parametros[0], (int)parametros[1]),
                TipoVeiculo.Moto when parametros.Length >= 2 => 
                    new Moto((string)parametros[0], (int)parametros[1]),
                TipoVeiculo.Bicicleta when parametros.Length >= 2 => 
                    new Bicicleta((string)parametros[0], (int)parametros[1]),
                _ => CriarVeiculo(tipo)
            };
        }
    }

    /// <summary>
    /// Factory específica para criação de carros
    /// Implementa o padrão Factory Method
    /// </summary>
    public static class CarroFactory
    {
        public static CarroEletrico CriarCarroEletrico()
        {
            return new CarroEletrico();
        }

        public static Carro CriarCarroSedan()
        {
            return new Carro("Sedan Premium", 4);
        }

        public static Carro CriarCarroHatch()
        {
            return new Carro("Hatchback Compacto", 2);
        }

        public static Carro CriarCarroSUV()
        {
            return new Carro("SUV 4x4", 5);
        }
    }

    /// <summary>
    /// Factory específica para criação de motos
    /// </summary>
    public static class MotoFactory
    {
        public static Moto CriarMotoEsportiva()
        {
            return new Moto("Esportiva", 1000);
        }

        public static Moto CriarMotoStreet()
        {
            return new Moto("Street", 250);
        }

        public static Moto CriarMotoChopper()
        {
            return new Moto("Chopper", 1200);
        }
    }
}
