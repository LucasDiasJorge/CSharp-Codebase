# RealWorldBubbleSort

Um exemplo simples que demonstra um fluxo prático: buscar dados de uma API pública, ordenar por um campo (username) e executar uma busca binária sobre os resultados.

## O que o projeto faz
1. Consulta a API pública https://randomuser.me para obter uma lista de usuários.
2. Mapeia e constrói objetos `User` com `Username` e `Name`.
3. Ordena a lista de usuários usando `List<T>.Sort` e uma comparação por `Username` (case-insensitive).
4. Executa uma busca binária para localizar um username informado pelo usuário.

> Observação: apesar do nome, este projeto não implementa um "Bubble Sort" — utiliza o sort embutido do .NET e demonstra busca binária aplicada ao mundo real.

## Arquivos principais
- `Program.cs` - Lógica principal: fetch da API, ordenação e busca binária.

## Requisitos
- .NET 6.0+ (o projeto foi criado com .NET 9 mas é compatível com versões recentes do .NET)
- Conexão com a internet para consultar https://randomuser.me

## Como executar
Abra um terminal na pasta do projeto e rode:

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\RealWorldBubbleSort"
dotnet run
```

O programa irá listar os usuários buscados, mostrar a lista ordenada por username e pedir que você informe um username para busca. Digite o username e pressione Enter.

## Exemplo de execução
```
Fetched Users:
jsmith - John Smith
...

Sorted Users:
ajones
jsmith
...

Enter a username to search: jsmith
User Found: jsmith - John Smith
```

## Considerações e pontos de atenção
- A API `randomuser.me` retorna resultados aleatórios; usernames mudam a cada chamada.
- A implementação usa `HttpClient` sem injeção de dependência. Para produção, prefira injetar/compartilhar `HttpClient`.
- A desserialização assume que os campos `login.username`, `name.first` e `name.last` existem — validar a resposta evita exceções.
- Busca binária requer lista ordenada; o projeto garante isso antes de pesquisar.

## Melhorias sugeridas
- Tornar a chamada HTTP resiliente (retry/circuit-breaker) e adicionar timeout configurável.
- Introduzir testes automatizados para: desserialização, ordenação e busca binária (happy path + not found).
- Suportar argumentos de linha de comando para número de resultados e username inicial de busca.
- Implementar versão assíncrona/streaming com paginação caso queira trabalhar com milhares de registros.

## Licença
Use livremente para estudo e referência.

---
Se quiser, eu adiciono este projeto ao `DesignPattern/README.md` com uma referência rápida.
