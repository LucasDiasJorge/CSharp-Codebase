# AssociationMedicalScheduling

## Visão geral

Aplicacao de console que simula uma agenda medica com pacientes, medicos e consultas.

## Conceitos abordados

- Association entre entidades de negocio: consulta conecta paciente e medico.
- Regras de agenda para evitar conflito de horario.
- Separacao de responsabilidades entre Models e Services.

## Objetivos de aprendizagem

- Entender por que Association representa vinculo sem posse de ciclo de vida.
- Aplicar validacoes de negocio em um fluxo proximo do mundo real.
- Demonstrar que paciente e medico continuam existindo sem uma consulta ativa.

## Estrutura do projeto

```text
AssociationMedicalScheduling/
+-- Models/
|   +-- Appointment.cs
|   +-- Doctor.cs
|   \-- Patient.cs
+-- Services/
|   \-- AppointmentBook.cs
+-- AssociationMedicalScheduling.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/AssociationMedicalScheduling/AssociationMedicalScheduling.csproj
```

## Boas práticas e pontos de atenção

- Validacoes de regra no servico de agenda.
- Modelos com construtores consistentes e imutabilidade parcial.
- Colecoes expostas como somente leitura.

### Pontos de Atencao

- Association nao implica ownership entre medico e paciente.
- Cancelar consulta nao deve excluir cadastro de medico/paciente.
- Conflito de horario deve considerar sobreposicao de intervalos.

## Conteúdo complementar

##### Estrutura do Projeto

```text
AssociationMedicalScheduling/
├── Models/
│   ├── Appointment.cs
│   ├── Doctor.cs
│   └── Patient.cs
├── Services/
│   └── AppointmentBook.cs
├── Program.cs
├── README.md
└── AssociationMedicalScheduling.csproj
```

##### Pre-requisitos

- .NET 9.0 SDK

##### Fluxo de Negocio Demonstrado

1. Cadastro de pacientes e medicos.
2. Agendamento de consultas.
3. Bloqueio de conflito na agenda do medico.
4. Cancelamento de consulta.
5. Comprovacao de independencia das entidades.

## Referências

- https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/
- https://martinfowler.com/bliki/Association.html
