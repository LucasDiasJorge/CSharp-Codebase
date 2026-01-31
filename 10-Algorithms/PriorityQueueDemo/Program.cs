// =============================================================================
// 🎯 Priority Queue Demo - Exemplos práticos de filas de prioridade em C#
// =============================================================================

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║           🎯 PRIORITY QUEUE DEMO - C# .NET                   ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

// =============================================================================
// 📘 Exemplo 1: Uso Básico da PriorityQueue<TElement, TPriority>
// =============================================================================
Console.WriteLine("📘 EXEMPLO 1: Uso Básico");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

var basicQueue = new PriorityQueue<string, int>();

// Enqueue: elemento + prioridade (menor número = maior prioridade)
basicQueue.Enqueue("Tarefa C - Baixa prioridade", 3);
basicQueue.Enqueue("Tarefa A - Alta prioridade", 1);
basicQueue.Enqueue("Tarefa B - Média prioridade", 2);

Console.WriteLine("Processando tarefas por prioridade:");
while (basicQueue.TryDequeue(out var tarefa, out var prioridade))
{
    Console.WriteLine($"  ✓ Prioridade {prioridade}: {tarefa}");
}

// =============================================================================
// 🏥 Exemplo 2: Triagem de Emergência Hospitalar
// =============================================================================
Console.WriteLine("\n🏥 EXEMPLO 2: Triagem de Emergência Hospitalar");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

var emergencyRoom = new PriorityQueue<Patient, int>();

// Pacientes chegando ao pronto-socorro
emergencyRoom.Enqueue(new Patient("João", "Dor de cabeça leve"), 4);          // Verde
emergencyRoom.Enqueue(new Patient("Maria", "Infarto em andamento"), 1);       // Vermelho
emergencyRoom.Enqueue(new Patient("Pedro", "Fratura exposta"), 2);            // Laranja
emergencyRoom.Enqueue(new Patient("Ana", "Febre alta"), 3);                   // Amarelo
emergencyRoom.Enqueue(new Patient("Carlos", "Parada respiratória"), 1);       // Vermelho

Console.WriteLine("Ordem de atendimento:");
while (emergencyRoom.TryDequeue(out var patient, out var severity))
{
    var color = severity switch
    {
        1 => "🔴 EMERGÊNCIA",
        2 => "🟠 URGENTE",
        3 => "🟡 POUCO URGENTE",
        _ => "🟢 NÃO URGENTE"
    };
    Console.WriteLine($"  {color}: {patient.Name} - {patient.Condition}");
}

// =============================================================================
// 💼 Exemplo 3: Sistema de Tickets de Suporte
// =============================================================================
Console.WriteLine("\n💼 EXEMPLO 3: Sistema de Tickets de Suporte");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

var ticketSystem = new PriorityQueue<SupportTicket, TicketPriority>();

ticketSystem.Enqueue(new SupportTicket(1001, "Sistema fora do ar", "Produção parada"), TicketPriority.Critical);
ticketSystem.Enqueue(new SupportTicket(1002, "Bug no relatório", "Dados incorretos"), TicketPriority.Medium);
ticketSystem.Enqueue(new SupportTicket(1003, "Dúvida sobre uso", "Como exportar?"), TicketPriority.Low);
ticketSystem.Enqueue(new SupportTicket(1004, "Vazamento de dados", "Possível breach"), TicketPriority.Critical);
ticketSystem.Enqueue(new SupportTicket(1005, "Lentidão no sistema", "Performance"), TicketPriority.High);

Console.WriteLine("Fila de atendimento:");
while (ticketSystem.TryDequeue(out var ticket, out var priority))
{
    var icon = priority switch
    {
        TicketPriority.Critical => "🚨",
        TicketPriority.High => "⚠️",
        TicketPriority.Medium => "📋",
        _ => "📝"
    };
    Console.WriteLine($"  {icon} [{priority}] Ticket #{ticket.Id}: {ticket.Title}");
}

// =============================================================================
// 🎮 Exemplo 4: Pathfinding - Algoritmo de Dijkstra Simplificado
// =============================================================================
Console.WriteLine("\n🎮 EXEMPLO 4: Pathfinding (Dijkstra Simplificado)");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

var graph = new Dictionary<string, List<(string node, int cost)>>
{
    ["A"] = [("B", 4), ("C", 2)],
    ["B"] = [("A", 4), ("C", 1), ("D", 5)],
    ["C"] = [("A", 2), ("B", 1), ("D", 8), ("E", 10)],
    ["D"] = [("B", 5), ("C", 8), ("E", 2)],
    ["E"] = [("C", 10), ("D", 2)]
};

var distances = FindShortestPaths(graph, "A");

Console.WriteLine("Menores distâncias a partir de 'A':");
foreach (var (node, distance) in distances.OrderBy(x => x.Key))
{
    Console.WriteLine($"  A → {node}: {distance}");
}

// =============================================================================
// 📊 Exemplo 5: Processamento de Jobs com Deadline
// =============================================================================
Console.WriteLine("\n📊 EXEMPLO 5: Processamento de Jobs com Deadline");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

var jobQueue = new PriorityQueue<Job, DateTime>();

var now = DateTime.Now;
jobQueue.Enqueue(new Job("Relatório Mensal", TimeSpan.FromMinutes(30)), now.AddHours(4));
jobQueue.Enqueue(new Job("Backup Crítico", TimeSpan.FromMinutes(15)), now.AddMinutes(30));
jobQueue.Enqueue(new Job("Email Marketing", TimeSpan.FromHours(1)), now.AddDays(1));
jobQueue.Enqueue(new Job("Deploy Hotfix", TimeSpan.FromMinutes(10)), now.AddMinutes(15));

Console.WriteLine("Jobs ordenados por deadline:");
while (jobQueue.TryDequeue(out var job, out var deadline))
{
    var urgency = (deadline - now).TotalMinutes switch
    {
        < 30 => "🔴 URGENTE",
        < 120 => "🟡 EM BREVE",
        _ => "🟢 NORMAL"
    };
    Console.WriteLine($"  {urgency} {job.Name} (Duração: {job.Duration.TotalMinutes}min) - Deadline: {deadline:HH:mm}");
}

// =============================================================================
// 🔧 Exemplo 6: PriorityQueue com Comparer Customizado
// =============================================================================
Console.WriteLine("\n🔧 EXEMPLO 6: Comparer Customizado (Maior = Maior Prioridade)");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

// Por padrão, PriorityQueue usa min-heap (menor valor = maior prioridade)
// Com Comparer.Create, podemos inverter para max-heap
var maxHeap = new PriorityQueue<string, int>(Comparer<int>.Create((a, b) => b.CompareTo(a)));

maxHeap.Enqueue("Bronze", 1);
maxHeap.Enqueue("Ouro", 3);
maxHeap.Enqueue("Prata", 2);
maxHeap.Enqueue("Diamante", 4);

Console.WriteLine("Ranking (maior score primeiro):");
var position = 1;
while (maxHeap.TryDequeue(out var player, out var score))
{
    Console.WriteLine($"  {position}º lugar: {player} (Score: {score})");
    position++;
}

// =============================================================================
// 📈 Exemplo 7: Merge de K Listas Ordenadas
// =============================================================================
Console.WriteLine("\n📈 EXEMPLO 7: Merge de K Listas Ordenadas");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

var lists = new List<int[]>
{
    new[] { 1, 4, 7 },
    new[] { 2, 5, 8 },
    new[] { 3, 6, 9 }
};

var merged = MergeKSortedLists(lists);
Console.WriteLine($"Listas: {string.Join(" | ", lists.Select(l => $"[{string.Join(", ", l)}]"))}");
Console.WriteLine($"Resultado: [{string.Join(", ", merged)}]");

// =============================================================================
Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                    ✅ Demo Concluída!                        ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

// =============================================================================
// 🛠️ Métodos Auxiliares
// =============================================================================

static Dictionary<string, int> FindShortestPaths(Dictionary<string, List<(string node, int cost)>> graph, string start)
{
    var distances = new Dictionary<string, int>();
    var visited = new HashSet<string>();
    var pq = new PriorityQueue<string, int>();

    foreach (var node in graph.Keys)
        distances[node] = int.MaxValue;

    distances[start] = 0;
    pq.Enqueue(start, 0);

    while (pq.TryDequeue(out var current, out var currentDist))
    {
        if (visited.Contains(current)) continue;
        visited.Add(current);

        foreach (var (neighbor, cost) in graph[current])
        {
            var newDist = currentDist + cost;
            if (newDist < distances[neighbor])
            {
                distances[neighbor] = newDist;
                pq.Enqueue(neighbor, newDist);
            }
        }
    }

    return distances;
}

static List<int> MergeKSortedLists(List<int[]> lists)
{
    var result = new List<int>();
    var pq = new PriorityQueue<(int listIndex, int elementIndex), int>();

    // Inicializa com o primeiro elemento de cada lista
    for (int i = 0; i < lists.Count; i++)
    {
        if (lists[i].Length > 0)
            pq.Enqueue((i, 0), lists[i][0]);
    }

    while (pq.TryDequeue(out var item, out var value))
    {
        result.Add(value);
        var (listIndex, elementIndex) = item;

        // Adiciona próximo elemento da mesma lista
        if (elementIndex + 1 < lists[listIndex].Length)
        {
            var nextIndex = elementIndex + 1;
            pq.Enqueue((listIndex, nextIndex), lists[listIndex][nextIndex]);
        }
    }

    return result;
}

// =============================================================================
// 📦 Records e Enums
// =============================================================================

record Patient(string Name, string Condition);

record SupportTicket(int Id, string Title, string Description);

enum TicketPriority { Critical = 1, High = 2, Medium = 3, Low = 4 }

record Job(string Name, TimeSpan Duration);
