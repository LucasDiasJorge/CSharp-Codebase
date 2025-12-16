using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CommonProblems.PoisonLooping
{
    public static class PoisonLoopingExamples
    {
        // 1) Exemplo de loop infinito (NÃO EXECUTAR)
        public static void InfiniteCpuLoop_Bad()
        {
            while (true)
            {
                // CPU 100%, sem pausa e sem condição
            }
        }

        // Mitigation: exit condition and pause
        public static void BoundedCpuLoop_Good(int maxIterations = 1000)
        {
            for (int i = 0; i < maxIterations; i++)
            {
                // Trabalho útil
                Thread.Sleep(1); // Cooperative pause
            }
        }

        // 2) Memory-exhaustion loop (DO NOT RUN)
        public static void MemoryExhaustionLoop_Bad()
        {
            var list = new List<byte[]>();
            while (true)
            {
                list.Add(new byte[1024 * 1024]); // +1MB per iteration
            }
        }

        // Mitigation: limits and release
        public static void MemoryLoop_Good(int count = 10)
        {
            var list = new List<byte[]>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new byte[1024 * 1024]);
            }
            list.Clear();
        }

        // 3) Logic bug: condition never changes
        public static void ConditionNeverChanges_Bad()
        {
            int x = 0;
            while (x < 10)
            {
                // x não atualiza
            }
        }

        public static void ConditionChanges_Good()
        {
            int x = 0;
            while (x < 10)
            {
                x++;
            }
        }

        // 4) Busy-wait without pause
        public static void BusyWait_Bad(Func<bool> condition)
        {
            while (!condition())
            {
                // gira sem pausa
            }
        }

        public static async Task BusyWait_GoodAsync(Func<bool> condition)
        {
            while (!condition())
            {
                await Task.Delay(10);
            }
        }

        // 5) Padrão seguro: processamento assíncrono com limites e pausa
        public static async Task ProcessQueueAsync(ChannelReader<string> reader, CancellationToken ct)
        {
            int processed = 0;
            var maxPerRun = 10_000;

            while (!ct.IsCancellationRequested && processed < maxPerRun)
            {
                while (await reader.WaitToReadAsync(ct))
                {
                    while (reader.TryRead(out var item))
                    {
                        // processar item
                        processed++;
                        if (processed % 1000 == 0)
                        {
                            await Task.Delay(1, ct);
                        }
                    }
                }
            }
        }

        // 6) Simple instrumentation
        public static void InstrumentedLoop()
        {
            var sw = Stopwatch.StartNew();
            int iterations = 0;

            while (iterations < 10_000)
            {
                iterations++;
                if (iterations % 1000 == 0)
                {
                    Console.WriteLine($"Iterations: {iterations}, elapsed: {sw.Elapsed}");
                    Thread.Sleep(1);
                }
            }
        }
    }
}
