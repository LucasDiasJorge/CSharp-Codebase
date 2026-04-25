# Projeto: Auditoria de Dados com Serilog

## Visão geral

Este documento serve como base para voce desenvolver um projeto focado em auditoria de dados usando Serilog no ecossistema .NET.

## Conceitos abordados

- Exemplo didático sobre Projeto: Auditoria de Dados com Serilog no contexto de utilitários, transformação de dados e observabilidade.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

Implementar trilha de auditoria para registrar:

1. Quem executou a acao
2. Qual acao foi executada
3. Em qual recurso
4. Quando ocorreu
5. Qual foi o resultado

## Estrutura do projeto

```text
SerilogDataAudit/
```

## Como executar

Consulte o código desta pasta e os projetos relacionados antes de executar comandos específicos.

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Escopo inicial sugerido

- API ASP.NET com autenticacao
- Logs estruturados com Serilog
- Correlacao por request e trace
- Registro de eventos de sucesso, cancelamento e falha
- Persistencia de logs em arquivo JSON (ou sink externo)

##### Pacotes recomendados

- Serilog.AspNetCore
- Serilog.Sinks.Console
- Serilog.Sinks.File
- Serilog.Formatting.Compact
- Serilog.Enrichers.Environment

##### Estrutura de auditoria (padrao)

Padronize propriedades em todos os eventos:

- audit_event
- audit_action
- user_id
- tenant_id (quando houver multi-tenant)
- resource
- trace_id
- request_id
- result
- error_type
- elapsed_ms

##### Exemplo de configuracao no Program.cs

```csharp
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());
```

##### Exemplo de configuracao no appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/audit-.json",
          "rollingInterval": "Day",
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName" ]
  }
}
```

##### Exemplo de evento auditavel em endpoint

```csharp
using var scope = logger.BeginScope(new Dictionary<string, object>
{
    ["audit_action"] = "forecast.read",
    ["resource"] = "forecast",
    ["trace_id"] = httpContext.TraceIdentifier,
    ["user_id"] = userId
});

logger.LogInformation("audit.forecast.requested days={Days}", days);

try
{
    var result = await service.GetAsync(days, ct);
    logger.LogInformation("audit.forecast.succeeded count={Count}", result.Count);
}
catch (OperationCanceledException)
{
    logger.LogWarning("audit.forecast.cancelled days={Days}", days);
    throw;
}
catch (Exception ex)
{
    logger.LogError(ex, "audit.forecast.failed days={Days}", days);
    throw;
}
```

##### Regras de seguranca para auditoria

- Nunca logar senha, token completo ou dados pessoais sensiveis.
- Preferir identificadores internos, hash ou mascaramento.
- Limitar payload logado ao minimo necessario.
- Definir retencao de logs e politica de acesso aos arquivos.

##### Roadmap de implementacao

1. Criar API base e endpoints criticos.
2. Configurar Serilog no bootstrap.
3. Definir contrato de campos de auditoria.
4. Instrumentar endpoints e casos de uso.
5. Validar logs por cenarios de sucesso e erro.
6. Conectar sink centralizado (ex: Seq, ELK, Datadog).

##### Checklist de pronto

- Eventos auditaveis padronizados por acao de negocio.
- Correlacao por request/trace funcionando.
- Falhas com contexto minimo e util.
- Ausencia de dados sensiveis nos logs.
- Retencao e acesso dos logs documentados.
