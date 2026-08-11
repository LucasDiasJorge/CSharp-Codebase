using System;
using System.Text;
using YieldReturnDemo.Demos;

namespace YieldReturnDemo;

public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        RunSection("1. O básico: escrevendo o primeiro iterador", BasicsDemo.Run);
        RunSection("2. Execução preguiçosa: quando o corpo do iterador roda", DeferredExecutionDemo.Run);
        RunSection("3. O que o compilador gera: enumerador manual vs yield return", ManualEnumeratorDemo.Run);
        RunSection("4. Sequências infinitas e yield break", InfiniteSequenceDemo.Run);
        RunSection("5. Pipeline em streaming: um item por vez", StreamingPipelineDemo.Run);
        RunSection("6. Armadilhas comuns de iteradores", PitfallsDemo.Run);

        Console.WriteLine();
        Console.WriteLine("Fim da demonstração.");
    }

    private static void RunSection(string title, Action demo)
    {
        Console.WriteLine();
        Console.WriteLine("==============================================================");
        Console.WriteLine(title);
        Console.WriteLine("==============================================================");
        demo();
    }
}
