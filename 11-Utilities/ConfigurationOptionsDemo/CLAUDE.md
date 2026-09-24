# CLAUDE.md — ConfigurationOptionsDemo

Console com pilha de configuração (JSON + ambiente + user secrets), Options Pattern, validação no start e recarga. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/ConfigurationOptionsDemo/ConfigurationOptionsDemo.csproj
dotnet run -c Release --project 11-Utilities/ConfigurationOptionsDemo/ConfigurationOptionsDemo.csproj

# Cenario 5 completo (e a limpeza depois)
dotnet user-secrets set "Smtp:Password" "s3nh4-local" --project 11-Utilities/ConfigurationOptionsDemo
dotnet user-secrets clear --project 11-Utilities/ConfigurationOptionsDemo
```

Roda cinco cenários e termina. Sem serviço externo.

## Estrutura interna

`Demo/LayeringScenarios` monta a pilha (`BuildFullStack`) e também cada camada isolada, para o cenário 1 poder mostrar o que cada provedor diz antes da combinação. O prefixo `CFGDEMO_` em `AddEnvironmentVariables` **não é decoração**: sem prefixo, todas as variáveis da máquina entram na configuração, e o `GetDebugView` do cenário 1 despejaria o ambiente do usuário no console. A saída também é filtrada para a seção `Smtp` por esse motivo.

`Demo/OptionsScenarios.BuildHost` usa `DisableDefaults = true` e adiciona **uma** fonte JSON explícita. É o que torna os cenários determinísticos — com os padrões, o host leria `appsettings.json` a partir do diretório atual, que com `dotnet run --project` é o de quem chamou.

`Demo/ReloadableConfigFile` cria `appsettings.Recarregavel.json` em `AppContext.BaseDirectory` e apaga no `Dispose`. Existe para os cenários 3 e 4 reescreverem configuração **sem tocar no `appsettings.json` do projeto**.

`appsettings.Invalido.json` alimenta o cenário 2 e viola três anotações de uma vez, de propósito: a mensagem mostra que a validação reporta tudo junto.

`SmtpOptions.ToString()` mascara `Password`. Centralizar o mascaramento no tipo é o que evita vazamento quando alguém logar o objeto inteiro.

## Pontos de atenção

- TFM `net10.0`. Pacotes: `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Configuration.UserSecrets`, `Microsoft.Extensions.Options.DataAnnotations`. **`ValidateDataAnnotations()` vem desse último** — sem ele o build quebra com `CS1061`, e a mensagem não diz qual pacote falta.
- `UserSecretsId` está no `.csproj` (`csharp-codebase-configuration-options-demo`). `AddUserSecrets<Program>()` depende da classe `Program` declarada no fim do `Program.cs` — top-level statements geram uma `Program` interna, e a declaração explícita é o que dá o tipo âncora.
- **O cenário 4 não é determinístico.** Foram observados 1 e 2 callbacks para a mesma gravação, em execuções diferentes na mesma máquina. O texto do cenário cobre os dois casos e o README registra as duas observações. **Não "consertar" para prometer sempre dois** — o ponto é justamente que não há garantia.
- O cenário 3 espera a recarga com `WaitForReloadAsync` (timeout de 5s, sondagem de 50ms), porque `FileSystemWatcher` leva centenas de milissegundos. Substituir a espera por um `Task.Delay` fixo torna o cenário instável.
- O cenário 2 chama `StartAsync` dentro de `try/catch` de propósito: `ValidateOnStart` lança em `StartAsync`, não em `Build`. Mover a chamada para fora do `try` derruba o programa.
- O cenário 5 é **inerte por padrão** — sem segredo definido, ele mostra o caminho do arquivo e a instrução. O caminho com segredo foi verificado à parte (`dotnet user-secrets set`, execução, `clear`) e a saída está no README. Não presumir que a execução limpa exercita esse ramo.
- Os arquivos `appsettings*.json` são copiados com `PreserveNewest`. Editar o que está em `bin/` não tem efeito duradouro; editar o do projeto exige rebuild.
- **Fronteira com os vizinhos**: `SerilogExample` cobre configuração de logging. Não expandir este exemplo para Key Vault ou configuração distribuída — o assunto é a pilha local e o Options Pattern.
