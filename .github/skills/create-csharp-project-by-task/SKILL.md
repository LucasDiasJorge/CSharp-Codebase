---
name: create-csharp-project-by-task
description: 'Cria projetos e samples C#/.NET no padrão do CSharp-Codebase. Use quando o pedido mencionar criar, adicionar, gerar ou implementar um projeto C#, console app, Web API, worker, class library ou exemplo didático; seleciona a categoria 01-13, atualiza a solução e os READMEs e exige build do csproj alvo.'
argument-hint: 'Descreva o tema, o tipo de projeto e o objetivo didático esperado.'
user-invocable: true
---

# Criar Projeto C# por Tarefa

## Objetivo
Criar um projeto C# didático, pequeno, compilável ou executável na categoria correta do repositório, documentá-lo no catálogo e concluir somente após validação do projeto alvo.

## Quando Não Usar
- Para alterar ou corrigir um projeto existente sem criar um novo projeto.
- Para adicionar apenas um arquivo, classe ou teste a um projeto existente.
- Para scaffolding que não seja C#/.NET.

## Entradas e Inferência
Identifique no pedido:
- Tema e comportamento esperado.
- Tipo de projeto: `console`, `webapi`, `classlib`, `worker`, projeto de testes ou outro template .NET.
- Objetivo didático e conceitos que devem ficar evidentes.

Se uma entrada não estiver explícita, infira-a quando houver uma escolha convencional e de baixo risco. Pergunte ao usuário somente quando a resposta alterar materialmente a categoria, o template, dependências externas ou o comportamento público. Não interrompa o trabalho apenas para confirmar nome, namespace ou detalhes que possam seguir as convenções locais.

## Regras Inegociáveis
1. Não usar `var`, exceto quando um tipo anônimo de LINQ tornar o tipo explícito impossível.
2. Usar Português-BR na documentação e nomes técnicos em inglês quando forem mais reconhecíveis.
3. Preservar configurações compartilhadas do repositório; não duplicar no `.csproj` propriedades já centralizadas.
4. Não sobrescrever diretórios ou projetos existentes. Se houver colisão de nome, inspecionar o conteúdo e escolher um nome inequívoco ou perguntar ao usuário.
5. Manter o exemplo focado no conceito solicitado, sem infraestrutura ou abstrações sem finalidade didática.
6. Criar o README local e atualizar o README raiz.
7. Considerar a tarefa concluída somente após `dotnet build` bem-sucedido no `.csproj` criado.

## Decisão de Categoria
Escolha a categoria pelo conceito principal ensinado, não apenas pelo tipo do template. Em caso de empate, prefira a categoria mais específica e registre a justificativa no relato final.

Mapeamento:
- 01-Fundamentals: sintaxe, OOP básica, LINQ, delegates, eventos e reflection.
- 02-AsyncAndConcurrency: async/await, tasks, threads, sincronização e concorrência.
- 03-WebAPIs: APIs REST, Minimal API, gRPC, middleware/filtros web.
- 04-Authentication: autenticação, autorização, JWT, OAuth e segurança de identidade.
- 05-Messaging: Kafka, RabbitMQ, filas/eventos.
- 06-Caching: Redis, padrões de cache e cache distribuído.
- 07-DesignPatterns: GoF, SOLID, code smells e DDD tático.
- 08-ArchitecturalPatterns: CQRS, Saga, circuit breaker, use cases e estilos arquiteturais.
- 09-Data: Entity Framework, Dapper, SQL, NoSQL e persistência.
- 10-Algorithms: algoritmos e estruturas de dados.
- 11-Utilities: serialização, arquivos e ferramentas utilitárias.
- 12-Testing: testes automatizados, benchmark e validação de regras.
- 13-SDKsAndLibraries: SDKs, packages e bibliotecas reutilizáveis.

Antes de criar, compare somente com um ou dois projetos vizinhos da categoria escolhida para confirmar nomenclatura, estrutura e template. Não faça uma varredura ampla do repositório.

## Procedimento
### 1. Carregar as Regras
Leia antes da primeira edição:
- [Instruções globais](../../copilot-instructions.md)
- [Convenções](../../../docs/CONVENCOES.md)
- [Template de README](../../../docs/README_TEMPLATE.md)
- As instruções `.github/instructions` aplicáveis aos arquivos que serão criados.

Inspecione também o arquivo de solução, a configuração compartilhada existente e os projetos vizinhos necessários. Não presuma que uma propriedade está centralizada sem verificar o checkout atual.

### 2. Definir o Projeto
1. Escolha e justifique a categoria `NN-NomeCategoria`.
2. Defina um nome PascalCase que descreva o conceito, sem sufixos genéricos desnecessários.
3. Selecione o template .NET mais simples que demonstre o objetivo.
4. Defina o caminho final e confirme que ele ainda não existe.
5. Planeje a menor implementação capaz de demonstrar o comportamento pedido.

### 3. Criar e Integrar
1. Execute `dotnet new <template> --name <NomeProjeto> --output <caminho>` com opções compatíveis com o SDK instalado.
2. Revise o `.csproj` gerado e remova apenas duplicações comprovadas de propriedades compartilhadas.
3. Adicione dependências somente quando forem necessárias ao conceito.
4. Implemente o exemplo com tipos explícitos e nomes claros.
5. Adicione o projeto a `CSharp-Codebase.sln` se ele ainda não estiver listado:

```bash
dotnet sln CSharp-Codebase.sln add <caminho-do-csproj>
```

Não recrie a solução e não remova outros projetos.

### 4. Documentar
Crie o README local seguindo integralmente:
- [Convenções](../../../docs/CONVENCOES.md)
- [Template de README](../../../docs/README_TEMPLATE.md)

O README deve conter uma árvore curta e comandos com o caminho real do `.csproj`. Não deixe placeholders do template.

Atualize o README raiz:
1. Adicione `- \`NomeProjeto\` - descrição curta` na categoria correta de **Índice Completo de Projetos**, preservando a ordenação local.
2. Atualize a quantidade exibida no título da categoria.
3. Atualize o total geral quando ele for numérico e puder ser calculado com segurança.
4. Altere a tabela **Trilhas temáticas** somente se o novo projeto mudar o resumo ou merecer destaque; não adicione uma linha duplicada para cada projeto.

### 5. Validar
Execute as verificações nesta ordem para obter feedback rápido:
1. Pesquise `var` somente nos arquivos `*.cs` do novo projeto, excluindo `bin/` e `obj/`. Revise manualmente cada ocorrência para distinguir código de comentários, strings e tipos anônimos inevitáveis.
2. Confirme que o projeto aparece em `dotnet sln CSharp-Codebase.sln list`.
3. Execute testes direcionados quando um projeto de testes fizer parte do sample:

```bash
dotnet test <caminho-do-projeto-de-testes>
```

4. Execute obrigatoriamente:

```bash
dotnet build <caminho-do-csproj>
```

5. Confirme ausência de placeholders no README local, links/caminhos válidos e contadores coerentes no README raiz.

Não use o build completo da solução como critério de conclusão. Se uma validação falhar, corrija o problema no escopo criado e repita a mesma verificação antes de prosseguir.

## Checklist de Conclusão
- [ ] Projeto criado na categoria correta (01 a 13).
- [ ] Nome e template coerentes com os projetos vizinhos.
- [ ] Projeto adicionado à solução sem duplicidade.
- [ ] Sem uso indevido de var.
- [ ] README local completo, sem placeholders e com comandos válidos.
- [ ] README raiz atualizado com item e contadores coerentes.
- [ ] Testes direcionados aprovados, quando existirem.
- [ ] `dotnet build` do `.csproj` alvo executado com sucesso.

## Relato Final
Informe de forma objetiva:
- Categoria escolhida e justificativa em uma frase.
- Projeto e principais arquivos criados ou alterados.
- Comandos de teste/build executados e seus resultados.
- Pendências reais, caso alguma validação não possa ser executada.

Não declare sucesso quando o build falhar ou não tiver sido executado.
