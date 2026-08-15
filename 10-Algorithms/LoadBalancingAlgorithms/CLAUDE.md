# CLAUDE.md — LoadBalancingAlgorithms

Console: quatro estratégias clássicas de balanceamento, comparadas. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/LoadBalancingAlgorithms/LoadBalancingAlgorithms.csproj
dotnet run --project 10-Algorithms/LoadBalancingAlgorithms/LoadBalancingAlgorithms.csproj
```

## Estrutura interna

- `Algorithms/ILoadBalancer.cs` — contrato comum, o que permite rodar as quatro sobre a mesma carga.
- `RoundRobinLoadBalancer` — rodízio simples; ignora capacidade e carga.
- `WeightedRoundRobinLoadBalancer` — rodízio proporcional ao peso do servidor.
- `LeastConnectionsLoadBalancer` — escolhe o menos ocupado; o único que reage ao estado real.
- `IpHashLoadBalancer` — hash do IP, garantindo **afinidade de sessão**: o mesmo cliente sempre cai no mesmo servidor.
- `Models/Server.cs`, `Models/Request.cs` — a simulação.

A distribuição impressa ao final é o que torna as diferenças visíveis.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas — simulação em memória, nenhum servidor real.
- Ao adicionar estratégia, implemente `ILoadBalancer` e a inclua na comparação; um algoritmo fora da bateria não gera o contraste que é o produto do projeto.
