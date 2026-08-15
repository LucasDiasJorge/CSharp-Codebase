# CLAUDE.md — Registry

Console: registro de criadores em dicionário, substituindo o `switch` da factory. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Creational/Registry/Registry.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Creational/Registry/Registry.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): chave → delegate criador em dicionário. Registrar um tipo novo é adicionar uma entrada, sem editar condicional existente — a diferença prática em relação a `Creational/Factory`, que vale ler antes deste.

## Pontos de atenção

- TFM **`net10.0`** (o resto de `DesignPattern/` está em `net9.0`).
- **Sem README local** e **ausente do índice do README raiz** — a trilha 07 está listada com 19 projetos, sem contar este. Ao mexer aqui, considere regularizar: README local + entrada no índice + contador da categoria.
- Sem dependências externas.
