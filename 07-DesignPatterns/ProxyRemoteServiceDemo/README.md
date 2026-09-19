# ProxyRemoteServiceDemo

Console que controla o acesso a um serviço remoto caro usando proxies — carregamento tardio, autorização e telemetria — sem alterar uma linha do código do cliente.

## Visão geral

O Proxy é um objeto que fica no lugar de outro e implementa a mesma interface. Como o cliente só conhece a interface, ele não tem como distinguir um do outro — e é exatamente isso que permite acrescentar comportamento em volta do serviço sem tocar nem no serviço nem em quem o chama.

O exemplo tem um serviço remoto deliberadamente caro: 600ms para estabelecer a conexão no construtor e 150ms por chamada. Em cima dele, três proxies com propósitos diferentes. O **virtual** adia a criação até a primeira chamada real: se o serviço nunca for usado, a conexão nunca é aberta. O de **proteção** verifica permissão antes de repassar, e barra tão cedo que o serviço real sequer chega a ser criado. O de **telemetria** mede e registra cada chamada.

O fio condutor é o método do cliente. Ele recebe `IReportService`, chama dois métodos e imprime o resultado — e é literalmente o mesmo código nos cinco cenários, do acesso direto à cadeia com três proxies encadeados. Nenhum deles exigiu adaptação no cliente.

Um detalhe que a execução revela: na cadeia, a telemetria registra 772ms na primeira chamada e 155ms na segunda. A diferença é o custo da conexão, que o proxy virtual pagou por dentro — a ordem dos proxies decide a quem o custo é atribuído.

## Conceitos abordados

- Proxy implementando a mesma interface do serviço real.
- Proxy virtual e carregamento tardio de recurso caro.
- Dupla checagem ao criar o objeto adiado sob concorrência.
- Proxy de proteção barrando a chamada antes de qualquer custo.
- Proxy de telemetria medindo inclusive as chamadas que falham.
- Encadeamento de proxies e o efeito da ordem.
- Diferença entre Proxy e Decorator.
- Transparência para o cliente como critério de sucesso do padrão.

## Objetivos de aprendizagem

- Acrescentar comportamento em torno de um serviço sem modificá-lo nem ao cliente.
- Escolher o tipo de proxy conforme o problema: custo, acesso ou observabilidade.
- Ordenar uma cadeia de proxies de propósito, não por acaso.
- Reconhecer o custo do padrão: indireção que o cliente não vê nem na depuração.

## Estrutura do projeto

```text
ProxyRemoteServiceDemo/
|-- Contracts/
|   |-- CallerContext.cs
|   `-- IReportService.cs
|-- Demo/
|   `-- ProxyDemoRunner.cs
|-- Proxies/
|   |-- AuthorizationProxy.cs
|   |-- LazyConnectionProxy.cs
|   `-- TelemetryProxy.cs
|-- Remote/
|   `-- RemoteReportService.cs
|-- Program.cs
|-- ProxyRemoteServiceDemo.csproj
`-- README.md
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/ProxyRemoteServiceDemo/ProxyRemoteServiceDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 07-DesignPatterns/ProxyRemoteServiceDemo/ProxyRemoteServiceDemo.csproj
```

Não exige serviço externo. A execução leva cerca de 5 segundos — quase tudo são as conexões simuladas de 600ms.

## Boas práticas e pontos de atenção

- O proxy precisa implementar a **mesma** interface do serviço real. Acrescentar um método que só o proxy tem obriga o cliente a saber com quem está falando, e o padrão deixa de valer.
- Faça dupla checagem ao criar o objeto adiado. Sem ela, duas chamadas simultâneas criam duas conexões — e o custo que o proxy virtual existia para evitar é pago em dobro.
- Barre cedo no proxy de proteção. Verificar permissão antes de repassar significa que uma chamada negada não abre conexão, não consome cota e não aparece na latência do serviço real.
- Meça no `finally`, não só no caminho feliz. A chamada que falha é justamente a mais interessante de instrumentar.
- A ordem da cadeia é uma decisão. Autorização por fora barra antes de medir e antes de conectar; telemetria por fora mediria inclusive as chamadas rejeitadas. Nenhuma das duas é errada — mas uma delas é a que você quis.
- Proxy e Decorator têm a mesma estrutura e intenções diferentes. Proxy **controla o acesso** ao objeto (quando criar, se pode chamar, o que registrar); Decorator **acrescenta comportamento** ao resultado. Quando a dúvida aparece, o nome importa menos que a clareza da intenção.
- O padrão tem um custo real na depuração: o cliente parece chamar o serviço e está chamando outra coisa. Uma pilha de chamadas com quatro proxies fica difícil de ler, e um comportamento inesperado pode vir de qualquer camada.
- Não empilhe proxies sem necessidade. Cada camada é uma indireção a mais; três já é bastante.
- Cache também é um proxy, e é o caso mais comum em produção — coberto na trilha `06-Caching`, que trata de expiração, invalidação e concorrência com a profundidade que o assunto exige.

## Conteúdo complementar

Os três proxies:

| Proxy | Tipo | O que controla |
|---|---|---|
| `LazyConnectionProxy` | Virtual | Quando o serviço real é criado |
| `AuthorizationProxy` | Proteção | Se a chamada pode acontecer |
| `TelemetryProxy` | Logging | O que é registrado sobre cada chamada |

Resultados observados:

**1 e 2. Custo da criação**

```text
sem proxy:              servico construido em 632ms, antes de qualquer uso
proxy virtual sem uso:  proxy construido em 0ms, instancias reais criadas: 0
```

**3. Criação adiada e reaproveitada**

```text
primeira chamada -> conexao estabelecida (600ms)
segunda rodada   -> nenhuma conexao nova
apos duas rodadas: 1 instancia real criada
```

**4. Bloqueio antes do custo**

```text
visitante sem a permissao reports:read: chamada bloqueada antes de sair
instancias reais criadas apos a tentativa negada: 0
```

A conexão nunca foi aberta: o proxy de proteção rejeitou a chamada antes de o proxy virtual precisar criar qualquer coisa.

**5. Cadeia de três, e o efeito da ordem**

```text
CountAvailableAsync: 772ms, sucesso=True
GenerateAsync:       155ms, sucesso=True
```

A primeira chamada custa 772ms porque a telemetria está **por fora** do proxy virtual e acaba medindo também os 600ms da conexão. A segunda custa 155ms, que é o custo real da chamada remota. Se a telemetria estivesse por dentro do lazy, os 600ms não apareceriam em nenhuma medição — e ninguém saberia que existiram.

Cadeia montada no cenário 5:

```text
cliente -> AuthorizationProxy -> TelemetryProxy -> LazyConnectionProxy -> RemoteReportService
           (barra cedo)          (mede)            (adia a criacao)       (caro)
```

Proxy comparado com padrões vizinhos:

| Padrão | Mesma interface? | Intenção |
|---|---|---|
| Proxy | Sim | Controlar o acesso ao objeto |
| Decorator | Sim | Acrescentar comportamento ao resultado |
| Adapter | Não — converte | Compatibilizar interfaces incompatíveis |
| Facade | Não — simplifica | Oferecer uma porta única para um subsistema |

Relação com os vizinhos da trilha: `DesignPattern/Structural` reúne implementações introdutórias de padrões estruturais. `PortsAndAdapters` trata de inversão de dependência na fronteira da aplicação, que é outro problema. Proxy de cache, o caso mais comum em produção, é assunto da trilha `06-Caching`.

## Referências e documentação complementar

- https://refactoring.guru/design-patterns/proxy
- https://learn.microsoft.com/dotnet/api/system.lazy-1
- https://en.wikipedia.org/wiki/Proxy_pattern
