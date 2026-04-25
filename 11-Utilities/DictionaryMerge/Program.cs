using System.Collections.Concurrent;

namespace DictionaryMerge;

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

        // Sistema remoto retorna apenas NFs "Aprovadas"
        ConcurrentDictionary<string, string> remoteApproved = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        remoteApproved["NF002"] = "Aprovada";
        remoteApproved["NF004"] = "Aprovada";
        remoteApproved["NF005"] = "Aprovada";

        // Sincroniza e aplica
        SyncResult syncResult = manager.Sync(remoteApproved);
        manager.ApplyUpdates(remoteApproved, syncResult);
    }
}