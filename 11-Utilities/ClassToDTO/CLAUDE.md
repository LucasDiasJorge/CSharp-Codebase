# CLAUDE.md — ClassToDTO

Web API que demonstra a fronteira entre entidade de persistência e DTO de contrato. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres

dotnet run --project 11-Utilities/ClassToDTO/ClassToDTO.csproj
```

## Estrutura interna

- `src/Models/Order.cs`, `Costumer.cs` — entidades EF Core, com navegações.
- `src/DTO/OrderDTO.cs` — o contrato exposto: achatado, sem ciclos, só com o que o cliente precisa.
- `src/Controllers/OrderController.cs` — faz a projeção.
- `src/Db/ApplicationDbContext.cs` — contexto.

O motivo prático do DTO aparece aqui: serializar a entidade diretamente vaza estrutura interna e, com navegações bidirecionais, gera referência cíclica na serialização. Projetar para DTO na consulta ainda evita carregar dados desnecessários.

## Pontos de atenção

- **Exige PostgreSQL ativo**; connection string em `appsettings.json`.
- TFM `net9.0`. `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4. Sem AutoMapper — o mapeamento é manual e explícito, coerente com o objetivo didático.
- Typo no nome do arquivo/tipo: `Costumer` (o correto seria `Customer`). Renomear exige acompanhar todos os usos.
