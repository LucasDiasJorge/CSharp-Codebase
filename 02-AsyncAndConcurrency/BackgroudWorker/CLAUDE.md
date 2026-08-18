# CLAUDE.md — BackgroudWorker

Web app ASP.NET Core hospedando um `IHostedService` com timer periódico. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/BackgroudWorker/BackgroudWorker.csproj
dotnet run --project 02-AsyncAndConcurrency/BackgroudWorker/BackgroudWorker.csproj
```

## Estrutura interna

- `TimedHostedService.cs` — o exemplo em si: `StartAsync` arma o timer, o callback executa o trabalho recorrente, `StopAsync` desarma e libera. O ciclo de vida completo é o conteúdo didático.
- `Program.cs` — registra o hosted service no contêiner e sobe o host web.

O SDK é `Microsoft.NET.Sdk.Web` mesmo o foco não sendo HTTP: o host web é apenas o processo que mantém o serviço vivo.

## Pontos de atenção

- TFM `net9.0`.
- O nome do diretório e do assembly tem o typo **`Backgroud`** (falta o `n`). É o nome real em disco, no `.sln` e no índice do README raiz — use como está nos comandos; renomear exige atualizar os três.
- O `.csproj` referencia um pacote **`Worker` 1.0.0**, que não é um pacote oficial da Microsoft e aparentemente não é usado pelo código. Confirme a necessidade antes de replicar esse `.csproj` em outro exemplo.
