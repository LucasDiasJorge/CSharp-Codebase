# InvoiceQueueDemo

Este exemplo demonstra uma fila de processamento concorrente de emissão de notas fiscais (invoices) em C# utilizando `Channel<T>` e operações atômicas (`Interlocked`). Cada nota tem um valor (representado em *cents* para segurança nas operações inteiras) e a soma total só é incrementada quando a emissão é bem-sucedida.

## Como funciona

- As notas são enfileiradas de forma assíncrona.
- M múltiplos workers (tarefas) consomem a fila em paralelo.
- Ao finalizar o processamento de uma nota, um identificador sequencial de conclusão é gerado usando `Interlocked.Increment` (evita race conditions na ordem de conclusão).
- Se a emissão for bem-sucedida, o valor da nota (em cents) é somado ao total usando `Interlocked.Add` para garantir atomicidade; em caso de falha, o valor não é adicionado.

## Executando

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\JobQueueDemo"
dotnet run
```

A saída exibirá, em ordem de conclusão, o status (sucesso/falha), o identificador da nota, o tempo gasto, o valor da nota e a chance de sucesso atribuída. No final o total acumulado das notas emitidas com sucesso será exibido.

## Pontos de destaque

- Utilização de `System.Threading.Channels` para criar uma fila eficiente e segura.
- Contador de conclusão protegido por `Interlocked.Increment` para ordenar conclusões sem condições de corrida.
- Soma atômica (`Interlocked.Add`) do valor das notas (em cents) apenas quando a emissão é bem-sucedida.
- Simulação de tempos de processamento e probabilidades de sucesso para cada nota.
- Código simples e pronto para ser adaptado para integrações reais com gateways de emissão.
