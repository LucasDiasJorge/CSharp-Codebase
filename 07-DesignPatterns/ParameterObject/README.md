# Parameter Object Pattern

Exemplo didático que mostra como substituir uma lista longa de parâmetros por um único objeto nomeado, aplicando o padrão Parameter Object.

## Visão geral

O projeto compara duas versões do mesmo método de geração de contrato: uma recebendo oito parâmetros posicionais e outra recebendo um único `ContratoParametros`. O objetivo é evidenciar como o padrão reduz o risco de troca acidental de argumentos e facilita a evolução da assinatura do método.

## Conceitos abordados

- Code smell "Long Parameter List" e sua relação com o Parameter Object Pattern.
- Encapsulamento de dados relacionados em um objeto imutável.
- Impacto do padrão na legibilidade de chamadas e na estabilidade de assinaturas.

## Objetivos de aprendizagem

- Reconhecer quando uma lista de parâmetros está grande ou frágil demais.
- Extrair um Parameter Object a partir de parâmetros relacionados.
- Comparar diretamente o "antes" e o "depois" da refatoração.

## Estrutura do projeto

```text
ParameterObject/
|-- ContratoParametros.cs
|-- GeradorContratoSemPattern.cs
|-- GeradorContratoComPattern.cs
|-- Program.cs
`-- ParameterObject.csproj
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/ParameterObject/ParameterObject.csproj
```

## Boas práticas e pontos de atenção

- `ContratoParametros` é imutável (propriedades somente leitura definidas no construtor), evitando estado inconsistente após a criação.
- O Parameter Object deve agrupar dados que mudam juntos e fazem sentido como um conceito único (aqui, os dados de um contrato), não parâmetros arbitrários.
- Use argumentos nomeados (`nomeCliente: ...`) ao construir o objeto para manter a leitura clara, já que os tipos por si só não desambiguam campos como `Email` e `Telefone`.

## Conteúdo complementar

### Antes (sem o padrão)

```csharp
GeradorContratoSemPattern.CriarContrato(
    "Ana Souza", "ana.souza@email.com", "11999990000",
    new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
    1500.00m, "BRL", true);
```

Oito argumentos posicionais do mesmo formato (`string`, `string`) tornam fácil inverter `email` e `telefone` sem que o compilador acuse erro.

### Depois (com o padrão)

```csharp
ContratoParametros parametros = new(
    nomeCliente: "Ana Souza",
    email: "ana.souza@email.com",
    telefone: "11999990000",
    dataInicio: new DateTime(2026, 1, 1),
    dataFim: new DateTime(2026, 12, 31),
    valorMensal: 1500.00m,
    moeda: "BRL",
    renovacaoAutomatica: true);

GeradorContratoComPattern.CriarContrato(parametros);
```

Adicionar um novo campo ao contrato (ex: `ClausulaMulta`) passa a exigir apenas uma alteração em `ContratoParametros`, sem quebrar a assinatura do método `CriarContrato`.

## Referências e documentação complementar

- [Refactoring Guru - Introduce Parameter Object](https://refactoring.guru/introduce-parameter-object)
- [Martin Fowler - Refactoring Catalog](https://refactoring.com/catalog/)
