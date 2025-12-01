# GraphTraversalDemo - Algoritmos de Busca em Grafos (DFS e BFS)

Projeto educacional demonstrando implementações de algoritmos de busca em grafos: **DFS (Depth-First Search)** e **BFS (Breadth-First Search)** em C#.

## 📚 Conceitos Implementados

### Depth-First Search (DFS) - Busca em Profundidade
- **Recursivo**: Implementação clássica usando recursão
- **Iterativo**: Implementação usando `Stack<T>`
- **Características**: Explora o grafo em profundidade, indo até o fim de um caminho antes de retornar

### Breadth-First Search (BFS) - Busca em Largura
- **Implementação**: Usando `Queue<T>`
- **Características**: Explora o grafo nível por nível, visitando todos os vizinhos antes de descer
- **Aplicações**: Caminho mais curto em grafos não-ponderados

## 🎯 Funcionalidades

### Classe `Graph`
A classe principal que representa um grafo usando lista de adjacências:

- ✅ **AddVertex(int vertex)**: Adiciona um vértice ao grafo
- ✅ **AddEdge(int source, int destination)**: Adiciona aresta direcionada
- ✅ **AddUndirectedEdge(int v1, int v2)**: Adiciona aresta bidirecional
- ✅ **DFS(int start)**: Busca em profundidade (recursiva)
- ✅ **DFSIterative(int start)**: Busca em profundidade (iterativa)
- ✅ **BFS(int start)**: Busca em largura
- ✅ **FindShortestPath(int start, int end)**: Encontra o caminho mais curto usando BFS
- ✅ **HasPath(int start, int end)**: Verifica se existe caminho entre dois vértices
- ✅ **HasCycle()**: Detecta ciclos no grafo usando DFS
- ✅ **GetVertices()**: Retorna todos os vértices
- ✅ **GetNeighbors(int vertex)**: Retorna vizinhos de um vértice
- ✅ **PrintGraph()**: Imprime a estrutura do grafo

## 🚀 Como Executar

```bash
# Navegar até o diretório do projeto
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GraphTraversalDemo"

# Executar o projeto
dotnet run

# Ou executar diretamente da raiz
dotnet run --project "GraphTraversalDemo\GraphTraversalDemo.csproj"
```

## 📊 Exemplos Demonstrados

### Exemplo 1: Grafo Direcionado
Demonstra DFS e BFS em um grafo direcionado simples, mostrando a diferença na ordem de visitação.

### Exemplo 2: Árvore Binária
Representa uma árvore binária como grafo não-direcionado, mostrando como DFS e BFS se comportam em estruturas de árvore.

### Exemplo 3: Múltiplos Componentes
Mostra como lidar com grafos que têm componentes desconectados.

### Exemplo 4: Detecção de Ciclos
Demonstra o algoritmo de detecção de ciclos usando DFS.

### Exemplo 5: Labirinto
Simula um labirinto 3x3 e encontra o caminho mais curto da entrada para a saída usando BFS.

## 🎓 Complexidade dos Algoritmos

### DFS (Depth-First Search)
- **Tempo**: O(V + E) onde V = vértices, E = arestas
- **Espaço**: O(V) para a pilha de recursão ou stack explícita
- **Uso**: Detecção de ciclos, ordenação topológica, componentes fortemente conectados

### BFS (Breadth-First Search)
- **Tempo**: O(V + E) onde V = vértices, E = arestas
- **Espaço**: O(V) para a fila
- **Uso**: Caminho mais curto (grafos não-ponderados), verificação de conectividade

## 🧩 Estrutura do Código

```
GraphTraversalDemo/
├── GraphTraversalDemo.csproj  # Configuração do projeto
├── Graph.cs                    # Implementação da classe Graph com DFS/BFS
├── Program.cs                  # Exemplos e demonstrações
└── README.md                   # Documentação
```

## 💡 Quando Usar Cada Algoritmo?

### Use DFS quando:
- Precisa explorar todos os caminhos possíveis
- Quer detectar ciclos
- Precisa fazer ordenação topológica
- Está resolvendo problemas de backtracking
- A solução está "longe" do nó inicial

### Use BFS quando:
- Precisa do caminho mais curto (grafos não-ponderados)
- A solução está "perto" do nó inicial
- Precisa explorar por níveis
- Quer verificar conectividade entre dois nós

## 📝 Conceitos Importantes

### Representação do Grafo
Este projeto usa **lista de adjacências** (`Dictionary<int, List<int>>`), que é eficiente para grafos esparsos.

### Controle de Visitação
Ambos os algoritmos usam `HashSet<int>` para rastrear vértices visitados, evitando loops infinitos.

### Grafo Direcionado vs Não-Direcionado
- **Direcionado**: Arestas têm direção (A → B)
- **Não-Direcionado**: Arestas bidirecionais (A ↔ B)

## 🔗 Extensões Possíveis

- [ ] Adicionar suporte para grafos ponderados
- [ ] Implementar algoritmo de Dijkstra
- [ ] Implementar algoritmo A*
- [ ] Adicionar visualização gráfica dos grafos
- [ ] Implementar algoritmo de Kruskal/Prim para MST
- [ ] Adicionar testes unitários

## 📖 Referências

- [Graph Theory - Wikipedia](https://en.wikipedia.org/wiki/Graph_theory)
- [DFS - GeeksforGeeks](https://www.geeksforgeeks.org/depth-first-search-or-dfs-for-a-graph/)
- [BFS - GeeksforGeeks](https://www.geeksforgeeks.org/breadth-first-search-or-bfs-for-a-graph/)

## 🎯 Objetivos de Aprendizado

Após estudar este projeto, você será capaz de:
- ✅ Entender a diferença entre DFS e BFS
- ✅ Implementar grafos usando lista de adjacências
- ✅ Aplicar DFS para detecção de ciclos
- ✅ Usar BFS para encontrar caminhos mais curtos
- ✅ Escolher o algoritmo adequado para cada problema
- ✅ Compreender complexidade de tempo e espaço

---

**Tecnologias**: .NET 9, C# 12
**Padrão**: Console Application

## ⚙️ Instalação Manual

Se encontrar problemas ao criar o projeto automaticamente, siga estes passos:

1. Crie a pasta manualmente: `GraphTraversalDemo`
2. Copie os arquivos fornecidos para a pasta
3. Execute: `dotnet restore` e depois `dotnet run`
