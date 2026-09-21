# CLAUDE.md — ModularMonolithCommerceDemo

Monólito modular com três módulos em assemblies separados, contratos públicos, dados isolados e eventos de integração internos. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/ModularMonolithCommerceDemo/src/Host/Host.csproj
dotnet run   --project 08-ArchitecturalPatterns/ModularMonolithCommerceDemo/src/Host/Host.csproj
```

O `Host` arrasta os outros quatro projetos. Sem serviço externo.

## Estrutura interna

**Layout `src/` com cinco projetos** (precedente no repo: `GrpcSample`, `MySimpleSdk`, `UnifiedCacheSdk`, `AtomicOperationsDemo`):

- `Modules.Abstractions` — contratos públicos (`ICatalogModule`, `IOrdersModule`, `IPaymentsModule`), DTOs e eventos de integração + `IEventBus`.
- `Modules.{Catalog,Orders,Payments}` — cada um referencia **só** `Modules.Abstractions`. Nenhum módulo referencia outro módulo.
- `Host` — único lugar que conhece as três implementações concretas; implementa `InProcessEventBus`.

**A separação em assemblies é o assunto do projeto, não organização.** Em assembly único, `internal` não separaria nada e a fronteira viraria disciplina. Verificado: uma sonda no `Host` tentando instanciar `Modules.Catalog.Internal.CatalogDatabase` falha com `error CS0122`. Se alguém fundir os projetos ou adicionar `InternalsVisibleTo`, a garantia central desaparece sem que nenhum teste quebre.

Cada módulo tem `Internal/` com o "banco" e as entidades, todos `internal`. Os DTOs públicos são projeções deliberadamente menores: `ProductInfo` omite `SupplierCode`, `PaymentInfo` omite `GatewayReference`.

## Pontos de atenção

- TFM `net10.0` nos cinco projetos. Pacotes: `Microsoft.Extensions.Logging.Console` (Host) e `Microsoft.Extensions.Logging.Abstractions` (Abstractions).
- **Registrar os cinco `.csproj` na solução**, não só o Host — como `GrpcSample` faz.
- `Orders` chama `ICatalogModule` de forma **síncrona** (precisa do preço) e publica `OrderPlaced` de forma **assíncrona** (notificação). Os dois estilos convivem de propósito; o README explica o critério. Não uniformizar.
- **Compensação obrigatória:** `CatalogModule` assina `PaymentFailed` e devolve o estoque reservado. Sem isso, o cenário 2 deixaria `XYZ-9` com estoque 0 para sempre por causa de um pedido cancelado — foi um defeito real da primeira versão, corrigido.
- **`OrderEntity.Fail` mantém a PRIMEIRA causa.** Dois módulos podem recusar o mesmo pedido de forma independente (sem estoque *e* valor acima do limite); sobrescrever apagaria a razão real. O cenário 3 depende disso para mostrar "cancelado (sem estoque)".
- `OnStockReservedAsync` casa o evento ao pedido **por SKU**, porque `StockReserved` não carrega o id do pedido. É simplificação do exemplo; em produção o evento levaria o id. Não confundir com descuido — está assim para manter o evento centrado no domínio do Catálogo.
- `InProcessEventBus` captura exceção de assinante e segue. É escolha explícita e está comentada; com broker real haveria retentativa.
- O `Section()` no `Program.cs` tem `Thread.Sleep(120)` pela fila do provider de console.
- Os números do README (estoque 8 e 2 ao final, totais 699.80 / 3798.00 / 94950.00) vêm do seed em `CatalogDatabase` e dos pedidos do `Program.cs`. Alterar qualquer um invalida as tabelas.
- **Fronteira com os vizinhos**: `PortsAndAdapters` separa aplicação de infraestrutura, outro eixo. `SagaPattern` cobre transação distribuída com compensação — aqui há só uma compensação simples, não uma saga. [EventSourcingBankAccountDemo](../EventSourcingBankAccountDemo/CLAUDE.md) usa evento como fonte da verdade de um agregado, que é coisa diferente de evento de integração.
