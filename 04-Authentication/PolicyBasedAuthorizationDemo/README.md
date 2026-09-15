# PolicyBasedAuthorizationDemo

API que demonstra autorização por policies no ASP.NET Core: claims, requirements, handlers e autorização sobre um recurso específico.

## Visão geral

Autorizar por perfil resolve pouco. `[Authorize(Roles = "admin")]` diz *quem* pode, não *por quê*, e a regra fica presa no atributo: mudar o critério exige recompilar e caçar todos os endpoints que o repetem. Policy inverte isso — o endpoint declara a intenção (`ConfidentialAccess`) e a regra vive em um lugar só.

A policy é composta de requirements, e cada requirement é apenas o dado da exigência. Quem decide é o handler. Essa separação tem uma consequência prática que costuma surpreender: registrar **dois handlers para o mesmo requirement** cria dois caminhos alternativos de satisfazê-lo — basta um chamar `Succeed`. É assim que "pode editar se for o dono **ou** admin" se expressa sem nenhum `if`. Para exigir as duas coisas ao mesmo tempo, o caminho é outro: dois requirements na mesma policy.

O caso que atributo nenhum resolve é a autorização por recurso. "Pode editar este documento" depende de qual documento é, e isso só se sabe depois de carregá-lo do banco. Aí a decisão sai do atributo e vira uma chamada explícita a `IAuthorizationService.AuthorizeAsync(user, documento, requirement)` no meio da ação.

Os tokens vêm de um endpoint de conveniência com três perfis fixos. Autenticação é andaime aqui — o assunto começa depois que o usuário já foi identificado.

## Conceitos abordados

- Autorização por perfil versus por policy, e o que se ganha na troca.
- `IAuthorizationRequirement` como dado e `AuthorizationHandler<T>` como decisão.
- Vários handlers para um requirement: semântica de OU.
- Vários requirements em uma policy: semântica de E.
- Autorização por recurso com `AuthorizationHandler<TRequirement, TResource>`.
- `IAuthorizationService.AuthorizeAsync` chamado de dentro da ação.
- `RequireClaim` e `RequireAssertion` para regras simples.
- Por que `context.Fail()` é diferente de "não chamar `Succeed`".
- `MapInboundClaims` e o efeito do remapeamento de claims sobre `User.FindFirst`.

## Objetivos de aprendizagem

- Escolher entre atributo e chamada explícita conforme a decisão dependa ou não do recurso.
- Compor exigências alternativas sem condicional espalhada pelo controller.
- Reconhecer quando uma regra inline basta e quando ela deveria ser um requirement.
- Entender por que um handler silencioso é melhor que um handler que falha.
- Decidir conscientemente entre responder 403 e 404 quando o acesso é negado.

## Estrutura do projeto

```text
PolicyBasedAuthorizationDemo/
|-- Authorization/
|   |-- Handlers/
|   |   |-- DocumentAdminHandler.cs
|   |   |-- DocumentOwnerHandler.cs
|   |   `-- MinimumClearanceHandler.cs
|   |-- Requirements/
|   |   |-- DocumentEditRequirement.cs
|   |   `-- MinimumClearanceRequirement.cs
|   `-- Policies.cs
|-- Controllers/
|   `-- DocumentsController.cs
|-- Documents/
|   |-- Document.cs
|   `-- DocumentStore.cs
|-- Users/
|   `-- DemoTokenIssuer.cs
|-- Properties/
|   `-- launchSettings.json
|-- PolicyBasedAuthorizationDemo.csproj
|-- PolicyBasedAuthorizationDemo.http
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 04-Authentication/PolicyBasedAuthorizationDemo/PolicyBasedAuthorizationDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 04-Authentication/PolicyBasedAuthorizationDemo/PolicyBasedAuthorizationDemo.csproj
```

A API sobe em `http://localhost:5079` e não exige serviço externo.

Pegue um token e exercite as policies:

```bash
TOKEN=$(curl -s -X POST http://localhost:5079/dev/token \
  -H "Content-Type: application/json" -d '{"user":"bruno"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

curl -s -o /dev/null -w "%{http_code}\n" -H "Authorization: Bearer $TOKEN" \
  http://localhost:5079/documents/by-policy         # 200

curl -s -o /dev/null -w "%{http_code}\n" -X PUT -H "Authorization: Bearer $TOKEN" \
  http://localhost:5079/documents/2                 # 200 - bruno e dono

curl -s -o /dev/null -w "%{http_code}\n" -X PUT -H "Authorization: Bearer $TOKEN" \
  http://localhost:5079/documents/1                 # 403 - nao e dono nem admin
```

## Boas práticas e pontos de atenção

- Prefira policy a perfil. Perfil é um rótulo; policy é a regra. Com policy, mudar "quem pode ler documento confidencial" acontece em um lugar, não em vinte atributos.
- Não chame `context.Fail()` só porque o seu handler não aprovou. `Fail()` é definitivo e derruba a autorização mesmo que outro handler tenha chamado `Succeed`. Para "eu não sei aprovar", basta retornar sem fazer nada — foi o que `MinimumClearanceHandler` faz. Reserve `Fail()` para o caso em que a negação é absoluta, tipo conta bloqueada.
- Requirement guarda dado, handler guarda decisão. Colocar lógica no requirement elimina justamente a flexibilidade que motivou o modelo.
- Autorização por recurso não cabe em atributo. Se a decisão depende do objeto, ela precisa acontecer depois da consulta, via `IAuthorizationService`.
- Centralize os nomes das policies em constantes. Policy é resolvida por string: um nome digitado errado no atributo não quebra o build — o endpoint só passa a negar tudo, ou a lançar em tempo de execução, sem indicar a causa.
- `RequireAssertion` é ótimo para condição pontual e ruim para regra que precise de serviço injetado ou de teste isolado. Quando a regra crescer, promova-a a requirement.
- Decida entre 403 e 404 de propósito. 403 diz "existe, mas você não pode"; 404 esconde a existência do recurso. As duas respostas são defensáveis — a escolha acidental não.
- Cuidado com o remapeamento de claims. Sem `MapInboundClaims = false`, o handler JWT converte `sub` para um nome longo de schema e `User.FindFirst("sub")` volta nulo, quebrando a autorização por dono sem nenhum erro visível.

## Conteúdo complementar

Perfis do exemplo:

| Usuário | Departamento | Clearance | Perfil | Documento próprio |
|---|---|---|---|---|
| `ana` | engineering | 5 | admin | 1 |
| `bruno` | engineering | 3 | member | 2 |
| `carla` | finance | 1 | member | 3 |

Matriz observada nos endpoints de policy:

| Endpoint | Regra | ana | bruno | carla |
|---|---|---|---|---|
| `GET /documents/by-role` | `[Authorize(Roles = "admin")]` | 200 | 403 | 403 |
| `GET /documents/by-policy` | clearance >= 3 | 200 | 200 | 403 |
| `GET /documents/engineering` | claim `department` = engineering | 200 | 200 | 403 |
| `GET /documents/senior-engineering` | engineering **E** clearance >= 5 | 200 | 403 | 403 |
| `GET /documents/weekday` | `RequireAssertion` sobre o dia | depende do dia da semana |

`bruno` reprova em `senior-engineering` apesar de ser de engenharia: a policy tem dois requirements e ambos precisam passar.

Autorização por recurso, em `PUT /documents/{id}` — satisfeita por ser dono **ou** admin:

| Documento (dono) | ana (admin) | bruno | carla |
|---|---|---|---|
| 1 (ana) | 200 | 403 | 403 |
| 2 (bruno) | 200 | 200 | 403 |
| 3 (carla) | 200 | 403 | 200 |

Cada usuário edita o próprio documento; `ana` edita todos, por um caminho diferente — o `DocumentAdminHandler`, não o de dono.

Como combinar exigências:

| Objetivo | Como se escreve |
|---|---|
| Satisfazer de duas formas alternativas (OU) | Dois handlers para o **mesmo** requirement |
| Exigir duas condições (E) | Dois requirements na **mesma** policy |
| Negar de forma definitiva | `context.Fail()` em um handler |
| Não opinar | Retornar sem chamar `Succeed` nem `Fail` |

Relação com os vizinhos da trilha: `Authentication/Auth` e `AdvancedAuthSystem` tratam de **autenticação** — provar quem é o usuário. Este projeto começa onde eles terminam: o usuário já está identificado, e a pergunta passa a ser o que ele pode fazer. `RefreshTokenRotationDemo` cobre o ciclo de vida do token que traz essas claims.

## Referências e documentação complementar

- https://learn.microsoft.com/aspnet/core/security/authorization/policies
- https://learn.microsoft.com/aspnet/core/security/authorization/resourcebased
- https://learn.microsoft.com/aspnet/core/security/authorization/claims
- https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authorization.iauthorizationservice
