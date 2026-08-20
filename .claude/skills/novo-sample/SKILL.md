---
name: novo-sample
description: Cria um projeto/sample C# novo no padrão do CSharp-Codebase — escolhe a trilha 01-13 pelo conceito ensinado, gera o projeto, registra na .sln, escreve o README local e o CLAUDE.md, atualiza o índice do README raiz e valida com dotnet build. Use quando o pedido for criar, adicionar, gerar ou implementar um projeto, sample, exemplo, demo, console app, Web API, worker ou class library neste repositório.
---

# Novo sample C#

Cria um exemplo didático pequeno e compilável na trilha correta, documentado no catálogo.

## Processo canônico

O procedimento completo já existe e é a fonte de verdade:
[`.github/skills/create-csharp-project-by-task/SKILL.md`](../../../.github/skills/create-csharp-project-by-task/SKILL.md).

**Leia esse arquivo antes da primeira edição** e siga-o. Esta skill não o substitui: ela adiciona
os dados do checkout atual e os passos que aquele documento não cobre.

Dados de apoio: [`.claude/reference/catalogo.md`](../../reference/catalogo.md) — taxonomia das
trilhas, TFMs reais, layouts irregulares, serviços externos.

## Quando não usar

- Alterar ou corrigir projeto existente sem criar um novo → edite direto, ou use `/revisar-sample`.
- Adicionar uma classe ou teste a um projeto existente.
- Scaffolding que não seja C#/.NET.

## Correções ao processo canônico

O SKILL.md do `.github/` foi escrito quando `Directory.Build.props` ainda existia. Dois pontos
dele estão desatualizados:

1. **"Preservar configurações compartilhadas do repositório; não duplicar no `.csproj`
   propriedades já centralizadas"** — não há mais centralização. `Directory.Build.props` foi
   removido no commit `50763d5`. Declare **sempre** as três propriedades explicitamente:

   ```xml
   <PropertyGroup>
     <OutputType>Exe</OutputType>
     <TargetFramework>net9.0</TargetFramework>
     <ImplicitUsings>enable</ImplicitUsings>
     <Nullable>enable</Nullable>
   </PropertyGroup>
   ```

   Omitir `TargetFramework` reproduz o `NETSDK1013` que já quebra 10 projetos do repositório.

2. **TFM padrão** — use `net9.0` (81 projetos, a maioria) salvo pedido explícito. `net10.0` é
   aceitável para recursos que exigem C# 14. Não escolha um TFM fora de suporte.

## Passos extras exigidos aqui

Além do checklist canônico:

### CLAUDE.md do projeto

Todo projeto do repositório tem um `CLAUDE.md` ao lado do `.csproj` (126 existentes). Crie o do
projeto novo — sem ele o sample fica incompleto. Formato e conteúdo: use `/atualizar-claude-md`,
ou siga o modelo em [`.claude/skills/atualizar-claude-md/SKILL.md`](../atualizar-claude-md/SKILL.md).

### Índice do README raiz

Não basta adicionar a linha do projeto. Atualize também:

1. `- \`NomeProjeto\` - descrição curta` na categoria certa de **Índice Completo de Projetos**,
   respeitando a ordenação alfabética local.
2. O contador no título da categoria: `#### 01-Fundamentals (15 projetos)` → `(16 projetos)`.
3. O total geral logo abaixo do índice (`**Total: 111+ subprojetos ...**`).
4. A tabela **Trilhas temáticas** só se o projeto novo merecer virar exemplo de referência da
   trilha — não adicione uma linha por projeto.

`/auditar-catalogo` confere esses quatro pontos depois.

## Validação (ordem de custo crescente)

```bash
# 1. sem var no código novo (revisar cada ocorrência: comentário e string não contam)
grep -rn --include="*.cs" -E "(^|[^A-Za-z0-9_])var[[:space:]]+[a-zA-Z_]" <caminho-do-projeto>

# 2. registrado na solução
dotnet sln CSharp-Codebase.sln add <caminho-do-csproj>
dotnet sln CSharp-Codebase.sln list | grep <NomeProjeto>

# 3. obrigatório — critério de conclusão
dotnet build <caminho-do-csproj>

# 4. se o sample incluir projeto de testes
dotnet test <caminho-do-tests-csproj>
```

Nunca use `dotnet build` da solução inteira como critério de conclusão: muitos projetos exigem
serviços externos, usam TFMs fora de suporte ou já estão quebrados no baseline.

## Checklist

- [ ] Trilha escolhida pelo conceito ensinado, com justificativa de uma frase.
- [ ] `TargetFramework`, `Nullable` e `ImplicitUsings` declarados explicitamente no `.csproj`.
- [ ] Nome PascalCase, sem sufixo genérico; caminho não colidia com projeto existente.
- [ ] Sem `var` (exceto tipo anônimo de LINQ inevitável); dependências injetadas em `readonly`.
- [ ] Exemplo focado no conceito — sem infraestrutura ou abstração sem finalidade didática.
- [ ] README local com as 9 seções na ordem fixa, sem placeholders, árvore sem `bin/`/`obj/`.
- [ ] `CLAUDE.md` do projeto criado.
- [ ] Projeto na `.sln`.
- [ ] Índice, contador da categoria e total do README raiz atualizados.
- [ ] `dotnet build` do `.csproj` alvo executado **com sucesso**.

## Relato final

- Trilha escolhida e por quê (uma frase).
- Arquivos criados e alterados.
- Comandos executados e resultado real.
- Pendências, se alguma validação não pôde rodar.

Não declare sucesso se o build falhou ou não foi executado.
