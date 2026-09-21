# ModularMonolithCommerceDemo

Monólito organizado em módulos com contratos públicos explícitos, dados logicamente isolados e comunicação por eventos de integração internos.

## Visão geral

Um monólito modular é um processo só, com fronteiras internas reais. A diferença para o monólito comum não está no deploy — é a mesma aplicação — mas em o que cada parte tem permissão de alcançar.

Aqui essa permissão é imposta pelo compilador. Cada módulo é um assembly separado: `Modules.Catalog`, `Modules.Orders` e `Modules.Payments`. Cada um expõe uma fachada pública que implementa um contrato, e mantém entidades, repositório e regras como `internal`. A consequência é concreta: tentar tocar o banco do Catálogo a partir do host **não compila** — `error CS0122: CatalogDatabase é inacessível devido ao seu nível de proteção`.

Isso resolve o problema que corrói monólitos com o tempo. Em uma aplicação de assembly único, nada impede o módulo de Pedidos de fazer um `SELECT` na tabela de produtos, e depois de dois anos todo mundo lê a tabela de todo mundo. A fronteira só existe se algo a impuser.

A comunicação acontece de duas formas, escolhidas de propósito. Quando Pedidos precisa do preço **agora**, chama o contrato público do Catálogo — síncrono e direto. Quando um pedido é criado, Pedidos apenas publica `OrderPlaced` e não sabe quem escuta: Catálogo reserva o estoque, Pagamentos cobra, e cada um publica o próprio resultado.

## Conceitos abordados

- Módulo como assembly, com fronteira imposta pelo compilador.
- Contrato público versus implementação `internal`.
- DTO do contrato como projeção da entidade, não a entidade.
- Dados logicamente isolados por módulo.
- Eventos de integração internos e inversão de dependência.
- Chamada síncrona por contrato versus publicação assíncrona por evento.
- Compensação: devolver estoque quando o pagamento falha.
- Isolamento de falha de um assinante no barramento.

## Objetivos de aprendizagem

- Desenhar fronteiras de módulo que não dependem de disciplina para serem respeitadas.
- Escolher entre chamar o contrato e publicar um evento.
- Entender por que o evento carrega dados, e não só um id.
- Reconhecer o que precisa existir antes de extrair um módulo para serviço.

## Estrutura do projeto

```text
ModularMonolithCommerceDemo/
`-- src/
    |-- Host/
    |   |-- InProcessEventBus.cs
    |   `-- Program.cs
    |-- Modules.Abstractions/
    |   |-- Contracts/ModuleContracts.cs
    |   `-- Events/
    |       |-- IEventBus.cs
    |       `-- IntegrationEvents.cs
    |-- Modules.Catalog/
    |   |-- CatalogModule.cs
    |   `-- Internal/CatalogDatabase.cs
    |-- Modules.Orders/
    |   |-- OrdersModule.cs
    |   `-- Internal/OrdersDatabase.cs
    `-- Modules.Payments/
        |-- PaymentsModule.cs
        `-- Internal/PaymentsDatabase.cs
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/ModularMonolithCommerceDemo/src/Host/Host.csproj
```

Para validar apenas a compilação (o host arrasta os quatro projetos):

```bash
dotnet build 08-ArchitecturalPatterns/ModularMonolithCommerceDemo/src/Host/Host.csproj
```

Não exige serviço externo. Roda quatro cenários e termina.

## Boas práticas e pontos de atenção

- Fronteira de módulo precisa ser imposta, não combinada. Em um assembly único, `internal` não separa nada e a regra vira disciplina — que se perde na primeira urgência. Um assembly por módulo faz o compilador cobrar.
- Exponha DTO, nunca a entidade. `ProductInfo` não tem `SupplierCode` e `PaymentInfo` não tem `GatewayReference`: são detalhes internos. Expor a entidade acopla todos os módulos ao modelo interno de um.
- Cada módulo é dono dos próprios dados. Nenhum outro lê nem escreve neles — nem por consulta direta, nem por join. Em produção isso costuma ser um schema por módulo no mesmo banco: isolamento lógico, não físico.
- O evento carrega os dados de que os assinantes precisam. Mandar só um id obriga quem escuta a ir buscar na fonte alheia, e a fronteira que o evento existia para preservar se desfaz.
- Escolha entre contrato e evento com critério. Precisa da resposta agora e o fluxo não continua sem ela? Contrato. É uma notificação de algo que aconteceu e outros podem reagir? Evento.
- Cuidado com reserva antes de confirmação. O Catálogo reserva estoque ao receber `OrderPlaced`, antes de o pagamento ser decidido; sem a compensação em `PaymentFailed`, o produto ficaria preso a um pedido que não existe.
- A ordem de chegada dos eventos não pode importar. O pedido só é confirmado quando estoque **e** pagamento chegam, em qualquer ordem.
- Falha de um assinante não derruba os outros. No barramento em processo isso é uma escolha explícita; com broker de verdade, a entrega seria retentada.
- O barramento é a peça que muda ao extrair um módulo. As assinaturas dos módulos continuam iguais: troca-se `InProcessEventBus` por uma implementação que fala com um broker. É esse o valor de começar modular.
- Extrair cedo demais custa caro. Monólito modular bem feito permite adiar a decisão de distribuir até haver motivo real.

## Conteúdo complementar

Dependências entre projetos:

```text
Host ----------> Modules.Catalog ---+
  |              Modules.Orders ----+---> Modules.Abstractions
  |              Modules.Payments --+
  |
  +-- so o Host conhece as tres implementacoes concretas
```

Nenhum módulo referencia outro módulo. `Orders` conhece apenas `ICatalogModule`, que vive nas abstrações.

Comunicação entre módulos:

| Origem | Destino | Como | Por quê |
|---|---|---|---|
| Orders | Catalog | Contrato `ICatalogModule.FindAsync` | Precisa do preço antes de continuar |
| Orders | (quem escutar) | Evento `OrderPlaced` | Notifica um fato; não espera ninguém |
| Catalog | (quem escutar) | `StockReserved` / `StockRejected` | Resultado da reserva |
| Payments | (quem escutar) | `PaymentConfirmed` / `PaymentFailed` | Resultado da cobrança |
| Catalog | — | Reage a `PaymentFailed` | Compensa a reserva |

Resultados observados:

**Pedido bem-sucedido**

```text
Pedidos:    PED-001 criado (2x ABC-1, total 699.80)
Bus:        OrderPlaced -> 2 assinantes
Catalogo:   2 unidades reservadas, restam 8
Pagamentos: 699.80 confirmado
Pedidos:    PED-001 -> confirmado
```

**Pagamento recusado, com compensação**

```text
Pedidos:    PED-002 criado (2x XYZ-9, total 3798.00)
Catalogo:   2 unidades reservadas, restam 0
Pagamentos: recusado (limite 2000.00)
Catalogo:   2 unidades devolvidas ao estoque
Pedidos:    PED-002 -> cancelado (valor acima do limite)
```

**Sem estoque, e duas recusas independentes**

```text
Catalogo:   estoque insuficiente (pedidas 50, disponiveis 2)
Pedidos:    PED-003 -> cancelado (sem estoque)
Pagamentos: tambem recusa, por valor
Pedidos:    mantem o primeiro motivo
```

Dois módulos rejeitaram o mesmo pedido de forma independente. O cancelamento guarda a **primeira** causa: sobrescrever apagaria a razão real.

Prova de isolamento — tentar alcançar o interno de um módulo a partir do host:

```text
error CS0122: "CatalogDatabase" é inacessível devido ao seu nível de proteção
```

O compilador recusa. Não é convenção, é barreira.

O que muda ao extrair um módulo para serviço:

| Peça | Muda? |
|---|---|
| Contrato público do módulo | Não — vira contrato HTTP/gRPC com a mesma forma |
| Eventos de integração | Não — passam a trafegar por um broker |
| Implementação do `IEventBus` | **Sim** — é o ponto de troca |
| Dados do módulo | Não — já eram isolados |
| Código interno do módulo | Não |

Relação com os vizinhos da trilha: `PortsAndAdapters` trata da fronteira entre aplicação e infraestrutura, que é outro eixo de separação. `SagaPattern` coordena transações distribuídas com compensação — aqui há apenas uma compensação simples, para que o estoque não fique preso. `EventSourcingBankAccountDemo` usa eventos como fonte da verdade de um agregado, o que é diferente de evento de integração entre módulos.

## Referências e documentação complementar

- https://www.kamilgrzybek.com/blog/posts/modular-monolith-primer
- https://martinfowler.com/bliki/MonolithFirst.html
- https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/development-process-for-azure
