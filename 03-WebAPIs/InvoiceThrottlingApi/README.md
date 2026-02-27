# Invoice Throttling API

Projeto de demonstração de técnicas de **throttling** (controle de taxa) em C# .NET 9 usando o domínio de processamento de **1000 notas fiscais**.

## 🎯 Objetivo

Demonstrar diferentes estratégias de controle de concorrência e taxa de processamento para evitar sobrecarga de recursos ao processar grandes volumes de notas fiscais.

## 🏗️ Arquitetura

O projeto implementa três estratégias de throttling:

### 1. **Processamento Ilimitado (Baseline)**
- Processa todas as notas fiscais simultaneamente sem controle
- Útil para comparação de performance
- Pode sobrecarregar recursos do sistema

### 2. **Semaphore (Controle de Concorrência)**
- Limita o número de processamentos simultâneos
- Exemplo: máximo de 50 notas fiscais sendo processadas ao mesmo tempo
- Ideal para controlar uso de threads/conexões

### 3. **Rate Limiter (Controle de Taxa)**
- Limita o número de requisições por unidade de tempo
- Exemplo: 100 requisições por segundo
- Usa `System.Threading.RateLimiting` (.NET 7+)
- Implementa padrão Fixed Window

### 4. **Rate Limiting de API**
- Proteção de endpoints HTTP
- Três algoritmos disponíveis:
  - **Fixed Window**: 10 requisições por minuto
  - **Sliding Window**: 20 requisições por minuto (4 segmentos)
  - **Token Bucket**: 100 tokens, reposição de 50 a cada 10 segundos

## 📦 Componentes

### Models
- `Invoice`: Nota fiscal com itens, cliente e valor total
- `InvoiceItem`: Item da nota fiscal
- `InvoiceStatus`: Status do processamento (Pending, Processing, Processed, Failed, ThrottledRejected)
- `InvoiceProcessingResult`: Resultado do processamento de uma nota
- `BatchProcessingResult`: Resultado do processamento em lote

### Services
- `IInvoiceGenerator` / `InvoiceGenerator`: Gera notas fiscais fictícias
- `IInvoiceProcessor` / `InvoiceProcessor`: Processa notas fiscais com diferentes estratégias

### Controllers
- `InvoiceController`: Endpoints para geração e processamento de notas fiscais

## 📁 Estrutura do Projeto

```
InvoiceThrottlingApi/
├── Properties/
│   └── launchSettings.json
├── InvoiceAppSettings.json      # Configurações da aplicação
├── InvoiceController.cs         # Controller com endpoints de throttling
├── InvoiceGenerator.cs          # Serviço de geração de notas fiscais
├── InvoiceModels.cs             # Models (Invoice, InvoiceItem, etc.)
├── InvoiceProcessor.cs          # Serviço de processamento com throttling
├── InvoiceProgram.cs            # Entry point da aplicação
├── InvoiceThrottlingApi.csproj  # Arquivo de projeto
└── README.md
```

## 🚀 Como Executar

### 1. Restaurar dependências

```bash
cd 03-WebAPIs/InvoiceThrottlingApi
dotnet restore
```

### 2. Executar o projeto

```bash
dotnet run
```

### 3. Acessar Swagger

Abra o navegador em: `https://localhost:5001/swagger`

## 📝 Endpoints Principais

### `POST /api/invoice/process/demo1000`

Executa uma demonstração completa processando 1000 notas fiscais com as três estratégias:

```json
{
  "TotalInvoices": 1000,
  "Results": {
    "Unlimited": {
      "Processed": 1000,
      "Failed": 0,
      "ThrottledRejected": 0,
      "DurationMs": 245.6
    },
    "Semaphore": {
      "MaxConcurrency": 50,
      "Processed": 1000,
      "Failed": 0,
      "ThrottledRejected": 0,
      "DurationMs": 532.1
    },
    "RateLimit": {
      "RequestsPerSecond": 100,
      "Processed": 1000,
      "Failed": 0,
      "ThrottledRejected": 0,
      "DurationMs": 10245.8
    }
  }
}
```

### `GET /api/invoice/generate/{count}`

Gera N notas fiscais para testes:

```bash
curl https://localhost:5001/api/invoice/generate/1000
```

### `POST /api/invoice/process/unlimited`

Processa notas sem controle de concorrência:

```bash
curl -X POST https://localhost:5001/api/invoice/process/unlimited \
  -H "Content-Type: application/json" \
  -d @invoices.json
```

### `POST /api/invoice/process/semaphore/{maxConcurrency}`

Processa com controle de concorrência:

```bash
curl -X POST https://localhost:5001/api/invoice/process/semaphore/50 \
  -H "Content-Type: application/json" \
  -d @invoices.json
```

### `POST /api/invoice/process/ratelimit/{requestsPerSecond}`

Processa com controle de taxa:

```bash
curl -X POST https://localhost:5001/api/invoice/process/ratelimit/100 \
  -H "Content-Type: application/json" \
  -d @invoices.json
```

### `GET /api/invoice/api-limited`

Endpoint com rate limiting (10 requisições por minuto):

```bash
curl https://localhost:5001/api/invoice/api-limited
```

## 🧪 Testes Manuais

### Cenário 1: Comparação de Performance

1. Gere 1000 notas fiscais
2. Chame o endpoint `/api/invoice/process/demo1000`
3. Compare os tempos de execução das três estratégias

**Resultado Esperado:**
- Unlimited: mais rápido, mas uso intenso de recursos
- Semaphore: tempo intermediário, uso controlado de threads
- Rate Limiter: mais lento, respeita limite de taxa

### Cenário 2: Teste de Rate Limiting de API

1. Faça 15 requisições consecutivas para `/api/invoice/api-limited`
2. As primeiras 10 devem retornar 200 OK
3. As seguintes devem retornar 429 Too Many Requests

```bash
for i in {1..15}; do
  curl -w "\n%{http_code}\n" https://localhost:5001/api/invoice/api-limited
  sleep 0.1
done
```

### Cenário 3: Teste com Volumes Diferentes

```bash
# 100 notas
curl -X POST https://localhost:5001/api/invoice/generate/100 -o 100-invoices.json
curl -X POST https://localhost:5001/api/invoice/process/semaphore/20 \
  -H "Content-Type: application/json" \
  -d @100-invoices.json

# 500 notas
curl -X POST https://localhost:5001/api/invoice/generate/500 -o 500-invoices.json
curl -X POST https://localhost:5001/api/invoice/process/semaphore/50 \
  -H "Content-Type: application/json" \
  -d @500-invoices.json

# 1000 notas
curl -X POST https://localhost:5001/api/invoice/generate/1000 -o 1000-invoices.json
curl -X POST https://localhost:5001/api/invoice/process/ratelimit/100 \
  -H "Content-Type: application/json" \
  -d @1000-invoices.json
```

## 🔧 Configurações de Throttling

### Ajustar Semaphore

No controller, altere o parâmetro `maxConcurrency`:

```csharp
ProcessWithSemaphore([FromBody] List<Invoice> invoices, int maxConcurrency = 50)
```

### Ajustar Rate Limiter

No controller, altere o parâmetro `requestsPerSecond`:

```csharp
ProcessWithRateLimit([FromBody] List<Invoice> invoices, int requestsPerSecond = 100)
```

### Ajustar Rate Limiting de API

No `Program.cs`, configure os limitadores:

```csharp
options.AddFixedWindowLimiter("fixed", opt =>
{
    opt.PermitLimit = 10;
    opt.Window = TimeSpan.FromMinutes(1);
});
```

## 📊 Métricas Coletadas

Para cada estratégia, o sistema retorna:

- **TotalInvoices**: Total de notas no lote
- **Processed**: Notas processadas com sucesso
- **Failed**: Notas que falharam no processamento
- **ThrottledRejected**: Notas rejeitadas por limite de taxa
- **Duration**: Tempo total de processamento

## 🎓 Conceitos Demonstrados

1. **Throttling vs Rate Limiting**
   - Throttling: controle de concorrência (quantos ao mesmo tempo)
   - Rate Limiting: controle de taxa (quantos por período de tempo)

2. **SemaphoreSlim**
   - Controla acesso concorrente a recursos
   - Não garante taxa constante ao longo do tempo

3. **System.Threading.RateLimiting**
   - Algoritmos: Fixed Window, Sliding Window, Token Bucket, Concurrency
   - Garante distribuição temporal das requisições

4. **ASP.NET Core Rate Limiting Middleware**
   - Proteção de endpoints HTTP
   - Resposta 429 Too Many Requests
   - Metadata RetryAfter

## 🔗 Dependências

- **.NET 9.0**
- **Microsoft.AspNetCore.OpenApi** 9.0.0
- **Swashbuckle.AspNetCore** 7.2.0
- **System.Threading.RateLimiting** 9.0.0 (incluído no .NET 9)

## 📚 Recursos Adicionais

- [Rate Limiting Middleware in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [System.Threading.RateLimiting](https://learn.microsoft.com/en-us/dotnet/api/system.threading.ratelimiting)
- [SemaphoreSlim Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim)

## 🐛 Troubleshooting

### Erro: "PermitLimit must be greater than zero"

Certifique-se de que os parâmetros de rate limiting sejam positivos.

### Alto uso de memória

Reduza o `maxConcurrency` do semaphore ou o `requestsPerSecond` do rate limiter.

### 429 Too Many Requests

Aguarde o período da janela (window) ou aumente o `PermitLimit`.

## 📄 Licença

Este é um projeto educacional para demonstração de conceitos de throttling em .NET.
