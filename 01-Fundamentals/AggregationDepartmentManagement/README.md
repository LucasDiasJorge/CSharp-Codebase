# AggregationDepartmentManagement

Aplicacao de console que simula alocacao de funcionarios em departamentos de uma empresa.

---

## Conceitos Abordados

- Aggregation entre Department e Employee.
- Transferencia e realocacao de pessoas entre departamentos.
- Encerramento de departamento sem excluir funcionarios.

---

## Objetivos de Aprendizado

- Entender que Aggregation representa relacao todo-parte sem dependencia de ciclo de vida.
- Aplicar regra de alocacao unica por funcionario.
- Visualizar transferencia de membros e desmobilizacao de departamentos.

---

## Estrutura do Projeto

```text
AggregationDepartmentManagement/
├── Models/
│   ├── Department.cs
│   └── Employee.cs
├── Services/
│   └── DepartmentCoordinator.cs
├── Program.cs
├── README.md
└── AggregationDepartmentManagement.csproj
```

---

## Como Executar

### Pre-requisitos

- .NET 9.0 SDK

### Execucao

```bash
cd 01-Fundamentals/AggregationDepartmentManagement
dotnet run
```

---

## Fluxo de Negocio Demonstrado

1. Cadastro dos funcionarios.
2. Criacao de departamentos.
3. Alocacao inicial de membros.
4. Transferencia entre departamentos.
5. Encerramento de departamento com funcionarios retornando para nao alocados.

---

## Boas Praticas

- Coordenacao centralizada da alocacao para manter consistencia.
- Exposicao de colecoes como somente leitura.
- Validacao de identidade para evitar duplicidade de cadastro.

---

## Pontos de Atencao

- Departamento agrega funcionarios, mas nao e dono deles.
- Encerrar departamento nao deve excluir colaboradores.
- Regras de alocacao devem impedir conflitos de estado.

---

## Referencias

- https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/
- https://www.uml-diagrams.org/aggregation.html