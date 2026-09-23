# CLAUDE.md — TrieAutocompleteDemo

Console com trie para autocomplete, custo medido por comprimento e o contraponto de quando não usar. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/TrieAutocompleteDemo/TrieAutocompleteDemo.csproj
dotnet run -c Release --project 10-Algorithms/TrieAutocompleteDemo/TrieAutocompleteDemo.csproj
```

**Usar `-c Release`**: o cenário 6 mede tempo, e em Debug os números não significam nada. Roda seis cenários e termina. Sem serviço externo.

## Estrutura interna

`Structures/Trie` mantém dois instrumentos de medição que **não são acessórios**:

- `NodeCount` — o custo de memória, usado nos cenários 5 e 6.
- `LastOperationSteps` — o custo da operação, usado nos cenários 2, 3 e 4. É o que transforma "O(m)" em número.

`TrieNode.WordCount` é incrementado na **inserção**, em cada nó do caminho. Por isso `CountWithPrefix` é O(m): sem esse contador, seria preciso percorrer a subárvore inteira. Removê-lo não quebra nada visivelmente — só torna o cenário 2 uma mentira.

`Collect` ordena as chaves filhas (`Children.Keys.Order()`) para as sugestões saírem em ordem alfabética. Sem isso a ordem seria a do `Dictionary`, que não tem garantia.

## Pontos de atenção

- TFM `net10.0`. A trilha é toda `net9.0`, mas a máquina não tem esse runtime. Pacote: `Microsoft.Extensions.Logging.Console`.
- **O cenário 6 aloca ~1,4 milhão de nós** (50.000 GUIDs de 32 caracteres). É intencional — é o número que prova o custo de memória —, mas leva alguns segundos e consome memória. Não aumentar `total` sem motivo.
- O número de nós do cenário 6 **varia entre execuções**, porque as chaves são GUIDs aleatórios (medi 1.438.001 e 1.438.072). O README diz "~1.400.000"; não fixar um valor exato.
- Os tempos do cenário 6 (3ms contra 257ms) dependem da máquina e do modo de build. A relação que precisa continuar valendo: HashSet **muito** mais rápido que a trie para busca exata.
- **O cenário 6 busca uma chave EXISTENTE.** Uma versão anterior buscava `"nao-existe-mesmo"`, que sai na primeira letra e não compara o custo real da descida — os dois davam 0ms e a comparação não dizia nada.
- `Children` é `Dictionary<char, TrieNode>` por generalidade. Para alfabeto pequeno e fixo, um array seria mais rápido e compacto; está citado no README como alternativa, não implementado.
- Radix tree / Patricia trie é mencionada como a estrutura comprimida, mas **não** implementada — outra escala de projeto.
- **Fronteira com os vizinhos**: [BinarySearchBoundariesDemo](../BinarySearchBoundariesDemo/CLAUDE.md) resolve consulta por prefixo em vetor ordenado com `lowerBound`, sem estrutura extra — os dois READMEs se referenciam. `PriorityQueueDemo` e `GraphTraversalDemo` cobrem outras estruturas.
