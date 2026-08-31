# MySqlNamedAdvisoryLockReservation

Projeto console que demonstra uma reserva atomica usando advisory lock nomeado do MySQL com `GET_LOCK` e `RELEASE_LOCK`.

## Visao geral

O exemplo simula uma reserva de estoque para o SKU `ROOM-101`. Antes de alterar o saldo, o codigo obtem um lock nomeado baseado no SKU, abre uma transacao na mesma conexao e so entao consulta, atualiza o estoque e grava a reserva.

Se a connection string nao for informada, o programa imprime um dry run com as etapas do fluxo. Com MySQL disponivel, ele cria tabelas demo, semeia o estoque e executa duas tentativas de reserva para mostrar uma confirmacao e uma recusa por saldo insuficiente.

## Conceitos abordados

- Advisory lock nomeado no MySQL com `GET_LOCK`.
- Liberacao explicita com `RELEASE_LOCK`.
- Transacao curta para atualizar estoque e gravar reserva.
- Bloqueio por chave de negocio, usando o SKU como parte do nome do lock.
- Separacao entre lock de aplicacao e locks transacionais de linha.

## Objetivos de aprendizagem

- Entender quando um advisory lock nomeado ajuda a serializar operacoes por recurso logico.
- Implementar uma reserva atomica mantendo aquisicao e liberacao do lock na mesma conexao.
- Combinar lock nomeado, `SELECT ... FOR UPDATE`, `UPDATE` e `INSERT` em um fluxo transacional.
- Identificar cuidados de timeout, liberacao e escopo de conexao.

## Estrutura do projeto

```text
MySqlNamedAdvisoryLockReservation/
|-- MySqlNamedAdvisoryLockReservation.csproj
|-- Program.cs
`-- README.md
```

## Como executar

Para apenas visualizar o fluxo sem conectar no banco:

```bash
dotnet run --project 09-Data/Data/MySqlNamedAdvisoryLockReservation/MySqlNamedAdvisoryLockReservation.csproj
```

Para executar contra MySQL local:

```bash
docker run -d --name mysql-advisory-lock-demo -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=reservation_demo -p 3306:3306 mysql:8.4
```

PowerShell:

```powershell
$env:MYSQL_RESERVATION_CONNECTION="Server=localhost;Port=3306;Database=reservation_demo;User ID=root;Password=root;"
dotnet run --project 09-Data/Data/MySqlNamedAdvisoryLockReservation/MySqlNamedAdvisoryLockReservation.csproj
```

Validacao de build:

```bash
dotnet build 09-Data/Data/MySqlNamedAdvisoryLockReservation/MySqlNamedAdvisoryLockReservation.csproj
```

## Boas praticas e pontos de atencao

- `GET_LOCK` e `RELEASE_LOCK` dependem da mesma conexao fisica; nao adquira o lock em uma conexao e execute a reserva em outra.
- Use nomes de lock especificos, como `reservation:sku:ROOM-101`, para reduzir contencao desnecessaria.
- Mantenha a secao critica curta: adquira o lock, execute a transacao e libere rapidamente.
- Sempre trate timeout de lock como resultado de negocio ou falha recuperavel.
- Fechar a conexao libera locks nomeados pendentes, mas o sample libera explicitamente para deixar a intencao clara.

## Conteudo complementar

Fluxo principal:

```text
GET_LOCK("reservation:sku:ROOM-101", 10)
BEGIN TRANSACTION
SELECT available_quantity FROM reservation_inventory WHERE sku = @sku FOR UPDATE
UPDATE reservation_inventory SET available_quantity = available_quantity - @quantity
INSERT INTO reservation_orders (...)
COMMIT
RELEASE_LOCK("reservation:sku:ROOM-101")
```

Em sistemas reais, avalie idempotencia por chave de requisicao para evitar reservar duas vezes quando o cliente repetir uma chamada apos timeout.

## Referencias e documentacao complementar

- https://dev.mysql.com/doc/refman/8.4/en/locking-functions.html
- https://mysqlconnector.net/
- https://learn.microsoft.com/dotnet/framework/data/adonet/local-transactions
