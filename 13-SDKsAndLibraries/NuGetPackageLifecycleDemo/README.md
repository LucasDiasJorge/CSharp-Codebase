# NuGetPackageLifecycleDemo

Biblioteca empacotada com metadados completos, símbolos e documentação XML, um feed local, um projeto que a consome **pelo pacote** e um script que percorre o ciclo de vida inteiro — incluindo a reprovação automática de uma quebra de SemVer.

## Visão geral

Compilar uma biblioteca não prova que o pacote dela funciona. Só o consumo por `PackageReference`, a partir de um feed, prova — e é essa a diferença que este exemplo constrói: `Acme.TextKit.Consumer` referencia o **pacote** `Acme.TextKit`, não o projeto. Ele só compila se o `.nupkg` existir no feed e estiver correto.

O script `Invoke-PackageLifecycle.ps1` executa seis passos, todos com saída verificável:

1. `dotnet pack` da versão 1.0.0, gerando `.nupkg` (6,2 KB) e `.snupkg` (9,4 KB).
2. Abertura do `.nupkg` — que é um zip — para conferir o que realmente entrou: `lib/net10.0/Acme.TextKit.dll`, o **`.xml` de documentação** e o **README do pacote**.
3. `dotnet restore` + `dotnet run` do consumidor, que imprime a versão resolvida: `1.0.0+b714182…` — o sufixo é o commit, vindo do SourceLink.
4. Publicação da 1.1.0 no feed e demonstração da resolução de versão: pino fixo `1.0.0` resolve exatamente 1.0.0; `1.*` resolve **1.1.0**.
5. Tentativa de publicar 2.0.0 sem o método `CountWords`, com `PackageValidation` comparando contra a baseline 1.0.0 — e o build **falha**:

   ```text
   error CP0002: O membro 'int Acme.TextKit.TextKit.CountWords(string)' existe em
   [Linha de base] lib/net10.0/Acme.TextKit.dll mas não em lib/net10.0/Acme.TextKit.dll
   ```

   É o passo que transforma SemVer de promessa em regra de build.

## Conceitos abordados

- Metadados de pacote: `PackageId`, `Description`, `PackageTags`, licença, README, `RepositoryUrl`.
- `dotnet pack` e sobrescrita de versão por `-p:Version=`.
- Documentação XML no pacote e seu efeito no IntelliSense do consumidor.
- Pacote de símbolos `.snupkg` e SourceLink.
- Build determinístico e `AssemblyInformationalVersion` com hash do commit.
- Feed local via `nuget.config` e `<clear />` de fontes.
- `PackageReference` contra `ProjectReference` como forma de validar o pacote.
- Resolução de versão fixa contra flutuante (`1.*`).
- `EnablePackageValidation` + `PackageValidationBaselineVersion` e os códigos `CP0002`/`PKV`.

## Objetivos de aprendizagem

- Empacotar uma biblioteca com tudo o que a página do NuGet e o consumidor esperam.
- Verificar o conteúdo de um `.nupkg` antes de publicar.
- Provar que um pacote funciona consumindo-o de outro projeto.
- Escolher o incremento de versão certo, com a ferramenta conferindo.
- Montar um feed local para testar publicação sem subir nada para o nuget.org.

## Estrutura do projeto

```text
NuGetPackageLifecycleDemo/
|-- local-feed/
|   `-- .gitkeep
|-- src/
|   |-- Acme.TextKit/
|   |   |-- Acme.TextKit.csproj
|   |   |-- PACKAGE-README.md
|   |   `-- TextKit.cs
|   `-- Acme.TextKit.Consumer/
|       |-- Acme.TextKit.Consumer.csproj
|       `-- Program.cs
|-- Invoke-PackageLifecycle.ps1
|-- nuget.config
`-- README.md
```

## Como executar

O ciclo de vida completo, que é o ponto do exemplo:

```bash
powershell -ExecutionPolicy Bypass -File .\13-SDKsAndLibraries\NuGetPackageLifecycleDemo\Invoke-PackageLifecycle.ps1
```

Opções: `-SkipBreakingChange` pula o passo 5 (o mais lento), `-Clean` apaga o feed no fim.

A biblioteca compila isolada normalmente:

```bash
dotnet build 13-SDKsAndLibraries/NuGetPackageLifecycleDemo/src/Acme.TextKit/Acme.TextKit.csproj
```

**O consumidor, não.** Antes de rodar o script, ele falha de propósito:

```text
error NU1101: Não é possível encontrar o pacote Acme.TextKit.
Não existe nenhum pacote com esta ID nas origens: acme-local, nuget.org
```

Isso **é** a demonstração: o consumidor depende do pacote existir, não do código-fonte estar ao lado. Rodar o script (passo 1 e 2) resolve, e a partir daí `dotnet build` dele funciona.

Não exige serviço externo, mas o `restore` do consumidor consulta o nuget.org para os pacotes do SDK.

## Boas práticas e pontos de atenção

- Preencha os metadados antes da primeira publicação. `PackageId`, versão e licença de um pacote publicado **não podem ser alterados** depois — só resta publicar outra versão ou depreciar.
- Ligue `GenerateDocumentationFile`. Sem o `.xml` no pacote, quem consome vê assinaturas sem explicação, e o compilador passa a avisar `CS1591` em membro público sem documentação — o que é o efeito desejado.
- Publique símbolos em `.snupkg` separado, com SourceLink. É o que permite entrar na biblioteca com o depurador a partir do projeto consumidor.
- Verifique o `.nupkg` antes de publicar: é um zip. O passo 2 do script lista o conteúdo, e é ali que se descobre um arquivo que não deveria estar no pacote, ou um que deveria e não está.
- Valide o pacote consumindo-o. `ProjectReference` compila contra o código-fonte e esconde erro de empacotamento, dependência faltando e TFM incompatível.
- Use `<clear />` no `nuget.config`. Sem isso, a ordem de fontes herdada da máquina muda o resultado do restore entre computadores.
- Fixe a versão em aplicação; aceite faixa em biblioteca. `1.*` num serviço faz o build de hoje ser diferente do de ontem sem ninguém mudar nada.
- Ligue `EnablePackageValidation` com baseline. É a diferença entre "prometemos seguir SemVer" e "o build não deixa quebrar SemVer".
- Cuidado com o cache global ao testar feed local. Depois de restaurar uma vez, o pacote fica em `~/.nuget/packages/acme.textkit`, e apagar só o feed **não** força uma nova resolução. O script avisa; para um teste limpo, apague a pasta do cache também.
- Não reutilize o mesmo número de versão. Com o pacote já em cache, um `.nupkg` novo com a mesma versão é ignorado — e o sintoma é "minha alteração não aparece".

## Conteúdo complementar

**1. O que entrou no pacote** (saída real do passo 2):

```text
Acme.TextKit.nuspec
README.md
lib/net10.0/Acme.TextKit.dll
lib/net10.0/Acme.TextKit.xml

documentacao XML no pacote: True
README no pacote          : True
```

| Artefato | Origem no `.csproj` |
|---|---|
| `lib/net10.0/*.dll` | `TargetFramework` |
| `lib/net10.0/*.xml` | `GenerateDocumentationFile` |
| `README.md` | `PackageReadmeFile` + item `None` com `PackagePath` |
| `.snupkg` | `IncludeSymbols` + `SymbolPackageFormat=snupkg` |
| hash do commit na versão | `PublishRepositoryUrl` + SourceLink |

**2. O consumidor provando o pacote**:

```text
assembly:             Acme.TextKit
AssemblyVersion:      1.0.0.0
InformationalVersion: 1.0.0+b71418286bfbb669c30636e73cb9983de82bb091
Slugify("Olá, Mundo Cruel!") = "ola-mundo-cruel"
Truncate("texto bem longo", 8) = "texto..."
CountWords("um dois  tres") = 3
```

Repare nas **duas** versões: `AssemblyVersion` é `1.0.0.0` (quatro componentes, usada pelo carregador) e `InformationalVersion` traz `1.0.0+<commit>`. São coisas diferentes, e é a segunda que identifica o build.

**3. Resolução de versão**, com 1.0.0 e 1.1.0 no feed:

| `PackageReference` | Resolve |
|---|---|
| `Version="1.0.0"` | exatamente **1.0.0** |
| `Version="1.*"` | **1.1.0** (a maior 1.x do feed) |

**4. SemVer como regra de build** — o passo 5, com `CountWords` removido:

```bash
dotnet pack -p:Version=2.0.0 -p:DefineConstants=BREAKING \
            -p:EnablePackageValidation=true \
            -p:PackageValidationBaselineVersion=1.0.0
```

```text
error CP0002: O membro 'int Acme.TextKit.TextKit.CountWords(string)' existe em
[Linha de base] lib/net10.0/Acme.TextKit.dll mas não em lib/net10.0/Acme.TextKit.dll
```

O `#if !BREAKING` em volta de `CountWords` existe só para isto: compilar com aquela constante remove o método da API pública, e a validação reprova. Qual incremento cabe em cada mudança:

| Mudança | Incremento |
|---|---|
| Corrigir comportamento sem mudar assinatura | **patch** (1.0.1) |
| Adicionar membro público | **minor** (1.1.0) |
| Remover ou alterar membro público | **major** (2.0.0) |
| Mudar o TFM mínimo suportado | **major** |
| Tornar obrigatório um parâmetro antes opcional | **major** |

Relação com os vizinhos: `MySimpleSdk` cobre a estrutura de uma biblioteca com demo e testes; `ResilientHttpSdk` cobre o desenho da API de um SDK. Aqui o assunto é a **distribuição** — o que acontece entre compilar e alguém conseguir usar.

## Referências e documentação complementar

- https://learn.microsoft.com/nuget/create-packages/creating-a-package-msbuild
- https://learn.microsoft.com/nuget/create-packages/symbol-packages-snupkg
- https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/overview
- https://semver.org/lang/pt-BR/
- https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files#floating-versions
