# Classe Abstrata vs Interface

## Visão geral

Projeto console que demonstra, em um domínio de biblioteca, a diferença prática entre classe abstrata e interface em C#.

A classe abstrata modela recursos do acervo que compartilham estado e comportamento base. A interface modela a capacidade de reserva, que pode ser aplicada a tipos diferentes sem exigir uma hierarquia comum.

## Conceitos abordados

- Classe abstrata com construtor, propriedades compartilhadas e método virtual.
- Interface como contrato de comportamento reutilizável.
- Polimorfismo com uma lista de `RecursoBiblioteca`.
- Reutilização de um mesmo objeto em mais de uma abstração.
- Diferença entre identidade comum e capacidade opcional.

## Objetivos de aprendizagem

- Entender quando uma classe abstrata é mais adequada que uma interface.
- Entender quando uma interface é mais flexível do que herança.
- Visualizar um domínio real onde os dois recursos coexistem.
- Reconhecer que um mesmo tipo pode herdar de uma classe e implementar uma interface ao mesmo tempo.

## Estrutura do projeto

```text
AbstractClassVsInterfaceDemo/
|-- Models/
|   |-- IReservavel.cs
|   |-- LivroFisico.cs
|   |-- RecursoBiblioteca.cs
|   |-- Revista.cs
|   `-- SalaEstudo.cs
|-- AbstractClassVsInterfaceDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/AbstractClassVsInterfaceDemo/AbstractClassVsInterfaceDemo.csproj
```

## Boas práticas e pontos de atenção

- Use classe abstrata quando houver estado compartilhado, comportamento comum e uma familia clara de tipos.
- Use interface quando a regra for um contrato transversal que pode ser aplicado a classes diferentes.
- Evite transformar interface em "classe sem estado" com implementacoes forçadas demais.
- Evite usar heranca apenas para reaproveitar codigo quando o relacionamento de dominio nao existir.

## Conteúdo complementar

### Leitura prática do exemplo

- `RecursoBiblioteca` centraliza codigo, titulo, setor e resumo base.
- `LivroFisico` reaproveita a classe abstrata e tambem implementa `IReservavel`.
- `Revista` participa apenas do acervo catalogado.
- `SalaEstudo` nao herda do acervo, mas ainda assim pode ser reservada.

### Comparação rápida

- Classe abstrata: compartilha identidade e implementação.
- Interface: compartilha contrato e flexibilidade de aplicação.
- Abstrata é melhor para uma familia de tipos.
- Interface é melhor para capacidades que atravessam familias diferentes.

## Referências e documentação complementar

- [Classes abstratas](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/abstract)
- [Interfaces](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/interface)
- [Herança e polimorfismo](https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/)
