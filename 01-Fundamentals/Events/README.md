# Events e Event Handling em C#

## Visão geral

Projeto didático do CSharp-101 dedicado a Events e Event Handling em C#, com foco em conceitos fundamentais da linguagem C# e orientação a objetos.

## Conceitos abordados

Este projeto demonstra o sistema de eventos em C#, incluindo:

- **Events**: Declaração e uso de eventos
- **EventHandler**: Delegate padrão para eventos
- **Publisher-Subscriber Pattern**: Padrão de comunicação
- **Event Subscription**: Inscrição e desinscrição de eventos
- **Multicast Delegates**: Múltiplos subscribers para um evento
- **Event Lifecycle**: Ciclo de vida de eventos

## Objetivos de aprendizagem

- Implementar o padrão Publisher-Subscriber
- Entender a diferença entre delegates e events
- Gerenciar subscriptions de eventos
- Evitar memory leaks em event handling
- Criar sistemas desacoplados usando eventos

### O que Você Aprenderá

1. **Publisher-Subscriber Pattern**:
   - Desacoplamento entre componentes
   - Comunicação um-para-muitos
   - Notificação de mudanças de estado

2. **Diferença entre Delegates e Events**:
   - Events são encapsulamentos especiais de delegates
   - Events só podem ser disparados pela classe que os declara
   - Maior segurança e encapsulamento

3. **Event Lifecycle**:
   - Subscription: Adição de handlers
   - Invocation: Disparo do evento
   - Unsubscription: Remoção de handlers

4. **Memory Management**:
   - Event handlers mantêm referências
   - Importância de unsubscribe para evitar memory leaks
   - Weak events para cenários específicos

## Estrutura do projeto

```text
Events/
+-- Events.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/Events/Events.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Declaração de Evento

```csharp
public class Button
{
    public event EventHandler? Clicked;
    
    protected virtual void OnClicked()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
```

##### Subscription/Unsubscription

```csharp
button.Clicked += OnButtonClicked;  // Subscribe
button.Clicked -= OnButtonClicked;  // Unsubscribe
```

##### Custom EventArgs

```csharp
public class CustomEventArgs : EventArgs
{
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}
```

##### 1. Standard Event Pattern

```csharp
public class Publisher
{
    public event EventHandler<CustomEventArgs>? SomethingHappened;
    
    protected virtual void OnSomethingHappened(CustomEventArgs e)
    {
        SomethingHappened?.Invoke(this, e);
    }
}
```

##### 2. Custom Delegate

```csharp
public delegate void CustomEventHandler(string message);

public class CustomPublisher
{
    public event CustomEventHandler? CustomEvent;
    
    protected virtual void OnCustomEvent(string message)
    {
        CustomEvent?.Invoke(message);
    }
}
```

##### 3. Event com Múltiplos Parâmetros

```csharp
public class DataEventArgs : EventArgs
{
    public string Data { get; set; }
    public int Id { get; set; }
}

public event EventHandler<DataEventArgs>? DataReceived;
```

##### 1. UI Events

```csharp
public class Button
{
    public event EventHandler? Click;
    public event EventHandler? MouseEnter;
    public event EventHandler? MouseLeave;
}
```

##### 2. Model Events

```csharp
public class Model
{
    public event EventHandler? PropertyChanged;
    public event EventHandler? ValidationFailed;
    public event EventHandler? Saved;
}
```

##### 3. Service Events

```csharp
public class DataService
{
    public event EventHandler<DataEventArgs>? DataReceived;
    public event EventHandler? ConnectionLost;
    public event EventHandler? Reconnected;
}
```

##### Memory Leaks

```csharp
// ❌ Problemático - não faz unsubscribe
publisher.Event += Handler;

// ✅ Correto - sempre faça unsubscribe
publisher.Event += Handler;
// ... depois
publisher.Event -= Handler;
```

##### Exception Handling

```csharp
protected virtual void OnEvent()
{
    var handlers = Event;
    if (handlers != null)
    {
        foreach (EventHandler handler in handlers.GetInvocationList())
        {
            try
            {
                handler.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // Log exception, continue with other handlers
            }
        }
    }
}
```

##### Thread Safety

```csharp
public event EventHandler? MyEvent;

protected virtual void OnMyEvent()
{
    var handler = MyEvent; // Cópia local para thread safety
    handler?.Invoke(this, EventArgs.Empty);
}
```

##### 1. Event Aggregator

Centralizador de eventos para sistemas complexos.

##### 2. Weak Events

Previne memory leaks automaticamente.

##### 3. Async Events

```csharp
public event Func<object, EventArgs, Task>? AsyncEvent;

protected virtual async Task OnAsyncEvent()
{
    var handler = AsyncEvent;
    if (handler != null)
    {
        await handler.Invoke(this, EventArgs.Empty);
    }
}
```

##### Estrutura do Projeto

O projeto cont�m os seguintes componentes principais:

- **Publisher (Button)**: Define um evento `Clicked`, ao qual outros objetos podem se inscrever.
- **Subscriber (Logger)**: Escuta o evento `Clicked` e executa uma a��o quando notificado.
- **Program**: Cont�m o ponto de entrada da aplica��o e demonstra a funcionalidade do padr�o Observer.

##### Classe Button (Publisher)

```csharp
public class Button
{
    public event EventHandler? Clicked;

    public void Click()
    {
        Console.WriteLine("[Button] Clicked!");
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
```
- Define um evento `Clicked` do tipo `EventHandler`.
- Quando o m�todo `Click()` � chamado, ele dispara o evento `Clicked`, notificando todos os assinantes.

##### Classe Logger (Subscriber)

```csharp
public class Logger
{
    public void OnButtonClicked(object? sender, EventArgs e)
    {
        Console.WriteLine("[Logger] Button was clicked!");
    }
}
```
- Define o manipulador de eventos `OnButtonClicked`, que responde ao evento `Clicked` do `Button`.
- Exibe uma mensagem no console sempre que o evento � acionado.

##### Classe Program (Execu��o do Programa)

```csharp
class Program
{
    static void Main()
    {
        var button = new Button();
        var logger = new Logger();

        button.Clicked += logger.OnButtonClicked; // Inscri��o no evento
        button.Click(); // Gera sa�da no console

        button.Clicked -= logger.OnButtonClicked; // Desinscri��o do evento
        button.Click();  // Nenhuma sa�da, pois n�o h� inscritos
    }
}
```
- Cria um objeto `Button` e um `Logger`.
- O `Logger` se inscreve no evento `Clicked` do `Button`.
- Quando `Click()` � chamado, `OnButtonClicked()` � acionado.
- Em seguida, o `Logger` � desinscrito do evento, e novos cliques n�o geram sa�da.

##### Similaridade com o Padr�o Observer

Os eventos em C# seguem a mesma filosofia do padr�o Observer:
- O **Button** � o *Sujeito (Subject)* que mant�m uma lista de ouvintes (subscribers).
- O **Logger** � o *Observer*, que responde �s mudan�as (cliques no bot�o).
- A notifica��o ocorre automaticamente atrav�s do evento `Clicked`.
- Podemos adicionar ou remover observadores dinamicamente, tornando o sistema flex�vel e modular.

##### Benef�cios

- **Desacoplamento**: O *Publisher* n�o precisa conhecer os detalhes dos *Subscribers*.
- **Flexibilidade**: Novos *Subscribers* podem ser adicionados sem modificar o c�digo do *Publisher*.
- **Manuten��o facilitada**: Segue o princ�pio **Open/Closed**, permitindo extens�es sem altera��es no c�digo existente.

##### Conclus�o

Este projeto demonstra como os eventos e delegados do C# implementam eficientemente o padr�o Observer. Usar eventos � uma maneira idiom�tica e segura de implementar comunica��o ass�ncrona entre objetos em C#.

## Referências

- [Events (C# Programming Guide)](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/)
- [Observer Pattern](https://refactoring.guru/design-patterns/observer)
- [Weak Event Patterns](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/weak-event-patterns)
