---
name: subir-servicos
description: Sobe e verifica os serviços externos que os samples do CSharp-Codebase exigem — Redis, Kafka, RabbitMQ, MySQL, PostgreSQL e MongoDB — via docker compose ou docker run, confere a connection string do projeto e diagnostica falha de conexão. Use quando um sample falhar ao executar por timeout ou erro de conexão, ou quando o pedido for subir, iniciar, preparar ou parar a infraestrutura/dependências de um projeto.
---

# Subir serviços externos

Build passando e `dotnet run` falhando com timeout ou conexão recusada quase sempre significa
serviço externo ausente — não é problema de código. Esta skill resolve isso.

Mapa completo projeto → serviço:
[`.claude/reference/catalogo.md`](../../reference/catalogo.md).

## 1. Descobrir de que o projeto precisa

Nunca suba tudo "por garantia". Determine pelo `.csproj`:

```bash
grep -E "StackExchange.Redis|Confluent.Kafka|RabbitMQ.Client|MongoDB.Driver|MySql|Npgsql|Sqlite" \
  <caminho-do-csproj>
```

| Pacote | Serviço | Porta padrão |
|---|---|---|
| `StackExchange.Redis` | Redis | 6379 |
| `Confluent.Kafka` | Kafka | 9092 |
| `RabbitMQ.Client` | RabbitMQ | 5672 (UI 15672) |
| `MongoDB.Driver` | MongoDB | 27017 |
| `MySql.Data` / `MySqlConnector` / `Pomelo` | MySQL | 3306 |
| `Npgsql` | PostgreSQL | 5432 |
| `Microsoft.Data.Sqlite` / `...EntityFrameworkCore.Sqlite` | **nenhum** — arquivo local | — |

SQLite não precisa de nada: se um projeto SQLite falha, o problema é caminho de arquivo ou
migration, não infraestrutura.

Confira também a connection string real antes de subir o container, para casar porta, usuário,
senha e nome do banco:

```bash
cat <pasta-do-projeto>/appsettings.json 2>/dev/null
grep -rn "ConnectionString\|localhost\|127.0.0.1" --include="*.cs" --include="*.json" <pasta-do-projeto>
```

Se o projeto espera `Password=root;Database=loja` e você subir o container com outra senha ou sem
o banco criado, a falha continua — com outra mensagem.

## 2. Subir

### Projetos com `docker-compose.yml`

Dois projetos trazem o compose pronto — prefira sempre:

```bash
cd 05-Messaging/Kafka && docker compose up -d
cd 06-Caching/Caching/CacheIncrement && docker compose up -d
```

O compose de `CacheIncrement` sobe Redis **e** MySQL já configurados para aquele sample. Leia o
arquivo antes para saber portas e credenciais.

### Demais projetos — `docker run`

```bash
# Redis
docker run -d --name redis -p 6379:6379 redis

# RabbitMQ (com painel em http://localhost:15672, guest/guest)
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management

# MongoDB
docker run -d --name mongo -p 27017:27017 mongo

# MySQL — ajuste senha e banco conforme a connection string do projeto
docker run -d --name mysql -p 3306:3306 \
  -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=app mysql:8

# PostgreSQL — idem
docker run -d --name postgres -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=app postgres:16
```

Para Kafka fora de `05-Messaging/Kafka`, reaproveite o compose de lá em vez de montar um
`docker run` — Kafka sozinho exige configuração de listeners que erra fácil.

## 3. Verificar que está no ar

Container "Up" não significa pronto: bancos e brokers aceitam conexão só depois do boot interno.
Cheque de verdade antes de rodar o sample.

```bash
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

docker exec redis redis-cli ping                      # PONG
docker exec mongo mongosh --quiet --eval "db.version()"
docker exec mysql mysqladmin ping -uroot -proot       # mysqld is alive
docker exec postgres pg_isready -U postgres           # accepting connections
docker exec rabbitmq rabbitmq-diagnostics -q ping     # Ping succeeded
docker exec <kafka-container> kafka-topics --bootstrap-server localhost:9092 --list
```

MySQL e PostgreSQL costumam levar 10-30 s no primeiro start. Se o check falhar logo após o
`docker run`, aguarde e repita — não conclua que o serviço quebrou.

## 4. Executar o sample

```bash
dotnet run --project <caminho-do-csproj>
```

Ainda falhando? Nesta ordem:

1. **Porta ocupada** — `docker ps` mostra outro container no mesmo bind; ou há serviço nativo
   instalado no Windows. Pare um dos dois.
2. **Credencial divergente** — a connection string do projeto contra o `-e` do container.
3. **Banco/tópico inexistente** — MySQL e PostgreSQL precisam do database criado; Kafka pode
   precisar do tópico. Veja se o README do projeto traz script de criação.
4. **`localhost` vs. nome do container** — samples usam `localhost`; isso funciona porque o
   `-p` publica a porta no host. Só falha se o próprio sample rodar dentro de um container.
5. **Migrations pendentes** — projetos EF Core podem exigir `dotnet ef database update`.

## 5. Parar e limpar

Serviços de estudo não precisam ficar de pé.

```bash
docker stop redis rabbitmq mongo mysql postgres
docker rm   redis rabbitmq mongo mysql postgres

cd 05-Messaging/Kafka && docker compose down
```

`docker compose down -v` remove também os volumes — apaga os dados. Não faça isso sem avisar o
usuário; ele pode ter dados de estudo ali.

## Limites

- **Confirme antes de parar ou remover containers que você não subiu.** Pode haver serviço de
  outro trabalho do usuário na mesma máquina. Suba com nome próprio quando houver risco de
  colisão, e liste o que já existia antes de mexer.
- Se o Docker não estiver rodando, diga isso e pare — não tente instalar nada.
