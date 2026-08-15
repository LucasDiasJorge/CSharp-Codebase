# CLAUDE.md — Builder

Console: montagem fluente de um pedido complexo. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Creational/Builder/Builder.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Creational/Builder/Builder.csproj
```

## Estrutura interna

- `Pedido.cs` — o produto e seu builder, com métodos encadeáveis que retornam `this` e um `Build()` final que valida antes de entregar o objeto.

O alvo é o construtor telescópico (muitos parâmetros opcionais posicionais). Compare com `07-DesignPatterns/ParameterObject`, que ataca o **mesmo** problema por outro caminho: um objeto de parâmetros em vez de uma API fluente.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- A validação em `Build()` é o que impede a construção de objeto inválido — se ela sair, o padrão vira só açúcar sintático.
