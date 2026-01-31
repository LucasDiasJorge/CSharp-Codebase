namespace GraphTraversalDemo;

/// <summary>
/// Representa um grafo usando lista de adjacências
/// </summary>
public class Graph
{
    private readonly Dictionary<int, List<int>> _adjacencyList;

    public Graph()
    {
        _adjacencyList = new Dictionary<int, List<int>>();
    }

    /// <summary>
    /// Adiciona um vértice ao grafo
    /// </summary>
    public void AddVertex(int vertex)
    {
        if (!_adjacencyList.ContainsKey(vertex))
        {
            _adjacencyList[vertex] = new List<int>();
        }
    }

    /// <summary>
    /// Adiciona uma aresta entre dois vértices (grafo direcionado)
    /// </summary>
    public void AddEdge(int source, int destination)
    {
        if (!_adjacencyList.ContainsKey(source))
        {
            AddVertex(source);
        }
        if (!_adjacencyList.ContainsKey(destination))
        {
            AddVertex(destination);
        }

        _adjacencyList[source].Add(destination);
    }

    /// <summary>
    /// Adiciona uma aresta bidirecional (grafo não-direcionado)
    /// </summary>
    public void AddUndirectedEdge(int vertex1, int vertex2)
    {
        AddEdge(vertex1, vertex2);
        AddEdge(vertex2, vertex1);
    }

    /// <summary>
    /// Busca em Profundidade (DFS) - Depth-First Search
    /// </summary>
    public List<int> DFS(int startVertex)
    {
        var visited = new HashSet<int>();
        var result = new List<int>();

        DFSRecursive(startVertex, visited, result);

        return result;
    }

    private void DFSRecursive(int vertex, HashSet<int> visited, List<int> result)
    {
        visited.Add(vertex);
        result.Add(vertex);

        if (_adjacencyList.ContainsKey(vertex))
        {
            foreach (var neighbor in _adjacencyList[vertex])
            {
                if (!visited.Contains(neighbor))
                {
                    DFSRecursive(neighbor, visited, result);
                }
            }
        }
    }

    /// <summary>
    /// Busca em Profundidade iterativa usando Stack
    /// </summary>
    public List<int> DFSIterative(int startVertex)
    {
        var visited = new HashSet<int>();
        var result = new List<int>();
        var stack = new Stack<int>();

        stack.Push(startVertex);

        while (stack.Count > 0)
        {
            var vertex = stack.Pop();

            if (!visited.Contains(vertex))
            {
                visited.Add(vertex);
                result.Add(vertex);

                if (_adjacencyList.ContainsKey(vertex))
                {
                    // Adiciona vizinhos na ordem reversa para manter a mesma ordem do DFS recursivo
                    for (int i = _adjacencyList[vertex].Count - 1; i >= 0; i--)
                    {
                        var neighbor = _adjacencyList[vertex][i];
                        if (!visited.Contains(neighbor))
                        {
                            stack.Push(neighbor);
                        }
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Busca em Largura (BFS) - Breadth-First Search
    /// </summary>
    public List<int> BFS(int startVertex)
    {
        var visited = new HashSet<int>();
        var result = new List<int>();
        var queue = new Queue<int>();

        visited.Add(startVertex);
        queue.Enqueue(startVertex);

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();
            result.Add(vertex);

            if (_adjacencyList.ContainsKey(vertex))
            {
                foreach (var neighbor in _adjacencyList[vertex])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Encontra o caminho mais curto usando BFS
    /// </summary>
    public List<int>? FindShortestPath(int start, int end)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        var parent = new Dictionary<int, int>();

        visited.Add(start);
        queue.Enqueue(start);
        parent[start] = -1;

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();

            if (vertex == end)
            {
                return ReconstructPath(parent, start, end);
            }

            if (_adjacencyList.ContainsKey(vertex))
            {
                foreach (var neighbor in _adjacencyList[vertex])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                        parent[neighbor] = vertex;
                    }
                }
            }
        }

        return null; // Caminho não encontrado
    }

    private List<int> ReconstructPath(Dictionary<int, int> parent, int start, int end)
    {
        var path = new List<int>();
        var current = end;

        while (current != -1)
        {
            path.Add(current);
            current = parent[current];
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Verifica se existe um caminho entre dois vértices usando DFS
    /// </summary>
    public bool HasPath(int start, int end)
    {
        var visited = new HashSet<int>();
        return HasPathDFS(start, end, visited);
    }

    private bool HasPathDFS(int current, int end, HashSet<int> visited)
    {
        if (current == end)
        {
            return true;
        }

        visited.Add(current);

        if (_adjacencyList.ContainsKey(current))
        {
            foreach (var neighbor in _adjacencyList[current])
            {
                if (!visited.Contains(neighbor))
                {
                    if (HasPathDFS(neighbor, end, visited))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Detecta ciclos no grafo usando DFS
    /// </summary>
    public bool HasCycle()
    {
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();

        foreach (var vertex in _adjacencyList.Keys)
        {
            if (!visited.Contains(vertex))
            {
                if (HasCycleDFS(vertex, visited, recursionStack))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasCycleDFS(int vertex, HashSet<int> visited, HashSet<int> recursionStack)
    {
        visited.Add(vertex);
        recursionStack.Add(vertex);

        if (_adjacencyList.ContainsKey(vertex))
        {
            foreach (var neighbor in _adjacencyList[vertex])
            {
                if (!visited.Contains(neighbor))
                {
                    if (HasCycleDFS(neighbor, visited, recursionStack))
                    {
                        return true;
                    }
                }
                else if (recursionStack.Contains(neighbor))
                {
                    return true;
                }
            }
        }

        recursionStack.Remove(vertex);
        return false;
    }

    /// <summary>
    /// Retorna todos os vértices do grafo
    /// </summary>
    public IEnumerable<int> GetVertices()
    {
        return _adjacencyList.Keys;
    }

    /// <summary>
    /// Retorna os vizinhos de um vértice
    /// </summary>
    public IEnumerable<int> GetNeighbors(int vertex)
    {
        return _adjacencyList.ContainsKey(vertex) ? _adjacencyList[vertex] : Enumerable.Empty<int>();
    }

    /// <summary>
    /// Imprime a representação do grafo
    /// </summary>
    public void PrintGraph()
    {
        Console.WriteLine("\n=== Estrutura do Grafo ===");
        foreach (var vertex in _adjacencyList.Keys.OrderBy(v => v))
        {
            Console.Write($"Vértice {vertex}: ");
            Console.WriteLine(string.Join(" -> ", _adjacencyList[vertex]));
        }
        Console.WriteLine();
    }
}
