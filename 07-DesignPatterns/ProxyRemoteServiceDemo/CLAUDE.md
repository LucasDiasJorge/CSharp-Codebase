# CLAUDE.md — ProxyRemoteServiceDemo

Console com três proxies (virtual, de proteção e de telemetria) sobre o mesmo contrato, sem alteração no cliente. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/ProxyRemoteServiceDemo/ProxyRemoteServiceDemo.csproj
dotnet run --project 07-DesignPatterns/ProxyRemoteServiceDemo/ProxyRemoteServiceDemo.csproj
```

Roda cinco cenários e termina. **Leva ~5 segundos**, quase tudo em conexões simuladas de 600ms. Sem serviço externo.

## Estrutura interna

`Demo/ProxyDemoRunner.RunClientAsync` é **o ponto do projeto**: recebe `IReportService`, chama dois métodos, e é chamado identicamente nos cinco cenários. Se alguém precisar adaptá-lo para algum proxy, o padrão foi quebrado.

`Remote/RemoteReportService` tem `Thread.Sleep(600)` **no construtor** — é o custo que o proxy virtual evita. O contador estático `InstancesCreated` é o que torna o adiamento mensurável; `ResetCounter` é chamado no início de cada cenário.

`Proxies/LazyConnectionProxy.EnsureCreatedAsync` faz **dupla checagem** dentro do semáforo. Sem ela, duas chamadas simultâneas criariam duas conexões — o custo pago em dobro, exatamente o que o proxy existe para evitar. Mesmo padrão do single-flight em `CacheStampedeProtectionDemo`.

`Proxies/TelemetryProxy.MeasureAsync` registra no `finally`, incluindo chamadas que falham.

## Pontos de atenção

- TFM `net10.0`. Pacote: `Microsoft.Extensions.Logging.Console` 10.0.12.
- **A ordem da cadeia no cenário 5 é deliberada**: `Authorization → Telemetry → Lazy → Real`. Por isso a telemetria mede 772ms na primeira chamada (inclui os 600ms da conexão) e 155ms na segunda. O README explica essa diferença; reordenar a cadeia muda os números e invalida o texto.
- Os tempos citados no README (632ms, 772ms, 155ms) variam um pouco entre máquinas. O que precisa continuar valendo: cenário 2 com **0 instâncias**, cenário 3 com **1 instância após duas rodadas**, cenário 4 com **0 instâncias após a negativa**, e a primeira medição da cadeia claramente maior que a segunda.
- `RemoteReportService` usa `Thread.Sleep` no construtor (não `Task.Delay`) porque construtor não é async. É intencional e o comentário diz isso.
- O contador de instâncias é **estático**. Cada cenário chama `ResetCounter` antes; ao acrescentar um cenário novo, fazer o mesmo, ou os números ficam cumulativos e sem sentido.
- `AuthorizationProxy` lança `UnauthorizedAccessException`. O cenário 4 captura; um cenário novo que não capture derruba a execução inteira.
- **Proxy de cache não está implementado, de propósito.** É o caso mais comum em produção, mas expiração, invalidação e concorrência são o assunto da trilha `06-Caching`, com a profundidade que merecem. Está citado no README como fronteira; não implementar aqui.
- **Fronteira com os vizinhos**: `DesignPattern/Structural` tem implementações introdutórias de padrões estruturais; `PortsAndAdapters` trata de inversão de dependência na fronteira, que é outro problema. Aqui o foco é controle de acesso ao objeto e a transparência para o cliente.
