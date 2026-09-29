# CLAUDE.md — NuGetPackageLifecycleDemo

Biblioteca empacotada, feed local, consumidor por `PackageReference` e script que percorre o ciclo de vida inteiro. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
# O exemplo inteiro (e o que vale rodar)
powershell -ExecutionPolicy Bypass -File .\13-SDKsAndLibraries\NuGetPackageLifecycleDemo\Invoke-PackageLifecycle.ps1

# Opcoes: -SkipBreakingChange (pula o passo 5), -Clean (apaga o feed no fim)

dotnet build 13-SDKsAndLibraries/NuGetPackageLifecycleDemo/src/Acme.TextKit/Acme.TextKit.csproj
```

## Armadilha principal

**`dotnet build` do consumidor FALHA antes de rodar o script, e isso é por desenho:**

```
error NU1101: Não é possível encontrar o pacote Acme.TextKit.
```

`Acme.TextKit.Consumer` usa `PackageReference`, não `ProjectReference` — é o que prova que o pacote funciona. Ele só restaura depois de os passos 1 e 2 do script publicarem o `.nupkg` em `local-feed/`. **Não "consertar" trocando por `ProjectReference`**: isso elimina o exemplo. Ao rodar auditoria de build do repositório, este projeto conta como dependente de passo prévio, igual aos que exigem Docker.

## Estrutura interna

`src/Acme.TextKit/Acme.TextKit.csproj` está agrupado em blocos comentados por assunto: metadados, documentação XML, símbolos/SourceLink, determinismo. Cada bloco existe por um motivo verificável no passo 2 do script.

`TextKit.CountWords` está dentro de `#if !BREAKING`. **É a peça do passo 5**: compilar com `-p:DefineConstants=BREAKING` remove o método da API pública e o `PackageValidation` reprova com `CP0002`. Remover a diretiva mata o cenário.

`local-feed/.gitkeep` é rastreado de propósito, com `local-feed/*` no `.gitignore`. Sem a pasta, o erro de um clone novo seria `NU1301` ("a fonte local não existe"), que aponta para o lugar errado; com ela, é `NU1101` ("pacote não encontrado").

`nuget.config` usa `<clear />` — sem isso a ordem de fontes da máquina muda o resultado do restore.

## Pontos de atenção

- TFM `net10.0` nos dois projetos. Layout `src/`, como os vizinhos da trilha.
- **Cache global do NuGet mascara alteração.** Depois do primeiro restore, o pacote fica em `~/.nuget/packages/acme.textkit`. Apagar só `local-feed/` não força nova resolução: para um teste limpo é preciso `rm -rf ~/.nuget/packages/acme.textkit`. Foi assim que a mensagem `NU1101` deste README foi verificada.
- Reempacotar a **mesma** versão com o pacote já em cache não tem efeito — o sintoma é "minha alteração não aparece". Subir a versão resolve.
- O passo 5 grava `Acme.TextKit.2.0.0.nupkg`? **Não** — o pack falha antes de gerar, e é esse o ponto. Se algum dia passar, o script imprime um alerta em vermelho em vez de fingir sucesso.
- O passo 4 copia o consumidor para `%TEMP%` para testar versão flutuante, reescrevendo o caminho do feed no `nuget.config` (que passa a precisar de caminho absoluto). A pasta é removida no `finally`.
- `ContinuousIntegrationBuild` está condicionado a `CI == 'true'`: ligado localmente, ele exige que o repositório esteja limpo e atrapalha o uso didático.
- A saída real conferida: `.nupkg` 6,2 KB, `.snupkg` 9,4 KB, `InformationalVersion` `1.0.0+b714182…`, `1.*` resolvendo 1.1.0, e `CP0002` no passo 5. Ao mexer no exemplo, reconferir esses números antes de atualizar o README.
- **Fronteira com os vizinhos**: `MySimpleSdk` cobre estrutura de biblioteca; `ResilientHttpSdk` cobre desenho de API de SDK. Aqui o assunto é distribuição. Não adicionar publicação real no nuget.org.
