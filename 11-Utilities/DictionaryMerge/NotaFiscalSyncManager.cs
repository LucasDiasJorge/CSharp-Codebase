using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DictionaryMerge;

public class NotaFiscalSyncManager
{
    private readonly ConcurrentDictionary<string, string> _localStatus = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    // Sincroniza com o sistema remoto — O(R + T)
    public SyncResult Sync(ConcurrentDictionary<string, string> remoteStatus)
    {
        SyncResult result = new SyncResult();

        foreach (KeyValuePair<string, string> remote in remoteStatus)
        {
            string chave = remote.Key;
            string statusRemoto = remote.Value;

            if (_localStatus.TryGetValue(chave, out string? statusLocal))
            {
                if (!string.Equals(statusLocal, statusRemoto, StringComparison.OrdinalIgnoreCase))
                    result.NeedsUpdate.Add(chave);
                else
                    result.AlreadySync.Add(chave);
            }
            else
            {
                result.NewInRemote.Add(chave);
            }
        }

        foreach (string chave in _localStatus.Keys)
        {
            if (!remoteStatus.ContainsKey(chave))
                result.NotInRemote.Add(chave);
        }

        result.TotalProcessed = remoteStatus.Count + _localStatus.Count;
        return result;
    }

    // Aplica as atualizações identificadas
    public void ApplyUpdates(ConcurrentDictionary<string, string> remoteStatus, SyncResult syncResult)
    {
        foreach (string chave in syncResult.NeedsUpdate)
        {
            if (remoteStatus.TryGetValue(chave, out string? novoStatus))
                _localStatus[chave] = novoStatus;
        }

        foreach (string chave in syncResult.NewInRemote)
        {
            if (remoteStatus.TryGetValue(chave, out string? status))
                _localStatus[chave] = status;
        }
    }

    // Busca O(1)
    public bool TryGetStatus(string chaveNF, out string? status)
    {
        return _localStatus.TryGetValue(chaveNF, out status);
    }

    // Adiciona/Atualiza status local
    public void SetStatus(string chaveNF, string status)
    {
        _localStatus[chaveNF] = status;
    }

    // Estatísticas
    public int TotalLocal => _localStatus.Count;
}
