---
name: consertar-build
description: Diagnostica e corrige falhas de build de projetos do CSharp-Codebase — NETSDK1013 (TargetFramework ausente após a remoção do Directory.Build.props), CS0246 por ProjectReference faltando, TFM fora de suporte, pacote NuGet incompatível com o TFM. Use quando um dotnet build/run/test falhar, quando o pedido for consertar, destravar ou fazer compilar um projeto, ou ao investigar erro de compilação.
---

# Consertar build

Diagnóstico dirigido: as falhas deste repositório se concentram em poucas causas conhecidas.
Identifique a causa antes de editar qualquer `.csproj`.

Dados: [`.claude/reference/catalogo.md`](../../reference/catalogo.md).

## Regra de escopo

Conserte **o projeto pedido**. Este repositório tem falhas de baseline em 11 projetos; corrigir
todos de passagem transforma um pedido pontual num diff enorme. Se encontrar outras quebras,
liste-as no relato e pergunte antes de tocar.

## 1. Reproduzir com o caminho certo

```bash
find <trilha>/<Projeto> -name "*.csproj" -not -path "*/bin/*" -not -path "*/obj/*"
dotnet build <caminho-do-csproj>
```

Leia o **primeiro** erro real, não o último. Erros em cascata (`CS0246` em série) quase sempre
vêm de uma única causa raiz acima.

## 2. Diagnóstico por código de erro

### `NETSDK1013` — "The TargetFramework value '' was not recognized"

Causa: o `.csproj` não declara `<TargetFramework>`. Ele conta com o `Directory.Build.props` da
raiz, removido no commit `50763d5`. Restam apenas comentários como
`<!-- TargetFramework herdado de Directory.Build.props (net9.0) -->`.

Atinge 10 projetos. TFM correto de cada um: tabela em
[`.claude/reference/catalogo.md`](../../reference/catalogo.md).

**Correção** — adicione o `PropertyGroup` completo e remova o comentário obsoleto:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net9.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

Escolha o TFM pelos **pacotes já declarados** no próprio `.csproj`, não pelo padrão da trilha:
pacotes `8.0.x` da Microsoft.Extensions/AspNetCore → `net8.0`; `9.0.x` → `net9.0`. Os quatro
projetos de `06-Caching` e o `09-Data/Data/MongoUserApi` são `net8.0`; os outros seis, `net9.0`.

Em Web API, `<OutputType>Exe</OutputType>` não é necessário — o SDK `Microsoft.NET.Sdk.Web` já
define. Confira o atributo `Sdk` do `<Project>` antes de copiar o bloco acima.

**Alternativa global** — restaurar `Directory.Build.props` na raiz conserta os 10 de uma vez,
mas força um TFM único sobre projetos que hoje precisam de `net8.0` e `net9.0`. Só faça isso se
o usuário pedir explicitamente a solução centralizada, e então ainda será preciso sobrescrever o
TFM nos cinco projetos `net8.0`.

### `CS0246` — "The type or namespace name could not be found"

Três causas, nesta ordem de probabilidade:

1. **`ProjectReference` faltando.** É o caso de
   `13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk.Tests`, que usa `SdkClient`/`SdkService` sem
   referenciar o projeto:

   ```bash
   dotnet add 13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk.Tests/MySimpleSdk.Tests.csproj \
     reference 13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk/MySimpleSdk.csproj
   ```

   Confira o TFM dos dois antes: o projeto referenciado precisa de TFM compatível (igual ou
   inferior). `MySimpleSdk` e `MySimpleSdk.Tests` estão em `net5.0`.

2. **`using` ausente** — com `ImplicitUsings` desabilitado ou namespace de pacote não importado.
3. **Pacote NuGet não instalado** — `dotnet add package <Pacote>` com versão compatível com o TFM.

### `NETSDK1045` / `NETSDK1064` — targeting pack ou runtime ausente

TFM fora de suporte (`net5.0`, `net6.0`, `net7.0`) sem o pack correspondente instalado. O SDK
local é 10.0.400.

**Não "conserte" migrando o TFM.** A heterogeneidade é intencional e documentada. Reporte a
limitação de ambiente e ofereça as opções: instalar o targeting pack, ou o usuário autorizar a
migração explicitamente.

### `NU1202` — pacote incompatível com o TFM

O pacote não suporta o TFM do projeto. Baixe a versão do pacote compatível com o TFM existente,
em vez de subir o TFM.

### `MSB3644` / erros de restore

```bash
dotnet restore <caminho-do-csproj>
dotnet build <caminho-do-csproj> --no-incremental
```

Se persistir, limpe os artefatos **do projeto** (nunca do repositório inteiro):

```bash
rm -rf <pasta-do-projeto>/bin <pasta-do-projeto>/obj
```

### Falha só ao executar, não ao compilar

Build passa, `dotnet run` falha com timeout ou erro de conexão → o projeto exige serviço externo
(Redis, Kafka, MySQL, PostgreSQL, MongoDB, RabbitMQ). Não é problema de build: use
`/subir-servicos`.

## 3. Validar

```bash
dotnet build <caminho-do-csproj>
```

Deve terminar em `Build succeeded`. Warnings preexistentes não são critério de falha, mas não
introduza novos. Se o projeto tiver testes associados, rode `dotnet test` também.

Nunca use build da solução inteira como validação.

## Relato

- Código do erro e causa raiz em uma frase.
- O que foi alterado, arquivo por arquivo.
- Resultado do build depois da correção (a saída real).
- Outras quebras encontradas e **não** tocadas, se houver.
