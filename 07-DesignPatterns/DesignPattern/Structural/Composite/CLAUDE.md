# CLAUDE.md — Composite

Console: estrutura em árvore tratada uniformemente entre folhas e nós. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Structural/Composite/Composite.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Structural/Composite/Composite.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): um componente comum implementado por folha e por composto. O cliente chama a mesma operação sem perguntar com qual dos dois está falando, e a recursão pela árvore acontece dentro do composto.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Se o cliente precisar testar o tipo (`is Folha`) para decidir algo, o padrão foi quebrado. Mantenha a interface uniforme ao estender.
