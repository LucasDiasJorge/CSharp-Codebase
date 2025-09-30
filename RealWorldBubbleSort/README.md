<!-- README padronizado (versão condensada) -->
# RealWorldBubbleSort (Na verdade: Ordenação + Busca Binária)

Exemplo prático: consumir usuários de API pública (`randomuser.me`), ordenar por `Username` e executar busca binária interativa. Nome histórico mantido, mas não há Bubble Sort (usa sort nativo eficiente do .NET).

## 1. Fluxo
| Etapa | Ação |
|-------|------|
| 1 | Requisição HTTP para API pública |
| 2 | Mapear JSON -> objetos `User` |
| 3 | Ordenar lista por Username (case-insensitive) |
| 4 | Solicitar input e realizar busca binária |

## 2. Execução
```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\RealWorldBubbleSort"
dotnet run
```

## 3. Considerações
- Resultados aleatórios: usuários mudam a cada execução.
- Ordenação antes da busca é obrigatória (pré-condição da busca binária).
- `HttpClient` direto: em cenário real usar `IHttpClientFactory`.
- Falhas de rede não tratadas extensivamente (exercício para expansão).

## 4. Possíveis Extensões
- Retry + timeout + circuit breaker.
- Testes unitários (ordenação, busca encontrada / não encontrada).
- Argumentos CLI: quantidade de usuários, filtro inicial.
- Paginação e streaming para grandes volumes.

## 5. Aprendizado Chave
Combinação de: acesso HTTP → transformação → ordenação → algoritmo de busca → interação usuário.

---
Versão condensada substitui descrição longa original.
