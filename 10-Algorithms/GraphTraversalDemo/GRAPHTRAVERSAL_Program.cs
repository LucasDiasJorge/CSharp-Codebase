namespace GraphTraversalDemo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Demonstração de Algoritmos DFS e BFS ===\n");

        // Exemplo 1: Grafo Direcionado Simples
        Console.WriteLine("📊 EXEMPLO 1: Grafo Direcionado");
        Console.WriteLine("================================");
        var graph1 = CreateDirectedGraph();
        graph1.PrintGraph();

        int startVertex = 0;
        Console.WriteLine($"Vértice inicial: {startVertex}\n");

        var dfsResult = graph1.DFS(startVertex);
        Console.WriteLine($"DFS Recursivo: {string.Join(" -> ", dfsResult)}");

        var dfsIterativeResult = graph1.DFSIterative(startVertex);
        Console.WriteLine($"DFS Iterativo: {string.Join(" -> ", dfsIterativeResult)}");

        var bfsResult = graph1.BFS(startVertex);
        Console.WriteLine($"BFS:           {string.Join(" -> ", bfsResult)}");

        // Verificar caminho
        int target = 4;
        bool hasPath = graph1.HasPath(startVertex, target);
        Console.WriteLine($"\nExiste caminho de {startVertex} para {target}? {(hasPath ? "Sim" : "Não")}");

        // Caminho mais curto
        var shortestPath = graph1.FindShortestPath(startVertex, target);
        if (shortestPath != null)
        {
            Console.WriteLine($"Caminho mais curto: {string.Join(" -> ", shortestPath)}");
        }

        // Detectar ciclo
        Console.WriteLine($"O grafo possui ciclos? {(graph1.HasCycle() ? "Sim" : "Não")}");

        Console.WriteLine("\n" + new string('=', 50) + "\n");

        // Exemplo 2: Grafo Não-Direcionado (Árvore)
        Console.WriteLine("🌳 EXEMPLO 2: Grafo Não-Direcionado (Árvore Binária)");
        Console.WriteLine("====================================================");
        var graph2 = CreateBinaryTreeGraph();
        graph2.PrintGraph();

        int treeRoot = 1;
        Console.WriteLine($"Raiz da árvore: {treeRoot}\n");

        var treeDFS = graph2.DFS(treeRoot);
        Console.WriteLine($"DFS (Pre-order): {string.Join(" -> ", treeDFS)}");

        var treeBFS = graph2.BFS(treeRoot);
        Console.WriteLine($"BFS (Level-order): {string.Join(" -> ", treeBFS)}");

        Console.WriteLine("\n" + new string('=', 50) + "\n");

        // Exemplo 3: Grafo com Múltiplos Componentes
        Console.WriteLine("🔗 EXEMPLO 3: Grafo com Múltiplos Componentes Conectados");
        Console.WriteLine("=========================================================");
        var graph3 = CreateDisconnectedGraph();
        graph3.PrintGraph();

        Console.WriteLine("Explorando componentes separadamente:\n");

        var component1 = graph3.BFS(1);
        Console.WriteLine($"Componente 1 (início=1): {string.Join(" -> ", component1)}");

        var component2 = graph3.BFS(5);
        Console.WriteLine($"Componente 2 (início=5): {string.Join(" -> ", component2)}");

        Console.WriteLine("\n" + new string('=', 50) + "\n");

        // Exemplo 4: Grafo com Ciclo
        Console.WriteLine("♻️  EXEMPLO 4: Grafo com Ciclo");
        Console.WriteLine("==============================");
        var graph4 = CreateGraphWithCycle();
        graph4.PrintGraph();

        Console.WriteLine($"O grafo possui ciclos? {(graph4.HasCycle() ? "Sim ✓" : "Não ✗")}");

        Console.WriteLine("\n" + new string('=', 50) + "\n");

        // Exemplo 5: Labirinto
        Console.WriteLine("🗺️  EXEMPLO 5: Encontrando Caminho em um Labirinto");
        Console.WriteLine("==================================================");
        var maze = CreateMazeGraph();
        maze.PrintGraph();

        int entrance = 0;
        int exit = 8;
        var mazePath = maze.FindShortestPath(entrance, exit);

        if (mazePath != null)
        {
            Console.WriteLine($"Caminho da entrada ({entrance}) para a saída ({exit}):");
            Console.WriteLine($"  {string.Join(" -> ", mazePath)}");
            Console.WriteLine($"  Distância: {mazePath.Count - 1} passos");
        }
        else
        {
            Console.WriteLine("Não existe caminho!");
        }

        Console.WriteLine("\n" + new string('=', 50) + "\n");

        // Comparação Visual
        Console.WriteLine("📈 COMPARAÇÃO: DFS vs BFS");
        Console.WriteLine("=========================");
        var compGraph = CreateComparisonGraph();
        compGraph.PrintGraph();

        Console.WriteLine("Ordem de visita dos vértices:\n");
        
        var compDFS = compGraph.DFS(1);
        Console.WriteLine($"DFS: {string.Join(", ", compDFS)}");
        Console.WriteLine("    → Explora em profundidade (vai até o fim antes de retornar)");

        var compBFS = compGraph.BFS(1);
        Console.WriteLine($"\nBFS: {string.Join(", ", compBFS)}");
        Console.WriteLine("    → Explora em largura (visita todos os vizinhos primeiro)");

        Console.WriteLine("\n=== Demonstração Concluída ===");
    }

    /// <summary>
    /// Cria um grafo direcionado simples
    ///     0 → 1 → 3
    ///     ↓   ↓   ↓
    ///     2 → 4 ← 5
    /// </summary>
    static Graph CreateDirectedGraph()
    {
        var graph = new Graph();
        graph.AddEdge(0, 1);
        graph.AddEdge(0, 2);
        graph.AddEdge(1, 3);
        graph.AddEdge(1, 4);
        graph.AddEdge(2, 4);
        graph.AddEdge(3, 4);
        graph.AddEdge(3, 5);
        graph.AddEdge(5, 4);
        return graph;
    }

    /// <summary>
    /// Cria uma árvore binária como grafo não-direcionado
    ///          1
    ///        /   \
    ///       2     3
    ///      / \   / \
    ///     4   5 6   7
    /// </summary>
    static Graph CreateBinaryTreeGraph()
    {
        var graph = new Graph();
        graph.AddUndirectedEdge(1, 2);
        graph.AddUndirectedEdge(1, 3);
        graph.AddUndirectedEdge(2, 4);
        graph.AddUndirectedEdge(2, 5);
        graph.AddUndirectedEdge(3, 6);
        graph.AddUndirectedEdge(3, 7);
        return graph;
    }

    /// <summary>
    /// Cria um grafo com componentes desconectados
    /// Componente 1: 1-2-3-4
    /// Componente 2: 5-6-7
    /// </summary>
    static Graph CreateDisconnectedGraph()
    {
        var graph = new Graph();
        // Componente 1
        graph.AddUndirectedEdge(1, 2);
        graph.AddUndirectedEdge(2, 3);
        graph.AddUndirectedEdge(3, 4);
        graph.AddUndirectedEdge(4, 1);

        // Componente 2
        graph.AddUndirectedEdge(5, 6);
        graph.AddUndirectedEdge(6, 7);
        graph.AddUndirectedEdge(7, 5);

        return graph;
    }

    /// <summary>
    /// Cria um grafo direcionado com ciclo
    /// 1 → 2 → 3 → 4
    /// ↑           ↓
    /// └───────────┘
    /// </summary>
    static Graph CreateGraphWithCycle()
    {
        var graph = new Graph();
        graph.AddEdge(1, 2);
        graph.AddEdge(2, 3);
        graph.AddEdge(3, 4);
        graph.AddEdge(4, 1); // Cria o ciclo
        return graph;
    }

    /// <summary>
    /// Cria um grafo representando um labirinto 3x3
    /// 0 - 1 - 2
    /// |   |   |
    /// 3 - 4 - 5
    /// |   |   |
    /// 6 - 7 - 8
    /// </summary>
    static Graph CreateMazeGraph()
    {
        var graph = new Graph();
        // Linha superior
        graph.AddUndirectedEdge(0, 1);
        graph.AddUndirectedEdge(1, 2);

        // Linha do meio
        graph.AddUndirectedEdge(3, 4);
        graph.AddUndirectedEdge(4, 5);

        // Linha inferior
        graph.AddUndirectedEdge(6, 7);
        graph.AddUndirectedEdge(7, 8);

        // Conexões verticais
        graph.AddUndirectedEdge(0, 3);
        graph.AddUndirectedEdge(1, 4);
        graph.AddUndirectedEdge(2, 5);
        graph.AddUndirectedEdge(3, 6);
        graph.AddUndirectedEdge(4, 7);
        graph.AddUndirectedEdge(5, 8);

        return graph;
    }

    /// <summary>
    /// Cria um grafo para demonstrar a diferença entre DFS e BFS
    ///       1
    ///      /|\
    ///     2 3 4
    ///    /| |
    ///   5 6 7
    /// </summary>
    static Graph CreateComparisonGraph()
    {
        var graph = new Graph();
        graph.AddEdge(1, 2);
        graph.AddEdge(1, 3);
        graph.AddEdge(1, 4);
        graph.AddEdge(2, 5);
        graph.AddEdge(2, 6);
        graph.AddEdge(3, 7);
        return graph;
    }
}
