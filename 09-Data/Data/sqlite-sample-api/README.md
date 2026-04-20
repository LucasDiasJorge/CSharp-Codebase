# SQLite Sample API (MVC Controllers)

## Visão geral

Exemplo de Web API em C# com Entity Framework Core + SQLite, usando **controllers tradicionais** (sem Minimal API).

## Conceitos abordados

- Exemplo didático sobre SQLite Sample API (MVC Controllers) no contexto de persistência, bancos de dados e acesso a dados.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como SQLite Sample API (MVC Controllers) se aplica em um cenário prático de persistência, bancos de dados e acesso a dados.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
sqlite-sample-api/
+-- Controllers/
|   +-- AuthorsController.cs
|   \-- BooksController.cs
+-- Data/
|   +-- AppDbContext.cs
|   \-- DbInitializer.cs
+-- Dtos/
|   +-- AuthorDto.cs
|   \-- BookDto.cs
+-- Models/
|   +-- Author.cs
|   \-- Book.cs
+-- Properties/
|   \-- launchSettings.json
+-- appsettings.Development.json
+-- appsettings.json
+-- Program.cs
\-- ...
```

## Como executar

```bash
dotnet run --project 09-Data/Data/sqlite-sample-api/sqlite-sample-api.csproj
```

Base URL local padrão: `http://localhost:5000`

Swagger em desenvolvimento: `http://localhost:5000/swagger`

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Domínio

- `Author` 1:N `Book`
- Um autor possui vários livros.
- Um livro pertence a um autor (`AuthorId`).

##### Banco em runtime

- O banco SQLite é criado automaticamente em execução via `EnsureCreated()`.
- O seed inicial também roda no startup (`DbInitializer`).
- String de conexão padrão:
  - `Data Source=sqlite-sample.db` (development)

##### Authors

- `GET /api/authors`
- `GET /api/authors/{id}`
- `POST /api/authors`
- `PUT /api/authors/{id}`
- `DELETE /api/authors/{id}`

Exemplo de payload (`POST /api/authors`):

```json
{
  "name": "Machado de Assis"
}
```

##### Books

- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}`
- `DELETE /api/books/{id}`

Exemplo de payload (`POST /api/books`):

```json
{
  "title": "Dom Casmurro",
  "genre": "Romance",
  "authorId": 1
}
```

##### Observações

- O projeto usa `Program + Startup` para manter estilo explícito/clássico de configuração.
- Exclusão de autor remove livros relacionados (cascade delete).
