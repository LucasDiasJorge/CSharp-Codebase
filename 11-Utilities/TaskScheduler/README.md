# TaskScheduler — Exemplo de Agendamento de Tarefas

Projeto de exemplo que demonstra como agendar e executar tarefas recorrentes em .NET usando a biblioteca `TaskScheduler` e um host simples.

**Principais objetivos:**
- Mostrar como configurar e usar o pacote `TaskScheduler` (NuGet) para agendamentos.
- Exemplo mínimo executável com `Program.cs` para facilitar experimentação.

**Pré-requisitos:**
- .NET 9 SDK (o repositório usa `net9.0` nos samples).

**Como executar**
Abra um terminal na raiz do repositório e execute:

```powershell
dotnet run --project 11-Utilities/TaskScheduler/project/project.csproj
```

Para apenas compilar:

```powershell
dotnet build --project 11-Utilities/TaskScheduler/project/project.csproj
```

**Estrutura do projeto**
- `project/Program.cs` — ponto de entrada do exemplo.
- `project/project.csproj` — definição do projeto e referências (inclui `TaskScheduler`).

**O que este exemplo faz**
- Registra um job de exemplo que é executado em intervalos configuráveis.
- Mostra como iniciar/encerrar o host corretamente e como observar logs/saída no console.

**Personalização rápida**
- Altere os parâmetros de agendamento em `Program.cs` para testar diferentes intervalos e políticas de execução.
- Se desejar persistência ou uma fila de trabalhos mais complexa, integre um banco de dados ou uma fila externa.

**Contribuição**
Pull requests são bem-vindos. Prefira mudanças pequenas e explicativas; atualize este README se introduzir novos arquivos ou opções de execução.

**Licença**
Use conforme o escopo do repositório principal. Se desejar, adicione uma licença explícita no diretório.

