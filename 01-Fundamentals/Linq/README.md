# 🎯 LINQ - Language Integrated Query em C#

<!-- commit: micro tweak 2 -->

## 📖 Sobre o Projeto

Este é um **guia didático completo** sobre LINQ (Language Integrated Query) em C#, uma das funcionalidades mais poderosas da linguagem. O projeto contém **12 demonstrações práticas** que cobrem desde conceitos básicos até operações avançadas, com exemplos do mundo real.

### 🌟 Diferenciais deste Guia

- ✅ **Exemplos Práticos**: Cenários reais de uso em sistemas empresariais
- ✅ **Tom Didático**: Explicações claras e progressivas
- ✅ **Comparações**: Method Syntax vs Query Syntax
- ✅ **Boas Práticas**: Quando usar cada operador
- ✅ **Performance**: Entenda execução adiada vs imediata
- ✅ **Casos de Uso**: Aplicações práticas em dashboards, relatórios e análises

---

## 📚 Conteúdo das Demonstrações

### 1️⃣ Filtros Básicos com WHERE
Aprenda a filtrar dados com condições simples e compostas.
```csharp
// Filtrar produtos em estoque com preço menor que R$ 500
var produtos = lista.Where(p => p.Preco < 500 && p.EmEstoque);
```

**Conceitos**: 
- Predicados e expressões lambda
- Operadores lógicos (&&, ||)
- Method Syntax vs Query Syntax

---

### 2️⃣ Projeção com SELECT
Transforme dados criando novas estruturas e objetos anônimos.
```csharp
// Criar objeto com desconto
var produtosComDesconto = produtos.Select(p => new 
{ 
    p.Nome, 
    PrecoOriginal = p.Preco,
    PrecoComDesconto = p.Preco * 0.9m 
});
```

**Conceitos**:
- Objetos anônimos
- SelectMany para achatar coleções
- Transformações e cálculos

---

### 3️⃣ Ordenação com ORDERBY
Ordene dados de forma simples ou com múltiplos critérios.
```csharp
// Ordenar por categoria e depois por preço
var ordenados = produtos
    .OrderBy(p => p.Categoria)
    .ThenByDescending(p => p.Preco);
```

**Conceitos**:
- OrderBy, OrderByDescending
- ThenBy para ordenação secundária
- Reverse para inverter sequências

---

### 4️⃣ Agrupamento com GROUPBY
Agrupe elementos e calcule estatísticas por grupo.
```csharp
// Estatísticas por categoria
var stats = produtos
    .GroupBy(p => p.Categoria)
    .Select(g => new
    {
        Categoria = g.Key,
        Total = g.Count(),
        PrecoMedio = g.Average(p => p.Preco)
    });
```

**Conceitos**:
- IGrouping<TKey, TElement>
- Agregações por grupo
- Análises estatísticas

---

### 5️⃣ Junção (JOIN) entre Coleções
Combine dados de múltiplas fontes relacionadas.
```csharp
// Join entre pedidos e clientes
var resultado = pedidos
    .Join(clientes,
        pedido => pedido.ClienteId,
        cliente => cliente.Id,
        (pedido, cliente) => new { pedido, cliente });
```

**Conceitos**:
- Inner Join vs Left Join (GroupJoin)
- Joins múltiplos
- Relacionamentos entre entidades

---

### 6️⃣ Operações de Agregação
Calcule valores únicos a partir de coleções.
```csharp
// Estatísticas gerais
var total = produtos.Count();
var soma = produtos.Sum(p => p.Preco);
var media = produtos.Average(p => p.Preco);
var min = produtos.Min(p => p.Preco);
var max = produtos.Max(p => p.Preco);
```

**Conceitos**:
- Count, Sum, Average, Min, Max
- Aggregate personalizado
- LongCount para grandes volumes

---

### 7️⃣ Quantificadores (ANY, ALL, CONTAINS)
Verifique condições booleanas em coleções.
```csharp
// Verificações
bool existe = produtos.Any(p => p.Preco > 1000);
bool todos = produtos.All(p => p.EmEstoque);
bool contem = cidades.Contains("São Paulo");
```

**Conceitos**:
- Any para existência
- All para validação total
- Contains para verificar elemento específico

---

### 8️⃣ Particionamento (TAKE, SKIP, Paginação)
Divida sequências em partes menores.
```csharp
// Paginação
int tamanhoPagina = 10;
int numeroPagina = 2;
var pagina = produtos
    .Skip((numeroPagina - 1) * tamanhoPagina)
    .Take(tamanhoPagina);
```

**Conceitos**:
- Take, Skip, TakeWhile, SkipWhile
- Implementação de paginação
- Chunk para processamento em lotes

---

### 9️⃣ Operações de Conjunto
Trate coleções como conjuntos matemáticos.
```csharp
// Operações de conjunto
var unicos = lista.Distinct();
var uniao = lista1.Union(lista2);
var intersecao = lista1.Intersect(lista2);
var diferenca = lista1.Except(lista2);
```

**Conceitos**:
- Distinct, DistinctBy
- Union, Intersect, Except
- Concat vs Union

---

### 🔟 Execução Adiada (Deferred Execution)
Entenda como e quando as queries LINQ são executadas.
```csharp
// Execução adiada
var query = numeros.Where(n => n > 5);  // NÃO executa
var lista = query.ToList();             // Executa agora
```

**Conceitos**:
- Lazy evaluation
- Operadores imediatos vs adiados
- ToList(), ToArray() para cache
- Impacto em performance

---

### 1️⃣1️⃣ Operações Avançadas
Explore funcionalidades sofisticadas do LINQ.
```csharp
// Operações avançadas
var combinados = lista1.Zip(lista2, (a, b) => $"{a}-{b}");
var sequencia = Enumerable.Range(1, 100);
var repetidos = Enumerable.Repeat("*", 5);
var lookup = produtos.ToLookup(p => p.Categoria);
```

**Conceitos**:
- Zip, Range, Repeat
- ToLookup para indexação
- Cast e OfType
- SequenceEqual

---

### 1️⃣2️⃣ Casos de Uso Reais
Aplicações práticas em cenários empresariais.

**Exemplos incluem**:
- 📊 Relatório de vendas por cliente
- 📈 Dashboard de produtos
- 🎯 Sistema de recomendação
- 👥 Análise de hierarquia organizacional
- 🔔 Auditoria de estoque crítico
- 📅 Análise temporal de pedidos

---

## 🚀 Como Executar

### Pré-requisitos
- .NET 6.0 ou superior
- Visual Studio, VS Code ou Rider

### Executando o Projeto

```bash
# Navegue até o diretório
cd Linq

# Execute o projeto
dotnet run
```

O programa apresentará um menu interativo onde você pode explorar cada demonstração passo a passo.

---

## 🎓 O que Você Aprenderá

### Fundamentos
- ✅ Sintaxe básica do LINQ (Method e Query Syntax)
- ✅ Expressões lambda e delegates
- ✅ Diferença entre IEnumerable e IQueryable
- ✅ Execução adiada vs execução imediata

### Operadores LINQ
- 🔍 **Filtragem**: Where, OfType
- 🔄 **Projeção**: Select, SelectMany
- 📊 **Ordenação**: OrderBy, ThenBy, Reverse
- 📦 **Agrupamento**: GroupBy, ToLookup
- 🔗 **Junção**: Join, GroupJoin
- 🧮 **Agregação**: Count, Sum, Average, Min, Max, Aggregate
- ✔️ **Quantificação**: Any, All, Contains
- ✂️ **Particionamento**: Take, Skip, TakeWhile, SkipWhile, Chunk
- 🎲 **Conjunto**: Distinct, Union, Intersect, Except, Concat
- 🎯 **Elemento**: First, Last, Single, ElementAt
- 📝 **Geração**: Range, Repeat, Empty

### Boas Práticas
- 💡 Quando usar execução adiada vs imediata
- 💡 Como evitar múltiplas execuções de queries caras
- 💡 Quando usar ToList() ou ToArray()
- 💡 Otimização de queries complexas
- 💡 Legibilidade: Method Syntax vs Query Syntax

---

## 📊 Estrutura do Código

```
Linq/
├── Program.cs              # Programa principal com 12 demonstrações
├── Models/                 # Classes de modelo
│   ├── Produto.cs         # Modelo de produto
│   ├── Cliente.cs         # Modelo de cliente
│   ├── Pedido.cs          # Modelo de pedido
│   └── Funcionario.cs     # Modelo de funcionário
├── README.md              # Este arquivo
├── EXERCICIOS.md          # Exercícios práticos
└── Linq.csproj            # Arquivo de projeto
```

### Classes de Modelo Utilizadas

O projeto utiliza classes que simulam um sistema de e-commerce:

```csharp
- Produto      (Id, Nome, Categoria, Preco, EmEstoque, Estoque)
- Cliente      (Id, Nome, Email, Cidade, Premium)
- Pedido       (Id, ClienteId, ProdutoId, Quantidade, DataPedido)
- Funcionario  (Id, Nome, Cargo, Salario, GerenteId)
```

---

## 💡 Dicas e Truques

### 1. Method Syntax vs Query Syntax

**Method Syntax** (mais usado):
```csharp
var resultado = produtos
    .Where(p => p.EmEstoque)
    .OrderBy(p => p.Preco)
    .Select(p => p.Nome);
```

**Query Syntax** (similar ao SQL):
```csharp
var resultado = from p in produtos
                where p.EmEstoque
                orderby p.Preco
                select p.Nome;
```

💡 **Recomendação**: Use Method Syntax na maioria dos casos. É mais flexível e permite encadear operações facilmente.

---

### 2. Evite Múltiplas Iterações

❌ **Ruim** (executa a query 3 vezes):
```csharp
var query = produtos.Where(p => p.Caro);
var count = query.Count();
var primeiro = query.First();
var ultimo = query.Last();
```

✅ **Bom** (executa 1 vez):
```csharp
var lista = produtos.Where(p => p.Caro).ToList();
var count = lista.Count();
var primeiro = lista.First();
var ultimo = lista.Last();
```

---

### 3. Use FirstOrDefault para Evitar Exceções

❌ **Ruim** (lança exceção se não encontrar):
```csharp
var produto = produtos.First(p => p.Id == 999);
```

✅ **Bom** (retorna null se não encontrar):
```csharp
var produto = produtos.FirstOrDefault(p => p.Id == 999);
if (produto != null)
{
    // Usar o produto
}
```

---

### 4. Encadeie Operações para Maior Legibilidade

✅ **Bom**:
```csharp
var resultado = produtos
    .Where(p => p.EmEstoque)           // Filtrar
    .OrderBy(p => p.Categoria)          // Ordenar
    .ThenByDescending(p => p.Preco)     // Ordenação secundária
    .Select(p => new                    // Projetar
    {
        p.Nome,
        PrecoComDesconto = p.Preco * 0.9m
    })
    .Take(10);                          // Limitar
```

---

## 🔗 Recursos Adicionais

### Documentação Oficial
- [LINQ (Language Integrated Query)](https://learn.microsoft.com/pt-br/dotnet/csharp/linq/)
- [101 LINQ Samples](https://learn.microsoft.com/en-us/samples/dotnet/try-samples/101-linq-samples/)
- [Standard Query Operators](https://learn.microsoft.com/pt-br/dotnet/csharp/programming-guide/concepts/linq/standard-query-operators-overview)

### Artigos Recomendados
- [LINQ Performance Tips](https://learn.microsoft.com/en-us/dotnet/standard/linq/performance)
- [Deferred vs Immediate Execution](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/classification-of-standard-query-operators-by-manner-of-execution)

---

## 🤝 Contribuindo

Este é um projeto didático. Sugestões de melhorias são bem-vindas!

---

## 📝 Licença

Este projeto é fornecido como material educacional e pode ser usado livremente para aprendizado.

---

## 👨‍💻 Autor

Desenvolvido como parte do repositório **CSharp-101** para ensino e aprendizado de C#.

---

## 🎯 Próximos Passos

Após dominar este conteúdo, recomendamos explorar:

1. **Entity Framework Core** - LINQ com bancos de dados
2. **LINQ to XML** - Manipulação de XML com LINQ
3. **Parallel LINQ (PLINQ)** - LINQ paralelo para alta performance
4. **Expression Trees** - Entenda como LINQ funciona internamente
5. **IQueryable vs IEnumerable** - Otimização de queries em banco de dados

---

<div align="center">

**⭐ Se este guia foi útil, considere dar uma estrela no repositório! ⭐**

</div>

1. **Operadores de Filtragem**:
   - `Where()`: Filtra elementos baseado em condição
   - `OfType<T>()`: Filtra por tipo específico
   - `Distinct()`: Remove duplicatas

2. **Operadores de Projeção**:
   - `Select()`: Transforma elementos
   - `SelectMany()`: Achatamento de coleções aninhadas

3. **Operadores de Ordenação**:
   - `OrderBy()` / `OrderByDescending()`
   - `ThenBy()` / `ThenByDescending()`

4. **Operadores de Agrupamento**:
   - `GroupBy()`: Agrupa elementos por chave

## 🎨 Operadores LINQ Essenciais

### Filtering (Filtragem)
```csharp
// Where - filtragem condicional
var adults = people.Where(p => p.Age >= 18);

// Distinct - elementos únicos
var uniqueNames = names.Distinct();

// Take/Skip - paginação
var firstFive = items.Take(5);
var skipFirst = items.Skip(10);
```

### Projection (Projeção)
```csharp
// Select - transformação
var names = people.Select(p => p.Name);

// Select com objeto anônimo
var summary = people.Select(p => new { 
    Name = p.Name, 
    IsAdult = p.Age >= 18 
});

// SelectMany - achatar coleções
var allPhones = people.SelectMany(p => p.PhoneNumbers);
```

### Aggregation (Agregação)
```csharp
// Count, Sum, Average, Min, Max
var totalAge = people.Sum(p => p.Age);
var averageAge = people.Average(p => p.Age);
var oldestAge = people.Max(p => p.Age);
var count = people.Count(p => p.Age > 30);
```

### Joining (Junção)
```csharp
var result = from person in people
             join address in addresses on person.Id equals address.PersonId
             select new { person.Name, address.City };
```

### Grouping (Agrupamento)
```csharp
var grouped = people.GroupBy(p => p.City)
                   .Select(g => new { 
                       City = g.Key, 
                       Count = g.Count() 
                   });
```

## 🏗️ Casos de Uso Práticos

### 1. Processamento de Dados
```csharp
var result = orders
    .Where(o => o.Date >= DateTime.Today.AddDays(-30))
    .GroupBy(o => o.CustomerId)
    .Select(g => new {
        CustomerId = g.Key,
        TotalAmount = g.Sum(o => o.Amount),
        OrderCount = g.Count()
    })
    .OrderByDescending(x => x.TotalAmount);
```

### 2. Validação e Filtros
```csharp
var validEmails = contacts
    .Where(c => !string.IsNullOrEmpty(c.Email))
    .Where(c => c.Email.Contains("@"))
    .Select(c => c.Email.ToLower())
    .Distinct();
```

### 3. Relatórios e Estatísticas
```csharp
var stats = sales
    .GroupBy(s => s.Date.Month)
    .Select(g => new {
        Month = g.Key,
        TotalSales = g.Sum(s => s.Amount),
        AverageSale = g.Average(s => s.Amount),
        TopProduct = g.GroupBy(s => s.Product)
                      .OrderByDescending(p => p.Sum(s => s.Amount))
                      .First().Key
    });
```

## 🔍 Pontos de Atenção

### Performance
```csharp
// ❌ Múltiplas enumerações
var data = GetData().Where(x => x.IsValid);
var count = data.Count(); // Primeira enumeração
var list = data.ToList(); // Segunda enumeração

// ✅ Uma única enumeração
var data = GetData().Where(x => x.IsValid).ToList();
var count = data.Count;
```

### Null Handling
```csharp
// ❌ Pode gerar NullReferenceException
var names = people.Select(p => p.Name.ToUpper());

// ✅ Tratamento seguro de null
var names = people
    .Where(p => p.Name != null)
    .Select(p => p.Name.ToUpper());
```

### Memory Usage
```csharp
// Para grandes datasets, considere usar yield return
public static IEnumerable<T> ProcessLargeDataset<T>(IEnumerable<T> source)
{
    foreach (var item in source)
    {
        // Processamento item por item
        yield return ProcessItem(item);
    }
}
```

## 🚀 LINQ Providers

### 1. LINQ to Objects
Para coleções em memória (IEnumerable).

### 2. LINQ to SQL / Entity Framework
Para consultas em banco de dados.

### 3. LINQ to XML
Para manipulação de documentos XML.

### 4. Parallel LINQ (PLINQ)
```csharp
var result = numbers.AsParallel()
                   .Where(n => IsExpensiveOperation(n))
                   .Select(n => ProcessNumber(n));
```

## 📚 Recursos Adicionais

- [LINQ (Language-Integrated Query)](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)
- [101 LINQ Samples](https://docs.microsoft.com/en-us/samples/dotnet/try-samples/101-linq-samples/)
- [LINQ Performance Tips](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/ef/language-reference/query-execution)
- [Artigo de referência](https://medium.com/@ravipatel.it/introduction-to-linq-in-c-26bf70607d14)