using System;
using System.Collections.Generic;

namespace DictionaryMerge;

// Sistema de sincronização de status de notas fiscais
public class NotaFiscalSyncManager
{
    private readonly Dictionary<string, string> _localStatus = new Dictionary<string, string>();
    private readonly object _lock = new object();
    
    // Sincroniza com o sistema remoto (O(n) mas eficiente)
    public SyncResult Sync(Dictionary<string, string> remoteStatus, string expectedStatus = "Aprovada")
    {
        lock (_lock)
        {
            SyncResult result = new SyncResult();
            
            // 1. Verifica NFs que existem no remoto
            foreach (var remote in remoteStatus)
            {
                string chave = remote.Key;
                string statusRemoto = remote.Value;
                
                if (_localStatus.TryGetValue(chave, out string? statusLocal))
                {
                    // Existe local - verifica se precisa atualizar
                    if (statusLocal != statusRemoto)
                    {
                        result.NeedsUpdate.Add(chave);
                    }
                    else
                    {
                        result.AlreadySync.Add(chave);
                    }
                }
                else
                {
                    // Nova NF vinda do remoto
                    result.NewInRemote.Add(chave);
                }
            }
            
            // 2. Verifica NFs que existem local mas não no remoto
            foreach (string chave in _localStatus.Keys)
            {
                if (!remoteStatus.ContainsKey(chave))
                {
                    result.NotInRemote.Add(chave);
                }
            }
            
            result.TotalProcessed = remoteStatus.Count + _localStatus.Count;
            return result;
        }
    }
    
    // Aplica as atualizações identificadas
    public void ApplyUpdates(Dictionary<string, string> remoteStatus, SyncResult syncResult)
    {
        lock (_lock)
        {
            // Atualiza NFs que mudaram
            foreach (string chave in syncResult.NeedsUpdate)
            {
                if (remoteStatus.TryGetValue(chave, out string? novoStatus))
                {
                    _localStatus[chave] = novoStatus;
                    Console.WriteLine($"✓ Atualizado: {chave} -> {novoStatus}");
                }
            }
            
            // Adiciona novas NFs
            foreach (string chave in syncResult.NewInRemote)
            {
                if (remoteStatus.TryGetValue(chave, out string? status))
                {
                    _localStatus[chave] = status;
                    Console.WriteLine($"+ Adicionado: {chave} -> {status}");
                }
            }
        }
    }
    
    // Busca O(1)
    public bool TryGetStatus(string chaveNF, out string? status)
    {
        lock (_lock)
        {
            return _localStatus.TryGetValue(chaveNF, out status);
        }
    }
    
    // Adiciona/Atualiza status local
    public void SetStatus(string chaveNF, string status)
    {
        lock (_lock)
        {
            _localStatus[chaveNF] = status;
        }
    }
    
    // Estatísticas
    public int TotalLocal => _localStatus.Count;
}
