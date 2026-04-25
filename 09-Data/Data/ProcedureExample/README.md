# ProcedureExample

## Visão geral

Este projeto demonstra o uso de Procedures (Stored Procedures) em MySQL integradas a uma aplicação C#.

## Conceitos abordados

- Exemplo didático sobre ProcedureExample no contexto de persistência, bancos de dados e acesso a dados.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como ProcedureExample se aplica em um cenário prático de persistência, bancos de dados e acesso a dados.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
ProcedureExample/
+-- ProcedureExample.csproj
+-- ProcedureExample.sln
+-- Program.cs
\-- setup.sql
```

## Como executar

```bash
dotnet run --project 09-Data/Data/ProcedureExample/ProcedureExample.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Casos de Uso de Procedures

- **Encapsulamento de lógica de negócio**: Centraliza regras de negócio no banco, facilitando manutenção e reaproveitamento.
- **Melhoria de performance**: Reduz o tráfego entre aplicação e banco, executando múltiplas operações em uma única chamada.
- **Segurança**: Permite restringir permissões de acesso a dados sensíveis, expondo apenas procedures específicas.
- **Automação de tarefas**: Ideal para rotinas de importação, exportação, cálculos e manipulação de grandes volumes de dados.
- **Transações complexas**: Garante atomicidade e integridade em operações que envolvem múltiplas tabelas.

##### Quão "Limpa" é o Uso de Procedures?

O uso de procedures pode ser considerado "limpo" quando:
- A lógica encapsulada é estável e pouco sujeita a mudanças frequentes.
- O código SQL é bem documentado e versionado junto ao projeto.
- Há separação clara entre lógica de negócio (no banco) e lógica de apresentação (na aplicação).

Por outro lado, pode dificultar a manutenção se:
- A lógica de negócio muda frequentemente (dificultando deploys e versionamento).
- Não há documentação adequada das procedures.
- O time não tem domínio sobre SQL avançado.

**Resumo:** Procedures são poderosas para performance, segurança e automação, mas exigem disciplina para manter o código limpo, documentado e versionado.
