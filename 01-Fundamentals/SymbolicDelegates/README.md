# Symbolic Delegates

## Visão geral

Este estudo sobre *symbolic delegates* e a criação de uma linguagem de script é relevante por vários motivos:

- **Simplicidade poderosa**: a técnica mostra como construir uma linguagem útil com pouca infraestrutura — um dicionário de delegates e um avaliador simples.
- **Extensibilidade prática**: permite adicionar palavras-chave e funcionalidades dinamicamente, facilitando criação de DSLs e extensões específicas de domínio.
- **Segurança e controle**: a linguagem pode ser mantida intencionalmente limitada ("JSON para código"), reduzindo vetores de ataque ao expor apenas funções autorizadas.
- **Casos de uso reais**: ideal para rule engines, automação de fluxo de trabalho, plugins de usuário e endpoints dinâmicos que aceitam scripts seguros.
- **Valor pedagógico**: é um excelente exercício para entender delegates, parsing mínimo, binding de contexto e avaliação em tempo de execução — conceitos úteis além do exemplo.

Adicionar essa implementação como um módulo pequeno e independente facilita experimentação rápida sem comprometer a arquitetura principal da aplicação.

## Conceitos abordados

O projeto demonstra como criar uma linguagem de script simples usando delegates simbólicos em C#. A ideia é usar um dicionário de delegates para criar uma mini-linguagem de programação dinâmica.

### Principais Conceitos

- **Symbolic Delegates**: Uso de dicionários para mapear palavras-chave a delegates
- **Dynamic Code Evaluation**: Avaliar código dinamicamente em tempo de execução
- **Lizzie Language**: Linguagem de script completa criada com base nesses conceitos

## Objetivos de aprendizagem

- Entender como Symbolic Delegates se aplica em um cenário prático de conceitos fundamentais da linguagem C# e orientação a objetos.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
SymbolicDelegates/
+-- SymbolicDelegates/
+-- hello
+-- Program.cs
\-- SymbolicDelegates.csproj
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/SymbolicDelegates/SymbolicDelegates.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Exemplo 1: Mini Linguagem

O código atual demonstra a criação de uma mini-linguagem com palavras-chave "hello" e "world".

##### Exemplo 2: Chaining Delegates

O artigo também mostra como encadear delegates para transformar dados sequencialmente.

##### Lizzie Language

O artigo introduz "Lizzie", uma linguagem de programação completa Turing-complete criada em apenas 2000 linhas de código.

## Referências

- [Lizzie no GitHub](https://github.com/polterguy/lizzie)
- Artigo original da MSDN Magazine (Novembro 2018)
