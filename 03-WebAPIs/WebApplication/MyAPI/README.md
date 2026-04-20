# WeatherForecast API

## Visão geral

Esta API � uma demonstra��o de uma aplica��o ASP.NET Core que utiliza Entity Framework Core para conectar-se a um banco de dados PostgreSQL e expor endpoints REST para opera��es CRUD na entidade `WeatherForecast`.

## Conceitos abordados

- Exemplo didático sobre WeatherForecast API no contexto de ASP.NET Core, contratos HTTP e pipeline web.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como WeatherForecast API se aplica em um cenário prático de ASP.NET Core, contratos HTTP e pipeline web.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
MyAPI/
+-- Controllers/
|   +-- AuthController.cs
|   \-- WeatherForecastController.cs
+-- Middleware/
|   \-- RequestResponseLoggingMiddleware.cs
+-- Migrations/
|   +-- 20250128142715_InitialCreate.cs
|   +-- 20250128142715_InitialCreate.Designer.cs
|   \-- AppDbContextModelSnapshot.cs
+-- Models/
|   \-- WeatherForecast.cs
+-- Properties/
|   \-- launchSettings.json
+-- .gitignore
+-- AppDbContext.cs
+-- appsettings.Development.json
\-- ...
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/WebApplication/MyAPI/MyAPI.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### 1. Instalar ferramentas e pacotes necess�rios

Certifique-se de que voc� tenha o .NET SDK instalado. Para verificar:
```bash
dotnet --version
```

Instale a ferramenta `dotnet-ef` globalmente:
```bash
dotnet tool install --global dotnet-ef
```

Adicione os seguintes pacotes ao projeto:
```bash
dotnet add package Microsoft.EntityFrameworkCore
```
```bash
dotnet add package Microsoft.EntityFrameworkCore.Design
```
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

##### 2. Configurar a string de conex�o no `appsettings.json`

No arquivo `appsettings.json`, adicione a seguinte configura��o:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=MyDatabase;Username=myuser;Password=mypassword"
  },
  "Jwt": {
    "Key": "supersecretkey12345",
    "Issuer": "MyAPI",
    "Audience": "MyAPIUsers"
  }
}
```

Substitua `myuser`, `mypassword`, e `MyDatabase` pelas credenciais do seu banco de dados PostgreSQL.

##### 3. Criar o banco de dados e aplicar migra��es

1. Gere uma migra��o inicial:
   ```bash
   dotnet ef migrations add InitialCreate
   ```

2. Aplique as migra��es ao banco de dados:
   ```bash
   dotnet ef database update
   ```

##### Popular o banco de dados

Execute o seguinte script SQL para inserir dados iniciais no banco de dados:

```sql
INSERT INTO "WeatherForecasts" ("Date", "TemperatureC")
VALUES
    ('2025-01-01', -5),
    ('2025-01-02', 5),
    ('2025-01-03', 15),
    ('2025-01-04', 25),
    ('2025-01-05', 35);
```

##### Listar todos os registros

**Endpoint:**
```http
GET /api/weather
```

**cURL:**
```bash
curl -X GET https://localhost:5001/api/weather \
     -H "Authorization: Bearer <SEU_TOKEN_JWT>"
```

##### Testar conectividade

**Endpoint:**
```http
GET /api/weather/ping
```

**cURL:**
```bash
curl -X GET https://localhost:5001/api/weather/ping
```

##### Estrutura do Projeto

- **Controllers**: Cont�m os endpoints da API, como o `WeatherForecastController`.
- **Models**: Define a entidade `WeatherForecast`.
- **Data**: Cont�m o contexto do banco de dados (`AppDbContext`).

##### Configura��o Adicional

Caso queira usar um banco de dados diferente ou configurar autentica��o JWT, atualize os valores correspondentes no `appsettings.json`.
