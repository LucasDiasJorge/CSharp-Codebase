# CLAUDE.md — AssociationMedicalScheduling

Console que demonstra **associação** entre objetos independentes numa agenda médica. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/AssociationMedicalScheduling/AssociationMedicalScheduling.csproj
dotnet run --project 01-Fundamentals/AssociationMedicalScheduling/AssociationMedicalScheduling.csproj
```

## Estrutura interna

- `Models/Patient.cs` e `Models/Doctor.cs` — entidades autônomas, nenhuma contém a outra.
- `Models/Appointment.cs` — o elo da associação: referencia paciente e médico sem ser dono de nenhum dos dois.
- `Services/AppointmentBook.cs` — agenda que cria e consulta consultas, mantendo a relação fora das entidades.

A consulta é o objeto que materializa a relação N:N. É esse deslocamento (relação como entidade própria) que o exemplo ensina.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Terceiro projeto do trio agregação/composição/associação — ver `AggregationDepartmentManagement` e `CompositionOrderFulfillment`.
