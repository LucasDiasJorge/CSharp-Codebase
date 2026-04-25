using System;
using System.Threading;

class Program
{
    static void Main()
    {
        // Criando uma nova thread e associando-a a um método
        Thread thread1 = new Thread(Trabalho);
        Thread thread2 = new Thread(Trabalho);

        // Iniciando as threads
        thread1.Start();
        thread2.Start();

        // A thread principal continua executando
        Console.WriteLine("Thread principal continua executando...");

        // Esperar as threads terminarem antes de encerrar o programa
        thread1.Join();
        thread2.Join();

        Console.WriteLine("Todas as threads finalizaram.");
    }

    static void Trabalho()
    {
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: {i}");
            Thread.Sleep(500); // Simula uma tarefa demorada
        }
    }
}
