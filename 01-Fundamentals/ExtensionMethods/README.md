# ExtensionMethods

## Visao geral

Exemplo simples de aplicacao de extension methods em C# para estender o tipo `string` sem alterar a implementacao original da classe.

O projeto demonstra como transformar uma chamada de utilitario estatico em uma chamada fluida e legivel no proprio objeto, melhorando a experiencia de uso no dia a dia.

## Conceitos abordados

- Extension methods com classe estatica.
- Uso de `this` no primeiro parametro para "estender" tipos existentes.
- Encapsulamento de logica recorrente de conversao.

## Objetivos de aprendizagem

- Entender a sintaxe necessaria para criar extension methods.
- Aplicar extension methods para deixar chamadas mais expressivas.
- Identificar quando extension methods ajudam e quando podem atrapalhar.

## Estrutura do projeto

```text
ExtensionMethods/
+-- Program.cs
\-- README.md
```

## Como/Pq usar extension methods

### Como usar

1. Crie uma classe estatica para agrupar extensoes.
2. Crie um metodo estatico cujo primeiro parametro use `this` no tipo que sera estendido.
3. Chame o metodo como se ele fosse nativo do tipo.

Exemplo do projeto:

```csharp
string date = "2024-01-01";
DateTime dateTime = date.ToDateTime();

public static class StringExtensions
{
	public static DateTime ToDateTime(this string str)
	{
		return DateTime.Parse(str);
	}
}
```

### Por que usar

- Melhora legibilidade: `date.ToDateTime()` comunica melhor a intencao.
- Aumenta reutilizacao: evita duplicar conversoes em varios pontos.
- Mantem o dominio limpo: centraliza comportamento util sem criar heranca desnecessaria.
- Facilita descoberta no IntelliSense: quem usa `string` encontra a extensao no proprio fluxo de codigo.

### Quando evitar

- Quando a operacao altera estado complexo (prefira servico/classe dedicada).
- Quando o nome do metodo pode causar ambiguidade com APIs nativas.
- Quando regras de erro nao estao claras. No caso de parse, considere variante com `TryParse` para fluxos robustos.

## Como executar

No diretorio do projeto:

```bash
dotnet run .\Program.cs
```

## Boas praticas e pontos de atencao

- Prefira extensoes pequenas, previsiveis e sem efeitos colaterais.
- Use nomes diretos, orientados a intencao de negocio.
- Evite transformar extension methods em "caixa de utilitarios" sem criterio.
- Documente comportamento de erro (ex.: `Parse` pode lancar excecao para entrada invalida).

## Conteudo complementar

##### Evolucao recomendada para este exemplo

1. Criar uma extensao segura com `TryParse`.
2. Aceitar `CultureInfo` como parametro opcional para cenarios internacionais.
3. Adicionar testes para entradas validas e invalidas.

## Referencias

- https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
- https://learn.microsoft.com/dotnet/api/system.datetime.parse