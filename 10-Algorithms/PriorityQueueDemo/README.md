# 🎯 Priority Queue Demo

## 📋 Conceitos

**Priority Queue** (Fila de Prioridade) é uma estrutura de dados onde cada elemento possui uma prioridade associada. Elementos com maior prioridade são processados antes dos elementos com menor prioridade.

### 🔑 Características

- **Heap-based**: Implementada usando Min-Heap por padrão
- **O(log n)**: Inserção e remoção
- **O(1)**: Peek (visualizar próximo elemento)
- **Genérica**: `PriorityQueue<TElement, TPriority>`

---

## 🎯 Objetivos

- ✅ Demonstrar uso básico da `PriorityQueue<T, P>`
- ✅ Implementar cenários reais (triagem, tickets, jobs)
- ✅ Mostrar uso com Comparer customizado
- ✅ Aplicar em algoritmos (Dijkstra, Merge K Lists)

---

## 📁 Estrutura

```
PriorityQueueDemo/
├── PriorityQueueDemo.csproj
├── Program.cs              # 7 exemplos práticos
└── README.md
```

---

## ▶️ Como Executar

```bash
cd 10-Algorithms/PriorityQueueDemo
dotnet run
```

---

## 📚 Exemplos Incluídos

### 1️⃣ Uso Básico
```csharp
var queue = new PriorityQueue<string, int>();
queue.Enqueue("Tarefa A", 1);  // Alta prioridade
queue.Enqueue("Tarefa B", 2);  // Média prioridade
queue.Enqueue("Tarefa C", 3);  // Baixa prioridade

while (queue.TryDequeue(out var item, out var priority))
    Console.WriteLine($"{priority}: {item}");
```

### 2️⃣ Triagem Hospitalar
Simula pronto-socorro com classificação de risco (Manchester):
- 🔴 Vermelho: Emergência
- 🟠 Laranja: Urgente
- 🟡 Amarelo: Pouco urgente
- 🟢 Verde: Não urgente

### 3️⃣ Sistema de Tickets
Fila de suporte com prioridades:
- Critical, High, Medium, Low

### 4️⃣ Pathfinding (Dijkstra)
Encontra menor caminho em grafo ponderado.

### 5️⃣ Jobs com Deadline
Processa tarefas ordenadas por prazo.

### 6️⃣ Max-Heap com Comparer
```csharp
// Inverte para maior valor = maior prioridade
var maxHeap = new PriorityQueue<string, int>(
    Comparer<int>.Create((a, b) => b.CompareTo(a))
);
```

### 7️⃣ Merge K Listas Ordenadas
Algoritmo clássico usando Priority Queue.

---

## 🔧 API Principal

| Método | Descrição | Complexidade |
|--------|-----------|--------------|
| `Enqueue(element, priority)` | Adiciona elemento | O(log n) |
| `Dequeue()` | Remove e retorna menor prioridade | O(log n) |
| `TryDequeue(out element, out priority)` | Tenta remover | O(log n) |
| `Peek()` | Visualiza próximo sem remover | O(1) |
| `TryPeek(out element, out priority)` | Tenta visualizar | O(1) |
| `Count` | Quantidade de elementos | O(1) |
| `Clear()` | Remove todos | O(n) |

---

## 💡 Casos de Uso Reais

| Cenário | Elemento | Prioridade |
|---------|----------|------------|
| Triagem médica | Paciente | Nível de urgência |
| Task scheduler | Processo | Nice value |
| Dijkstra/A* | Nó do grafo | Distância |
| Huffman coding | Nó da árvore | Frequência |
| Event simulation | Evento | Timestamp |
| Load balancer | Request | Latência |

---

## ⚠️ Importante

- **Min-Heap padrão**: Menor valor = maior prioridade
- **Não é thread-safe**: Use locks para concorrência
- **Não permite update**: Para atualizar prioridade, remova e adicione novamente
- **Disponível desde .NET 6**

---

## 📖 Referências

- [PriorityQueue<TElement,TPriority> Class](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.priorityqueue-2)
- [Heap Data Structure](https://en.wikipedia.org/wiki/Heap_(data_structure))
- [Dijkstra's Algorithm](https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm)
