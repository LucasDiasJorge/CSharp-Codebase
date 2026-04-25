# Convenções do CSharp-101

## Idioma e estilo

- Usar Português-BR como idioma principal.
- Manter termos técnicos em inglês quando forem mais reconhecíveis do que a tradução.
- Preferir texto direto, didático e sem ornamentação visual.
- Evitar excesso de emojis, numeração manual de seções e títulos longos demais.

## Estrutura mínima de README

Todo README de projeto ou subpasta didática deve seguir esta ordem:

1. Título.
2. Visão geral.
3. Conceitos abordados.
4. Objetivos de aprendizagem.
5. Estrutura do projeto.
6. Como executar.
7. Boas práticas e pontos de atenção.
8. Conteúdo complementar, quando necessário.
9. Referências e documentação complementar, quando houver.

## Regras para comandos

- Preferir `dotnet run --project <caminho-do-csproj>` para projetos executáveis.
- Preferir `dotnet build <caminho-do-csproj>` para bibliotecas ou exemplos sem ponto de entrada.
- Preferir `dotnet test <caminho-do-csproj>` para projetos de teste.
- Quando a pasta agrupar vários exemplos, o README deve apontar claramente para os subprojetos e seus comandos.

## Estrutura e navegação

- Exibir árvore curta da pasta, omitindo `bin/`, `obj/`, `.git/` e `.vs/`.
- Preservar seções especializadas úteis depois do bloco mínimo padronizado.
- Referenciar arquivos Markdown auxiliares da mesma pasta como documentação complementar.

## Manutenção

- Atualizar o README local sempre que o exemplo for alterado.
- Atualizar o README raiz quando o catálogo do repositório mudar.
- Se existir conteúdo útil em um README antigo, reorganizar antes de reescrever do zero.