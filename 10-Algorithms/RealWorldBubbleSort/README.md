# RealWorldBubbleSort (Na verdade: Ordenação + Busca Binária)

## Visão geral

Exemplo prático: consumir usuários de API pública (`randomuser.me`), ordenar por `Username` e executar busca binária interativa. Nome histórico mantido, mas não há Bubble Sort (usa sort nativo eficiente do .NET).

## Conceitos abordados

- Exemplo didático sobre RealWorldBubbleSort (Na verdade: Ordenação + Busca Binária) no contexto de algoritmos, estruturas de dados e análise de cenários.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como RealWorldBubbleSort (Na verdade: Ordenação + Busca Binária) se aplica em um cenário prático de algoritmos, estruturas de dados e análise de cenários.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
RealWorldBubbleSort/
+-- RealWorldBubbleSort/
+-- Program.cs
\-- RealWorldBubbleSort.csproj
```

## Como executar

```bash
dotnet run --project 10-Algorithms/RealWorldBubbleSort/RealWorldBubbleSort.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### 1. Fluxo

| Etapa | Ação |
|-------|------|
| 1 | Requisição HTTP para API pública |
| 2 | Mapear JSON -> objetos `User` |
| 3 | Ordenar lista por Username (case-insensitive) |
| 4 | Solicitar input e realizar busca binária |

##### 3. Considerações

- Resultados aleatórios: usuários mudam a cada execução.
- Ordenação antes da busca é obrigatória (pré-condição da busca binária).
- `HttpClient` direto: em cenário real usar `IHttpClientFactory`.
- Falhas de rede não tratadas extensivamente (exercício para expansão).

##### 4. Possíveis Extensões

- Retry + timeout + circuit breaker.
- Testes unitários (ordenação, busca encontrada / não encontrada).
- Argumentos CLI: quantidade de usuários, filtro inicial.
- Paginação e streaming para grandes volumes.

##### 5. Aprendizado Chave

Combinação de: acesso HTTP → transformação → ordenação → algoritmo de busca → interação usuário.
