# CLAUDE.md — AbstractClassVsInterfaceDemo

Console que contrasta classe abstrata e interface num domínio de biblioteca. Regras globais do repositório estão em [CLAUDE.md](../../CLAUDE.md); aqui fica só o que é específico deste projeto.

## Comandos

```bash
dotnet build 01-Fundamentals/AbstractClassVsInterfaceDemo/AbstractClassVsInterfaceDemo.csproj
dotnet run --project 01-Fundamentals/AbstractClassVsInterfaceDemo/AbstractClassVsInterfaceDemo.csproj
```

## Estrutura interna

O ponto didático está na separação dos dois eixos de modelagem:

- `Models/RecursoBiblioteca.cs` — classe abstrata: carrega **estado e comportamento base** compartilhados por todo item do acervo. É a raiz da hierarquia.
- `Models/IReservavel.cs` — interface: modela a **capacidade** de ser reservado, aplicável a tipos sem ancestral comum.
- `LivroFisico`, `Revista` herdam de `RecursoBiblioteca`; `SalaEstudo` implementa `IReservavel` sem participar da hierarquia do acervo — é justamente esse caso que prova por que a interface existe.
- `Program.cs` exercita ambos por polimorfismo.

Ao estender o exemplo, mantenha essa assimetria: herança para estado compartilhado, interface para capacidade transversal. Um novo tipo que "poderia ser os dois" destrói a lição.

## Pontos de atenção

- TFM `net9.0`, sem pacotes externos. Nada de I/O ou infraestrutura — é intencional.
