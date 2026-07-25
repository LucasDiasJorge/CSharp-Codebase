using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        Scheduler.RegistrarAgendamento();
        Scheduler.RemoverAgendamento("MinhaRotinaCSharp");
    }
}

static class Scheduler
{
    public static void RegistrarAgendamento()
    {
        using TaskService ts = new TaskService();

        // Cria uma nova definição de tarefa
        TaskDefinition td = ts.NewTask();
        td.RegistrationInfo.Description = "Executa a rotina diária às 18:01";

        // Dispara diariamente às 18:01
        td.Triggers.Add(new DailyTrigger { StartBoundary = DateTime.Today.AddHours(18).AddMinutes(1) });

        // Ação: executar o próprio .exe atual
        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;
        td.Actions.Add(new ExecAction(exePath));

        // Registra a tarefa na pasta raiz do Task Scheduler
        ts.RootFolder.RegisterTaskDefinition(@"MinhaRotinaCSharp", td);

        Console.WriteLine("Tarefa agendada com sucesso! Ela será executada diariamente às 18:01.");
    }

    public static void RemoverAgendamento(string nomeDaTarefa)
    {
        using TaskService ts = new TaskService();

        // Verifica se a tarefa existe antes de tentar deletar
        if (ts.RootFolder.Tasks.Exists(nomeDaTarefa))
        {
            ts.RootFolder.DeleteTask(nomeDaTarefa);
            Console.WriteLine($"Tarefa '{nomeDaTarefa}' removida com sucesso!");
        }
        else
        {
            Console.WriteLine($"A tarefa '{nomeDaTarefa}' não foi encontrada.");
        }
    }
}



