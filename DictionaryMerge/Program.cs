using System;
using System.Collections.Generic;

namespace DictionaryMerge;

// Exemplo de uso
public class Program
{
    public static void Main()
    {
        NotaFiscalSyncManager manager = new NotaFiscalSyncManager();
        
        // Estado inicial local
        manager.SetStatus("NF001", "Processando");
        manager.SetStatus("NF002", "Aprovada");
        manager.SetStatus("NF003", "Rejeitada");
        manager.SetStatus("NF004", "Processando");
        
        Console.WriteLine("=== Estado Inicial Local ===");
        Console.WriteLine($"Total local: {manager.TotalLocal} NFs\n");
        
        // Sistema remoto retorna apenas NFs "Aprovadas"
        Dictionary<string, string> remoteApproved = new Dictionary<string, string>
        {
            { "NF002", "Aprovada" },  // Já estava aprovada
            { "NF004", "Aprovada" },  // Mudou de Processando -> Aprovada
            { "NF005", "Aprovada" }   // Nova NF aprovada
        };
        
        Console.WriteLine("=== Sincronizando com Sistema Remoto ===");
        SyncResult syncResult = manager.Sync(remoteApproved);
        
        Console.WriteLine($"\n📊 Resultado da Sincronização:");
        Console.WriteLine($"   • Precisa atualizar: {syncResult.NeedsUpdate.Count}");
        foreach (string nf in syncResult.NeedsUpdate)
            Console.WriteLine($"     - {nf}");
            
        Console.WriteLine($"   • Novas no remoto: {syncResult.NewInRemote.Count}");
        foreach (string nf in syncResult.NewInRemote)
            Console.WriteLine($"     - {nf}");
            
        Console.WriteLine($"   • Não existem no remoto: {syncResult.NotInRemote.Count}");
        foreach (string nf in syncResult.NotInRemote)
            Console.WriteLine($"     - {nf}");
            
        Console.WriteLine($"   • Já sincronizadas: {syncResult.AlreadySync.Count}");
        
        // Aplica as mudanças
        Console.WriteLine("\n=== Aplicando Atualizações ===");
        manager.ApplyUpdates(remoteApproved, syncResult);
        
        // Verifica status final
        Console.WriteLine("\n=== Estado Final ===");
        manager.TryGetStatus("NF002", out string? status2);
        Console.WriteLine($"NF002: {status2}");
        
        manager.TryGetStatus("NF004", out string? status4);
        Console.WriteLine($"NF004: {status4}");
        
        manager.TryGetStatus("NF005", out string? status5);
        Console.WriteLine($"NF005: {status5}");
        
        Console.WriteLine($"\nTotal local após sync: {manager.TotalLocal} NFs");
    }
}