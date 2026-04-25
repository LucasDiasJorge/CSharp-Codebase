# Plano de ação para melhorias no CustomFilterApi

## TODOs

### 1. Refatorar validação de cabeçalhos no filtro
**Objetivo:** Tornar a seleção de serviço mais flexível, testável e configurável.

**Subtarefas:**
- [ ] Extrair lógica de parsing/validação de cabeçalhos para uma classe separada (e.g., `HeaderSelector`)
- [ ] Suportar múltiplos critérios de seleção (ex: cabeçalho X-Service, X-Region, X-Role)
- [ ] Implementar políticas configuráveis (ordenadas) para avaliar critérios e decidir o serviço
- [ ] Implementar fallback para um serviço padrão quando nenhum critério casar
- [ ] Adicionar cobertura de testes unitários para cada política e para o fluxo completo
- [ ] Atualizar `LogPropertyFilter` para delegar seleção ao `HeaderSelector`
- [ ] Atualizar `SelectedServiceAccessor` para aceitar seleção via estratégia/serviço

**Critérios de aceite:**
- [ ] A seleção deve suportar pelo menos 3 critérios diferentes configuráveis por código e por appsettings
- [ ] Em caso de cabeçalhos ausentes ou inválidos, o sistema deve usar o serviço de fallback sem lançar exceções não tratadas
- [ ] Cobertura de testes: testes unitários cobrindo parsing, políticas e fallback (mínimo 80% para os componentes novos)
- [ ] Controllers existentes continuam funcionando sem mudanças na assinatura/rotas

**Arquivos-alvo:**
- `Filters/LogPropertyFilter.cs` (delegar seleção)
- `Services/SelectedServiceAccessor.cs` (aceitar injeção da estratégia)
- `Attributes/LogPropertyAttribute.cs` (verificar se precisa de ajustes)
- Novo arquivo: `Services/HeaderSelector.cs` ou `Filters/HeaderSelector.cs`

**Notas de implementação:**
   - Usar pattern Strategy/Policy para facilitar testes e extensão
   - Permitir configuração via `IConfiguration` (appsettings.json) para mapear valores de cabeçalho para serviços
   - Evitar lógica complexa dentro do filtro (manter filtro como orquestrador)
   - Garantir que a injeção de dependência registre políticas como `IEnumerable<IHeaderPolicy>` para avaliação ordenada


### 2. Centralizar tratamento de erros
**Objetivo:** Ter um ponto único que capture exceções, padronize respostas de erro e registre informações úteis para diagnóstico.

**Subtarefas:**
- [ ] Implementar um middleware `ErrorHandlingMiddleware` que capture exceções não tratadas e retorne um payload padronizado
- [ ] Definir um modelo de erro padrão (e.g., `ErrorResponse` com campos `traceId`, `message`, `details`, `code`)
- [ ] Mapear exceções customizadas para códigos HTTP (e.g., `ValidationException` -> 400, `NotFoundException` -> 404)
- [ ] Integrar o middleware com `ILogger` para logs estruturados (incluir `traceId`, headers relevantes e contexto do usuário se disponível)
- [ ] Atualizar `Program.cs` para registrar o middleware na pipeline e garantir execução precoce
- [ ] Fornecer extensão `IApplicationBuilder.UseErrorHandling()` para fácil reutilização
- [ ] Documentar o formato de erro no README e adicionar exemplos HTTP (cURL/HTTPie)

**Critérios de aceite:**
- [ ] Todas as exceções não tratadas retornam um JSON com o modelo `ErrorResponse`
- [ ] Exceções esperadas (ex.: validação) retornam códigos HTTP apropriados com mensagens amigáveis
- [ ] Logs contêm `traceId` correlacionável com a resposta retornada ao cliente
- [ ] Middleware testado com pelo menos 3 cenários (exceção genérica, exceção de validação, not found)

**Arquivos-alvo:**
- Novo arquivo: `Middlewares/ErrorHandlingMiddleware.cs`
- Novo arquivo: `Models/ErrorResponse.cs`
- `Program.cs` (pipeline e registro de middleware)
- `Controllers/*` (opcionalmente lançar exceções customizadas para testes)

**Notas de implementação:**
- Usar `Activity.Current?.Id` ou `HttpContext.TraceIdentifier` para gerar/propagar `traceId`
- Evitar vazar stack traces em produção; incluir `details` opcionalmente apenas em Development
- Permitir extensão para enviar eventos a um sistema APM (ex.: Application Insights, Sentry)
- Considerar compatibilidade com clientes que esperam mensagens de erro antigas — migrar gradualmente

---

### 3. Expandir documentação
**Objetivo:** Tornar o projeto autoexplicativo para novos contribuidores e consumidores, facilitando a adoção e testes manuais.

**Subtarefas:**
- [ ] Atualizar `README.md` com:
  - [ ] Descrição do propósito do projeto
  - [ ] Como rodar localmente (dotnet restore/build/run)
  - [ ] Exemplos de requests e respostas (cURL e `CustomFilterApi.http` snippets)
  - [ ] Como configurar os mapeamentos de cabeçalhos em `appsettings.json`
  - [ ] Como adicionar novos serviços e políticas
- [ ] Adicionar um `CONTRIBUTING.md` com convenções de código e como rodar testes
- [ ] Incluir um diagrama simples (PNG/SVG) mostrando o fluxo: Request -> Middleware/Filter -> HeaderSelector -> Service -> Controller -> Response
- [ ] Preencher `SERVICE_SELECTION_TODO.md` com exemplos práticos e cases de uso
- [ ] Adicionar exemplos de logs esperados e como interpretar o `traceId` em `LOG_EXAMPLES.md`
- [ ] Criar um `docs/` pequeno com:
  - [ ] FAQ sobre cabeçalhos suportados
  - [ ] Lista de erros padronizados e formatos

**Critérios de aceite:**
- [ ] `README.md` contém instruções completas para rodar o projeto localmente e exemplos de requisições que funcionam com a solução atual
- [ ] `CONTRIBUTING.md` cobre como executar e escrever testes, e o padrão de commits/PRs
- [ ] Diagramas embutidos no repositório e referenciados do README
- [ ] `SERVICE_SELECTION_TODO.md` atualizado com pelo menos 3 cenários práticos

**Arquivos-alvo:**
- `README.md`
- `CONTRIBUTING.md` (novo)
- `SERVICE_SELECTION_TODO.md` (atualizar)
- `LOG_EXAMPLES.md` (atualizar)
- `docs/architecture.png` ou `docs/architecture.svg` (novo)

**Notas de implementação:**
- Incluir comandos copy-paste para PowerShell (Windows) e bash (Linux/macOS) nas seções de run/test
- Usar `CustomFilterApi.http` como fonte de exemplos executáveis (VS Code REST Client)
- Manter linguagem simples e direta; exemplos práticos ajudam mais que longas explicações teóricas

---

### 4. Adicionar testes unitários
**Objetivo:** Garantir comportamento correto do filtro, seleção de serviço e controllers, permitindo mudanças seguras e regressões controladas.

**Subtarefas:**
- [ ] Criar um projeto de testes `CustomFilterApi.Tests` usando xUnit
- [ ] Adicionar dependências: `Moq` para mocks, `FluentAssertions` para asserções legíveis
- [ ] Testes para `HeaderSelector`/policies:
  - [ ] Validar parsing de cabeçalhos válidos e inválidos
  - [ ] Validar seleção correta quando múltiplos critérios aplicáveis
  - [ ] Validar fallback quando nenhum critério casar
- [ ] Testes para `LogPropertyFilter` (ou filtro equivalente):
  - [ ] Simular `HttpContext` com cabeçalhos e verificar que o `SelectedServiceAccessor` recebe a instância correta
  - [ ] Garantir que o filtro não lança exceções para cenários inválidos (usa fallback)
- [ ] Testes para `SelectedServiceAccessor` e serviços:
  - [ ] Verificar injeção e resolução de serviços (mockar serviços A/B)
  - [ ] Validar comportamento esperado para cada serviço (usar fakes/mocks com respostas previsíveis)
- [ ] Testes de integração leves (in-memory):
  - [ ] Usar `WebApplicationFactory<TEntryPoint>` para testar controllers end-to-end com middleware e filtros ativos
  - [ ] Testar 3 endpoints principais (Users, Products) cobrindo seleção de serviço via cabeçalhos

**Critérios de aceite:**
- [ ] Projeto de testes compila e roda localmente com `dotnet test`
- [ ] Cobertura mínima de testes para componentes novos (HeaderSelector, filtro) >= 80%
- [ ] Testes de integração cobrem fluxo completo para ao menos 2 cenários distintos (serviço A e serviço B)

**Arquivos-alvo / Estrutura de teste sugerida:**
- `tests/CustomFilterApi.Tests/` (novo projeto)
  - `HeaderSelectorTests.cs`
  - `LogPropertyFilterTests.cs`
  - `SelectedServiceAccessorTests.cs`
  - `Integration/ControllersIntegrationTests.cs`

**Notas de implementação:**
- Preferir testes pequenos, determinísticos e rápidos; reserve testes de integração para cenários end-to-end
- Usar `HttpContext` builder helpers para facilitar a criação de contextos de requisição nos testes de filtro
- Mockar `IConfiguration` quando necessário para controlar mapeamentos de cabeçalhos
- Integrar comando `dotnet test` ao README e ao `CONTRIBUTING.md`

---

### 5. Melhorar logging
**Objetivo:** Tornar os logs mais úteis para diagnóstico e monitoramento, preservando privacidade e performance.

**Subtarefas:**
- [ ] Revisar `Attributes/LogPropertyAttribute.cs` para garantir clareza semântico (ex: `LogSensitive = true/false`)
- [ ] Atualizar `LogPropertyFilter` para capturar as propriedades marcadas pelo atributo e enviá-las ao `ILogger` de forma estruturada
- [ ] Padronizar o formato do evento de log (ex: `LogLevel`, `EventId`, `Properties` com `traceId` e `selectedService`)
- [ ] Integrar um provider de logging configurável (por padrão `ILogger` + Console); adicionar instruções para integrar `Serilog` ou outro sink
- [ ] Implementar masking para valores sensíveis configuráveis (ex: números, tokens)
- [ ] Adicionar exemplos em `LOG_EXAMPLES.md` mostrando linhas de log em JSON e formato legível
- [ ] Escrever testes unitários que validem que propriedades corretas são incluídas/omitidas no log

**Critérios de aceite:**
- [ ] Logs incluem `traceId` e `selectedService` em todos os eventos relevantes
- [ ] Propriedades marcadas como sensíveis não aparecem em texto plano nos logs por padrão
- [ ] É fácil trocar o provider de logging por configuração (appsettings)
- [ ] Testes cobrem máscaras e inclusão de propriedades (mínimo 80% para componentes novos)

**Arquivos-alvo:**
- `Attributes/LogPropertyAttribute.cs` (atualizar assinatura e XML docs)
- `Filters/LogPropertyFilter.cs` (emitir logs estruturados)
- `LOG_EXAMPLES.md` (atualizar com exemplos JSON e texto)
- `Program.cs` (registrar provider configurável e instruções de Serilog)

**Notas de implementação:**
- Use `ILogger.BeginScope()` para propagar `traceId` e `selectedService` nos logs de forma automática
- Considere usar `System.Text.Json` para serializar propriedades complexas nos logs quando necessário
- Evite operações de logging custosas no caminho crítico; agregue ou serialize de forma eficiente
- Documente como desabilitar logs detalhados em produção via `appsettings` por nível

---

### 6. Preparar para escalabilidade
**Objetivo:** Estruturar o projeto para crescimento sem comprometer manutenção, deploys e performance.

**Subtarefas:**
- [ ] Definir convenções para adicionar novos serviços (naming, pasta `Services/`, interfaces, registro DI)
- [ ] Criar um `SERVICE_TEMPLATE.md` mostrando passo-a-passo para adicionar um novo `IBusinessService` e testá-lo localmente
- [ ] Padronizar registro de serviços em `Program.cs` (ex: registrar por convention ou usar extensão `services.AddBusinessServices()`)
- [ ] Documentar estratégia de versionamento de API (URI versioning vs header vs media type)
- [ ] Planejar estratégia de deploy (releases menores, feature flags) e como testar em staging
- [ ] Definir métricas e monitoramento mínimos (latência, erros, taxa de acerto de seleção de serviço)
- [ ] Preparar exemplos de CI/CD (GitHub Actions) com etapas: build, test, lint, publish
- [ ] Avaliar necessidade de caching e circuit-breakers para serviços externos

**Critérios de aceite:**
- [ ] Existe um documento `SERVICE_TEMPLATE.md` com passos claros para adicionar e registrar um novo serviço
- [ ] `Program.cs` contém uma extensão ou padrão claro para registro de serviços por convenção
- [ ] Há pelo menos uma proposta documentada de versionamento de API com prós/cons e exemplo de rota
- [ ] Pipeline de CI básico (ex.: GitHub Actions YAML) esboçado no repositório

**Arquivos-alvo:**
- `SERVICE_TEMPLATE.md` (novo)
- `docs/ci/github-actions.yml` ou `.github/workflows/ci.yml` (exemplo de pipeline)
- `README.md` (seção sobre versionamento e deploy)
- `Program.cs` (opcional: adicionar `AddBusinessServices` extension)

**Notas de implementação:**
- Para registro por convenção, use reflexão limitada (`Assembly.GetTypes()` com filtros) em tempo de startup; documente implicações de performance
- Recomendação inicial: usar URL versioning (/v1/products) para simplicidade em exemplos, com nota sobre alternativas
- Use feature flags para rotear tráfego para novos serviços durante rollout
- Comece com um pipeline CI simples que rode `dotnet restore`, `dotnet build`, `dotnet test` e publique artefato; evolua conforme necessário

---

### 7. Melhorias futuras e gaps identificados
**Objetivo:** Preparar o projeto para requisitos corporativos e produção.

**Subtarefas:**

**7.1 Segurança e Autenticação**
- [ ] Implementar autenticação (JWT Bearer, API Keys, ou OAuth2)
- [ ] Adicionar autorização baseada em roles/policies
- [ ] Configurar HTTPS obrigatório e HSTS
- [ ] Implementar validação de input (FluentValidation ou DataAnnotations)
- [ ] Adicionar proteção contra CSRF se necessário

**7.2 Rate Limiting e Throttling**
- [ ] Implementar rate limiting por IP/usuário usando `AspNetCoreRateLimit` ou middleware customizado
- [ ] Configurar políticas diferentes por endpoint (ex: 100 req/min para leitura, 10 req/min para escrita)
- [ ] Adicionar headers de rate limit nas respostas (X-RateLimit-Limit, X-RateLimit-Remaining)

**7.3 CORS e Configuração de Ambiente**
- [ ] Configurar CORS adequadamente com origens permitidas por ambiente
- [ ] Separar configurações por ambiente (Development, Staging, Production) em `appsettings.{Environment}.json`
- [ ] Adicionar variáveis de ambiente para segredos (connection strings, API keys)
- [ ] Documentar como usar Azure Key Vault ou similar para gestão de segredos

**7.4 Health Checks e Observabilidade**
- [ ] Implementar endpoints de health check (`/health`, `/ready`) usando `Microsoft.Extensions.Diagnostics.HealthChecks`
- [ ] Adicionar checks para dependências externas (banco de dados, cache, APIs)
- [ ] Configurar liveness e readiness probes para Kubernetes/Docker
- [ ] Integrar com Application Insights, Prometheus, ou Grafana para métricas

**7.5 Swagger/OpenAPI Avançado**
- [ ] Melhorar documentação Swagger com exemplos de request/response
- [ ] Adicionar autenticação no Swagger UI (bearer token)
- [ ] Configurar XML comments para documentação automática
- [ ] Adicionar versionamento na documentação Swagger

**7.6 Performance e Otimização**
- [ ] Implementar caching (in-memory, Redis, distributed cache)
- [ ] Adicionar compressão de resposta (Gzip, Brotli)
- [ ] Implementar circuit breakers usando Polly para chamadas externas
- [ ] Criar benchmarks de performance usando BenchmarkDotNet
- [ ] Configurar response caching headers apropriados

**7.7 Containerização e Deploy**
- [ ] Criar Dockerfile otimizado (multi-stage build)
- [ ] Criar docker-compose.yml para desenvolvimento local
- [ ] Adicionar .dockerignore adequado
- [ ] Documentar deploy em Azure App Service, AWS ECS, ou Kubernetes
- [ ] Preparar manifests Kubernetes (deployment, service, ingress) se aplicável

**7.8 Persistência de Dados (se necessário)**
- [ ] Escolher e configurar ORM (Entity Framework Core, Dapper)
- [ ] Implementar pattern Repository/Unit of Work se apropriado
- [ ] Configurar migrações de banco de dados
- [ ] Adicionar seeding de dados para desenvolvimento
- [ ] Implementar estratégia de backup e recuperação

**Critérios de aceite:**
- [ ] Pelo menos 3 itens de segurança implementados
- [ ] Health checks funcionais e testáveis
- [ ] Projeto containerizado e executável via Docker
- [ ] Documentação atualizada para refletir novas features

**Notas de implementação:**
- Priorize segurança e health checks antes de outras features
- Rate limiting pode ser crucial para APIs públicas
- Containerização facilita deploys e CI/CD
- Não implemente persistência a menos que seja necessária para o caso de uso

---

## Progresso geral

**Legenda:**
- [ ] Não iniciado
- [x] Concluído

**Próximos passos sugeridos:**
1. Começar pelo tópico 1 (Refatorar validação de cabeçalhos) para estabelecer a base
2. Implementar tópico 2 (Tratamento de erros) em paralelo
3. Adicionar testes (tópico 4) à medida que implementa novas features
4. Documentar (tópico 3) continuamente durante o desenvolvimento
5. Revisar tópico 7 para identificar gaps críticos antes de ir para produção
