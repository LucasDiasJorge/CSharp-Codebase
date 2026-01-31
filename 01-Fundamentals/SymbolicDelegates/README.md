# Symbolic Delegates

Este projeto é um estudo baseado no artigo da Microsoft:
**".NET - Create Your Own Script Language with Symbolic Delegates"**

Artigo: https://learn.microsoft.com/en-us/archive/msdn-magazine/2018/november/net-create-your-own-script-language-with-symbolic-delegates

## Conceito

O projeto demonstra como criar uma linguagem de script simples usando delegates simbólicos em C#. A ideia é usar um dicionário de delegates para criar uma mini-linguagem de programação dinâmica.

## Principais Conceitos

- **Symbolic Delegates**: Uso de dicionários para mapear palavras-chave a delegates
- **Dynamic Code Evaluation**: Avaliar código dinamicamente em tempo de execução
- **Lizzie Language**: Linguagem de script completa criada com base nesses conceitos

## Como Executar

```bash
dotnet run
```

## Exemplos do Artigo

### Exemplo 1: Mini Linguagem
O código atual demonstra a criação de uma mini-linguagem com palavras-chave "hello" e "world".

### Exemplo 2: Chaining Delegates
O artigo também mostra como encadear delegates para transformar dados sequencialmente.

### Lizzie Language
O artigo introduz "Lizzie", uma linguagem de programação completa Turing-complete criada em apenas 2000 linhas de código.

## Recursos Adicionais

- [Lizzie no GitHub](https://github.com/polterguy/lizzie)
- Artigo original da MSDN Magazine (Novembro 2018)

## Por que este projeto é interessante

Este estudo sobre *symbolic delegates* e a criação de uma linguagem de script é relevante por vários motivos:

- **Simplicidade poderosa**: a técnica mostra como construir uma linguagem útil com pouca infraestrutura — um dicionário de delegates e um avaliador simples.
- **Extensibilidade prática**: permite adicionar palavras-chave e funcionalidades dinamicamente, facilitando criação de DSLs e extensões específicas de domínio.
- **Segurança e controle**: a linguagem pode ser mantida intencionalmente limitada ("JSON para código"), reduzindo vetores de ataque ao expor apenas funções autorizadas.
- **Casos de uso reais**: ideal para rule engines, automação de fluxo de trabalho, plugins de usuário e endpoints dinâmicos que aceitam scripts seguros.
- **Valor pedagógico**: é um excelente exercício para entender delegates, parsing mínimo, binding de contexto e avaliação em tempo de execução — conceitos úteis além do exemplo.

Adicionar essa implementação como um módulo pequeno e independente facilita experimentação rápida sem comprometer a arquitetura principal da aplicação.
