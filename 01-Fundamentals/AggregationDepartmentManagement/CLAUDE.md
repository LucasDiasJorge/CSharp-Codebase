# CLAUDE.md — AggregationDepartmentManagement

Console que demonstra **agregação**: o departamento referencia funcionários que sobrevivem a ele. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/AggregationDepartmentManagement/AggregationDepartmentManagement.csproj
dotnet run --project 01-Fundamentals/AggregationDepartmentManagement/AggregationDepartmentManagement.csproj
```

## Estrutura interna

- `Models/Employee.cs` — criado **fora** do departamento e passado por referência. Esse é o ponto: seu ciclo de vida é independente.
- `Models/Department.cs` — mantém a coleção de funcionários alocados, mas não os instancia nem os destrói.
- `Services/DepartmentCoordinator.cs` — orquestra alocação/realocação entre departamentos, evidenciando que o mesmo `Employee` migra sem ser recriado.

Par didático com `CompositionOrderFulfillment` (composição, ciclo de vida dependente) e `AssociationMedicalScheduling` (associação simples). Se alterar a semântica aqui, verifique se o contraste com esses dois continua legível.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Não faça `Department` instanciar `Employee`: isso converteria o exemplo em composição e apagaria a diferença que ele existe para ensinar.
