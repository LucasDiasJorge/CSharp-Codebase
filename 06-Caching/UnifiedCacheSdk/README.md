# UnifiedCacheSdk

## Visão geral

O UnifiedCacheSdk é um SDK didático para padronizar acesso a cache em projetos C#, abstraindo o provedor subjacente e impondo uma convenção consistente de chaves por serviço.

No estado atual, o projeto já entrega um cliente simples, um construtor de chave, um provider para memória, um provider para Redis, integração com DI e empacotamento como biblioteca. Isso é suficiente para estudo e para cenários internos de baixa complexidade.

Ao mesmo tempo, o projeto também é um ótimo ponto de partida para discutir uma pergunta mais importante: como fazer um bom SDK Redis para reutilizar em vários projetos C# sem cair em acoplamento, fragilidade operacional ou APIs confusas.

Este README foi escrito com esse objetivo.

Ele documenta o que o SDK já faz hoje.

Ele mostra como ler a implementação existente.

Ele explica o que separar em camadas.

Ele detalha decisões avançadas de design.

Ele cobre serialização, TTL, observabilidade, resiliência, invalidação, versionamento, testes e publicação.

Ele também descreve o que normalmente falta em SDKs Redis caseiros e como evoluir isso sem perder simplicidade.

## Conceitos abordados

- Cliente de cache orientado a recurso.
- Abstração de provedor com `ICacheProvider`.
- Padronização de chaves com `ICacheKeyBuilder`.
- `MemoryCache` como fallback local.
- Redis como provedor distribuído.
- Registro em DI por extensão de `IServiceCollection`.
- `DefaultTtl`, `ServiceId`, `GlobalPrefix` e `Separator`.
- Serialização com `System.Text.Json`.
- Uso de `IConnectionMultiplexer` como singleton.
- Estratégias para tornar um SDK Redis reutilizável e seguro.
- Modelagem de API pública estável.
- Isolamento de tenant, ambiente e bounded context na chave.
- Observabilidade por métrica, log e tracing.
- Políticas de expiração e invalidação.
- Tratamento de falhas transitórias e falhas permanentes.
- Evolução de contratos sem quebrar consumidores.
- Boas práticas para empacotamento e distribuição via NuGet.
- Estratégia de testes de unidade, integração e carga.

## Objetivos de aprendizagem

- Entender a arquitetura atual do UnifiedCacheSdk.
- Identificar o papel de cada pasta e de cada contrato público.
- Aprender a desenhar um SDK Redis com API simples para consumo.
- Saber quando abstrair e quando expor a capacidade nativa do Redis.
- Escolher convenções de chave que suportem escala e multi-tenant.
- Definir TTL de forma coerente com o tipo de dado.
- Evitar armadilhas clássicas de serialização e versionamento.
- Controlar conexões, latência e falhas sem espalhar código infra nos consumidores.
- Incluir observabilidade desde o início.
- Organizar testes que protejam o contrato do SDK.
- Publicar o SDK com versão e semântica previsíveis.
- Evoluir o projeto atual rumo a um SDK Redis mais robusto para produção.

## Estrutura do projeto

```text
UnifiedCacheSdk/
+-- README.md
+-- src/
|   \-- UnifiedCacheSdk/
|       +-- Abstractions/
|       |   +-- ICacheKeyBuilder.cs
|       |   \-- ICacheProvider.cs
|       +-- Core/
|       |   \-- CacheKeyBuilder.cs
|       +-- Extensions/
|       |   \-- ServiceCollectionExtensions.cs
|       +-- Options/
|       |   \-- UnifiedCacheOptions.cs
|       +-- Providers/
|       |   +-- MemoryCacheProvider.cs
|       |   \-- RedisCacheProvider.cs
|       +-- UnifiedCacheClient.cs
|       \-- UnifiedCacheSdk.csproj
\-- UnifiedCacheSdk/
    \-- src/
```

Observação importante:

- A implementação principal está em `src/UnifiedCacheSdk`.
- A pasta `UnifiedCacheSdk/src` externa pode ser tratada como estrutura auxiliar do workspace.
- Ao documentar e consumir o SDK, use como referência principal a árvore sob `src/UnifiedCacheSdk`.

## Como executar

Build da biblioteca:

```bash
dotnet build 06-Caching/UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj
```

Empacotamento local:

```bash
dotnet pack 06-Caching/UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj -c Release
```

Se quiser apenas validar o pacote gerado no diretório padrão:

```bash
dotnet build 06-Caching/UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj -c Release
```

## Boas práticas e pontos de atenção

- Use `ServiceId` único por aplicação ou por bounded context.
- Não compartilhe a mesma convenção de chave entre serviços sem namespace explícito.
- Defina TTL por caso de uso, não por conveniência genérica.
- Evite usar cache como fonte primária de verdade.
- Trate Redis como infraestrutura falível.
- Não acople regras de negócio ao provider concreto.
- Não esconda comportamento caro por trás de nomes aparentemente simples.
- Padronize serialização e versionamento de payloads desde o início.
- Registre métricas de hit, miss, set, remove, falha e latência.
- Planeje invalidação antes de popular o cache em massa.
- Modele chaves de modo que remoção seletiva continue possível.
- Tenha estratégia explícita para dados nulos, faltantes e expirados.

## Conteúdo complementar

### 1. Resumo executivo

Se o seu objetivo é ter um bom SDK Redis para vários projetos C#, o ponto principal não é apenas encapsular chamadas do Redis.

O ponto principal é entregar um contrato estável e previsível.

Um bom SDK Redis precisa responder bem às seguintes perguntas:

- Como as chaves são formadas.
- Como o TTL é decidido.
- Como os objetos são serializados.
- Como erros são observados.
- Como o cache é invalidado.
- Como evitar colisão entre serviços.
- Como reduzir acoplamento com o Redis concreto.
- Como suportar fallback ou troca de provider.
- Como proteger o consumidor de decisões ruins de infraestrutura.
- Como evoluir sem quebrar compatibilidade.

O UnifiedCacheSdk já resolve parte desse problema ao centralizar provider, chave e opções.

O que ainda falta, se a meta for produção em larga escala, é aprofundar o tratamento de serialização, métricas, invalidação, bulk operations, stampede control, compressão, circuit breaker e políticas mais explícitas por recurso.

O valor deste README está justamente em mostrar essa transição.

### 2. Leitura rápida da implementação atual

Os arquivos principais do SDK hoje são estes:

| Arquivo | Papel atual | Observação arquitetural |
|---------|-------------|-------------------------|
| [src/UnifiedCacheSdk/UnifiedCacheClient.cs](./src/UnifiedCacheSdk/UnifiedCacheClient.cs) | Fachada pública para get, set, remove e exists | API simples e fácil de consumir |
| [src/UnifiedCacheSdk/Abstractions/ICacheProvider.cs](./src/UnifiedCacheSdk/Abstractions/ICacheProvider.cs) | Contrato do provedor | Bom ponto de abstração para memória e Redis |
| [src/UnifiedCacheSdk/Abstractions/ICacheKeyBuilder.cs](./src/UnifiedCacheSdk/Abstractions/ICacheKeyBuilder.cs) | Contrato de construção de chave | Permite trocar convenção de nomes |
| [src/UnifiedCacheSdk/Core/CacheKeyBuilder.cs](./src/UnifiedCacheSdk/Core/CacheKeyBuilder.cs) | Implementação padrão da chave | Usa prefixo global, serviço, escopo e recurso |
| [src/UnifiedCacheSdk/Providers/MemoryCacheProvider.cs](./src/UnifiedCacheSdk/Providers/MemoryCacheProvider.cs) | Provider local em memória | Útil para fallback e cenários simples |
| [src/UnifiedCacheSdk/Providers/RedisCacheProvider.cs](./src/UnifiedCacheSdk/Providers/RedisCacheProvider.cs) | Provider distribuído com StackExchange.Redis | Serializa em JSON e usa string get/set |
| [src/UnifiedCacheSdk/Options/UnifiedCacheOptions.cs](./src/UnifiedCacheSdk/Options/UnifiedCacheOptions.cs) | Configurações centrais | Define namespace e TTL padrão |
| [src/UnifiedCacheSdk/Extensions/ServiceCollectionExtensions.cs](./src/UnifiedCacheSdk/Extensions/ServiceCollectionExtensions.cs) | Registro em DI | Decide provider padrão e valida opções |
| [src/UnifiedCacheSdk/UnifiedCacheSdk.csproj](./src/UnifiedCacheSdk/UnifiedCacheSdk.csproj) | Empacotamento | Já gera pacote no build |

Essa base é boa porque a separação principal já existe.

O cliente não conhece Redis diretamente.

O provider não conhece regra de nome de chave.

O key builder não conhece serialização.

As opções ficam concentradas em um objeto próprio.

O DI fica isolado em extensões.

Esse desenho reduz acoplamento e melhora a chance de evolução sem reescrever consumidores.

### 3. O que o projeto já faz bem hoje

- Usa `IConnectionMultiplexer` como singleton quando configurado via extensão.
- Encapsula o provider concreto atrás de uma interface pequena.
- Evita que o consumidor precise montar chave manualmente em todos os pontos.
- Permite uso em memória sem mudar a assinatura do cliente.
- Centraliza `DefaultTtl` em opções.
- Já está empacotado como biblioteca .NET.
- Mantém a API pública pequena, o que reduz curva de aprendizado.
- Não vaza detalhes do `IDatabase` para a aplicação consumidora.
- Possui um caminho simples de registro via `AddUnifiedCache`.
- Tem base suficiente para evoluir em camadas.

### 4. O que ainda falta se a meta for um SDK Redis forte para produção

- Abstração de serialização.
- Política explícita para cache nulo.
- API para `GetOrSetAsync`.
- Política para `refresh ahead`.
- Controle de stampede.
- Métricas integradas.
- Instrumentação com `ActivitySource`.
- Suporte a tags ou índices de invalidação.
- Operações em lote.
- Configuração por recurso e não apenas por default global.
- Compressão opcional para payloads grandes.
- Tratamento mais claro de falhas transitórias.
- Circuit breaker ou degradação controlada.
- Estratégia de versionamento de payload serializado.
- Estratégia de migração de chave.
- Testes de concorrência e performance orientados ao contrato.

Isso não significa que o projeto atual esteja errado.

Significa apenas que ele está em um estágio didático e organizado.

O caminho para um SDK maduro é adicionar capacidade sem perder clareza.

### 5. O que define um bom SDK Redis

Um bom SDK Redis para C# precisa equilibrar cinco tensões ao mesmo tempo:

- Simplicidade para o consumidor.
- Capacidade operacional para produção.
- Baixo acoplamento com detalhes de infraestrutura.
- Bom desempenho em cenários de alto volume.
- Facilidade de evolução sem quebrar aplicações antigas.

Em termos práticos, isso significa:

- API pequena, mas expressiva.
- Convenção de chave previsível.
- Boas opções padrão.
- Possibilidade de override local quando necessário.
- Logs e métricas desde a primeira versão.
- Testes de contrato e integração.
- Pacote publicável com versionamento limpo.
- Documentação clara sobre limites e trade-offs.

Um SDK mediano costuma errar de três formas:

- Ele apenas embrulha o client do Redis e muda pouco ou nada.
- Ele tenta esconder tudo e termina bloqueando casos avançados.
- Ele mistura regras de negócio, infraestrutura e convenção de chave em um único bloco.

O melhor caminho normalmente é este:

- Padronizar o que quase todo projeto precisa.
- Tornar extensível o que muda por contexto.
- Não prometer mais do que o SDK realmente garante.

### 6. Leitura arquitetural do cliente atual

O `UnifiedCacheClient` é a sua fachada principal.

Ele recebe um `ICacheProvider` e um `ICacheKeyBuilder`.

Isso é bom porque a aplicação consumidora fala com um objeto de alto nível.

Esse objeto traduz `resource` e `scope` em uma chave final.

Depois delega a chamada ao provider.

Esse desenho traz três ganhos:

- Centraliza a formação da chave.
- Preserva a liberdade de trocar o provider.
- Mantém a assinatura pública simples.

Ao pensar em evolução, vale considerar ampliar essa fachada com métodos como:

- `GetOrSetAsync`.
- `RefreshAsync`.
- `SetIfNotExistsAsync`.
- `RemoveByTagAsync`.
- `TryGetAsync`.
- `GetManyAsync`.
- `SetManyAsync`.

Mas a regra aqui é importante:

Só adicione um método público novo quando o comportamento for recorrente e bem definido.

Métodos demais costumam virar dívida de compatibilidade.

### 7. Leitura arquitetural do provider atual

O `ICacheProvider` atual define quatro operações:

- Get.
- Set.
- Remove.
- Exists.

Esse contrato é pequeno e útil.

Ele funciona bem como base comum entre memória e Redis.

Ao mesmo tempo, ele não cobre comportamentos distribuídos mais sofisticados.

Isso cria uma decisão de design importante:

- Manter o contrato mínimo e adicionar serviços especializados por fora.
- Ou expandir o contrato para recursos avançados do Redis.

Na maioria dos SDKs reutilizáveis, a primeira opção é mais segura.

Ou seja:

- Tenha um contrato central pequeno.
- Crie extensões especializadas para recursos avançados.
- Evite contaminar a API básica com detalhes demais.

Exemplos de contratos separados que podem surgir no futuro:

- `ICacheLockService`.
- `ICachePubSubService`.
- `ICacheMetrics`.
- `ICacheSerializer`.
- `ICachePolicyResolver`.

### 8. Leitura arquitetural do key builder atual

O `CacheKeyBuilder` atual concatena:

- `GlobalPrefix`.
- `ServiceId`.
- `scope` opcional.
- `resource`.

Esse padrão é muito bom como ponto de partida.

Ele resolve o problema mais comum de colisão entre serviços.

Ele também cria um namespace claro para observabilidade e manutenção.

A forma geral fica assim:

`globalPrefix:serviceId:scope:resource`

Quando não há `scope`, a chave fica menor.

Esse desenho suporta bem:

- Multi-tenant.
- Ambientes separados.
- Múltiplos serviços usando o mesmo Redis.
- Busca e depuração por prefixo.

O que ainda pode melhorar:

- Sanitização de separadores inválidos.
- Normalização de case.
- Versionamento de schema na chave.
- Regras de truncamento para chaves muito grandes.
- Rejeição de `resource` vazio.
- Proteção contra concatenação acidental de dados sensíveis.

### 9. Convenção de chave: o coração do SDK

Muitos times subestimam o design da chave.

Esse é um erro caro.

Chave ruim gera:

- Colisão entre serviços.
- Dificuldade de invalidação.
- Dificuldade de observar uso.
- Aumento de cardinalidade sem controle.
- Acoplamento entre múltiplas equipes.

Uma chave boa precisa responder:

- Quem é o dono do dado.
- Qual é o tipo do recurso.
- Qual o contexto do recurso.
- Qual a granularidade da informação.
- Como ela será invalidada.
- Qual a chance de reuso entre requests.

Uma taxonomia recomendada costuma incluir:

- Prefixo global.
- Identificador do serviço.
- Ambiente ou tenant quando necessário.
- Tipo do recurso.
- Identificador do recurso.
- Eventual versão do schema.

Exemplos conceituais:

- `cache:catalogo-api:produto:123`
- `cache:pedido-api:tenant-a:pedido:987`
- `cache:usuario-api:v2:perfil:42`
- `cache:billing-api:tenant-b:fatura:2026-05`

Recomendações objetivas:

- Prefira substantivos de domínio claros.
- Não coloque texto livre vindo do usuário sem sanitização.
- Não use data local formatada de modo ambíguo.
- Evite chaves com tamanho imprevisível.
- Evite chaves montadas em vários lugares da solução.
- Centralize padrões no SDK.

Uma evolução boa para o projeto atual seria permitir um resolvedor de convenção por recurso.

Exemplo conceitual:

- `PedidoPorId` pode usar um padrão.
- `ResumoFinanceiroMensal` pode usar outro.
- `PerfilPublico` pode usar TTL e nome de chave distintos.

### 10. Versionamento de chave

Versionar apenas o pacote não resolve todos os problemas.

Quando o payload muda, a chave também pode precisar mudar.

Sem isso, um consumidor novo pode desserializar um payload antigo e falhar.

Estratégias comuns:

- Colocar versão na chave.
- Colocar versão no payload.
- Fazer os dois em cenários críticos.

Exemplos conceituais:

- `cache:pedido-api:v1:pedido:123`
- `cache:pedido-api:v2:pedido:123`

Quando usar versão na chave:

- Mudou o shape do objeto de forma incompatível.
- Mudou a política de serialização.
- Mudou o significado semântico do recurso.

Quando versão no payload pode bastar:

- Alteração compatível.
- Adição de campos opcionais.
- Leitores tolerantes a versão antiga.

Para o UnifiedCacheSdk, uma boa evolução seria:

- `UnifiedCacheOptions` aceitar `SchemaVersion` opcional.
- `ICacheKeyBuilder` decidir quando incluir esse valor.

### 11. TTL: não trate todos os dados do mesmo jeito

`DefaultTtl` é útil.

Mas um SDK Redis maduro normalmente precisa de mais que um TTL global.

Tipos diferentes de dado pedem tempos diferentes.

Exemplos:

- Catálogo de produto muda pouco.
- Saldo de conta muda rápido.
- Perfil público pode aceitar leve staleness.
- Sessão de autenticação tem limites rígidos.

Boas decisões sobre TTL costumam considerar:

- Frequência de atualização da fonte original.
- Custo de recomputação.
- Impacto de servir dado desatualizado.
- Volume de acessos.
- Tamanho do payload.

Categorias úteis de TTL:

- Curto.
- Médio.
- Longo.
- Sem cache.

Em vez de espalhar `TimeSpan.FromMinutes` pelo código consumidor, considere centralizar perfis de TTL.

Exemplo conceitual:

- `HotReadModel`.
- `WarmReferenceData`.
- `ColdStaticContent`.
- `NegativeLookup`.

Uma evolução saudável do SDK seria introduzir um resolvedor de política:

- `ICachePolicyResolver`.

Esse componente poderia decidir TTL, compressão, serialização e até namespace por recurso.

### 12. Negative caching

Um tema avançado e importante é o cache de ausência.

Se um recurso não existe, repetir a mesma consulta ao banco em alto volume custa caro.

Nesses casos, pode valer armazenar o fato de que nada foi encontrado por um curto período.

Isso é chamado de negative caching.

Cuidados importantes:

- O TTL deve ser curto.
- O valor precisa ser distinguível de falha de infraestrutura.
- O consumidor não pode confundir negativo com valor real.

Erros comuns:

- Cachear ausência por tempo longo demais.
- Misturar payload nulo com falha de serialização.
- Esquecer de invalidar quando o dado passa a existir.

Se você evoluir o SDK para isso, documente com precisão:

- Qual é o marcador usado.
- Qual é o TTL padrão.
- Como o consumidor interpreta o resultado.

### 13. Serialização: onde muitos SDKs quebram

O provider Redis atual usa `System.Text.Json` diretamente.

Isso é uma escolha boa para começar.

É moderno.

É rápido para muitos cenários.

É nativo da plataforma.

Mas um SDK reutilizável de longo prazo geralmente precisa de uma camada a mais.

Motivos:

- Opções de serialização podem variar por contexto.
- Payloads complexos podem exigir conversores customizados.
- Alguns times precisam comprimir dados.
- Alguns recursos são melhores em binário que em JSON textual.
- Versionamento de contrato fica mais fácil com um serializer explícito.

Recomendação arquitetural:

- Introduza `ICacheSerializer`.
- Faça o provider Redis depender dessa abstração.
- Permita trocar serializer via DI.

Responsabilidades recomendadas do serializer:

- Serializar.
- Desserializar.
- Informar content type ou formato.
- Opcionalmente comprimir e descomprimir.

Decisões importantes:

- JSON texto é ótimo para depuração.
- Binário pode economizar espaço.
- Compressão ajuda payload grande, mas piora CPU.
- Nem todo dado precisa do mesmo formato.

Evite duas armadilhas:

- Serializer com opções escondidas demais.
- Serializer impossível de customizar.

### 14. Exemplo de abstração de serialização recomendada

```csharp
public interface ICacheSerializer
{
    ReadOnlyMemory<byte> Serialize<T>(T value);
    T? Deserialize<T>(ReadOnlyMemory<byte> payload);
}

public sealed class JsonCacheSerializer : ICacheSerializer
{
    private readonly JsonSerializerOptions _options;

    public JsonCacheSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    public ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, _options);
    }

    public T? Deserialize<T>(ReadOnlyMemory<byte> payload)
    {
        return JsonSerializer.Deserialize<T>(payload.Span, _options);
    }
}
```

Essa camada desacopla o provider da implementação concreta.

Ela também facilita testes.

Ela melhora evolução futura.

E permite tratar serialização como um contrato real do SDK.

### 15. Compressão: use quando fizer sentido

Compressão é tentadora.

Mas compressão indiscriminada pode piorar desempenho.

Ela só compensa bem quando:

- O payload é relativamente grande.
- O custo de rede é relevante.
- A frequência de acesso justifica o ganho.
- A CPU disponível suporta a compressão.

Boas práticas:

- Ative acima de um tamanho mínimo.
- Registre métrica de ganho real.
- Não comprima payload pequeno.
- Documente o impacto em CPU.
- Permita desligar por política.

Uma boa evolução do SDK seria tratar compressão no serializer ou em um decorator.

### 16. Conexão com Redis: trate como recurso caro e compartilhado

O `IConnectionMultiplexer` do StackExchange.Redis deve ser compartilhado.

O projeto atual já segue essa linha quando usa singleton.

Isso está correto.

Erros graves comuns:

- Abrir conexão por request.
- Criar multiplexer em toda chamada.
- Descartar e recriar frequentemente.
- Esconder reconexão em pontos errados.

Boas práticas de conexão:

- Um multiplexer por aplicação na maioria dos casos.
- Configuração centralizada.
- Health check separado.
- Timeout alinhado ao SLO.
- Log de eventos importantes de conexão.
- Observação de reconexões e backlog.

Se o SDK crescer, vale considerar uma camada de configuração mais rica para Redis:

- Endpoint.
- SSL.
- User.
- Password.
- Connect timeout.
- Sync timeout.
- Abort on connect fail.
- Keep alive.
- Nome do client.

### 17. Resiliência: Redis falha, rede falha, serializer falha

Um bom SDK Redis não assume caminho feliz o tempo todo.

Ele precisa deixar claro:

- O que acontece quando Redis está fora.
- O que acontece quando há timeout.
- O que acontece quando a desserialização falha.
- O que acontece quando a chave existe com payload incompatível.

Padrões possíveis:

- Propagar exceção.
- Retornar nulo em casos específicos.
- Marcar resultado com metadados.
- Fazer fallback para memória ou fonte primária.

O importante é não misturar todos esses comportamentos sem contrato.

Recomendação prática:

- Para API básica, mantenha semântica simples e documentada.
- Para cenários críticos, ofereça métodos mais ricos com resultado estruturado.

Exemplo de retorno mais expressivo no futuro:

- `CacheGetResult<T>` com `Found`, `Value`, `Source`, `IsStale`, `FailureType`.

Isso ajuda muito em observabilidade e tratamento refinado.

### 18. Circuit breaker e degradação controlada

Nem todo SDK Redis precisa nascer com circuit breaker.

Mas, em ambientes de alto tráfego, é valioso.

Quando Redis começa a falhar em massa, insistir em cada request pode amplificar o problema.

Um circuit breaker ajuda a:

- Cortar chamadas sabidamente problemáticas por uma janela.
- Reduzir pressão na infraestrutura.
- Melhorar previsibilidade do tempo de resposta.
- Permitir fallback controlado.

Se adicionar esse comportamento, documente:

- Critério de abertura.
- Critério de fechamento.
- Janela de amostragem.
- Impacto no consumidor.
- Métricas emitidas.

Evite esconder esse tipo de decisão sem documentação.

### 19. Stampede protection

Cache stampede acontece quando muitos requests descobrem ao mesmo tempo que a chave expirou.

Todos vão à fonte original.

O resultado é pico de carga e latência.

Mecanismos comuns de proteção:

- Lock distribuído leve.
- Single flight em memória.
- Refresh ahead.
- Stale while revalidate.

Cada um tem trade-off.

Single flight local é simples e útil em uma instância única.

Lock distribuído é mais complexo e exige muito cuidado.

Refresh ahead antecipa renovação, mas pode gerar trabalho desnecessário.

Stale while revalidate melhora latência, mas serve dado possivelmente antigo.

Se o SDK evoluir para isso, separe essa lógica da API básica.

Ela merece componente próprio.

### 20. Exemplo conceitual de `GetOrSetAsync`

```csharp
public async Task<T> GetOrSetAsync<T>(
    string resource,
    Func<CancellationToken, Task<T>> factory,
    string? scope = null,
    TimeSpan? ttl = null,
    CancellationToken ct = default)
{
    T? cached = await GetAsync<T>(resource, scope, ct);

    if (cached is not null)
    {
        return cached;
    }

    T value = await factory(ct);
    await SetAsync(resource, value, scope, ttl, ct);
    return value;
}
```

Esse exemplo é útil didaticamente.

Mas, em produção, ele ainda não resolve stampede sozinho.

Vários callers simultâneos podem executar a `factory` ao mesmo tempo.

Por isso, quando o SDK amadurece, `GetOrSetAsync` costuma caminhar junto com single flight ou lock.

### 21. Invalidação: o tema que decide se o cache realmente ajuda

Cache sem invalidação clara é fonte de bugs.

Antes de criar o SDK, responda:

- Quem escreve o dado original.
- Quem pode invalidar.
- Quando invalidar.
- O que remover junto.
- Como evitar cache órfão.

Estratégias comuns:

- Remoção por chave específica.
- Expiração por TTL.
- Versionamento de namespace.
- Pub/Sub para invalidação distribuída.
- Índices auxiliares por grupo.

Remoção por chave é simples.

TTL é barato, mas pode servir dado vencido do ponto de vista de negócio.

Pub/Sub é poderoso, mas aumenta complexidade operacional.

Índices auxiliares ajudam, mas precisam de manutenção consistente.

Para muitos sistemas, a combinação ideal é:

- TTL moderado.
- Remoção explícita nos fluxos de escrita.
- Namespace bom para facilitar inspeção.

### 22. Tags e índices de invalidação

Um recurso avançado bastante útil é invalidar grupos de chaves.

Exemplos:

- Tudo que pertence a um tenant.
- Tudo que pertence a uma categoria.
- Tudo que pertence a uma visão agregada.

Redis não tem tags nativas para chaves string da forma que muitos times imaginam.

Então o SDK precisa modelar isso.

Abordagens comuns:

- Manter sets com as chaves de cada grupo.
- Manter índices por tag.
- Usar namespace versionado por grupo.

Namespace versionado costuma ser mais simples de operar.

Em vez de deletar milhares de chaves, você incrementa a versão lógica do namespace.

Isso troca custo de remoção por custo de coexistência temporária até expiração.

É uma decisão muito útil quando o volume é alto.

### 23. Multi-tenant e isolamento por ambiente

O `scope` do projeto atual já aponta para essa necessidade.

Isso é ótimo.

Mas é importante definir regra operacional.

Boas perguntas:

- `scope` representa tenant, ambiente ou ambos.
- Ele é obrigatório ou opcional.
- Ele entra antes ou depois do tipo do recurso.
- Quem monta esse valor.
- Há risco de vazamento entre tenants.

Se o SDK for usado em vários sistemas, defina padrão forte.

Exemplo de hierarquia útil:

- Prefixo global.
- Ambiente.
- Serviço.
- Tenant.
- Recurso.

Ou:

- Prefixo global.
- Serviço.
- Ambiente.
- Scope de domínio.
- Recurso.

O importante é ter um padrão único documentado.

### 24. Segurança e dados sensíveis

Redis costuma ser usado como infraestrutura interna.

Mesmo assim, trate segurança com seriedade.

Um bom SDK Redis deve evitar ou, no mínimo, documentar claramente:

- Armazenamento de segredo em payload sem necessidade.
- Inclusão de PII diretamente na chave.
- Exposição de dados sensíveis em log.
- Reuso de mesma instância entre ambientes inseguros.

Boas práticas:

- Nunca coloque e-mail, documento ou token puro na chave.
- Prefira ids técnicos.
- Masque logs quando necessário.
- Defina TTL curto para dados mais sensíveis.
- Considere criptografia no payload quando o caso exigir.
- Separe bancos Redis por ambiente crítico.

### 25. Observabilidade: parte do contrato, não detalhe opcional

Se ninguém consegue medir o uso do cache, ninguém consegue melhorar o uso do cache.

Instrumentação mínima recomendada:

- Contador de hit.
- Contador de miss.
- Contador de set.
- Contador de remove.
- Contador de falhas.
- Histograma de latência.
- Tamanho médio de payload.
- Taxa de erro por operação.

Também vale registrar dimensões úteis:

- Serviço.
- Recurso.
- Provider.
- Resultado.
- Exceção.

Em tracing distribuído, um `ActivitySource` no SDK ajuda muito.

Isso permite ligar a latência do cache ao request principal.

Não é luxo.

É operacionalidade.

### 26. Exemplo conceitual de métrica por decorator

```csharp
public sealed class MetricsCacheProvider : ICacheProvider
{
    private readonly ICacheProvider _inner;
    private readonly ICacheMetrics _metrics;

    public MetricsCacheProvider(ICacheProvider inner, ICacheMetrics metrics)
    {
        _inner = inner;
        _metrics = metrics;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ValueStopwatch stopwatch = ValueStopwatch.StartNew();
        T? value = await _inner.GetAsync<T>(key, cancellationToken);
        _metrics.RecordGet(key, value is not null, stopwatch.GetElapsedTime());
        return value;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        => _inner.SetAsync(key, value, ttl, cancellationToken);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => _inner.RemoveAsync(key, cancellationToken);

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        => _inner.ExistsAsync(key, cancellationToken);
}
```

O ponto não é exatamente essa implementação.

O ponto é o padrão:

- Use decorator para cross cutting concern.
- Evite misturar métrica com lógica principal do provider.
- Mantenha o provider simples e composicional.

### 27. Design de API pública

Ao desenhar o SDK, faça a API pública responder a três critérios:

- Clareza.
- Simetria.
- Evolução.

Clareza significa que o nome do método deve sugerir custo e semântica.

Simetria significa que operações equivalentes têm forma equivalente.

Evolução significa que você consegue acrescentar capacidade sem quebrar chamadas existentes.

Boas perguntas de revisão:

- O nome `GetAsync` deixa claro se pode lançar exceção.
- O nome `ExistsAsync` vale a pena ou induz double roundtrip.
- `RemoveAsync` precisa retornar status.
- O método expõe `resource` e `scope` de forma consistente.
- O consumidor consegue entender o TTL sem ler a implementação.

Um ponto sutil sobre `ExistsAsync`:

Ele é útil.

Mas, em alguns fluxos, ele incentiva padrão ruim.

Exemplo ruim:

- Chamar `ExistsAsync`.
- Depois chamar `GetAsync`.

Isso gera duas idas ao provider.

Em muitos casos, `GetAsync` já basta.

A documentação do SDK deve deixar esse trade-off explícito.

### 28. Registro em DI: simples por fora, flexível por dentro

O método `AddUnifiedCache` do projeto atual é um bom começo.

Ele registra opções.

Ele registra o key builder.

Ele escolhe um provider padrão quando ninguém define outro.

Esse desenho é prático.

Mas ainda pode evoluir em alguns pontos:

- Registrar `UnifiedCacheClient` automaticamente.
- Permitir mais opções para Redis sem exigir conexão em string simples.
- Permitir decorators de forma oficial.
- Permitir `ICacheSerializer` via builder.
- Permitir política por recurso.

Exemplo de uso atual recomendado:

```csharp
builder.Services.AddUnifiedCache(
    options =>
    {
        options.ServiceId = CacheDefaults.ServiceId;
        options.GlobalPrefix = CacheDefaults.GlobalPrefix;
        options.Separator = CacheDefaults.Separator;
        options.DefaultTtl = CacheDefaults.DefaultTtl;
    },
    cache =>
    {
        cache.UseRedis(CacheDefaults.RedisConnectionString);
    });

builder.Services.AddSingleton<UnifiedCacheClient>();
```

Esse exemplo reflete um detalhe real do projeto atual:

- O consumidor ainda precisa registrar `UnifiedCacheClient` explicitamente.

Vale documentar isso para evitar surpresa.

### 29. Opções e governança de configuração

`UnifiedCacheOptions` hoje contém:

- `ServiceId`.
- `GlobalPrefix`.
- `Separator`.
- `DefaultTtl`.

É um conjunto bom e enxuto.

Para crescimento saudável, adicione novas opções com cautela.

Boas candidatas futuras:

- `EnableCompression`.
- `CompressionThresholdBytes`.
- `SchemaVersion`.
- `AllowNegativeCaching`.
- `DefaultNegativeTtl`.
- `EmitMetrics`.
- `EmitTracing`.
- `EnablePayloadVersioning`.

Más candidatas para a raiz de opções:

- Flags demais muito específicas de um único projeto.
- Configurações que deveriam estar em políticas por recurso.
- Regras de negócio misturadas com infraestrutura.

### 30. Redis não é banco de documentos genérico sem custo

Outro erro comum em SDKs Redis caseiros é empilhar qualquer objeto no cache como se o custo fosse zero.

Não é.

Custos relevantes:

- Serialização.
- Desserialização.
- Rede.
- Memória.
- GC no consumidor.
- Expiração e churn de chave.

Por isso, um bom SDK Redis também ensina moderação.

Nem tudo precisa ser cacheado.

Nem todo cache precisa guardar o objeto completo.

Às vezes o melhor payload é um read model menor.

Às vezes o melhor cache é apenas um marcador.

Às vezes o melhor resultado é não cachear.

### 31. Bulk operations

À medida que o SDK amadurece, surge a necessidade de operações em lote.

Casos comuns:

- Ler muitos itens por id.
- Popular várias chaves de uma vez.
- Remover grupo pequeno de chaves conhecidas.

Redis suporta bem alguns cenários em lote.

Mas a API precisa ser pensada.

Boas perguntas:

- O lote é homogêneo ou heterogêneo.
- O retorno preserva ordem.
- O retorno distingue item ausente de falha.
- O custo de serialização em lote compensa.

Em SDK reutilizável, comece simples.

Adicione lote apenas quando houver uso real recorrente.

### 32. Pipeline e batch

StackExchange.Redis já oferece meios de agrupar operações.

Mas um SDK alto nível não precisa expor tudo logo de início.

Uma boa regra:

- Exponha comportamento de negócio recorrente.
- Não exponha detalhe técnico só porque a biblioteca base possui.

Se batch for realmente necessário, documente:

- Qual o ganho esperado.
- Limites de uso.
- Semântica de erro parcial.
- Impacto em tracing e métricas.

### 33. Política por recurso

Um ponto avançado que diferencia SDK bom de SDK excelente é permitir política por recurso.

Exemplo conceitual:

- `produto-por-id` usa TTL médio e compressão off.
- `relatorio-mensal` usa TTL longo e compressão on.
- `saldo-atual` usa TTL curto e sem stale.

Em vez de exigir que cada aplicação repita isso manualmente, o SDK pode oferecer um resolvedor.

Esse resolvedor poderia responder por:

- TTL.
- Serializer.
- Compressão.
- Namespace.
- Negative caching.
- Stampede control.

### 34. Mudanças compatíveis e incompatíveis

Quando publicar um SDK, separe bem dois tipos de mudança.

Compatíveis:

- Novo overload opcional.
- Nova opção com default seguro.
- Nova métrica.
- Novo provider opcional.

Incompatíveis:

- Mudança no formato de chave.
- Mudança na serialização padrão.
- Mudança na semântica de erro.
- Mudança no TTL default com impacto forte.

Para mudanças incompatíveis, documente claramente:

- O que mudou.
- Quem pode ser afetado.
- Como migrar.
- Se haverá coexistência temporária.

### 35. Documentação operacional é parte do SDK

Um bom SDK Redis não termina na API.

Ele precisa de documentação operacional.

Itens que valem estar sempre documentados:

- Prefixo de chave.
- Chaves mais importantes.
- TTL por categoria.
- Dependências externas.
- Comportamento quando Redis cai.
- Comportamento quando serializer falha.
- Estratégia de invalidação.
- Métricas emitidas.
- Health checks esperados.
- Versões suportadas do .NET.

### 36. Testes: trate o SDK como produto

Quando o SDK é usado por vários projetos, ele deixa de ser apenas código compartilhado.

Ele vira produto interno.

E produto interno precisa de testes proporcionais ao impacto.

Camadas recomendadas de teste:

- Unidade para key builder.
- Unidade para validação de options.
- Unidade para decorators.
- Integração com Redis real ou container.
- Integração com MemoryCache.
- Teste de serialização e desserialização.
- Teste de concorrência para fluxos críticos.
- Teste de contrato da API pública.

Casos que merecem teste explícito:

- Chave com scope.
- Chave sem scope.
- TTL default.
- TTL customizado.
- Item inexistente.
- Item nulo.
- Falha de serialização.
- Mudança de opção obrigatória.
- Provider default quando nenhum é configurado.
- Registro explícito de Redis.

### 37. Integração com Redis em teste

Evite confiar apenas em mocks para provider Redis.

Mocks ajudam no contrato.

Mas não capturam detalhes reais de serialização, tempo de rede e comportamento do client.

Para integração, prefira:

- Container local com Redis.
- Setup isolado por suíte.
- Prefixo de chave exclusivo por teste.
- Limpeza determinística após execução.

Com isso você valida:

- Roundtrip do payload.
- Expiração real.
- Existência de chave.
- Compatibilidade da convenção de nomes.

### 38. Benchmark: desempenho orientado a cenário

Cache é assunto de performance.

Então benchmark não é luxo.

É parte da validação do SDK.

Meça pelo menos:

- Get hit em memória.
- Get hit em Redis.
- Set pequeno em Redis.
- Set grande em Redis.
- Payload com e sem compressão.
- Serializer padrão e serializer alternativo.

Não compare apenas média.

Observe:

- p95.
- p99.
- throughput.
- alocação de memória.
- tamanho do payload.

### 39. Publicação como pacote

O `UnifiedCacheSdk.csproj` já define `GeneratePackageOnBuild`.

Isso é um passo bom.

Para um pacote maduro, vale acrescentar:

- `PackageReadmeFile`.
- `RepositoryUrl`.
- `PackageProjectUrl`.
- `PackageLicenseExpression` quando aplicável.
- `PackageReleaseNotes`.
- Symbol package quando fizer sentido.

Também é útil padronizar:

- Política de versionamento semântico.
- Fluxo de release.
- Compatibilidade mínima de framework.
- Política para breaking changes.

### 40. SemVer aplicado a SDK Redis

Use SemVer com disciplina.

`MAJOR` para mudanças incompatíveis.

`MINOR` para capacidade nova compatível.

`PATCH` para correção sem quebra.

Em um SDK Redis, exemplos típicos:

- Trocar formato de chave padrão é `MAJOR`.
- Adicionar decorator opcional é `MINOR`.
- Corrigir bug em `ExistsAsync` é `PATCH`.

Não subestime mudanças de comportamento implícito.

Alterar TTL default pode ser quase breaking para certas cargas.

Mesmo quando não for tecnicamente breaking, documente muito bem.

### 41. Exemplo de uso atual do SDK em uma aplicação C#

```csharp
public static class CacheDefaults
{
    public const string ServiceId = nameof(PedidoApi);
    public const string GlobalPrefix = nameof(Cache);
    public const string Separator = ":";
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);
    public const string RedisConnectionString = CacheInfrastructure.RedisConnectionString;
}

builder.Services.AddUnifiedCache(
    options =>
    {
        options.ServiceId = CacheDefaults.ServiceId;
        options.GlobalPrefix = CacheDefaults.GlobalPrefix;
        options.Separator = CacheDefaults.Separator;
        options.DefaultTtl = CacheDefaults.DefaultTtl;
    },
    cache =>
    {
        cache.UseRedis(CacheDefaults.RedisConnectionString);
    });

builder.Services.AddSingleton<UnifiedCacheClient>();

public sealed class PedidoCacheService
{
    private readonly UnifiedCacheClient _cache;

    public PedidoCacheService(UnifiedCacheClient cache)
    {
        _cache = cache;
    }

    public Task<PedidoResumo?> GetAsync(Guid pedidoId, CancellationToken cancellationToken)
    {
        return _cache.GetAsync<PedidoResumo>(PedidoCacheResources.PorId(pedidoId), cancellationToken: cancellationToken);
    }

    public Task SetAsync(PedidoResumo pedido, CancellationToken cancellationToken)
    {
        return _cache.SetAsync(PedidoCacheResources.PorId(pedido.Id), pedido, ttl: TimeSpan.FromMinutes(5), ct: cancellationToken);
    }

    public Task RemoveAsync(Guid pedidoId, CancellationToken cancellationToken)
    {
        return _cache.RemoveAsync(PedidoCacheResources.PorId(pedidoId), ct: cancellationToken);
    }
}
```

Observe três pontos importantes:

- A chave do recurso foi centralizada em `PedidoCacheResources`.
- O TTL foi escolhido por caso de uso.
- O consumidor não fala com Redis diretamente.

### 42. Exemplo de centralização de recursos

```csharp
public static class PedidoCacheResources
{
    public static string PorId(Guid pedidoId)
    {
        return string.Concat(nameof(Pedido), ":", pedidoId);
    }

    public static string ListaPorCliente(Guid clienteId)
    {
        return string.Concat(nameof(PedidosDoCliente), ":", clienteId);
    }
}
```

Esse padrão é simples e muito importante.

Ele evita que chaves sejam montadas manualmente em vários lugares.

Isso reduz divergência, bug de invalidação e inconsistência entre times.

### 43. Roadmap de evolução recomendado para este projeto

Fase 1:

- Registrar `UnifiedCacheClient` automaticamente na extensão.
- Adicionar testes de unidade para `CacheKeyBuilder`.
- Adicionar testes de integração com Redis real.
- Documentar convenção oficial de chave.

Fase 2:

- Introduzir `ICacheSerializer`.
- Permitir `JsonSerializerOptions` customizadas.
- Tratar erro de desserialização com diagnóstico melhor.
- Adicionar métricas básicas.

Fase 3:

- Introduzir `GetOrSetAsync`.
- Adicionar política por recurso.
- Adicionar negative caching opcional.
- Adicionar decorator de observabilidade.

Fase 4:

- Estudar stale while revalidate.
- Adicionar bulk operations selecionadas.
- Adicionar suporte opcional a compressão.
- Adicionar namespace versionado.

Fase 5:

- Adicionar tracing.
- Adicionar health checks padronizados.
- Formalizar estratégia de release.
- Publicar pacote com documentação operacional completa.

### 44. Anti-patterns comuns em SDKs Redis

- Montar chave manualmente em toda aplicação.
- Embutir regra de negócio no provider.
- Usar TTL fixo para todos os recursos.
- Recriar conexão a cada chamada.
- Tratar cache como banco primário.
- Ignorar observabilidade.
- Fazer log de payload sensível.
- Misturar provider, serializer e métrica no mesmo método.
- Esconder erro crítico atrás de retorno silencioso.
- Criar API pública grande demais sem uso validado.
- Ignorar versionamento de payload.
- Usar `Exists` antes de todo `Get`.
- Invalidar o cache inteiro por preguiça de modelagem.
- Não testar expiração real.
- Não documentar fallback quando Redis falha.
- Armazenar objetos excessivamente grandes sem medir.
- Tratar Redis como infraestrutura infalível.
- Não separar ambientes na chave.
- Não separar tenants na chave quando necessário.
- Publicar pacote sem release notes.

### 45. Checklist de arquitetura para um bom SDK Redis

1. A API pública cabe em uma página mental.
2. O contrato central tem poucos métodos.
3. O provider concreto não vaza para o consumidor.
4. O formato de chave está documentado.
5. O `ServiceId` é obrigatório.
6. O prefixo global está padronizado.
7. O `scope` tem semântica definida.
8. O TTL default é conservador.
9. O consumidor pode sobrescrever TTL quando necessário.
10. Existe um caminho para política por recurso.
11. A serialização está centralizada.
12. A serialização é testada com payloads reais.
13. Mudanças de schema têm plano de migração.
14. Falhas de Redis são observáveis.
15. Falhas de serialização são observáveis.
16. A conexão é singleton.
17. O SDK não abre conexão por request.
18. O SDK tem teste de integração com Redis.
19. O SDK tem teste de unidade para key builder.
20. O SDK tem documentação de invalidação.
21. O SDK documenta dados recomendados para cache.
22. O SDK documenta dados que não devem ser cacheados.
23. O SDK define convenção para recursos.
24. O SDK evita depender de string solta espalhada.
25. O SDK diferencia responsabilidade de cliente e provider.
26. O SDK diferencia responsabilidade de provider e serializer.
27. O SDK pode ser usado com MemoryCache.
28. O SDK pode ser usado com Redis.
29. O fallback padrão está documentado.
30. O pacote pode ser gerado localmente.
31. A versão do pacote é explícita.
32. O pacote possui descrição clara.
33. O pacote possui tags coerentes.
34. O pacote informa compatibilidade de framework.
35. O SDK não mistura autenticação com cache.
36. O SDK não mistura autorização com cache.
37. O SDK não injeta dependências desnecessárias no consumidor.
38. O SDK permite decorator para métrica.
39. O SDK permite decorator para tracing.
40. O SDK pode crescer sem reescrever todos os consumidores.

### 46. Checklist operacional para uso em produção

1. Redis está com autenticação configurada quando necessário.
2. O timeout do client foi revisado.
3. A política de retry foi definida fora ou dentro do SDK.
4. Existe dashboard de latência.
5. Existe dashboard de hit ratio.
6. Existe alerta para aumento brusco de miss.
7. Existe alerta para timeout em Redis.
8. Existe alerta para exceção de desserialização.
9. O namespace de chaves está revisado por serviço.
10. O ambiente entra na chave quando necessário.
11. O tenant entra na chave quando necessário.
12. Payloads grandes foram medidos.
13. Compressão foi avaliada com benchmark.
14. O time sabe invalidar um recurso específico.
15. O time sabe invalidar um grupo de recursos.
16. O time sabe lidar com queda de Redis.
17. O time sabe lidar com stale data.
18. O time conhece o TTL de cada recurso principal.
19. O time revisou se há PII na chave.
20. O time revisou se há PII no log.
21. O time revisou se há segredo no payload.
22. O time revisou se existe negative caching acidental.
23. O time revisou se existe double roundtrip desnecessário.
24. O time revisou se o uso de `ExistsAsync` faz sentido.
25. O time revisou se o cache está mascarando problema de consulta ruim.
26. O time revisou se a fonte primária aguenta miss em massa.
27. O time avaliou stampede em recursos quentes.
28. O time testou reinício da aplicação com Redis disponível.
29. O time testou Redis indisponível.
30. O time testou payload incompatível em chave antiga.
31. O time testou limpeza seletiva de cache.
32. O time testou mudança de versão do pacote.
33. O time possui release notes internas.
34. O time possui política de rollback.
35. O time sabe como depurar uma chave específica.
36. O time sabe como localizar chaves por prefixo.
37. O time sabe quais recursos geram maior volume.
38. O time sabe quais recursos mais falham.
39. O time sabe quais recursos têm maior p95.
40. O time sabe se o ganho do cache compensa o custo operacional.

### 47. FAQ avançado

#### O SDK deve esconder completamente o Redis do consumidor

Não completamente.

Ele deve esconder o detalhe operacional que gera acoplamento desnecessário.

Mas também deve permitir extensão quando um cenário avançado exigir capacidade nativa.

#### Vale a pena expor `IDatabase` diretamente

Como contrato principal, não.

Isso enfraquece o valor do SDK.

Se for inevitável para cenários especiais, prefira uma extensão separada e conscientemente menos abstrata.

#### Um SDK Redis precisa sempre suportar memória local também

Não sempre.

Mas, em muitos times, isso é um ganho excelente para desenvolvimento local, testes e fallback simples.

#### Vale a pena ter `ExistsAsync`

Sim, desde que a documentação explique quando usar.

Em muitos fluxos, `GetAsync` é suficiente e evita roundtrip duplicado.

#### Preciso de `GetOrSetAsync`

Quase sempre, sim.

Mas ele só fica realmente bom quando a estratégia contra stampede também está clara.

#### Devo usar JSON para tudo

Não obrigatoriamente.

JSON é ótimo para começar.

Mas payload grande, contrato rígido e custo de serialização podem justificar outra estratégia.

#### Devo colocar versão na chave ou no payload

Depende.

Se a mudança é incompatível, colocar versão na chave costuma simplificar a convivência entre formatos.

#### Vale a pena usar compressão

Somente quando benchmark comprovar ganho líquido.

Sem medição, compressão vira aposta cega.

#### Redis substitui banco relacional

Não como regra geral.

Cache é aceleração, não substituição automática da fonte de verdade.

#### Um SDK Redis deve cuidar de lock distribuído

Pode cuidar, mas não precisa estar no núcleo da API.

Esse recurso merece módulo ou contrato próprio.

#### Vale a pena suportar Pub/Sub no mesmo pacote

Só se o escopo do SDK justificar.

Caso contrário, o pacote pode ficar amplo demais e perder foco.

#### Como evitar colisão entre microserviços

Com `ServiceId` obrigatório e convenção forte de chave.

#### Como evitar vazamento entre tenants

Com `scope` ou namespace dedicado, revisado e testado.

#### Como saber se o cache está ajudando

Métrica.

Sem hit ratio, latência e taxa de erro, a resposta é opinião.

#### O que fazer quando o payload muda

Planejar versionamento de chave, payload ou ambos.

#### O SDK deve cachear nulo

Às vezes sim.

Mas somente com semântica explícita e TTL curto.

#### Devo usar chaves humanas e legíveis

Na maior parte do tempo, sim.

Legibilidade ajuda suporte, observabilidade e troubleshooting.

#### Devo usar nomes curtos demais para economizar bytes

Somente quando o volume justificar e a equipe aceitar a perda de clareza.

Na maioria dos sistemas internos, clareza vence micro-otimização de nome.

#### O SDK precisa ter health check próprio

Em cenários de produção, sim, ou pelo menos fornecer integração simples.

#### Quando adicionar bulk operations

Quando houver cenário recorrente, benchmark e semântica clara de erro parcial.

#### Quando remover uma funcionalidade do SDK

Quase nunca sem plano de migração.

Deprecar primeiro costuma ser o caminho certo.

### 48. Recomendações finais para este projeto

Se o objetivo for manter o UnifiedCacheSdk como exemplo didático avançado, o projeto já está muito bem posicionado.

Ele tem estrutura organizada.

Ele tem contrato pequeno.

Ele tem empacotamento.

Ele tem uma história clara para ensinar abstração de cache.

Se o objetivo for transformá-lo em SDK interno de uso amplo, as próximas melhores evoluções são:

- Formalizar observabilidade.
- Formalizar serialização.
- Formalizar política por recurso.
- Formalizar estratégia de invalidação.
- Formalizar testes de integração e contrato.

Comece por esses pontos.

Eles trazem valor real sem explodir a complexidade do pacote.

### 49. Ordem recomendada de implementação das melhorias

1. Testes.
2. Registro automático do cliente.
3. Documentação de chave e TTL.
4. Serializer abstrato.
5. Métricas.
6. `GetOrSetAsync`.
7. Política por recurso.
8. Negative caching.
9. Stampede protection.
10. Compressão opcional.

Essa ordem preserva a simplicidade e reduz risco de adicionar recursos sofisticados demais cedo demais.

### 50. Conclusão

Um bom SDK Redis para projetos C# não é apenas uma coleção de helpers.

Ele é uma peça de plataforma.

Ele padroniza como múltiplas aplicações nomeiam, serializam, expiram, observam e invalidam dados em cache.

O UnifiedCacheSdk já possui a base arquitetural correta para esse caminho.

O segredo agora não é torná-lo maior por vaidade.

É torná-lo mais confiável, mais observável e mais explícito nas decisões importantes.

Quanto mais clara for a convenção de chave, a política de TTL, a semântica de erro e a estratégia de invalidação, mais útil esse SDK será para os seus projetos C#.

## Referências e documentação complementar

- [Microsoft Docs - Distributed caching in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/distributed)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis docs](https://redis.io/docs/)
- [src/UnifiedCacheSdk/UnifiedCacheClient.cs](./src/UnifiedCacheSdk/UnifiedCacheClient.cs)
- [src/UnifiedCacheSdk/Providers/RedisCacheProvider.cs](./src/UnifiedCacheSdk/Providers/RedisCacheProvider.cs)
- [src/UnifiedCacheSdk/Providers/MemoryCacheProvider.cs](./src/UnifiedCacheSdk/Providers/MemoryCacheProvider.cs)
- [src/UnifiedCacheSdk/Core/CacheKeyBuilder.cs](./src/UnifiedCacheSdk/Core/CacheKeyBuilder.cs)
- [src/UnifiedCacheSdk/Extensions/ServiceCollectionExtensions.cs](./src/UnifiedCacheSdk/Extensions/ServiceCollectionExtensions.cs)
- [src/UnifiedCacheSdk/Options/UnifiedCacheOptions.cs](./src/UnifiedCacheSdk/Options/UnifiedCacheOptions.cs)
