# CLAUDE.md — TestDoublesDemo

Biblioteca de domínio + suíte xUnit com os cinco tipos de dublê escritos à mão e a comparação entre verificação de estado e de comportamento. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet test 12-Testing/TestDoublesDemo/TestDoublesDemo.Tests/TestDoublesDemo.Tests.csproj
dotnet build 12-Testing/TestDoublesDemo/TestDoublesDemo.csproj

# Um teste especifico
dotnet test 12-Testing/TestDoublesDemo/TestDoublesDemo.Tests/TestDoublesDemo.Tests.csproj --filter "FullyQualifiedName~MockSobreContagemDeChamadas"
```

10 testes, todos passando. Sem serviço externo.

## Estrutura interna

`Abstractions/Collaborators.cs` tem quatro interfaces escolhidas para **forçar** um dublê diferente em cada uma: `IInventory` é consulta (stub), `INotificationSender` é comando sem retorno (spy/mock), `IOrderRepository` tem estado (fake), `IAuditLog` fica fora do caminho do pedido vazio (dummy). Os comentários de cada interface dizem qual papel ela existe para ensinar — **não trocar as assinaturas** sem revisar os testes correspondentes.

`Checkout/OrderCheckout.cs` tem `OrderCheckoutBase` com a regra e **duas** subclasses que diferem só em `Notify`. Essa duplicação aparente é o exemplo: `PerItemOrderCheckout` envia uma mensagem por item, `BatchedOrderCheckout` agrupa. O resultado de negócio é idêntico, e é isso que permite mostrar que verificação de comportamento quebra onde a de estado não quebra. **Unificar as duas classes destrói o cenário central.**

`TestDoublesDemo.Tests/Doubles/Doubles.cs` tem os cinco dublês à mão, cada um com o comentário do que o define. `DummyAuditLog.Record` **lança de propósito**; deixar silencioso remove a asserção implícita.

`MockNotificationSender.VerifyExpectations` é o que caracteriza o mock: a cobrança mora no dublê. O teste `MockSobreContagemDeChamadas_FalhaNaRefatoracaoQueNaoMudouOResultado` verifica a mensagem exata `"mock esperava 2 envio(s), recebeu 1"` — mudar o texto da exceção quebra esse teste.

## Pontos de atenção

- TFM `net10.0` nos dois projetos. A trilha tem `OrderRuleConsole` em `net9.0`, que compila mas **não roda** (o runtime net9.0 não está instalado nesta máquina). Para `dotnet test` funcionar, projeto novo de teste precisa ser `net10.0`.
- **O projeto de testes fica aninhado na pasta da biblioteca** (convenção do `12-Testing`, igual a `OrderRuleConsole`). Isso exige o `<Compile Remove="TestDoublesDemo.Tests\**\*.cs" />` no `.csproj` da biblioteca — sem ele, a biblioteca tenta compilar os testes e falha com dezenas de `CS0246` sobre `Fact`/`Theory`, apontando para o **csproj errado** na mensagem. Foi exatamente o que aconteceu ao criar este projeto.
- Pacotes do teste: `xunit` 2.9.3, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector`, `NSubstitute`.
- `Assert.Contains(result.Reason)` depende de `Reason` ser `string?` não nulo naquele caminho — o `OutOfStock` sempre preenche.
- O teste do `NSubstitute` usa `Received(2)` de propósito, para mostrar o papel de mock com biblioteca. Ele é **igualmente frágil** à refatoração; isso é intencional e está dito no README.
- **Fronteira com os vizinhos**: `OrderRuleConsole` cobre regra de negócio testada de forma direta. `PropertyBasedTestingDemo` trata do que verificar. Não expandir este exemplo para cobertura, benchmark ou teste de integração — cada um tem seu projeto.
