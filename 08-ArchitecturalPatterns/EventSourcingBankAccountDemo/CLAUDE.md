# CLAUDE.md — EventSourcingBankAccountDemo

Console com conta bancária modelada por eventos: estado derivado, concorrência otimista por versão e snapshots. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/EventSourcingBankAccountDemo/EventSourcingBankAccountDemo.csproj
dotnet run --project 08-ArchitecturalPatterns/EventSourcingBankAccountDemo/EventSourcingBankAccountDemo.csproj
```

Roda cinco cenários e termina. Sem serviço externo.

## Estrutura interna

`Domain/BankAccount` tem a separação que define o padrão e **não pode ser dissolvida**:

- Os métodos de comando (`Deposit`, `Withdraw`, `Freeze`) **validam** e chamam `Raise`.
- `Apply` **não valida nada** — só muda campos. Eventos do passado já aconteceram e precisam continuar aplicáveis mesmo que a regra de hoje seja outra. Colocar validação em `Apply` quebra a releitura de fluxos antigos, e o sintoma aparece só quando alguém recarrega uma conta velha.

`Raise` incrementa a versão e guarda o evento em `_uncommitted`; `Persist` no runner calcula a versão esperada como `Version - UncommittedEvents.Count`.

`Store/EventStore.Append` compara a versão esperada com a atual e lança `ConcurrencyConflictException`. É append-only: não há update nem delete, de propósito.

`EventStore.Load(accountId, useSnapshot)` existe com o parâmetro justamente para o cenário 4 comparar as duas leituras. O contador `EventsReadSinceReset` mede a diferença.

## Pontos de atenção

- TFM `net10.0`. A trilha é majoritariamente `net8.0`, mas a máquina tem só os runtimes 8.0 e 10.0; `net10.0` segue o que `CountryRulesTimeProviderDemo` já usa. Pacote: `Microsoft.Extensions.Logging.Console` 10.0.12.
- **O cenário 3 depende dos valores exatos.** B saca 1200 com saldo 1300 (válido na leitura); A saca 300 e o saldo vai a 1000; na releitura, os 1200 passam a ser inválidos. Uma versão anterior usava 900, e o saque continuava válido após o conflito — o cenário terminava em silêncio, sem mostrar a lição. Não mexer nos valores sem refazer a conta.
- `Section()` tem `Thread.Sleep(120)`. É por causa da fila do provider de console: sem a pausa, o cabeçalho escrito com `Console.WriteLine` aparece antes das mensagens do cenário anterior. Mesmo motivo do delay em `Program.cs`.
- Os números do cenário 4 (201 / 1 / 202 eventos) vêm do laço de 200 depósitos. Alterar o laço invalida a tabela do README.
- `AccountEvent` é `abstract record` com `Version` em `init` — `Raise` usa `with { Version = ... }`. Trocar para classe mutável permitiria alterar eventos já gravados, que é exatamente o que o padrão proíbe.
- O event store é em memória e **não** serializa os eventos. Versionamento de schema de evento é citado no README como cuidado de produção, mas não é demonstrado aqui.
- Projeções e consultas por atributo não existem neste projeto — é a fraqueza reconhecida do modelo, e o encaminhamento é `CQRSDemo`.
- **Fronteira com os vizinhos**: `CQRSDemo` cobre a separação leitura/escrita; `CarriedEvent` trata de eventos persistidos em outro contexto; `SagaPattern` coordena transações distribuídas. Aqui o assunto é só o agregado e o seu fluxo.
