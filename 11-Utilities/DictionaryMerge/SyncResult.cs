using System.Collections.Generic;

namespace DictionaryMerge;

// Resultado da sincronização com detalhes das diferenças
public class SyncResult
{
    public HashSet<string> NeedsUpdate { get; set; } = new HashSet<string>();      // NFs que precisam ser atualizadas localmente
    public HashSet<string> NotInRemote { get; set; } = new HashSet<string>();      // NFs que existem local mas não no remoto
    public HashSet<string> NewInRemote { get; set; } = new HashSet<string>();      // NFs novas que vieram do remoto
    public HashSet<string> AlreadySync { get; set; } = new HashSet<string>();      // NFs já sincronizadas
    public int TotalProcessed { get; set; } = 0;
}
