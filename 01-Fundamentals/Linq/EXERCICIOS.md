# 🎓 Exercícios Práticos de LINQ

<!-- commit: micro tweak 3 -->

## 📝 Como Usar Este Guia

Cada exercício possui:
- 🎯 **Objetivo**: O que você deve aprender
- 📋 **Descrição**: O problema a resolver
- 💡 **Dicas**: Orientações para a solução
- ✅ **Solução**: Resposta comentada (no final)

---

## Nível Básico

### Exercício 1: Filtros Simples
**🎯 Objetivo**: Praticar o operador Where

**📋 Descrição**:
Dada uma lista de números de 1 a 100:
1. Encontre todos os números pares
2. Encontre todos os números maiores que 50
3. Encontre todos os números divisíveis por 3 e 5

**💡 Dicas**:
- Use o operador `%` (módulo) para verificar divisibilidade
- Combine múltiplas condições com `&&`

```csharp
var numeros = Enumerable.Range(1, 100);
// Seu código aqui
```

---

### Exercício 2: Projeção Básica
**🎯 Objetivo**: Usar Select para transformar dados

**📋 Descrição**:
Crie uma lista de nomes e:
1. Converta todos para maiúsculas
2. Extraia apenas a primeira letra de cada nome
3. Crie objetos com o nome e seu comprimento

**💡 Dicas**:
- Use `.ToUpper()` para maiúsculas
- Use `.Substring(0, 1)` ou `[0]` para primeira letra
- Crie objetos anônimos com `new { }`

```csharp
var nomes = new[] { "João", "Maria", "Pedro", "Ana" };
// Seu código aqui
```

---

### Exercício 3: Ordenação
**🎯 Objetivo**: Ordenar dados com OrderBy

**📋 Descrição**:
Dada uma lista de produtos com Nome e Preço:
1. Ordene por preço crescente
2. Ordene por nome alfabeticamente
3. Ordene por preço decrescente e depois por nome

**💡 Dicas**:
- Use `OrderBy` para crescente e `OrderByDescending` para decrescente
- Use `ThenBy` para ordenação secundária

```csharp
var produtos = new[]
{
    new { Nome = "Mouse", Preco = 50m },
    new { Nome = "Teclado", Preco = 150m },
    new { Nome = "Monitor", Preco = 800m },
    new { Nome = "WebCam", Preco = 200m }
};
// Seu código aqui
```

---

## Nível Intermediário

### Exercício 4: Agrupamento
**🎯 Objetivo**: Usar GroupBy para análises

**📋 Descrição**:
Dada uma lista de alunos com suas notas:
1. Agrupe por faixa de nota (0-4: Reprovado, 5-6: Regular, 7-8: Bom, 9-10: Excelente)
2. Calcule a média de notas por grupo
3. Conte quantos alunos existem em cada grupo

**💡 Dicas**:
- Use expressão condicional para classificar
- Use `g.Average()` e `g.Count()` nos grupos

```csharp
var alunos = new[]
{
    new { Nome = "João", Nota = 8.5 },
    new { Nome = "Maria", Nota = 9.2 },
    new { Nome = "Pedro", Nota = 5.5 },
    new { Nome = "Ana", Nota = 7.8 },
    new { Nome = "Carlos", Nota = 4.5 },
    new { Nome = "Julia", Nota = 9.8 }
};
// Seu código aqui
```

---

### Exercício 5: Join
**🎯 Objetivo**: Relacionar duas coleções

**📋 Descrição**:
Você tem duas listas: Pedidos e Clientes.
1. Una as listas para mostrar o nome do cliente em cada pedido
2. Mostre apenas pedidos de clientes Premium
3. Calcule o valor total de pedidos por cliente

**💡 Dicas**:
- Use `Join` para relacionar as coleções
- Use a chave comum (ClienteId)
- Combine com `Where` para filtrar

```csharp
var clientes = new[]
{
    new { Id = 1, Nome = "João", Premium = true },
    new { Id = 2, Nome = "Maria", Premium = false },
    new { Id = 3, Nome = "Pedro", Premium = true }
};

var pedidos = new[]
{
    new { PedidoId = 1, ClienteId = 1, Valor = 100m },
    new { PedidoId = 2, ClienteId = 2, Valor = 50m },
    new { PedidoId = 3, ClienteId = 1, Valor = 200m },
    new { PedidoId = 4, ClienteId = 3, Valor = 150m }
};
// Seu código aqui
```

---

### Exercício 6: Agregações
**🎯 Objetivo**: Calcular estatísticas

**📋 Descrição**:
Dada uma lista de vendas:
1. Calcule o total de vendas
2. Encontre a maior e menor venda
3. Calcule a média de vendas
4. Conte quantas vendas foram acima de R$ 100

**💡 Dicas**:
- Use `Sum`, `Max`, `Min`, `Average`
- Use `Count` com predicado

```csharp
var vendas = new[] { 50m, 120m, 80m, 200m, 45m, 150m, 90m, 110m };
// Seu código aqui
```

---

## Nível Avançado

### Exercício 7: Paginação
**🎯 Objetivo**: Implementar sistema de paginação

**📋 Descrição**:
Crie uma função que:
1. Receba uma lista, número da página e tamanho da página
2. Retorne apenas os itens daquela página
3. Retorne também o total de páginas

**💡 Dicas**:
- Use `Skip` e `Take`
- Calcule total de páginas: `Math.Ceiling(total / (double)tamanho)`

```csharp
// Implemente esta função
public static PaginaResultado<T> Paginar<T>(
    IEnumerable<T> fonte, 
    int numeroPagina, 
    int tamanhoPagina)
{
    // Seu código aqui
}

public class PaginaResultado<T>
{
    public IEnumerable<T> Itens { get; set; }
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalItens { get; set; }
}
```

---

### Exercício 8: Hierarquia
**🎯 Objetivo**: Trabalhar com dados hierárquicos

**📋 Descrição**:
Dada uma lista de funcionários com seus gerentes:
1. Liste todos os subordinados de cada gerente
2. Calcule a folha salarial de cada equipe (gerente + subordinados)
3. Encontre a maior cadeia hierárquica (mais níveis)

**💡 Dicas**:
- Use `GroupBy` no GerenteId
- Use recursão ou loops para percorrer a hierarquia
- Use `GroupJoin` para left joins

```csharp
var funcionarios = new[]
{
    new { Id = 1, Nome = "Carlos", Salario = 10000m, GerenteId = (int?)null },
    new { Id = 2, Nome = "Ana", Salario = 8000m, GerenteId = (int?)1 },
    new { Id = 3, Nome = "João", Salario = 6000m, GerenteId = (int?)2 },
    new { Id = 4, Nome = "Maria", Salario = 6000m, GerenteId = (int?)2 },
    new { Id = 5, Nome = "Pedro", Salario = 8000m, GerenteId = (int?)1 }
};
// Seu código aqui
```

---

### Exercício 9: Performance
**🎯 Objetivo**: Otimizar queries LINQ

**📋 Descrição**:
Compare a performance de:
1. Executar uma query complexa múltiplas vezes
2. Cachear o resultado com ToList()
3. Usar um Lookup para buscas repetidas

Meça o tempo de execução de cada abordagem.

**💡 Dicas**:
- Use `Stopwatch` para medir tempo
- Use `ToLookup()` para índices eficientes
- Teste com grandes volumes de dados

```csharp
var dados = Enumerable.Range(1, 100000)
    .Select(i => new { Id = i, Categoria = i % 10, Valor = i * 1.5 });

// Compare estas abordagens
// Abordagem 1: Query múltiplas vezes
// Abordagem 2: ToList() uma vez
// Abordagem 3: ToLookup() para buscas
```

---

### Exercício 10: Sistema Completo
**🎯 Objetivo**: Integrar múltiplos conceitos

**📋 Descrição**:
Crie um sistema de análise de e-commerce que:
1. Calcule o cliente que mais gastou
2. Liste os produtos mais vendidos
3. Identifique categorias com pior desempenho
4. Sugira produtos para reposição de estoque
5. Gere um relatório mensal de vendas

**💡 Dicas**:
- Combine Join, GroupBy, OrderBy
- Use múltiplas queries
- Organize o código em métodos separados

```csharp
// Dados fornecidos
var clientes = new[] { /* dados */ };
var produtos = new[] { /* dados */ };
var pedidos = new[] { /* dados */ };
var itensPedido = new[] { /* dados */ };

// Implemente os métodos de análise
```

---

## 🎯 Desafios Extras

### Desafio 1: LINQ sem LINQ
Implemente as funções Where, Select e GroupBy manualmente sem usar LINQ.

### Desafio 2: Query Builder
Crie um sistema que gera queries LINQ dinamicamente baseado em parâmetros do usuário.

### Desafio 3: LINQ to Objects vs LINQ to SQL
Compare o comportamento e diferenças entre trabalhar com listas em memória e queries de banco de dados.

---

## ✅ Soluções

<details>
<summary>Clique para ver as soluções (tente fazer sozinho primeiro!)</summary>

### Solução Exercício 1

```csharp
var numeros = Enumerable.Range(1, 100);

// 1. Números pares
var pares = numeros.Where(n => n % 2 == 0);
Console.WriteLine($"Pares: {string.Join(", ", pares.Take(10))}...");

// 2. Maiores que 50
var maioresQue50 = numeros.Where(n => n > 50);
Console.WriteLine($"Total maiores que 50: {maioresQue50.Count()}");

// 3. Divisíveis por 3 e 5
var divisiveis = numeros.Where(n => n % 3 == 0 && n % 5 == 0);
Console.WriteLine($"Divisíveis por 3 e 5: {string.Join(", ", divisiveis)}");
```

### Solução Exercício 2

```csharp
var nomes = new[] { "João", "Maria", "Pedro", "Ana" };

// 1. Maiúsculas
var maiusculas = nomes.Select(n => n.ToUpper());
Console.WriteLine(string.Join(", ", maiusculas));

// 2. Primeira letra
var primeirasLetras = nomes.Select(n => n[0]);
Console.WriteLine(string.Join(", ", primeirasLetras));

// 3. Nome e comprimento
var comComprimento = nomes.Select(n => new { Nome = n, Tamanho = n.Length });
foreach (var item in comComprimento)
{
    Console.WriteLine($"{item.Nome}: {item.Tamanho} letras");
}
```

### Solução Exercício 3

```csharp
var produtos = new[]
{
    new { Nome = "Mouse", Preco = 50m },
    new { Nome = "Teclado", Preco = 150m },
    new { Nome = "Monitor", Preco = 800m },
    new { Nome = "WebCam", Preco = 200m }
};

// 1. Por preço crescente
var porPreco = produtos.OrderBy(p => p.Preco);

// 2. Por nome
var porNome = produtos.OrderBy(p => p.Nome);

// 3. Por preço decrescente e nome
var ordenado = produtos
    .OrderByDescending(p => p.Preco)
    .ThenBy(p => p.Nome);
```

### Solução Exercício 4

```csharp
var alunos = new[]
{
    new { Nome = "João", Nota = 8.5 },
    new { Nome = "Maria", Nota = 9.2 },
    new { Nome = "Pedro", Nota = 5.5 },
    new { Nome = "Ana", Nota = 7.8 },
    new { Nome = "Carlos", Nota = 4.5 },
    new { Nome = "Julia", Nota = 9.8 }
};

var agrupadoPorDesempenho = alunos
    .GroupBy(a => 
        a.Nota < 5 ? "Reprovado" :
        a.Nota < 7 ? "Regular" :
        a.Nota < 9 ? "Bom" : "Excelente")
    .Select(g => new
    {
        Classificacao = g.Key,
        Quantidade = g.Count(),
        MediaGrupo = g.Average(a => a.Nota),
        Alunos = g.Select(a => a.Nome)
    });

foreach (var grupo in agrupadoPorDesempenho)
{
    Console.WriteLine($"\n{grupo.Classificacao}:");
    Console.WriteLine($"  Quantidade: {grupo.Quantidade}");
    Console.WriteLine($"  Média: {grupo.MediaGrupo:N2}");
    Console.WriteLine($"  Alunos: {string.Join(", ", grupo.Alunos)}");
}
```

### Solução Exercício 5

```csharp
var clientes = new[]
{
    new { Id = 1, Nome = "João", Premium = true },
    new { Id = 2, Nome = "Maria", Premium = false },
    new { Id = 3, Nome = "Pedro", Premium = true }
};

var pedidos = new[]
{
    new { PedidoId = 1, ClienteId = 1, Valor = 100m },
    new { PedidoId = 2, ClienteId = 2, Valor = 50m },
    new { PedidoId = 3, ClienteId = 1, Valor = 200m },
    new { PedidoId = 4, ClienteId = 3, Valor = 150m }
};

// 1. Join básico
var pedidosComCliente = pedidos
    .Join(clientes,
        p => p.ClienteId,
        c => c.Id,
        (p, c) => new { p.PedidoId, Cliente = c.Nome, p.Valor });

// 2. Apenas Premium
var pedidosPremium = pedidos
    .Join(clientes,
        p => p.ClienteId,
        c => c.Id,
        (p, c) => new { Pedido = p, Cliente = c })
    .Where(x => x.Cliente.Premium)
    .Select(x => new { x.Pedido.PedidoId, x.Cliente.Nome, x.Pedido.Valor });

// 3. Total por cliente
var totalPorCliente = pedidos
    .Join(clientes,
        p => p.ClienteId,
        c => c.Id,
        (p, c) => new { Cliente = c.Nome, p.Valor })
    .GroupBy(x => x.Cliente)
    .Select(g => new
    {
        Cliente = g.Key,
        TotalPedidos = g.Count(),
        ValorTotal = g.Sum(x => x.Valor)
    });
```

### Solução Exercício 6

```csharp
var vendas = new[] { 50m, 120m, 80m, 200m, 45m, 150m, 90m, 110m };

// 1. Total
var total = vendas.Sum();
Console.WriteLine($"Total de vendas: R$ {total:N2}");

// 2. Maior e menor
var maior = vendas.Max();
var menor = vendas.Min();
Console.WriteLine($"Maior venda: R$ {maior:N2}");
Console.WriteLine($"Menor venda: R$ {menor:N2}");

// 3. Média
var media = vendas.Average();
Console.WriteLine($"Média: R$ {media:N2}");

// 4. Acima de R$ 100
var acimaDe100 = vendas.Count(v => v > 100);
Console.WriteLine($"Vendas acima de R$ 100: {acimaDe100}");
```

### Solução Exercício 7

```csharp
public class PaginaResultado<T>
{
    public IEnumerable<T> Itens { get; set; }
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalItens { get; set; }
    public bool TemPaginaAnterior => PaginaAtual > 1;
    public bool TemProximaPagina => PaginaAtual < TotalPaginas;
}

public static PaginaResultado<T> Paginar<T>(
    IEnumerable<T> fonte,
    int numeroPagina,
    int tamanhoPagina)
{
    var totalItens = fonte.Count();
    var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);
    
    var itens = fonte
        .Skip((numeroPagina - 1) * tamanhoPagina)
        .Take(tamanhoPagina);
    
    return new PaginaResultado<T>
    {
        Itens = itens,
        PaginaAtual = numeroPagina,
        TotalPaginas = totalPaginas,
        TotalItens = totalItens
    };
}

// Uso
var numeros = Enumerable.Range(1, 100);
var pagina2 = Paginar(numeros, 2, 10);

Console.WriteLine($"Página {pagina2.PaginaAtual} de {pagina2.TotalPaginas}");
Console.WriteLine($"Itens: {string.Join(", ", pagina2.Itens)}");
```

</details>

---

## 🎓 Próximos Passos

Após completar estes exercícios:

1. ✅ Revise os conceitos que tiveram dificuldade
2. ✅ Tente otimizar suas soluções
3. ✅ Compare suas soluções com as fornecidas
4. ✅ Crie seus próprios exercícios baseados em problemas reais
5. ✅ Explore LINQ com Entity Framework Core

---

<div align="center">

**💪 Continue praticando! A prática leva à perfeição! 💪**

</div>
