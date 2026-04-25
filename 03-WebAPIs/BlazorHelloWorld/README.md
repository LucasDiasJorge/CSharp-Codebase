# Blazor Hello World

## Visão geral

Um projeto simples de "Hello World" em Blazor para demonstrar os conceitos básicos do framework.

## Conceitos abordados

- Exemplo didático sobre Blazor Hello World no contexto de ASP.NET Core, contratos HTTP e pipeline web.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Blazor Hello World se aplica em um cenário prático de ASP.NET Core, contratos HTTP e pipeline web.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
BlazorHelloWorld/
+-- Components/
|   +-- Layout/
|   +-- Pages/
|   +-- App.razor
|   +-- Routes.razor
|   \-- _Imports.razor
+-- Properties/
|   \-- launchSettings.json
+-- wwwroot/
|   +-- lib/
|   +-- app.css
|   \-- favicon.png
+-- appsettings.Development.json
+-- appsettings.json
+-- BlazorHelloWorld.csproj
+-- BlazorHelloWorld.csproj.user
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/BlazorHelloWorld/BlazorHelloWorld.csproj
```

1. Navegue até o diretório do projeto:

2. Execute o projeto:

3. Abra o navegador e acesse:

   ou

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Sobre o Projeto

Este é um projeto Blazor Server que demonstra:
- Criação de componentes Blazor (.razor)
- Data binding com `@bind`
- Manipulação de eventos com `@onclick`
- Renderização condicional com `@if`
- Estilização CSS incorporada
- Interatividade do lado do servidor

##### Funcionalidades

A página Hello World inclui:
- Campo de entrada para o nome do usuário
- Saudação personalizada baseada no input
- Contador de cliques
- Estilização moderna com gradientes

##### Estrutura do Componente

```razor
@page "/helloworld"
@rendermode InteractiveServer

<div>
</div>

@code {
    // Código C# do componente
    private string nome = "";
    private int contador = 0;

    private void IncrementarContador()
    {
        contador++;
    }
}

<style>
    /* Estilos CSS */
</style>
```

##### Navegação

O projeto inclui um menu de navegação com um link para a página Hello World. Você pode acessá-la:
- Diretamente pela URL `/helloworld`
- Através do menu de navegação

##### 1. **Roteamento**

```razor
@page "/helloworld"
```
Define a rota da página.

##### 2. **Two-Way Data Binding**

```razor
<input @bind="nome" @bind:event="oninput" />
```
Vincula o input ao campo `nome` e atualiza em tempo real.

##### 3. **Event Handling**

```razor
<button @onclick="IncrementarContador">Clique Aqui!</button>
```
Manipula eventos de clique.

##### 4. **Renderização Condicional**

```razor
@if (!string.IsNullOrWhiteSpace(nome))
{
    <h2>Olá, @nome!</h2>
}
```
Renderiza elementos baseado em condições.

##### 5. **Interpolação**

```razor
<p>Você clicou <strong>@contador</strong> vez(es)</p>
```
Exibe valores de variáveis C# no HTML.

##### Tecnologias

- .NET 9.0
- Blazor Server
- C# 13
- ASP.NET Core

##### Aprendendo Mais

Para aprender mais sobre Blazor:
- [Documentação Oficial do Blazor](https://docs.microsoft.com/aspnet/core/blazor)
- [Blazor University](https://blazor-university.com/)
- [Microsoft Learn - Blazor](https://learn.microsoft.com/training/paths/build-web-apps-with-blazor/)

##### Próximos Passos

Após dominar este Hello World, você pode:
1. Adicionar mais componentes
2. Implementar comunicação entre componentes
3. Conectar com APIs
4. Adicionar validação de formulários
5. Implementar autenticação e autorização
