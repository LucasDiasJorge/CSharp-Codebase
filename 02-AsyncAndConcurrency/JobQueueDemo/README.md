# JobQueueDemo - Filas de Processamento em C#

## Visão geral

Um sistema didático e progressivo para aprender **filas de processamento concorrente** em C# usando `System.Threading.Channels`.

## Conceitos abordados

- Exemplo didático sobre JobQueueDemo - Filas de Processamento em C# no contexto de assincronia, tasks, threads e coordenação de trabalho.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

Este projeto ensina os seguintes conceitos de forma **progressiva** e **prática**:

1. **Filas Thread-Safe** com `Channel<T>`
2. **Processamento Paralelo** com múltiplos workers
3. **Operações Atômicas** usando `Interlocked`
4. **Padrão Producer-Consumer**
5. **Estatísticas e Métricas** de processamento
6. **Aplicações do Mundo Real** (e-commerce, sistemas financeiros, etc)

## Estrutura do projeto

```text
JobQueueDemo/
+-- JobQueueDemo.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 02-AsyncAndConcurrency/JobQueueDemo/JobQueueDemo.csproj
```

Você verá um **menu interativo** para escolher qual exemplo executar:

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Estrutura do Projeto

O projeto está organizado em **3 exemplos progressivos**:

##### 1️⃣ Exemplo Básico

**Objetivo:** Entender os fundamentos de filas de processamento

- Como criar e usar a fila
- Enfileirar itens para processamento
- Processar itens em paralelo
- Entender ordem de conclusão vs ordem de entrada

##### 2️⃣ Exemplo Avançado

**Objetivo:** Processar grande volume com estatísticas

- Processamento em larga escala (50 notas)
- Coleta de estatísticas em tempo real
- Cálculo de métricas (taxa de sucesso, tempo médio)
- Operações atômicas para valores monetários

##### 3️⃣ Exemplo Mundo Real

**Objetivo:** Aplicar em cenário realista de e-commerce

- Simulação de sistema de emissão de notas fiscais
- Tratamento de falhas e timeouts
- Relatório de reconciliação
- Cálculo de throughput

##### Pré-requisitos

- .NET 9 SDK instalado

##### Channel<T> - Filas Thread-Safe

`System.Threading.Channels` fornece uma forma moderna e eficiente de implementar filas:

```csharp
// Cria canal ilimitado (unbounded)
var channel = Channel.CreateUnbounded<Job>(new UnboundedChannelOptions
{
    SingleReader = false,  // Múltiplos workers podem ler
    SingleWriter = false   // Múltiplos produtores podem escrever
});
```

##### Workers Paralelos

Múltiplas tarefas processam a fila simultaneamente:

```csharp
// Inicia 4 workers em paralelo
var workers = Enumerable.Range(0, 4)
    .Select(id => Task.Run(() => ProcessQueueAsync(id)))
    .ToList();
```

##### Operações Atômicas com Interlocked

Garantem thread-safety sem locks pesados:

```csharp
// Incrementa contador de forma atômica
long order = Interlocked.Increment(ref _completionSequence);

// Soma valores de forma atômica
Interlocked.Add(ref _totalAmountCents, invoice.AmountCents);
```

##### Padrão Producer-Consumer

```csharp
// Producer: adiciona itens à fila
await queue.EnqueueAsync(new Invoice(...));

// Consumer: workers processam itens
await foreach (Job job in _channel.Reader.ReadAllAsync())
{
    // Processa job...
}
```

##### Exemplo Básico (2 Workers)

```
🚀 Fila iniciada com 2 workers paralelos
📝 Adicionando 5 notas à fila...

  ➕ Enfileirada: NF #001 - R$ 150,00
  ➕ Enfileirada: NF #002 - R$ 85,00
  ...

⏳ Processando...

    🔄 [Worker 0] Processando NF #001...
    🔄 [Worker 1] Processando NF #002...
    ✅ SUCESSO | Ordem #01 | NF 002 - R$ 85,00 | 305ms
    ✅ SUCESSO | Ordem #02 | NF 004 - R$ 60,00 | 198ms
    ...
```

##### Exemplo Avançado (50 Notas, 6 Workers)

```
╔═══════════════════════════════════════════════════════════════╗
║           📊 ESTATÍSTICAS DE PROCESSAMENTO                   ║
╠═══════════════════════════════════════════════════════════════╣
║ Total Processado:          50 notas                          ║
║ ✅ Sucessos:                 42 (84.0%)                       ║
║ ❌ Falhas:                    8 (16.0%)                       ║
║                                                               ║
║ 💰 Valor Total:         R$    23.456,78                      ║
║ ⏱️  Tempo Médio:              435 ms                          ║
╚═══════════════════════════════════════════════════════════════╝

⏱️  Tempo total: 4.23s
```

##### Modelos de Domínio

**Invoice** - Representa uma nota fiscal
```csharp
public readonly record struct Invoice(
    int Id,                     // Identificador único
    TimeSpan SimulatedWork,     // Tempo de processamento
    double SuccessProbability,  // Probabilidade de sucesso (0.0 a 1.0)
    long AmountCents            // Valor em centavos
);
```

**InvoiceResult** - Resultado do processamento
```csharp
public readonly record struct InvoiceResult(
    long CompletionOrder,       // Ordem de conclusão
    bool Success,               // Sucesso ou falha
    TimeSpan Elapsed,           // Tempo decorrido
    DateTimeOffset FinishedAt   // Timestamp de conclusão
);
```

**QueueStatistics** - Estatísticas de processamento
```csharp
public class QueueStatistics
{
    public long TotalProcessed { get; }
    public long SuccessCount { get; }
    public decimal TotalAmountReais { get; }
    public double SuccessRate { get; }
    public double AvgTimeMs { get; }
}
```

##### Serviço de Fila

**InvoiceQueue** - Gerenciador da fila com workers
```csharp
public sealed class InvoiceQueue : IAsyncDisposable
{
    public event Action<Invoice, InvoiceResult>? InvoiceProcessed;
    
    public ValueTask EnqueueAsync(Invoice invoice);
    public Task DrainAsync();
}
```

##### Casos de Uso Reais

Este padrão pode ser aplicado em:

##### E-Commerce

- Emissão de notas fiscais em lote
- Processamento de pedidos em alta demanda
- Envio de e-mails de confirmação

##### Sistemas Financeiros

- Processamento de transações
- Conciliação bancária
- Cálculo de juros e multas

##### Integração de Sistemas

- Sincronização de dados entre sistemas
- Processamento de webhooks
- ETL (Extract, Transform, Load)

##### Processamento de Arquivos

- Upload em lote de documentos
- Geração de relatórios
- Conversão de formatos

##### Notificações

- E-mails em massa
- SMS/Push notifications
- Alertas de sistema

##### Por que usar Channels em vez de Queue<T>?

| Aspecto | Queue<T> | Channel<T> |
|---------|----------|------------|
| **Thread-Safety** | Requer lock manual | Built-in thread-safe |
| **Async/Await** | Não suporta | Suporte nativo |
| **Backpressure** | Não tem | Bounded channels |
| **Performance** | Locks pesados | Lock-free algorithms |

##### Por que usar Interlocked?

```csharp
// ❌ NÃO thread-safe
_total = _total + 1;

// ✅ Thread-safe com Interlocked
Interlocked.Increment(ref _total);

// ❌ NÃO thread-safe
_amount = _amount + invoice.Amount;

// ✅ Thread-safe com Interlocked
Interlocked.Add(ref _amount, invoice.Amount);
```

##### Por que valores em centavos?

```csharp
// ❌ Float/Double: imprecisão
double total = 0.1 + 0.2; // 0.30000000000000004

// ✅ Inteiros (centavos): precisão total
long totalCents = 10 + 20; // 30 centavos = R$ 0,30
```

##### Próximos Passos para Produção

Para usar em produção, considere adicionar:

1. **Retry Mechanism** - Tentativas automáticas em caso de falha
2. **Dead Letter Queue** - Fila separada para falhas persistentes
3. **Circuit Breaker** - Proteção contra serviços indisponíveis
4. **Rate Limiting** - Controle de taxa de processamento
5. **Persistent Queue** - Persistir estado em banco de dados
6. **Logging Estruturado** - Serilog, NLog, etc
7. **Telemetria** - OpenTelemetry, Application Insights
8. **Health Checks** - Monitoramento de saúde da fila
9. **Graceful Shutdown** - Finalização segura dos workers
10. **Bounded Channels** - Limitar tamanho da fila

##### Contribuindo

Este é um projeto educacional. Sugestões de melhorias são bem-vindas!

##### Licença

Este projeto faz parte do repositório **CSharp-101** e segue a mesma licença.

💡 **Dica**: Execute os exemplos na sequência (1 → 2 → 3) para melhor compreensão dos conceitos!

## Referências

- [System.Threading.Channels Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels)
- [An Introduction to System.Threading.Channels](https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/)
- [Interlocked Class](https://docs.microsoft.com/en-us/dotnet/api/system.threading.interlocked)
- [Producer-Consumer Pattern](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library)
