# Blazor Hello World

Um projeto simples de "Hello World" em Blazor para demonstrar os conceitos básicos do framework.

## 🚀 Sobre o Projeto

Este é um projeto Blazor Server que demonstra:
- Criação de componentes Blazor (.razor)
- Data binding com `@bind`
- Manipulação de eventos com `@onclick`
- Renderização condicional com `@if`
- Estilização CSS incorporada
- Interatividade do lado do servidor

## 📋 Funcionalidades

A página Hello World inclui:
- Campo de entrada para o nome do usuário
- Saudação personalizada baseada no input
- Contador de cliques
- Estilização moderna com gradientes

## 🎯 Estrutura do Componente

```razor
@page "/helloworld"
@rendermode InteractiveServer

<div>
    <!-- HTML Markup -->
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

## 🏃 Como Executar

1. Navegue até o diretório do projeto:
   ```bash
   cd BlazorHelloWorld
   ```

2. Execute o projeto:
   ```bash
   dotnet run
   ```

3. Abra o navegador e acesse:
   ```
   https://localhost:5001/helloworld
   ```
   ou
   ```
   http://localhost:5000/helloworld
   ```

## 🔗 Navegação

O projeto inclui um menu de navegação com um link para a página Hello World. Você pode acessá-la:
- Diretamente pela URL `/helloworld`
- Através do menu de navegação

## 📚 Conceitos Blazor Demonstrados

### 1. **Roteamento**
```razor
@page "/helloworld"
```
Define a rota da página.

### 2. **Two-Way Data Binding**
```razor
<input @bind="nome" @bind:event="oninput" />
```
Vincula o input ao campo `nome` e atualiza em tempo real.

### 3. **Event Handling**
```razor
<button @onclick="IncrementarContador">Clique Aqui!</button>
```
Manipula eventos de clique.

### 4. **Renderização Condicional**
```razor
@if (!string.IsNullOrWhiteSpace(nome))
{
    <h2>Olá, @nome!</h2>
}
```
Renderiza elementos baseado em condições.

### 5. **Interpolação**
```razor
<p>Você clicou <strong>@contador</strong> vez(es)</p>
```
Exibe valores de variáveis C# no HTML.

## 🛠️ Tecnologias

- .NET 9.0
- Blazor Server
- C# 13
- ASP.NET Core

## 📖 Aprendendo Mais

Para aprender mais sobre Blazor:
- [Documentação Oficial do Blazor](https://docs.microsoft.com/aspnet/core/blazor)
- [Blazor University](https://blazor-university.com/)
- [Microsoft Learn - Blazor](https://learn.microsoft.com/training/paths/build-web-apps-with-blazor/)

## 💡 Próximos Passos

Após dominar este Hello World, você pode:
1. Adicionar mais componentes
2. Implementar comunicação entre componentes
3. Conectar com APIs
4. Adicionar validação de formulários
5. Implementar autenticação e autorização
