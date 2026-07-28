using System;
using Microsoft.Win32.TaskScheduler;

namespace DailyTaskRunner;

class Program
{
    private const string NomeDaTarefa = "MinhaRotinaCSharp";

    static void Main(string[] args)
    {
        // 1. Verifica se o programa foi chamado pelo Agendador de Tarefas com o argumento
        if (args.Length > 0 && args[0] == "--executar-job")
        {
            ExecutarJobDiario();
            return;
        }

        // 2. Se for executado sem argumentos (ex: clique duplo do usuário),
        // ele apenas registra a tarefa no Agendador de Tarefas do Windows
        Console.WriteLine("Registrando a tarefa no Agendador do Windows...");
        RegistrarAgendamento();
    }

    /// <summary>
    /// Esta é a função/job real que deve rodar diariamente às 18:01
    /// </summary>
    private static void ExecutarJobDiario()
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Rodando o job diário...");

        try
        {
            // === COLOQUE A LÓGICA DA SUA FUNÇÃO AQUI ===
            MinhaFuncaoDeNegocio();

            Console.WriteLine("Job concluído com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro durante a execução do Job: {ex.Message}");
        }
    }

    private static void MinhaFuncaoDeNegocio()
    {
        // Exemplo: Buscar dados no banco, enviar e-mail, etc.
        Console.WriteLine("Processando lógica de negócio...");
    }

    /// <summary>
    /// Cria o agendamento no Windows configurado para passar o parâmetro --executar-job
    /// </summary>
    private static void RegistrarAgendamento()
    {
        using TaskService ts = new TaskService();

        TaskDefinition td = ts.NewTask();
        td.RegistrationInfo.Description = "Executa o job do C# diariamente às 18:01";

        // Dispara diariamente às 18:01
        td.Triggers.Add(new DailyTrigger { StartBoundary = DateTime.Today.AddHours(18).AddMinutes(1) });

        // Caminho do .exe atual
        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;

        // O SEGREDO ESTÁ AQUI: O segundo parâmetro do ExecAction passa os argumentos de linha de comando
        td.Actions.Add(new ExecAction(exePath, "--executar-job"));

        // Registra a tarefa
        ts.RootFolder.RegisterTaskDefinition(NomeDaTarefa, td);

        Console.WriteLine($"Tarefa '{NomeDaTarefa}' registrada com sucesso para rodar às 18:01!");
    }
}