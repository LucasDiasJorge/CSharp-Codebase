# CLAUDE.md — StrategyResolver

Console: Advanced Resolver Pattern num processador de pagamentos — cada método declara sua aplicabilidade (`AppliesTo`) e sua precedência (`Priority`), com auto-registro por varredura de assembly. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/StrategyResolver/StrategyResolver.csproj
dotnet run --project 07-DesignPatterns/StrategyResolver/StrategyResolver.csproj
```

## Estrutura interna

- `Resolvers/IPaymentResolver.cs` — **o contrato onde o padrão vive**. `AppliesTo(PaymentRequest)` recebe o objeto de domínio inteiro (permite critério composto, não só uma string) e `Priority` torna a precedência um dado do resolver. Menor valor é avaliado primeiro.
- `Resolvers/CreditCardResolver.cs` (`Priority` 200) e `Resolvers/CreditCardInstallmentResolver.cs` (`Priority` 100) — **o par que justifica o `Priority`**: os dois aceitam `CREDIT_CARD`, e o parcelado aceita um subconjunto, então precisa ser avaliado antes. Ao mexer em qualquer um dos dois, preservar essa diferença de prioridade.
- `Resolvers/{Pix,Boleto,DigitalWallet}Resolver.cs` — métodos sem disputa, todos com prioridade 100.
- `Resolvers/UnsupportedPaymentResolver.cs` — fallback com `AppliesTo => true` e `Priority => int.MaxValue`. **Fica fora da coleção `IEnumerable<IPaymentResolver>`**: o scanning o exclui por tipo e ele é injetado pelo tipo concreto. Dentro da coleção, engoliria os demais.
- `DependencyInjection/PaymentResolverRegistration.cs` — extension method `AddPaymentResolvers`, com varredura de assembly. O parâmetro `typeFilter` **existe só para a demonstração do `Program`** (simula o assembly antes de `DigitalWalletResolver` existir); em produção a chamada é sem argumento.
- `Services/PaymentProcessingService.cs` — orquestrador. Ordena por `Priority` uma vez no construtor, agrupa **pelo resolver vencedor** (não pelo método de pagamento) e dispara os lotes com `Task.WhenAll`. É o arquivo que **não muda** quando um resolver novo entra.
- `Legacy/SwitchPaymentService.cs` — a versão com `switch`, mantida só como contraste didático na etapa 1 da saída.
- `Program.cs` — top-level statements em três etapas: `switch` → scanning sem carteira digital (cai no fallback) → scanning completo.

## Pontos de atenção

- TFM **`net10.0`**, não o `net9.0` padrão do repositório: a máquina tem apenas os runtimes 8.0 e 10.0 instalados, então um build `net9.0` compila mas falha em `dotnet run`. Não "padronizar" para net9.0 sem instalar o runtime correspondente.
- Pacotes `Microsoft.Extensions.DependencyInjection` e `Microsoft.Extensions.Logging.Console` 10.0.0. Sem serviço externo nem adquirente real — a liquidação é `Task.Delay` mais log, roda offline.
- **Prioridade empatada é indeterminada.** `OrderBy` é estável, então resolvers de mesma prioridade ficam na ordem que `Assembly.GetTypes()` devolveu — que a especificação não garante. Ao adicionar resolver novo, ou dar prioridade distinta ou garantir que o `AppliesTo` seja mutuamente exclusivo com os já existentes.
- O agrupamento em `ProcessAsync` usa a instância do resolver como chave, o que depende do registro como singleton. Trocar para `AddScoped`/`AddTransient` quebra o agrupamento.
- A saída mistura logs de lotes paralelos; a ordem entre grupos varia entre execuções e isso não é defeito.
- Par de estudo: `StrategyIntegration` (mesma pasta) tem o Strategy clássico com `SetStrategy`; `DesignPattern/Behavioral/Strategy` tem a forma GoF pura; `PortsAndAdapters/example` leva seleção dinâmica para uma API.
