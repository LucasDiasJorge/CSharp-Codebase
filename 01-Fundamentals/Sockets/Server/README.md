# Sockets - Server

Descrição
-	Servidor TCP simples que escuta em todas as interfaces na porta `2621`, aceita um cliente, envia uma mensagem (`"Hello from the server!"`) e encerra a conexão.

Arquivos principais
-	`Server.cs` — implementação do servidor (`TcpListener`, aceite de `Socket` e envio de mensagem).

Pré-requisitos
-	.NET SDK 6.0 ou superior instalado.

Como executar (rápido)
1. Abra um terminal no diretório do projeto:

```powershell
cd 01-Fundamentals/Sockets/Server
```

2. Crie um projeto console e substitua o `Program.cs` pelo arquivo existente (opção simples):

```powershell
dotnet new console --output . --force
del Program.cs
move Server.cs Program.cs
dotnet run
```

Observações
-	O servidor aceita apenas uma conexão e depois faz `Stop()`; para aceitar múltiplos clientes, envolva o `AcceptSocket()` em um laço e trate cada cliente em uma thread/tarefa.
-	Porta padrão: `2621`.

Dicas de evolução
-	Adicionar / melhorar logging, troca de mensagens bidirecional e suporte a handling assíncrono com `async/await`.
