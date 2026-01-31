using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkTool.Lib
{
    public class Benchmark : IBenchmark
    {
        public async void Run(Func<Task> func)
        {
            DateTime clock = DateTime.Now;

            Console.WriteLine("Benchmark is running...");
            try
            {
                await func();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during benchmark execution: {ex.Message}");
            } finally
            {
                DateTime endClock = DateTime.Now;
                TimeSpan duration = endClock - clock;
                Console.WriteLine($"Benchmark completed in {duration} TimeSpan.");
            }
        }

    }
}
