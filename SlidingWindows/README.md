# Sliding Window Example

Este projeto demonstra o uso do algoritmo Sliding Window (Janela Deslizante) em C# para resolver problemas relacionados a subarrays em arrays de inteiros de forma eficiente. O exemplo implementado calcula a soma máxima de qualquer subarray de tamanho fixo dentro de um array.

## Descrição

O padrão Sliding Window é uma técnica eficiente para resolver problemas que envolvem arrays ou listas, especialmente quando se busca por subarrays ou subsequências de tamanho fixo ou variável. Em vez de usar loops aninhados (O(n·k)), movemos uma janela ao longo do array, atualizando incrementos e decrementos de forma incremental (O(n)).

No exemplo deste projeto, o método `MaxSumSlidingWindow` recebe um array de inteiros e um tamanho de janela fixa, retornando a soma máxima encontrada em qualquer subarray contíguo desse tamanho.

## Como funciona o algoritmo

1. Definimos dois ponteiros (início e fim) que representam a janela deslizante.  
2. Inicializamos a métrica desejada (soma, contagem, etc.) com os primeiros elementos da janela.  
3. Deslizamos a janela movendo o ponteiro de fim e atualizamos a métrica adicionando o novo elemento.  
4. Removemos a contribuição do elemento que sai da janela movendo o ponteiro de início.  
5. A cada passo, registramos o melhor resultado encontrado.

## Complexidade

- **Tempo:** O(n), pois cada elemento entra e sai da janela apenas uma vez.  
- **Espaço:** O(1), pois usamos apenas variáveis auxiliares para calcular a métrica.

## Exemplo de Uso

```csharp
using System;

namespace SlidingWindows
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] nums = { 8, 3, -2, 4, 5, -1, 0, 5, 3, 9, -6 };
            int windowSize = 5;
            int maxSum = MaxSumSlidingWindow(nums, windowSize);
            Console.WriteLine($"Max sum of subarray of length {windowSize} is: {maxSum}");
        }

        static int MaxSumSlidingWindow(int[] nums, int k)
        {
            if (nums == null || nums.Length < k)
                throw new ArgumentException("Invalid input");

            int maxSum = 0;
            for (int i = 0; i < k; i++)
                maxSum += nums[i];

            int windowSum = maxSum;
            for (int i = k; i < nums.Length; i++)
            {
                windowSum += nums[i] - nums[i - k];
                maxSum = Math.Max(maxSum, windowSum);
            }
            return maxSum;
        }
    }
}
```

## Observação

Antes de deslizar a janela, calculamos a soma inicial dos primeiros `k` elementos e atribuimos a `windowSum` e `maxSum`. Em cada passo subsequente, adicionamos o novo valor que entra na janela e subtraímos o que sai, evitando recalcular a soma completa. Assim, reduzimos a complexidade de O(n·k) para O(n).

## Exemplos de Uso no Mundo Real

- **Análise de tráfego de rede:** em sistemas de monitoramento de redes, coletamos métricas de pacotes ou bytes recebidos em intervalos de tempo fixos (por exemplo, 1 minuto) e usamos uma janela deslizante para calcular médias móveis. Isso ajuda a identificar picos incomuns ou quedas súbitas no tráfego, permitindo alertas em tempo real e diagnóstico de problemas.

- **Processamento de sinais:** em aplicações de áudio ou sensoriamento (como acelerômetros ou temperatura), filtros de média móvel são usados para suavizar ruído. Com Sliding Window, calculamos rapidamente a média dos últimos N valores, garantindo resposta em tempo real sem reprocessar todo o histórico a cada nova amostra.

- **Monitoramento de desempenho de sistemas:** para acompanhar métricas como tempo de resposta de APIs ou taxa de erros, uma janela deslizante sobre as últimas requisições (por exemplo, últimas 100 chamadas) permite calcular latência média ou proporção de falhas em tempo real. Isso facilita detectar degradações antes que afetem usuários.

- **Processamento de texto e strings:** para problemas como "maior substring sem caracteres repetidos", usamos Sliding Window para expandir e contrair uma janela sobre a string, mantendo um conjunto de caracteres válidos e atualizando o resultado máximo à medida que avançamos, tudo em O(n).

- **Análise financeira (Moving Average):** em finanças, indicadores como média móvel simples (SMA) são usados para suavizar flutuações de preço de ações e identificar tendências. Sliding Window calcula o SMA dos últimos N períodos sem recalcular a soma completa a cada atualização, essencial para processamento de grandes volumes de dados em tempo real.