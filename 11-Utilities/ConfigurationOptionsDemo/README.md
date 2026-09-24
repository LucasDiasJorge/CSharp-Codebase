# ConfigurationOptionsDemo

Console que combina `appsettings`, variáveis de ambiente e user secrets com o Options Pattern, mostrando validação no startup e recarga de configuração em funcionamento.

## Visão geral

Configuração em .NET é uma pilha de provedores: cada um registrado depois sobrescreve os anteriores na mesma chave. O exemplo monta a pilha com quatro camadas e mostra o resultado chave a chave — `Smtp:Host` acaba vindo do arquivo de Development, `Smtp:Port` da variável de ambiente e `Smtp:SenderEmail` do `appsettings.json`, tudo confirmado pelo `GetDebugView()`, que diz **qual provedor venceu cada chave**.

O Options Pattern transforma essa pilha em uma classe tipada. E é aí que entra o que separa um serviço que falha no deploy de um que falha em produção: com `ValidateDataAnnotations().ValidateOnStart()`, uma configuração inválida derruba o host **no start**, listando os três erros de uma vez. Sem `ValidateOnStart`, o mesmo host sobe normalmente, parece saudável, e a exceção só aparece quando alguém tenta enviar o primeiro e-mail.

Os cenários 3 e 4 tratam das três interfaces que parecem intercambiáveis. Com o arquivo mudando embaixo, `IOptions<T>` continua devolvendo **1025**, enquanto `IOptionsSnapshot<T>` e `IOptionsMonitor<T>` devolvem **9999**. E o callback de mudança dispara um número **não garantido** de vezes por gravação — neste repositório foram observados 1 e 2 disparos para a mesma escrita, em execuções diferentes.

## Conceitos abordados

- Pilha de provedores de configuração e ordem de precedência.
- `GetDebugView()` para descobrir de onde cada valor veio.
- Variáveis de ambiente com `__` para hierarquia e prefixo para isolamento.
- Options Pattern com `AddOptions<T>().Bind()`.
- Validação com Data Annotations e `ValidateOnStart`.
- `IOptions<T>` × `IOptionsSnapshot<T>` × `IOptionsMonitor<T>` e seus tempos de vida.
- `reloadOnChange` e `OnChange`.
- User secrets: onde ficam, o que resolvem e o que não resolvem.

## Objetivos de aprendizagem

- Prever qual camada vence uma chave, e comprovar com `GetDebugView()`.
- Fazer configuração inválida falhar no start, com mensagem útil.
- Escolher entre as três interfaces de options pelo tempo de vida de quem consome.
- Saber o que pode ser recarregado a quente e o que não pode.
- Manter segredo fora do repositório em desenvolvimento.

## Estrutura do projeto

```text
ConfigurationOptionsDemo/
|-- Configuration/
|   `-- SmtpOptions.cs
|-- Demo/
|   |-- LayeringScenarios.cs
|   |-- OptionsScenarios.cs
|   `-- ReloadableConfigFile.cs
|-- appsettings.json
|-- appsettings.Development.json
|-- appsettings.Invalido.json
|-- ConfigurationOptionsDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run -c Release --project 11-Utilities/ConfigurationOptionsDemo/ConfigurationOptionsDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 11-Utilities/ConfigurationOptionsDemo/ConfigurationOptionsDemo.csproj
```

Não exige serviço externo. O cenário 5 fica inerte até existir um segredo local; para vê-lo completo:

```bash
dotnet user-secrets set "Smtp:Password" "s3nh4-local" --project 11-Utilities/ConfigurationOptionsDemo
dotnet run -c Release --project 11-Utilities/ConfigurationOptionsDemo/ConfigurationOptionsDemo.csproj
dotnet user-secrets clear --project 11-Utilities/ConfigurationOptionsDemo
```

## Boas práticas e pontos de atenção

- Registre os provedores na ordem da precedência desejada: do mais genérico para o mais específico. Trocar duas linhas de lugar muda silenciosamente qual ambiente vence.
- Use `GetDebugView()` para depurar configuração, mas **nunca** o imprima inteiro em produção: ele traz todos os valores, inclusive os sensíveis. Aqui a saída é filtrada só na seção `Smtp`.
- Registre variáveis de ambiente **com prefixo**. Sem prefixo, toda variável da máquina entra na configuração — junto com o que houver de token e senha no ambiente.
- Hierarquia em variável de ambiente usa `__` (dois sublinhados), não `:`. `:` não é válido em nome de variável em vários sistemas.
- `ValidateOnStart()` não vem de graça: sem ele, a validação é preguiçosa e só roda no primeiro acesso. Em serviço web, isso significa descobrir o erro com o primeiro usuário.
- Prefira `IOptions<T>` para o que não muda, `IOptionsSnapshot<T>` para valor por requisição e `IOptionsMonitor<T>` dentro de singletons. Injetar `IOptionsSnapshot<T>` num singleton captura o primeiro valor para sempre — o bug não aparece em teste, só depois de uma alteração em produção.
- Torne o callback de `OnChange` idempotente e barato. O número de disparos por gravação não é garantido, e o callback roda em thread de I/O.
- Nem tudo recarrega. Nível de log, feature flag e timeout sim; connection string e porta de escuta não — quem já abriu a conexão não sabe que a configuração mudou.
- Nunca ponha segredo em `appsettings.json`: ele vai para o commit, para a imagem do container e para qualquer ZIP do código.
- User secrets **não é cofre**: é um JSON em texto puro no perfil do usuário, só para desenvolvimento. Em produção, use o cofre do provedor (Key Vault, Secrets Manager, Secret do Kubernetes).
- Mascare segredo no `ToString()` da classe de options, não em cada ponto de log. Centralizar é o que torna difícil esquecer.

## Conteúdo complementar

**1. Camadas e precedência** — a mesma seção vista por cada provedor:

| Camada | `Smtp:Host` | `Smtp:Port` |
|---|---|---|
| `appsettings.json` | smtp.producao.exemplo.com | 25 |
| `appsettings.Development.json` | localhost | 1025 |
| variáveis `CFGDEMO_*` | (ausente) | 2525 |
| **resultado combinado** | **localhost** | **2525** |

E quem venceu cada chave, pelo `GetDebugView()`:

```text
Host=localhost (JsonConfigurationProvider for 'appsettings.Development.json' (Optional))
Port=2525 (EnvironmentVariablesConfigurationProvider Prefix: 'CFGDEMO_')
SenderEmail=no-reply@exemplo.com (JsonConfigurationProvider for 'appsettings.json' (Required))
```

A variável de ambiente é `CFGDEMO_Smtp__Port` — prefixo para não arrastar o ambiente inteiro, e `__` porque `:` não é válido em nome de variável.

**2. Validação: dois momentos de descoberta** — com `appsettings.Invalido.json` (Host vazio, Port 70000, e-mail sem formato):

```text
com ValidateOnStart: o host NAO sobe. OptionsValidationException:
  - DataAnnotation validation failed for 'SmtpOptions' members: 'Host' ... 'Smtp:Host e obrigatorio'
  - DataAnnotation validation failed for 'SmtpOptions' members: 'Port' ... 'entre 1 e 65535'
  - DataAnnotation validation failed for 'SmtpOptions' members: 'SenderEmail' ... 'precisa ser um e-mail valido'

sem ValidateOnStart: o host SOBE normalmente, e a aplicacao parece saudavel.
  a mesma excecao acontece — mas so no primeiro acesso as opcoes.
```

Os **três** erros vêm juntos, não um por vez. Com `ValidateOnStart`, o deploy falha na hora; sem ele, o deploy passa e a falha chega com o tráfego.

**3. As três interfaces**, com o arquivo alterado de `Port=1025` para `Port=9999` em tempo de execução:

| Interface | Tempo de vida | Valor após a mudança |
|---|---|---:|
| `IOptions<T>` | singleton | **1025** |
| `IOptionsSnapshot<T>` | scoped | **9999** |
| `IOptionsMonitor<T>` | singleton | **9999** |

`IOptions<T>` calcula uma vez e nunca mais. `IOptionsSnapshot<T>` recalcula por escopo — é o certo por requisição, e é exatamente por isso que **não pode** ser injetado num singleton. `IOptionsMonitor<T>` é o único que entrega valor atual dentro de um objeto de vida longa.

**4. O callback de recarga**:

```text
callback #1: Port agora e 7777
callback #2: Port agora e 7777
uma gravacao no arquivo -> 2 chamada(s) de callback
```

Uma escrita, dois callbacks. Em outra execução deste mesmo exemplo o resultado foi **um** callback: o número de disparos depende do editor e do sistema de arquivos, e **não é garantido**. Por isso o callback precisa ser idempotente.

A detecção também não é instantânea — é `FileSystemWatcher`, leva algumas centenas de milissegundos. Gravar o arquivo e ler o valor novo na linha seguinte não funciona.

**5. User secrets**:

```text
arquivo: C:\Users\<usuario>\AppData\Roaming\Microsoft\UserSecrets\<UserSecretsId>\secrets.json
```

Com um segredo definido, a saída passa a ser `Smtp:Password veio dos user secrets, com 20 caracteres` — o valor entra na configuração sem existir em nenhum arquivo do repositório. No Linux e no macOS o caminho é `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`.

Relação com os vizinhos: `SerilogExample` também lê configuração de arquivo, com foco em logging. A trilha `04-Authentication` consome segredos de configuração; aqui o assunto é como eles chegam até lá.

## Referências e documentação complementar

- https://learn.microsoft.com/dotnet/core/extensions/configuration
- https://learn.microsoft.com/dotnet/core/extensions/options
- https://learn.microsoft.com/aspnet/core/security/app-secrets
- https://learn.microsoft.com/dotnet/core/extensions/options#options-validation
