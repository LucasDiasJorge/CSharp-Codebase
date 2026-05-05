# DictionaryMaster

## Visao geral

Aplicacao de console interativa para estudar Dictionary<TKey, TValue> em cenarios praticos do dia a dia.

O projeto oferece cinco modulos acessados por menu, cada um destacando operacoes essenciais de dicionarios em C#: cadastro, busca segura, atualizacao, remocao, iteracao e ordenacao com LINQ.

## Conceitos abordados

- Add, Remove, ContainsKey e TryGetValue em fluxo real de inventario.
- Contagem de frequencia com o padrao TryGetValue + incremento.
- Dicionario com valor complexo (objeto Contato) para CRUD completo.
- Ranking com OrderByDescending, Take e iteracao com indice.
- Diferencas de comportamento entre indexador, Add e TryGetValue.

## Objetivos de aprendizagem

- Entender como evitar KeyNotFoundException em consultas de chaves.
- Aplicar boas praticas de validacao de entrada e tratamento de erros.
- Praticar LINQ para extrair top N resultados de um Dictionary.
- Reforcar conceitos de dicionario por meio de quiz de multipla escolha.

## Estrutura do projeto

```text
DictionaryMaster/
+-- Models/
|   +-- Contato.cs
|   \-- PerguntaQuiz.cs
+-- DictionaryMaster.csproj
+-- Program.cs
\-- README.md
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/DictionaryMaster/DictionaryMaster.csproj
```

## Boas praticas e pontos de atencao

- O projeto prioriza TryGetValue para consultas seguras e mensagens mais claras ao usuario.
- O inventario usa ContainsKey antes de Add para evitar ArgumentException por chave duplicada.
- O contador de palavras evita excecoes e faz acumulacao direta com TryGetValue + incremento.
- A agenda demonstra dicionario com objeto como valor para agrupar telefone e email por chave.
- O ranking mostra top 3 com OrderByDescending e Take, evitando comparacoes manuais.

## Conteudo complementar

### Modulos disponiveis

| Modulo | Pratica principal |
|--------|-------------------|
| Inventario de Loja | Add, Remove, ContainsKey, TryGetValue |
| Contador de Palavras | foreach + TryGetValue + incremento |
| Agenda de Contatos | CRUD com valor complexo |
| Ranking de Pontuacao | OrderByDescending, Take, indice |
| Quiz sobre Dicionarios | Comportamento e boas praticas |

### Sugestoes de estudo

1. Execute cada modulo separadamente e teste entradas invalidas.
2. Compare o uso de indexador com TryGetValue em chaves inexistentes.
3. Altere perguntas do quiz para revisar novos cenarios.

## Referencias e documentacao complementar

- https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2
- https://learn.microsoft.com/dotnet/csharp/programming-guide/concepts/linq/
