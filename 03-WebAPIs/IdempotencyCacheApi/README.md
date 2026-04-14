# Idempotency Cache API

Projeto educacional em ASP.NET Core Web API para demonstrar como aplicar chave de idempotencia em requests de escrita, com validacao de payload e invalidacao explicita de cache.

## Conceitos Abordados

- Idempotencia em operacoes HTTP de escrita
- Validacao de entrada com DataAnnotations
- Cache em memoria com IMemoryCache
- Fingerprint do payload para detectar reuso indevido da chave
- Invalidacao manual de cache por endpoint

## Objetivos de Aprendizado

- Entender como evitar processamento duplicado de uma mesma request
- Garantir que a mesma chave nao seja reutilizada com payload diferente
- Reaproveitar resposta em chamadas repetidas (replay controlado)
- Implementar invalidacao de cache para liberar chave antes da expiracao

## Estrutura do Projeto

```text
IdempotencyCacheApi/
|-- Controllers/
|   |-- PaymentsController.cs
|-- Models/
|   |-- IdempotencyCacheEntry.cs
|   |-- IdempotencyCacheOptions.cs
|   |-- IdempotencyExecutionResult.cs
|   |-- IdempotencyExecutionStatus.cs
|   |-- PaymentRequest.cs
|   |-- PaymentResponse.cs
|-- Services/
|   |-- IIdempotencyService.cs
|   |-- IdempotencyService.cs
|   |-- IPaymentProcessor.cs
|   |-- PaymentProcessor.cs
|-- Program.cs
|-- appsettings.json
|-- IdempotencyCacheApi.http
|-- IdempotencyCacheApi.csproj
|-- README.md
```

## Fluxo de Idempotencia

1. Cliente envia POST com header `Idempotency-Key`.
2. API valida o payload da request.
3. Servico gera hash do payload (`orderId|amount|currency|description`).
4. Cache e consultado pela chave de idempotencia.
5. Se chave nao existe: processa pagamento, salva resposta no cache e retorna `200`.
6. Se chave existe com mesmo hash: retorna resposta em replay (`X-Idempotency-Replay: true`).
7. Se chave existe com hash diferente: retorna `409 Conflict`.

## Endpoints

### POST /api/payments

Cria um pagamento idempotente.

Headers obrigatorios:

- `Idempotency-Key`: identificador unico da operacao

Exemplo de body:

```json
{
  "orderId": "ORDER-1001",
  "amount": 199.90,
  "currency": "BRL",
  "description": "Payment for premium plan"
}
```

### DELETE /api/payments/cache/{idempotencyKey}

Invalida a chave no cache para permitir novo processamento antes da expiracao natural.

## Como Executar

```bash
cd 03-WebAPIs/IdempotencyCacheApi
dotnet restore
dotnet run
```

Durante desenvolvimento, o OpenAPI fica disponivel no endpoint mapeado por `MapOpenApi()`.

## Como Testar Rapido

Use o arquivo `IdempotencyCacheApi.http`:

1. Execute o primeiro POST com uma chave fixa.
2. Execute o segundo POST com a mesma chave e mesmo payload (deve retornar replay).
3. Execute o DELETE para invalidar cache.
4. Repita o POST para observar novo processamento.

## Boas Praticas e Pontos de Atencao

- Nao reutilizar a mesma chave para payloads diferentes.
- Definir expiracao de cache apropriada ao SLA da operacao.
- Registrar logs do fluxo para auditoria.
- Em cenario distribuido, trocar IMemoryCache por cache centralizado (ex: Redis).
