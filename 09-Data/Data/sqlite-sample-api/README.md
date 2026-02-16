# SQLite Sample API (MVC Controllers)

Exemplo de Web API em C# com Entity Framework Core + SQLite, usando **controllers tradicionais** (sem Minimal API).

## Domínio

- `Author` 1:N `Book`
- Um autor possui vários livros.
- Um livro pertence a um autor (`AuthorId`).

## Banco em runtime

- O banco SQLite é criado automaticamente em execução via `EnsureCreated()`.
- O seed inicial também roda no startup (`DbInitializer`).
- String de conexão padrão:
  - `Data Source=sqlite-sample.db` (development)

## Como executar

```bash
dotnet restore
dotnet run
```

Base URL local padrão: `http://localhost:5000`

Swagger em desenvolvimento: `http://localhost:5000/swagger`

## Endpoints

### Authors

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

### Books

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

## Observações

- O projeto usa `Program + Startup` para manter estilo explícito/clássico de configuração.
- Exclusão de autor remove livros relacionados (cascade delete).