# CLAUDE.md — ShareableUser

API que demonstra estado compartilhado em serviço singleton e os riscos de thread safety. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/ShareableUser/ShareableUser.csproj
dotnet run --project 03-WebAPIs/ShareableUser/ShareableUser.csproj
```

## Estrutura interna

- `Services/UserServices.cs` — registrado como **singleton**: uma instância para todas as requisições. É onde o estado compartilhado vive e onde a corrida pode acontecer.
- `Middleware/UserMiddleware.cs` — popula/consulta o usuário por requisição, atravessando o singleton.

O ponto didático é o descompasso entre o tempo de vida do serviço (aplicação) e o escopo do dado (requisição). Trocar o registro para `Scoped` "conserta" o problema e apaga a lição — se fizer isso, faça deliberadamente e documente.

## Pontos de atenção

- TFM `net9.0`, sem pacotes externos.
- Para observar a corrida é preciso **carga concorrente**; uma requisição por vez não reproduz.
