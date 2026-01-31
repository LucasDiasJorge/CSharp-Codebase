# Factory Pattern - Sistema de Veículos

## 📖 Visão Geral

Este projeto implementa o **Design Pattern Factory** através de um sistema de criação de veículos. O padrão Factory é usado para criar objetos sem especificar a classe exata do objeto que será criado, promovendo o desacoplamento entre a criação e o uso dos objetos.

## 🎯 Problema Resolvido

O padrão Factory resolve os seguintes problemas:
- **Acoplamento forte** - Reduz dependência entre cliente e classes concretas
- **Criação complexa** - Centraliza lógica de criação de objetos
- **Violação do DIP** - Depende de abstrações, não de implementações
- **Dificuldade de manutenção** - Facilita mudanças na criação de objetos

## 🏗️ Tipos de Factory Implementados

### 1. Simple Factory (Static Factory)
A implementação mais simples, com um método estático que decide qual objeto criar.

```csharp
public static class VeiculoFactory
{
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
}
```

### 2. Factory Method
Implementações específicas para cada tipo de produto.

```csharp
public static class CarroFactory
{
    public static CarroEletrico CriarCarroEletrico()
        => new CarroEletrico();

    public static Carro CriarCarroSedan()
        => new Carro("Sedan Premium", 4);

    public static Carro CriarCarroSUV()
        => new Carro("SUV 4x4", 5);
}
```

## 🚗 Hierarquia de Veículos

### Interface Base
```csharp
public interface IVeiculo
{
    void ExibirInfo();
    void Acelerar();
}
```

### Implementações Concretas

#### Carro
```csharp
public class Carro : IVeiculo
{
    public string Modelo { get; set; }
    public int NumeroPortas { get; set; }

    public void ExibirInfo()
        => Console.WriteLine($"Carro: {Modelo} com {NumeroPortas} portas");

    public void Acelerar()
        => Console.WriteLine("Carro acelerando com motor a combustão... Vrum!");
}
```

#### Moto
```csharp
public class Moto : IVeiculo
{
    public string Tipo { get; set; }
    public int Cilindradas { get; set; }

    public void ExibirInfo()
        => Console.WriteLine($"Moto: {Tipo} de {Cilindradas}cc");

    public void Acelerar()
        => Console.WriteLine("Moto acelerando... Vroooom!");
}
```

#### Carro Elétrico
```csharp
public class CarroEletrico : IVeiculo
{
    public string Modelo { get; set; }
    public int AutonomiaKm { get; set; }

    public void ExibirInfo()
        => Console.WriteLine($"Carro Elétrico: {Modelo} com autonomia de {AutonomiaKm}km");

    public void Acelerar()
        => Console.WriteLine("Carro elétrico acelerando silenciosamente... Whoosh!");
}
```

## 🚀 Como Usar

### Simple Factory
```csharp
// Criação usando enum
IVeiculo carro = VeiculoFactory.CriarVeiculo(TipoVeiculo.Carro);
carro.ExibirInfo();
carro.Acelerar();

// Criação customizada
IVeiculo carroCustom = VeiculoFactory.CriarVeiculoCustomizado(
    TipoVeiculo.Carro, 
    "BMW X5", 
    5
);
```

### Factory Method
```csharp
// Usando factories específicas
var carroEletrico = CarroFactory.CriarCarroEletrico();
var motoEsportiva = MotoFactory.CriarMotoEsportiva();

carroEletrico.ExibirInfo();
motoEsportiva.ExibirInfo();
```

## ✅ Vantagens

1. **Desacoplamento**: Cliente não depende de classes concretas
2. **Centralização**: Lógica de criação em um local
3. **Extensibilidade**: Fácil adição de novos tipos
4. **Testabilidade**: Permite injeção de mocks
5. **Princípios SOLID**: Segue SRP, OCP e DIP

## 🔄 Variações Avançadas

### Abstract Factory
Para famílias de objetos relacionados:

```csharp
public interface IVeiculoAbstractFactory
{
    IVeiculo CriarVeiculoTerrestre();
    IVeiculo CriarVeiculoAquatico();
}

public class VeiculoEleticoFactory : IVeiculoAbstractFactory
{
    public IVeiculo CriarVeiculoTerrestre() => new CarroEletrico();
    public IVeiculo CriarVeiculoAquatico() => new BarcoEletrico();
}

public class VeiculoCombustaoFactory : IVeiculoAbstractFactory
{
    public IVeiculo CriarVeiculoTerrestre() => new Carro();
    public IVeiculo CriarVeiculoAquatico() => new Lancha();
}
```

### Factory com Dependency Injection
```csharp
public interface IVeiculoFactory
{
    IVeiculo CriarVeiculo(TipoVeiculo tipo);
}

public class VeiculoFactory : IVeiculoFactory
{
    private readonly IServiceProvider _serviceProvider;

    public VeiculoFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IVeiculo CriarVeiculo(TipoVeiculo tipo)
    {
        return tipo switch
        {
            TipoVeiculo.Carro => _serviceProvider.GetService<Carro>(),
            TipoVeiculo.Moto => _serviceProvider.GetService<Moto>(),
            _ => throw new ArgumentException("Tipo não suportado")
        };
    }
}
```

### Factory com Configuração
```csharp
public class VeiculoFactoryComConfig
{
    private readonly IConfiguration _config;

    public VeiculoFactoryComConfig(IConfiguration config)
    {
        _config = config;
    }

    public IVeiculo CriarVeiculoPadrao()
    {
        var tipoConfig = _config["VeiculoPadrao"];
        return Enum.Parse<TipoVeiculo>(tipoConfig) switch
        {
            TipoVeiculo.Carro => new Carro(),
            TipoVeiculo.Moto => new Moto(),
            _ => new Bicicleta()
        };
    }
}
```

## 🎯 Casos de Uso Ideais

- **Sistemas de logging** com diferentes providers
- **Acesso a dados** com diferentes repositórios
- **Criação de UI** com diferentes temas
- **Parsers** para diferentes formatos de arquivo
- **Conexões de banco** para diferentes SGBDs

## ❌ Quando NÃO usar

- Apenas uma implementação de interface
- Criação de objetos é trivial
- Não há necessidade de desacoplamento
- Sistema muito simples

## 🧪 Exemplos de Teste

```csharp
[Test]
public void VeiculoFactory_DeveCriarCarro_QuandoTipoForCarro()
{
    // Act
    var veiculo = VeiculoFactory.CriarVeiculo(TipoVeiculo.Carro);

    // Assert
    Assert.IsType<Carro>(veiculo);
}

[Test]
public void VeiculoFactory_DeveLancarExcecao_QuandoTipoInvalido()
{
    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        VeiculoFactory.CriarVeiculo((TipoVeiculo)999)
    );
}

[Test]
public void CarroFactory_DeveCriarCarroEletrico_ComPropriedadesPadrao()
{
    // Act
    var carro = CarroFactory.CriarCarroEletrico();

    // Assert
    Assert.IsType<CarroEletrico>(carro);
    Assert.Equal("Tesla Model S", ((CarroEletrico)carro).Modelo);
    Assert.Equal(500, ((CarroEletrico)carro).AutonomiaKm);
}
```

## 📊 Comparação de Factories

| Tipo | Complexidade | Flexibilidade | Uso |
|------|-------------|---------------|-----|
| **Simple Factory** | Baixa | Baixa | Casos simples |
| **Factory Method** | Média | Média | Extensibilidade |
| **Abstract Factory** | Alta | Alta | Famílias de objetos |

## 🔧 Integrações

### Com Builder Pattern
```csharp
public static class VeiculoFactory
{
    public static Carro.Builder CriarBuilderCarro()
    {
        return new Carro.Builder();
    }
}

// Uso combinado
var carro = VeiculoFactory.CriarBuilderCarro()
    .ComModelo("Civic")
    .ComPortas(4)
    .ComCor("Preto")
    .Build();
```

### Com Strategy Pattern
```csharp
public class VeiculoComStrategy
{
    private readonly IVeiculo _veiculo;
    private readonly IAceleracaoStrategy _strategy;

    public VeiculoComStrategy(TipoVeiculo tipo)
    {
        _veiculo = VeiculoFactory.CriarVeiculo(tipo);
        _strategy = AceleracaoStrategyFactory.Criar(tipo);
    }
}
```

## 📚 Referências

- [Factory Pattern - Refactoring Guru](https://refactoring.guru/design-patterns/factory-method)
- [Abstract Factory Pattern](https://refactoring.guru/design-patterns/abstract-factory)
- [Design Patterns - Gang of Four](https://en.wikipedia.org/wiki/Design_Patterns)
- [Effective Java - Joshua Bloch](https://www.oracle.com/java/technologies/javase/effectivejava.html)
