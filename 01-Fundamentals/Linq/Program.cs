using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqDemo;

// commit: micro tweak 1

/// <summary>
/// Programa demonstrativo completo de LINQ (Language Integrated Query)
/// Este projeto explora desde conceitos básicos até operações avançadas
/// </summary>
class Program
{
    static void Main()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         LINQ - Language Integrated Query Demonstrations       ║");
        Console.WriteLine("║              Guia Completo e Didático com C#                  ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

        // Prepara dados de exemplo
        PrepararDadosExemplo();

        // Executa demonstrações
        Demonstracao01_FiltrosBasicos();
        Demonstracao02_Projecao();
        Demonstracao03_Ordenacao();
        Demonstracao04_Agrupamento();
        Demonstracao05_Juncao();
        Demonstracao06_Agregacao();
        Demonstracao07_Quantificadores();
        Demonstracao08_Particionamento();
        Demonstracao09_OperacoesConjunto();
        Demonstracao10_ExecucaoAdiada();
        Demonstracao11_OperacoesAvancadas();
        Demonstracao12_CasosReais();

        Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                     Fim das Demonstrações                     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine("\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }

    #region Dados de Exemplo

    static List<Produto> produtos = null!;
    static List<Cliente> clientes = null!;
    static List<Pedido> pedidos = null!;
    static List<Funcionario> funcionarios = null!;

    static void PrepararDadosExemplo()
    {
        // Lista de produtos
        produtos = new List<Produto>
        {
            new Produto { Id = 1, Nome = "Notebook Dell", Categoria = "Eletrônicos", Preco = 3500.00m, EmEstoque = true, Estoque = 15 },
            new Produto { Id = 2, Nome = "Mouse Logitech", Categoria = "Periféricos", Preco = 120.00m, EmEstoque = true, Estoque = 50 },
            new Produto { Id = 3, Nome = "Teclado Mecânico", Categoria = "Periféricos", Preco = 450.00m, EmEstoque = true, Estoque = 30 },
            new Produto { Id = 4, Nome = "Monitor LG 27\"", Categoria = "Eletrônicos", Preco = 1200.00m, EmEstoque = true, Estoque = 10 },
            new Produto { Id = 5, Nome = "Cadeira Gamer", Categoria = "Móveis", Preco = 800.00m, EmEstoque = false, Estoque = 0 },
            new Produto { Id = 6, Nome = "Mesa de Escritório", Categoria = "Móveis", Preco = 600.00m, EmEstoque = true, Estoque = 5 },
            new Produto { Id = 7, Nome = "Webcam HD", Categoria = "Periféricos", Preco = 250.00m, EmEstoque = true, Estoque = 20 },
            new Produto { Id = 8, Nome = "Headset Gamer", Categoria = "Periféricos", Preco = 350.00m, EmEstoque = true, Estoque = 25 },
            new Produto { Id = 9, Nome = "SSD 1TB", Categoria = "Armazenamento", Preco = 500.00m, EmEstoque = true, Estoque = 40 },
            new Produto { Id = 10, Nome = "HD Externo 2TB", Categoria = "Armazenamento", Preco = 400.00m, EmEstoque = false, Estoque = 0 }
        };

        // Lista de clientes
        clientes = new List<Cliente>
        {
            new Cliente { Id = 1, Nome = "João Silva", Email = "joao@email.com", Cidade = "São Paulo", Premium = true },
            new Cliente { Id = 2, Nome = "Maria Santos", Email = "maria@email.com", Cidade = "Rio de Janeiro", Premium = false },
            new Cliente { Id = 3, Nome = "Pedro Oliveira", Email = "pedro@email.com", Cidade = "São Paulo", Premium = true },
            new Cliente { Id = 4, Nome = "Ana Costa", Email = "ana@email.com", Cidade = "Belo Horizonte", Premium = false },
            new Cliente { Id = 5, Nome = "Carlos Souza", Email = "carlos@email.com", Cidade = "São Paulo", Premium = true }
        };

        // Lista de pedidos
        pedidos = new List<Pedido>
        {
            new Pedido { Id = 1, ClienteId = 1, ProdutoId = 1, Quantidade = 1, DataPedido = new DateTime(2024, 1, 15) },
            new Pedido { Id = 2, ClienteId = 1, ProdutoId = 2, Quantidade = 2, DataPedido = new DateTime(2024, 1, 16) },
            new Pedido { Id = 3, ClienteId = 2, ProdutoId = 3, Quantidade = 1, DataPedido = new DateTime(2024, 2, 10) },
            new Pedido { Id = 4, ClienteId = 3, ProdutoId = 4, Quantidade = 1, DataPedido = new DateTime(2024, 2, 20) },
            new Pedido { Id = 5, ClienteId = 2, ProdutoId = 7, Quantidade = 3, DataPedido = new DateTime(2024, 3, 5) },
            new Pedido { Id = 6, ClienteId = 4, ProdutoId = 9, Quantidade = 2, DataPedido = new DateTime(2024, 3, 12) },
            new Pedido { Id = 7, ClienteId = 5, ProdutoId = 8, Quantidade = 1, DataPedido = new DateTime(2024, 4, 1) },
            new Pedido { Id = 8, ClienteId = 3, ProdutoId = 6, Quantidade = 1, DataPedido = new DateTime(2024, 4, 15) }
        };

        // Lista de funcionários (para demonstrar hierarquia)
        funcionarios = new List<Funcionario>
        {
            new Funcionario { Id = 1, Nome = "Roberto Alves", Cargo = "CEO", Salario = 15000.00m, GerenteId = null },
            new Funcionario { Id = 2, Nome = "Fernanda Lima", Cargo = "Gerente TI", Salario = 10000.00m, GerenteId = 1 },
            new Funcionario { Id = 3, Nome = "Lucas Martins", Cargo = "Desenvolvedor", Salario = 6000.00m, GerenteId = 2 },
            new Funcionario { Id = 4, Nome = "Juliana Rocha", Cargo = "Desenvolvedor", Salario = 6500.00m, GerenteId = 2 },
            new Funcionario { Id = 5, Nome = "Ricardo Pinto", Cargo = "Gerente Vendas", Salario = 9000.00m, GerenteId = 1 },
            new Funcionario { Id = 6, Nome = "Patrícia Dias", Cargo = "Vendedor", Salario = 4000.00m, GerenteId = 5 }
        };
    }

    #endregion

    #region Demonstração 01: Filtros Básicos

    static void Demonstracao01_FiltrosBasicos()
    {
        ImprimirTitulo("01. FILTROS BÁSICOS COM WHERE");

        Console.WriteLine("📌 Conceito: O operador Where filtra elementos baseado em uma condição (predicado).");
        Console.WriteLine("   Retorna apenas os elementos que satisfazem a condição especificada.\n");

        // Exemplo 1: Números pares
        Console.WriteLine("─── Exemplo 1: Filtrar números pares ───");
        List<int> numeros = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        // Method Syntax
        var numerosPares = numeros.Where(n => n % 2 == 0);
        Console.WriteLine("Method Syntax: numeros.Where(n => n % 2 == 0)");
        Console.WriteLine($"Resultado: {string.Join(", ", numerosPares)}");
        
        // Query Syntax
        var numerosParesQuery = from n in numeros
                                where n % 2 == 0
                                select n;
        Console.WriteLine("\nQuery Syntax: from n in numeros where n % 2 == 0 select n");
        Console.WriteLine($"Resultado: {string.Join(", ", numerosParesQuery)}\n");

        // Exemplo 2: Produtos em estoque
        Console.WriteLine("─── Exemplo 2: Produtos disponíveis em estoque ───");
        var produtosDisponiveis = produtos.Where(p => p.EmEstoque);
        Console.WriteLine($"Total de produtos em estoque: {produtosDisponiveis.Count()}");
        foreach (var produto in produtosDisponiveis.Take(3))
        {
            Console.WriteLine($"  • {produto.Nome} - R$ {produto.Preco:N2} (Estoque: {produto.Estoque})");
        }
        Console.WriteLine($"  ... e mais {produtosDisponiveis.Count() - 3} produtos\n");

        // Exemplo 3: Filtros compostos
        Console.WriteLine("─── Exemplo 3: Filtros compostos (múltiplas condições) ───");
        var produtosBaratosEmEstoque = produtos
            .Where(p => p.Preco < 500 && p.EmEstoque && p.Estoque > 10);
        
        Console.WriteLine("Produtos com preço < R$ 500, em estoque e com mais de 10 unidades:");
        foreach (var produto in produtosBaratosEmEstoque)
        {
            Console.WriteLine($"  • {produto.Nome} - R$ {produto.Preco:N2} ({produto.Estoque} unidades)");
        }

        Console.WriteLine("\n💡 Dica: Use && para AND lógico e || para OR lógico nas condições.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 02: Projeção

    static void Demonstracao02_Projecao()
    {
        ImprimirTitulo("02. PROJEÇÃO COM SELECT");

        Console.WriteLine("📌 Conceito: Select transforma cada elemento da sequência em uma nova forma.");
        Console.WriteLine("   Você pode criar objetos anônimos, extrair propriedades ou fazer cálculos.\n");

        // Exemplo 1: Projeção simples
        Console.WriteLine("─── Exemplo 1: Extrair apenas nomes dos produtos ───");
        var nomesProdutos = produtos.Select(p => p.Nome);
        Console.WriteLine($"Primeiros 5 produtos: {string.Join(", ", nomesProdutos.Take(5))}\n");

        // Exemplo 2: Transformação com cálculo
        Console.WriteLine("─── Exemplo 2: Calcular preço com 10% de desconto ───");
        var produtosComDesconto = produtos
            .Select(p => new 
            { 
                p.Nome, 
                PrecoOriginal = p.Preco, 
                PrecoComDesconto = p.Preco * 0.9m 
            });
        
        foreach (var produto in produtosComDesconto.Take(4))
        {
            Console.WriteLine($"  • {produto.Nome}");
            Console.WriteLine($"    De: R$ {produto.PrecoOriginal:N2} → Por: R$ {produto.PrecoComDesconto:N2}");
        }

        // Exemplo 3: Objeto anônimo complexo
        Console.WriteLine("\n─── Exemplo 3: Criar objeto com múltiplas propriedades ───");
        var resumoProdutos = produtos
            .Where(p => p.EmEstoque)
            .Select(p => new
            {
                Produto = p.Nome,
                Categoria = p.Categoria,
                Preco = p.Preco,
                ValorEstoque = p.Preco * p.Estoque,
                Classificacao = p.Preco > 1000 ? "Premium" : p.Preco > 500 ? "Médio" : "Econômico"
            });

        Console.WriteLine("Produtos com classificação:");
        foreach (var item in resumoProdutos.Take(5))
        {
            Console.WriteLine($"  • {item.Produto} ({item.Classificacao})");
            Console.WriteLine($"    Valor total em estoque: R$ {item.ValorEstoque:N2}");
        }

        // Exemplo 4: SelectMany (achatamento)
        Console.WriteLine("\n─── Exemplo 4: SelectMany - achatar coleções aninhadas ───");
        var categorias = new[]
        {
            new { Nome = "Tech", Tags = new[] { "eletrônicos", "inovação", "digital" } },
            new { Nome = "Casa", Tags = new[] { "móveis", "decoração", "conforto" } }
        };

        var todasTags = categorias.SelectMany(c => c.Tags);
        Console.WriteLine($"Todas as tags: {string.Join(", ", todasTags)}");

        Console.WriteLine("\n💡 Dica: Use Select para transformar e SelectMany para achatar coleções aninhadas.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 03: Ordenação

    static void Demonstracao03_Ordenacao()
    {
        ImprimirTitulo("03. ORDENAÇÃO COM ORDERBY E THENBY");

        Console.WriteLine("📌 Conceito: OrderBy ordena elementos em ordem crescente, OrderByDescending em decrescente.");
        Console.WriteLine("   ThenBy e ThenByDescending permitem ordenação secundária.\n");

        // Exemplo 1: Ordenação simples
        Console.WriteLine("─── Exemplo 1: Produtos ordenados por preço (crescente) ───");
        var produtosPorPreco = produtos.OrderBy(p => p.Preco);
        foreach (var produto in produtosPorPreco.Take(5))
        {
            Console.WriteLine($"  • R$ {produto.Preco,8:N2} - {produto.Nome}");
        }

        // Exemplo 2: Ordenação decrescente
        Console.WriteLine("\n─── Exemplo 2: Produtos mais caros (decrescente) ───");
        var produtosMaisCaros = produtos.OrderByDescending(p => p.Preco);
        foreach (var produto in produtosMaisCaros.Take(5))
        {
            Console.WriteLine($"  • R$ {produto.Preco,8:N2} - {produto.Nome}");
        }

        // Exemplo 3: Ordenação múltipla
        Console.WriteLine("\n─── Exemplo 3: Ordenar por categoria e depois por preço ───");
        var produtosOrdenados = produtos
            .OrderBy(p => p.Categoria)
            .ThenByDescending(p => p.Preco);

        foreach (var produto in produtosOrdenados)
        {
            Console.WriteLine($"  • [{produto.Categoria,-15}] {produto.Nome,-25} R$ {produto.Preco:N2}");
        }

        // Exemplo 4: Reverse
        Console.WriteLine("\n─── Exemplo 4: Inverter ordem ───");
        var numeros = Enumerable.Range(1, 5);
        Console.WriteLine($"Original: {string.Join(", ", numeros)}");
        Console.WriteLine($"Invertido: {string.Join(", ", numeros.Reverse())}");

        Console.WriteLine("\n💡 Dica: OrderBy retorna IOrderedEnumerable, permitindo usar ThenBy para ordenação secundária.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 04: Agrupamento

    static void Demonstracao04_Agrupamento()
    {
        ImprimirTitulo("04. AGRUPAMENTO COM GROUPBY");

        Console.WriteLine("📌 Conceito: GroupBy agrupa elementos que compartilham uma chave comum.");
        Console.WriteLine("   Retorna uma coleção de grupos (IGrouping<TKey, TElement>).\n");

        // Exemplo 1: Agrupar por categoria
        Console.WriteLine("─── Exemplo 1: Produtos agrupados por categoria ───");
        var produtosPorCategoria = produtos.GroupBy(p => p.Categoria);
        
        foreach (var grupo in produtosPorCategoria)
        {
            Console.WriteLine($"\n📦 {grupo.Key} ({grupo.Count()} produtos):");
            foreach (var produto in grupo)
            {
                Console.WriteLine($"  • {produto.Nome} - R$ {produto.Preco:N2}");
            }
        }

        // Exemplo 2: Agrupar com projeção
        Console.WriteLine("\n─── Exemplo 2: Estatísticas por categoria ───");
        var estatisticasPorCategoria = produtos
            .GroupBy(p => p.Categoria)
            .Select(g => new
            {
                Categoria = g.Key,
                TotalProdutos = g.Count(),
                PrecoMedio = g.Average(p => p.Preco),
                PrecoMinimo = g.Min(p => p.Preco),
                PrecoMaximo = g.Max(p => p.Preco),
                ValorTotalEstoque = g.Sum(p => p.Preco * p.Estoque)
            });

        foreach (var stat in estatisticasPorCategoria)
        {
            Console.WriteLine($"\n📊 {stat.Categoria}:");
            Console.WriteLine($"  Total de produtos: {stat.TotalProdutos}");
            Console.WriteLine($"  Preço médio: R$ {stat.PrecoMedio:N2}");
            Console.WriteLine($"  Faixa de preço: R$ {stat.PrecoMinimo:N2} - R$ {stat.PrecoMaximo:N2}");
            Console.WriteLine($"  Valor total em estoque: R$ {stat.ValorTotalEstoque:N2}");
        }

        // Exemplo 3: Query Syntax
        Console.WriteLine("\n─── Exemplo 3: Agrupamento com Query Syntax ───");
        var clientesPorCidade = from c in clientes
                                group c by c.Cidade into g
                                select new { Cidade = g.Key, Total = g.Count() };

        foreach (var item in clientesPorCidade)
        {
            Console.WriteLine($"  • {item.Cidade}: {item.Total} cliente(s)");
        }

        Console.WriteLine("\n💡 Dica: Use GroupBy para análises agregadas e estatísticas por categoria.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 05: Junção

    static void Demonstracao05_Juncao()
    {
        ImprimirTitulo("05. JUNÇÃO (JOIN) ENTRE COLEÇÕES");

        Console.WriteLine("📌 Conceito: Join combina elementos de duas coleções baseado em uma chave comum.");
        Console.WriteLine("   Similar ao JOIN do SQL, permite relacionar dados de diferentes fontes.\n");

        // Exemplo 1: Join simples
        Console.WriteLine("─── Exemplo 1: Join entre Pedidos e Clientes ───");
        var pedidosComClientes = pedidos
            .Join(clientes,
                pedido => pedido.ClienteId,
                cliente => cliente.Id,
                (pedido, cliente) => new
                {
                    NumeroPedido = pedido.Id,
                    Cliente = cliente.Nome,
                    DataPedido = pedido.DataPedido.ToString("dd/MM/yyyy")
                });

        foreach (var item in pedidosComClientes.Take(5))
        {
            Console.WriteLine($"  • Pedido #{item.NumeroPedido} - {item.Cliente} em {item.DataPedido}");
        }

        // Exemplo 2: Join múltiplo
        Console.WriteLine("\n─── Exemplo 2: Join entre Pedidos, Clientes e Produtos ───");
        var pedidosCompletos = pedidos
            .Join(clientes, p => p.ClienteId, c => c.Id, (p, c) => new { Pedido = p, Cliente = c })
            .Join(produtos, pc => pc.Pedido.ProdutoId, prod => prod.Id, 
                (pc, prod) => new
                {
                    NumeroPedido = pc.Pedido.Id,
                    Cliente = pc.Cliente.Nome,
                    Produto = prod.Nome,
                    Quantidade = pc.Pedido.Quantidade,
                    ValorUnitario = prod.Preco,
                    ValorTotal = pc.Pedido.Quantidade * prod.Preco
                });

        Console.WriteLine("\nPedidos completos:");
        foreach (var item in pedidosCompletos.Take(4))
        {
            Console.WriteLine($"  • Pedido #{item.NumeroPedido}");
            Console.WriteLine($"    Cliente: {item.Cliente}");
            Console.WriteLine($"    Produto: {item.Produto} x{item.Quantidade}");
            Console.WriteLine($"    Valor: R$ {item.ValorTotal:N2}");
            Console.WriteLine();
        }

        // Exemplo 3: Query Syntax Join
        Console.WriteLine("─── Exemplo 3: Join com Query Syntax ───");
        var pedidosQuery = from pedido in pedidos
                          join cliente in clientes on pedido.ClienteId equals cliente.Id
                          join produto in produtos on pedido.ProdutoId equals produto.Id
                          where cliente.Premium
                          select new
                          {
                              Cliente = cliente.Nome,
                              Produto = produto.Nome,
                              Total = pedido.Quantidade * produto.Preco
                          };

        Console.WriteLine("Pedidos de clientes Premium:");
        foreach (var item in pedidosQuery)
        {
            Console.WriteLine($"  • {item.Cliente} comprou {item.Produto} - R$ {item.Total:N2}");
        }

        // Exemplo 4: GroupJoin
        Console.WriteLine("\n─── Exemplo 4: GroupJoin - cliente com seus pedidos ───");
        var clientesComPedidos = clientes
            .GroupJoin(pedidos,
                cliente => cliente.Id,
                pedido => pedido.ClienteId,
                (cliente, pedidosCliente) => new
                {
                    Cliente = cliente.Nome,
                    TotalPedidos = pedidosCliente.Count(),
                    Pedidos = pedidosCliente.Select(p => p.Id)
                });

        foreach (var item in clientesComPedidos)
        {
            Console.WriteLine($"  • {item.Cliente}: {item.TotalPedidos} pedido(s)");
        }

        Console.WriteLine("\n💡 Dica: Use Join para inner join e GroupJoin para left join.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 06: Agregação

    static void Demonstracao06_Agregacao()
    {
        ImprimirTitulo("06. OPERAÇÕES DE AGREGAÇÃO");

        Console.WriteLine("📌 Conceito: Operações de agregação calculam um único valor a partir de uma coleção.");
        Console.WriteLine("   Incluem Count, Sum, Average, Min, Max e Aggregate.\n");

        // Exemplo 1: Operações básicas
        Console.WriteLine("─── Exemplo 1: Estatísticas gerais dos produtos ───");
        Console.WriteLine($"  Total de produtos: {produtos.Count()}");
        Console.WriteLine($"  Produtos em estoque: {produtos.Count(p => p.EmEstoque)}");
        Console.WriteLine($"  Soma total dos preços: R$ {produtos.Sum(p => p.Preco):N2}");
        Console.WriteLine($"  Preço médio: R$ {produtos.Average(p => p.Preco):N2}");
        Console.WriteLine($"  Produto mais barato: R$ {produtos.Min(p => p.Preco):N2}");
        Console.WriteLine($"  Produto mais caro: R$ {produtos.Max(p => p.Preco):N2}");

        // Exemplo 2: Aggregate personalizado
        Console.WriteLine("\n─── Exemplo 2: Aggregate - concatenar nomes ───");
        var nomesConcatenados = produtos
            .Take(4)
            .Select(p => p.Nome)
            .Aggregate((atual, proximo) => atual + " | " + proximo);
        Console.WriteLine($"  Produtos: {nomesConcatenados}");

        // Exemplo 3: Aggregate com seed
        Console.WriteLine("\n─── Exemplo 3: Aggregate com valor inicial ───");
        var valorTotalEstoque = produtos
            .Where(p => p.EmEstoque)
            .Aggregate(0m, (total, produto) => total + (produto.Preco * produto.Estoque));
        Console.WriteLine($"  Valor total em estoque: R$ {valorTotalEstoque:N2}");

        // Exemplo 4: Estatísticas por categoria
        Console.WriteLine("\n─── Exemplo 4: Agregação por categoria ───");
        var estatisticasPorCategoria = produtos
            .GroupBy(p => p.Categoria)
            .Select(g => new
            {
                Categoria = g.Key,
                Quantidade = g.Count(),
                ValorMedio = g.Average(p => p.Preco),
                EstoqueTotal = g.Sum(p => p.Estoque)
            })
            .OrderByDescending(x => x.ValorMedio);

        foreach (var stat in estatisticasPorCategoria)
        {
            Console.WriteLine($"\n  📊 {stat.Categoria}:");
            Console.WriteLine($"     Produtos: {stat.Quantidade}");
            Console.WriteLine($"     Preço médio: R$ {stat.ValorMedio:N2}");
            Console.WriteLine($"     Estoque total: {stat.EstoqueTotal} unidades");
        }

        // Exemplo 5: LongCount para grandes coleções
        Console.WriteLine("\n─── Exemplo 5: LongCount para grandes volumes ───");
        long totalRegistros = produtos.LongCount();
        Console.WriteLine($"  Total de registros (long): {totalRegistros}");

        Console.WriteLine("\n💡 Dica: Use Aggregate para operações customizadas e complexas.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 07: Quantificadores

    static void Demonstracao07_Quantificadores()
    {
        ImprimirTitulo("07. QUANTIFICADORES (ANY, ALL, CONTAINS)");

        Console.WriteLine("📌 Conceito: Quantificadores verificam se elementos atendem a certas condições.");
        Console.WriteLine("   Retornam valores booleanos (true/false).\n");

        // Exemplo 1: Any - verifica se existe pelo menos um
        Console.WriteLine("─── Exemplo 1: Any - verificar existência ───");
        bool existeProdutoCaro = produtos.Any(p => p.Preco > 3000);
        Console.WriteLine($"  Existe produto acima de R$ 3000? {(existeProdutoCaro ? "✓ Sim" : "✗ Não")}");
        
        bool existeProdutoSemEstoque = produtos.Any(p => !p.EmEstoque);
        Console.WriteLine($"  Existe produto sem estoque? {(existeProdutoSemEstoque ? "✓ Sim" : "✗ Não")}");

        bool existeCategoriaMoveis = produtos.Any(p => p.Categoria == "Móveis");
        Console.WriteLine($"  Existe produto na categoria Móveis? {(existeCategoriaMoveis ? "✓ Sim" : "✗ Não")}");

        // Exemplo 2: All - verifica se todos atendem
        Console.WriteLine("\n─── Exemplo 2: All - verificar se todos atendem ───");
        bool todosEmEstoque = produtos.All(p => p.EmEstoque);
        Console.WriteLine($"  Todos os produtos estão em estoque? {(todosEmEstoque ? "✓ Sim" : "✗ Não")}");
        
        bool todosAcimaDeReal = produtos.All(p => p.Preco > 100);
        Console.WriteLine($"  Todos os produtos custam mais de R$ 100? {(todosAcimaDeReal ? "✓ Sim" : "✗ Não")}");

        var perifericos = produtos.Where(p => p.Categoria == "Periféricos");
        bool todosPerifericosBaratos = perifericos.All(p => p.Preco < 500);
        Console.WriteLine($"  Todos os periféricos custam menos de R$ 500? {(todosPerifericosBaratos ? "✓ Sim" : "✗ Não")}");

        // Exemplo 3: Contains - verifica se contém elemento
        Console.WriteLine("\n─── Exemplo 3: Contains - verificar se contém ───");
        var cidadesDisponiveis = clientes.Select(c => c.Cidade).Distinct();
        bool atendeRio = cidadesDisponiveis.Contains("Rio de Janeiro");
        bool atendeBrasilia = cidadesDisponiveis.Contains("Brasília");
        
        Console.WriteLine($"  Atende Rio de Janeiro? {(atendeRio ? "✓ Sim" : "✗ Não")}");
        Console.WriteLine($"  Atende Brasília? {(atendeBrasilia ? "✓ Sim" : "✗ Não")}");

        // Exemplo 4: Combinação de quantificadores
        Console.WriteLine("\n─── Exemplo 4: Uso prático combinado ───");
        
        // Validar se cliente fez algum pedido
        foreach (var cliente in clientes.Take(3))
        {
            bool fezPedido = pedidos.Any(p => p.ClienteId == cliente.Id);
            Console.WriteLine($"  • {cliente.Nome}: {(fezPedido ? "Cliente ativo" : "Cliente inativo")}");
        }

        // Validar categorias
        Console.WriteLine("\n  Validações de negócio:");
        bool todasCategoriasTemEstoque = produtos
            .GroupBy(p => p.Categoria)
            .All(g => g.Any(p => p.EmEstoque));
        Console.WriteLine($"  ✓ Todas categorias têm pelo menos um produto em estoque? {(todasCategoriasTemEstoque ? "Sim" : "Não")}");

        Console.WriteLine("\n💡 Dica: Use Any para validações de existência e All para regras de negócio.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 08: Particionamento

    static void Demonstracao08_Particionamento()
    {
        ImprimirTitulo("08. PARTICIONAMENTO (TAKE, SKIP, PAGINAÇÃO)");

        Console.WriteLine("📌 Conceito: Particionamento divide uma sequência em partes menores.");
        Console.WriteLine("   Útil para paginação, limitar resultados e processamento por lotes.\n");

        // Exemplo 1: Take e Skip
        Console.WriteLine("─── Exemplo 1: Take - pegar os primeiros N elementos ───");
        var primeiros3Produtos = produtos.Take(3);
        Console.WriteLine("Primeiros 3 produtos:");
        foreach (var p in primeiros3Produtos)
        {
            Console.WriteLine($"  • {p.Nome}");
        }

        Console.WriteLine("\n─── Exemplo 2: Skip - pular os primeiros N elementos ───");
        var depoisDos5Primeiros = produtos.Skip(5).Take(3);
        Console.WriteLine("3 produtos após pular os 5 primeiros:");
        foreach (var p in depoisDos5Primeiros)
        {
            Console.WriteLine($"  • {p.Nome}");
        }

        // Exemplo 3: Paginação
        Console.WriteLine("\n─── Exemplo 3: Implementar paginação ───");
        int tamanhoPagina = 3;
        int numeroPagina = 2; // Segunda página
        
        var paginaProdutos = produtos
            .OrderBy(p => p.Nome)
            .Skip((numeroPagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina);

        Console.WriteLine($"Página {numeroPagina} (tamanho: {tamanhoPagina}):");
        foreach (var p in paginaProdutos)
        {
            Console.WriteLine($"  • {p.Nome} - R$ {p.Preco:N2}");
        }

        // Exemplo 4: TakeWhile e SkipWhile
        Console.WriteLine("\n─── Exemplo 4: TakeWhile - enquanto condição for verdadeira ───");
        var numerosOrdenados = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var ateMenorQue6 = numerosOrdenados.TakeWhile(n => n < 6);
        Console.WriteLine($"  Pegar enquanto < 6: {string.Join(", ", ateMenorQue6)}");

        Console.WriteLine("\n─── Exemplo 5: SkipWhile - pular enquanto condição for verdadeira ───");
        var depoisDe5 = numerosOrdenados.SkipWhile(n => n <= 5);
        Console.WriteLine($"  Pular enquanto <= 5: {string.Join(", ", depoisDe5)}");

        // Exemplo 6: Chunk (disponível em .NET 6+)
        Console.WriteLine("\n─── Exemplo 6: Chunk - dividir em grupos de tamanho fixo ───");
        var produtosEmLotes = produtos.Chunk(4);
        int lote = 1;
        foreach (var grupo in produtosEmLotes.Take(2))
        {
            Console.WriteLine($"\n  Lote {lote++} ({grupo.Length} produtos):");
            foreach (var p in grupo)
            {
                Console.WriteLine($"    • {p.Nome}");
            }
        }

        // Exemplo 7: Implementação de paginação completa
        Console.WriteLine("\n─── Exemplo 7: Sistema de paginação completo ───");
        var totalProdutos = produtos.Count();
        var totalPaginas = (int)Math.Ceiling(totalProdutos / (double)tamanhoPagina);
        
        Console.WriteLine($"  Total de produtos: {totalProdutos}");
        Console.WriteLine($"  Produtos por página: {tamanhoPagina}");
        Console.WriteLine($"  Total de páginas: {totalPaginas}");

        Console.WriteLine("\n💡 Dica: Use Take/Skip para paginação eficiente em APIs e interfaces.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 09: Operações de Conjunto

    static void Demonstracao09_OperacoesConjunto()
    {
        ImprimirTitulo("09. OPERAÇÕES DE CONJUNTO");

        Console.WriteLine("📌 Conceito: Operações de conjunto tratam coleções como conjuntos matemáticos.");
        Console.WriteLine("   Incluem Distinct, Union, Intersect, Except.\n");

        // Exemplo 1: Distinct - remover duplicatas
        Console.WriteLine("─── Exemplo 1: Distinct - valores únicos ───");
        var categorias = produtos.Select(p => p.Categoria).Distinct();
        Console.WriteLine($"  Categorias únicas: {string.Join(", ", categorias)}");

        var cidades = clientes.Select(c => c.Cidade).Distinct().OrderBy(c => c);
        Console.WriteLine($"  Cidades atendidas: {string.Join(", ", cidades)}");

        // Exemplo 2: Union - união de conjuntos
        Console.WriteLine("\n─── Exemplo 2: Union - combinar coleções sem duplicatas ───");
        var clientesSP = clientes.Where(c => c.Cidade == "São Paulo").Select(c => c.Nome);
        var clientesPremium = clientes.Where(c => c.Premium).Select(c => c.Nome);
        var clientesEspeciais = clientesSP.Union(clientesPremium);
        
        Console.WriteLine("  Clientes de SP ou Premium (sem duplicatas):");
        foreach (var nome in clientesEspeciais)
        {
            Console.WriteLine($"    • {nome}");
        }

        // Exemplo 3: Intersect - interseção
        Console.WriteLine("\n─── Exemplo 3: Intersect - elementos comuns ───");
        var clientesSPePremium = clientesSP.Intersect(clientesPremium);
        Console.WriteLine("  Clientes que são de SP E Premium:");
        foreach (var nome in clientesSPePremium)
        {
            Console.WriteLine($"    • {nome}");
        }

        // Exemplo 4: Except - diferença
        Console.WriteLine("\n─── Exemplo 4: Except - elementos exclusivos ───");
        var apenasNaoPremium = clientes.Select(c => c.Nome)
            .Except(clientesPremium);
        Console.WriteLine("  Clientes não premium:");
        foreach (var nome in apenasNaoPremium)
        {
            Console.WriteLine($"    • {nome}");
        }

        // Exemplo 5: Concat vs Union
        Console.WriteLine("\n─── Exemplo 5: Diferença entre Concat e Union ───");
        var lista1 = new[] { 1, 2, 3, 4 };
        var lista2 = new[] { 3, 4, 5, 6 };
        
        var comConcat = lista1.Concat(lista2);
        var comUnion = lista1.Union(lista2);
        
        Console.WriteLine($"  Concat (permite duplicatas): {string.Join(", ", comConcat)}");
        Console.WriteLine($"  Union (sem duplicatas): {string.Join(", ", comUnion)}");

        // Exemplo 6: DistinctBy (disponível em .NET 6+)
        Console.WriteLine("\n─── Exemplo 6: DistinctBy - distintos por propriedade ───");
        var primeirosPorCategoria = produtos.DistinctBy(p => p.Categoria);
        Console.WriteLine("  Primeiro produto de cada categoria:");
        foreach (var p in primeirosPorCategoria)
        {
            Console.WriteLine($"    • [{p.Categoria}] {p.Nome}");
        }

        // Exemplo 7: Caso prático - produtos nunca pedidos
        Console.WriteLine("\n─── Exemplo 7: Caso prático - produtos sem pedidos ───");
        var idsProdutosPedidos = pedidos.Select(p => p.ProdutoId).Distinct();
        var idsTodosProdutos = produtos.Select(p => p.Id);
        var produtosSemPedidos = idsTodosProdutos.Except(idsProdutosPedidos);
        
        Console.WriteLine($"  Produtos nunca pedidos: {produtosSemPedidos.Count()}");
        foreach (var id in produtosSemPedidos)
        {
            var produto = produtos.First(p => p.Id == id);
            Console.WriteLine($"    • {produto.Nome}");
        }

        Console.WriteLine("\n💡 Dica: Use Distinct para remover duplicatas e Except para encontrar diferenças.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 10: Execução Adiada

    static void Demonstracao10_ExecucaoAdiada()
    {
        ImprimirTitulo("10. EXECUÇÃO ADIADA (DEFERRED EXECUTION)");

        Console.WriteLine("📌 Conceito: LINQ usa execução adiada - a query não é executada até ser iterada.");
        Console.WriteLine("   Operadores como Where, Select retornam IEnumerable sem executar.");
        Console.WriteLine("   Operadores como ToList, Count, First forçam execução imediata.\n");

        // Exemplo 1: Demonstração de execução adiada
        Console.WriteLine("─── Exemplo 1: Execução adiada em ação ───");
        var numeros = new List<int> { 1, 2, 3, 4, 5 };
        Console.WriteLine($"Lista original: {string.Join(", ", numeros)}");
        
        var query = numeros.Where(n => n > 3);
        Console.WriteLine("\nQuery criada (numeros.Where(n => n > 3))");
        Console.WriteLine("⚠️  Ainda NÃO foi executada!");
        
        numeros.Add(6);
        numeros.Add(7);
        Console.WriteLine($"\nAdicionados 6 e 7 à lista: {string.Join(", ", numeros)}");
        
        Console.WriteLine("\n🔄 Agora iterando a query...");
        Console.WriteLine($"Resultado: {string.Join(", ", query)}");
        Console.WriteLine("✓ A query incluiu os novos elementos!");

        // Exemplo 2: Execução imediata
        Console.WriteLine("\n─── Exemplo 2: Forçar execução imediata ───");
        var numerosOriginais = new List<int> { 1, 2, 3, 4, 5 };
        
        var queryImediata = numerosOriginais.Where(n => n > 3).ToList();
        Console.WriteLine($"Query com ToList(): {string.Join(", ", queryImediata)}");
        
        numerosOriginais.Add(6);
        numerosOriginais.Add(7);
        Console.WriteLine($"Lista modificada: {string.Join(", ", numerosOriginais)}");
        Console.WriteLine($"Query NÃO mudou: {string.Join(", ", queryImediata)}");
        Console.WriteLine("✓ ToList() capturou o snapshot no momento da execução");

        // Exemplo 3: Operadores de execução imediata
        Console.WriteLine("\n─── Exemplo 3: Operadores que forçam execução ───");
        var produtos10 = produtos.Take(5);
        
        Console.WriteLine("Operadores que NÃO executam imediatamente:");
        Console.WriteLine("  • Where, Select, OrderBy, Skip, Take");
        Console.WriteLine("  • Join, GroupBy, SelectMany");
        
        Console.WriteLine("\nOperadores que executam imediatamente:");
        Console.WriteLine("  • ToList(), ToArray(), ToDictionary()");
        Console.WriteLine("  • Count(), Sum(), Average(), Min(), Max()");
        Console.WriteLine("  • First(), Single(), Last()");
        Console.WriteLine("  • Any(), All(), Contains()");

        // Exemplo 4: Problema de execução múltipla
        Console.WriteLine("\n─── Exemplo 4: Cuidado com múltiplas iterações ───");
        Console.WriteLine("Sem ToList() - executa a query cada vez que itera:");
        
        var queryComplexa = produtos
            .Where(p => 
            {
                Console.Write(".");  // Indica cada execução
                return p.EmEstoque;
            });
        
        Console.WriteLine("\nPrimeira iteração:");
        var count1 = queryComplexa.Count();
        
        Console.WriteLine("\nSegunda iteração:");
        var count2 = queryComplexa.Count();
        
        Console.WriteLine($"\n\n✓ Query executada 2x! Total: {count1}");

        Console.WriteLine("\n\nCom ToList() - executa uma única vez:");
        var listaCache = produtos
            .Where(p => 
            {
                Console.Write(".");
                return p.EmEstoque;
            })
            .ToList();
        
        Console.WriteLine("\nPrimeira iteração:");
        var countLista1 = listaCache.Count();
        
        Console.WriteLine("Segunda iteração:");
        var countLista2 = listaCache.Count();
        
        Console.WriteLine($"\n✓ Query executada apenas 1x! Total: {countLista1}");

        // Exemplo 5: Quando usar cada abordagem
        Console.WriteLine("\n─── Exemplo 5: Quando usar cada abordagem ───");
        Console.WriteLine("\n✓ Use execução ADIADA quando:");
        Console.WriteLine("  • Trabalhar com grandes volumes de dados");
        Console.WriteLine("  • Fazer apenas uma iteração");
        Console.WriteLine("  • Precisar de dados sempre atualizados");
        Console.WriteLine("  • Compor queries complexas passo a passo");
        
        Console.WriteLine("\n✓ Use execução IMEDIATA (ToList/ToArray) quando:");
        Console.WriteLine("  • Iterar múltiplas vezes sobre o resultado");
        Console.WriteLine("  • Retornar de métodos (evitar múltiplas execuções)");
        Console.WriteLine("  • Precisar de snapshot dos dados");
        Console.WriteLine("  • Query muito custosa (cache o resultado)");

        Console.WriteLine("\n💡 Dica: Use ToList() ao retornar de métodos para evitar execuções múltiplas.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 11: Operações Avançadas

    static void Demonstracao11_OperacoesAvancadas()
    {
        ImprimirTitulo("11. OPERAÇÕES AVANÇADAS");

        Console.WriteLine("📌 Conceito: LINQ oferece operações sofisticadas para cenários complexos.\n");

        // Exemplo 1: Zip - combinar duas sequências
        Console.WriteLine("─── Exemplo 1: Zip - combinar sequências elemento por elemento ───");
        var numeros = new[] { 1, 2, 3, 4, 5 };
        var letras = new[] { "A", "B", "C", "D", "E" };
        var combinados = numeros.Zip(letras, (n, l) => $"{l}{n}");
        Console.WriteLine($"  Resultado: {string.Join(", ", combinados)}");

        // Exemplo 2: Range e Repeat
        Console.WriteLine("\n─── Exemplo 2: Range e Repeat - gerar sequências ───");
        var sequencia = Enumerable.Range(1, 10);
        Console.WriteLine($"  Range(1, 10): {string.Join(", ", sequencia)}");
        
        var repetidos = Enumerable.Repeat("★", 5);
        Console.WriteLine($"  Repeat('★', 5): {string.Join(" ", repetidos)}");

        // Exemplo 3: SequenceEqual
        Console.WriteLine("\n─── Exemplo 3: SequenceEqual - comparar sequências ───");
        var lista1 = new[] { 1, 2, 3 };
        var lista2 = new[] { 1, 2, 3 };
        var lista3 = new[] { 1, 3, 2 };
        
        Console.WriteLine($"  [1,2,3] == [1,2,3]: {lista1.SequenceEqual(lista2)}");
        Console.WriteLine($"  [1,2,3] == [1,3,2]: {lista1.SequenceEqual(lista3)}");

        // Exemplo 4: DefaultIfEmpty
        Console.WriteLine("\n─── Exemplo 4: DefaultIfEmpty - valor padrão se vazio ───");
        var listaVazia = new List<int>();
        var comPadrao = listaVazia.DefaultIfEmpty(0);
        Console.WriteLine($"  Lista vazia com padrão: {string.Join(", ", comPadrao)}");

        // Exemplo 5: Cast e OfType
        Console.WriteLine("\n─── Exemplo 5: Cast e OfType - conversão de tipos ───");
        var listaMista = new object[] { 1, "texto", 2, 3.5, "outro", 4 };
        
        var apenasInteiros = listaMista.OfType<int>();
        Console.WriteLine($"  Apenas inteiros: {string.Join(", ", apenasInteiros)}");
        
        var apenasStrings = listaMista.OfType<string>();
        Console.WriteLine($"  Apenas strings: {string.Join(", ", apenasStrings)}");

        // Exemplo 6: Lookup - agrupamento otimizado
        Console.WriteLine("\n─── Exemplo 6: ToLookup - agrupamento indexado ───");
        var produtosPorCategoria = produtos.ToLookup(p => p.Categoria);
        
        Console.WriteLine($"  Periféricos no lookup:");
        foreach (var p in produtosPorCategoria["Periféricos"].Take(3))
        {
            Console.WriteLine($"    • {p.Nome}");
        }

        // Exemplo 7: Join customizado (Left Join)
        Console.WriteLine("\n─── Exemplo 7: Left Join customizado ───");
        var produtosComPedidos = produtos
            .GroupJoin(pedidos,
                produto => produto.Id,
                pedido => pedido.ProdutoId,
                (produto, pedidosProduto) => new
                {
                    Produto = produto.Nome,
                    TotalPedidos = pedidosProduto.Count(),
                    Status = pedidosProduto.Any() ? "Com pedidos" : "Sem pedidos"
                });

        Console.WriteLine("  Produtos e seus pedidos:");
        foreach (var item in produtosComPedidos.Take(5))
        {
            Console.WriteLine($"    • {item.Produto}: {item.TotalPedidos} pedido(s) - {item.Status}");
        }

        // Exemplo 8: Operações customizadas com Aggregate
        Console.WriteLine("\n─── Exemplo 8: Aggregate avançado - estatísticas customizadas ───");
        var estatisticas = produtos
            .Where(p => p.EmEstoque)
            .Aggregate(
                new { Count = 0, Sum = 0m, Min = decimal.MaxValue, Max = decimal.MinValue },
                (acc, p) => new
                {
                    Count = acc.Count + 1,
                    Sum = acc.Sum + p.Preco,
                    Min = Math.Min(acc.Min, p.Preco),
                    Max = Math.Max(acc.Max, p.Preco)
                },
                acc => new
                {
                    acc.Count,
                    Average = acc.Sum / acc.Count,
                    acc.Min,
                    acc.Max
                });

        Console.WriteLine($"  Produtos em estoque: {estatisticas.Count}");
        Console.WriteLine($"  Preço médio: R$ {estatisticas.Average:N2}");
        Console.WriteLine($"  Faixa: R$ {estatisticas.Min:N2} - R$ {estatisticas.Max:N2}");

        Console.WriteLine("\n💡 Dica: Explore operações avançadas para cenários complexos e customizados.\n");
        PausarExecucao();
    }

    #endregion

    #region Demonstração 12: Casos de Uso Reais

    static void Demonstracao12_CasosReais()
    {
        ImprimirTitulo("12. CASOS DE USO REAIS");

        Console.WriteLine("📌 Conceito: Aplicações práticas de LINQ em cenários do mundo real.\n");

        // Caso 1: Relatório de vendas
        Console.WriteLine("─── Caso 1: Relatório de vendas por cliente ───");
        var relatorioVendas = pedidos
            .Join(clientes, p => p.ClienteId, c => c.Id, (p, c) => new { Pedido = p, Cliente = c })
            .Join(produtos, pc => pc.Pedido.ProdutoId, prod => prod.Id, (pc, prod) => new
            {
                Cliente = pc.Cliente.Nome,
                ValorPedido = pc.Pedido.Quantidade * prod.Preco
            })
            .GroupBy(x => x.Cliente)
            .Select(g => new
            {
                Cliente = g.Key,
                TotalPedidos = g.Count(),
                ValorTotal = g.Sum(x => x.ValorPedido)
            })
            .OrderByDescending(x => x.ValorTotal);

        Console.WriteLine("Top clientes por valor:");
        foreach (var item in relatorioVendas.Take(3))
        {
            Console.WriteLine($"  • {item.Cliente}");
            Console.WriteLine($"    Pedidos: {item.TotalPedidos} | Total: R$ {item.ValorTotal:N2}");
        }

        // Caso 2: Dashboard de produtos
        Console.WriteLine("\n─── Caso 2: Dashboard de produtos ───");
        var dashboard = new
        {
            TotalProdutos = produtos.Count(),
            ProdutosEmEstoque = produtos.Count(p => p.EmEstoque),
            ValorTotalEstoque = produtos.Sum(p => p.Preco * p.Estoque),
            ProdutoMaisCaro = produtos.OrderByDescending(p => p.Preco).First(),
            CategoriaComMaisProdutos = produtos
                .GroupBy(p => p.Categoria)
                .OrderByDescending(g => g.Count())
                .First().Key,
            EstoqueTotal = produtos.Sum(p => p.Estoque)
        };

        Console.WriteLine($"  📊 Total de produtos: {dashboard.TotalProdutos}");
        Console.WriteLine($"  📦 Em estoque: {dashboard.ProdutosEmEstoque}");
        Console.WriteLine($"  💰 Valor total: R$ {dashboard.ValorTotalEstoque:N2}");
        Console.WriteLine($"  🏆 Mais caro: {dashboard.ProdutoMaisCaro.Nome} (R$ {dashboard.ProdutoMaisCaro.Preco:N2})");
        Console.WriteLine($"  📁 Categoria líder: {dashboard.CategoriaComMaisProdutos}");
        Console.WriteLine($"  📈 Estoque total: {dashboard.EstoqueTotal} unidades");

        // Caso 3: Recomendação de produtos
        Console.WriteLine("\n─── Caso 3: Sistema de recomendação ───");
        var clienteId = 1;
        var categoriasCompradas = pedidos
            .Where(p => p.ClienteId == clienteId)
            .Join(produtos, p => p.ProdutoId, prod => prod.Id, (p, prod) => prod.Categoria)
            .Distinct();

        var recomendacoes = produtos
            .Where(p => categoriasCompradas.Contains(p.Categoria))
            .Where(p => !pedidos.Any(ped => ped.ClienteId == clienteId && ped.ProdutoId == p.Id))
            .Where(p => p.EmEstoque)
            .OrderByDescending(p => p.Preco)
            .Take(3);

        var nomeCliente = clientes.First(c => c.Id == clienteId).Nome;
        Console.WriteLine($"Recomendações para {nomeCliente}:");
        foreach (var produto in recomendacoes)
        {
            Console.WriteLine($"  • {produto.Nome} ({produto.Categoria}) - R$ {produto.Preco:N2}");
        }

        // Caso 4: Análise de performance de funcionários
        Console.WriteLine("\n─── Caso 4: Análise de hierarquia organizacional ───");
        var estruturaOrganizacional = funcionarios
            .GroupBy(f => f.GerenteId)
            .Select(g => new
            {
                GerenteId = g.Key,
                Subordinados = g.Count(),
                FolhaPagamento = g.Sum(f => f.Salario)
            });

        foreach (var item in estruturaOrganizacional.Where(x => x.GerenteId.HasValue))
        {
            var gerente = funcionarios.First(f => f.Id == item.GerenteId);
            Console.WriteLine($"  • {gerente.Nome} ({gerente.Cargo})");
            Console.WriteLine($"    Equipe: {item.Subordinados} pessoa(s)");
            Console.WriteLine($"    Folha: R$ {item.FolhaPagamento:N2}");
        }

        // Caso 5: Auditoria e rastreamento
        Console.WriteLine("\n─── Caso 5: Auditoria de estoque crítico ───");
        var alertasEstoque = produtos
            .Where(p => p.EmEstoque && p.Estoque < 15)
            .Select(p => new
            {
                Produto = p.Nome,
                EstoqueAtual = p.Estoque,
                PedidosRecentes = pedidos.Count(ped => ped.ProdutoId == p.Id),
                Status = p.Estoque < 5 ? "🔴 CRÍTICO" : "🟡 ATENÇÃO"
            })
            .OrderBy(x => x.EstoqueAtual);

        Console.WriteLine("Produtos com estoque baixo:");
        foreach (var alerta in alertasEstoque)
        {
            Console.WriteLine($"  {alerta.Status} {alerta.Produto}");
            Console.WriteLine($"    Estoque: {alerta.EstoqueAtual} | Pedidos: {alerta.PedidosRecentes}");
        }

        // Caso 6: Análise temporal
        Console.WriteLine("\n─── Caso 6: Análise temporal de pedidos ───");
        var pedidosPorMes = pedidos
            .GroupBy(p => new { Ano = p.DataPedido.Year, Mes = p.DataPedido.Month })
            .Select(g => new
            {
                Periodo = $"{g.Key.Mes:00}/{g.Key.Ano}",
                TotalPedidos = g.Count(),
                TicketMedio = g.Join(produtos, 
                    p => p.ProdutoId, 
                    prod => prod.Id, 
                    (p, prod) => p.Quantidade * prod.Preco).Average()
            })
            .OrderBy(x => x.Periodo);

        Console.WriteLine("Evolução mensal:");
        foreach (var mes in pedidosPorMes)
        {
            Console.WriteLine($"  📅 {mes.Periodo}: {mes.TotalPedidos} pedidos | Ticket médio: R$ {mes.TicketMedio:N2}");
        }

        Console.WriteLine("\n💡 Dica: Combine operações LINQ para criar análises e relatórios complexos.\n");
        PausarExecucao();
    }

    #endregion

    #region Métodos Auxiliares

    static void ImprimirTitulo(string titulo)
    {
        Console.WriteLine("\n" + new string('═', 80));
        Console.WriteLine($"  {titulo}");
        Console.WriteLine(new string('═', 80) + "\n");
    }

    static void PausarExecucao()
    {
        Console.WriteLine("Pressione qualquer tecla para continuar...");
        Console.ReadKey();
        Console.Clear();
    }

    #endregion
}