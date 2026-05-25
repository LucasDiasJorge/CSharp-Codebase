# Sockets - Client

Descrição
-	Cliente TCP simples que conecta ao servidor em `localhost:2621`, lê uma mensagem e se fecha.

Arquivos principais
-	`Client.cs` — implementação do cliente (conexão com `TcpClient`, leitura com timeout de 5s).

Pré-requisitos
-	.NET SDK 6.0 ou superior instalado.

Como executar (rápido)
1. Abra um terminal no diretório do projeto:

```powershell
cd 01-Fundamentals/Sockets/Client
```

2. Crie um projeto console e substitua o `Program.cs` pelo arquivo existente (opção simples):

```powershell
dotnet new console --output . --force
del Program.cs
move Client.cs Program.cs
dotnet run
```

Observações
-	O cliente tenta conectar em `localhost` na porta `2621` e espera até 5 segundos por resposta.
-	Execute o `Server` antes do `Client`.

Dicas de evolução
-	Adicionar tratamento de reconexão, logging e laço de leitura para múltiplas mensagens.
