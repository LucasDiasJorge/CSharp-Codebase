# Implementação do Padrão Observer com Eventos em C#

## Introdução
Este projeto demonstra o uso do padrão de projeto **Observer** utilizando **eventos e delegados** em C#. O padrão Observer é amplamente utilizado para estabelecer uma comunicação desacoplada entre objetos, onde um objeto (o *Publisher*) notifica automaticamente seus *Subscribers* quando ocorre uma mudança de estado.

## Conceitos Principais
### Padrão Observer
O **Observer** é um padrão de design comportamental que define uma dependência de um para muitos entre objetos. Sempre que um objeto muda de estado, todos os objetos dependentes são notificados automaticamente.

No C#, a implementação deste padrão pode ser feita utilizando eventos e delegados, que fornecem uma maneira integrada de gerenciar assinaturas e notificações de forma segura.

## Estrutura do Projeto
O projeto contém os seguintes componentes principais:

- **Publisher (Button)**: Define um evento `Clicked`, ao qual outros objetos podem se inscrever.
- **Subscriber (Logger)**: Escuta o evento `Clicked` e executa uma ação quando notificado.
- **Program**: Contém o ponto de entrada da aplicação e demonstra a funcionalidade do padrão Observer.

## Código Explicado
### Classe Button (Publisher)
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
- Quando o método `Click()` é chamado, ele dispara o evento `Clicked`, notificando todos os assinantes.

### Classe Logger (Subscriber)
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
- Exibe uma mensagem no console sempre que o evento é acionado.

### Classe Program (Execução do Programa)
```csharp
class Program
{
    static void Main()
    {
        var button = new Button();
        var logger = new Logger();

        button.Clicked += logger.OnButtonClicked; // Inscrição no evento
        button.Click(); // Gera saída no console

        button.Clicked -= logger.OnButtonClicked; // Desinscrição do evento
        button.Click();  // Nenhuma saída, pois não há inscritos
    }
}
```
- Cria um objeto `Button` e um `Logger`.
- O `Logger` se inscreve no evento `Clicked` do `Button`.
- Quando `Click()` é chamado, `OnButtonClicked()` é acionado.
- Em seguida, o `Logger` é desinscrito do evento, e novos cliques não geram saída.

## Similaridade com o Padrão Observer
Os eventos em C# seguem a mesma filosofia do padrão Observer:
- O **Button** é o *Sujeito (Subject)* que mantém uma lista de ouvintes (subscribers).
- O **Logger** é o *Observer*, que responde às mudanças (cliques no botão).
- A notificação ocorre automaticamente através do evento `Clicked`.
- Podemos adicionar ou remover observadores dinamicamente, tornando o sistema flexível e modular.

## Benefícios
- **Desacoplamento**: O *Publisher* não precisa conhecer os detalhes dos *Subscribers*.
- **Flexibilidade**: Novos *Subscribers* podem ser adicionados sem modificar o código do *Publisher*.
- **Manutenção facilitada**: Segue o princípio **Open/Closed**, permitindo extensões sem alterações no código existente.

## Conclusão
Este projeto demonstra como os eventos e delegados do C# implementam eficientemente o padrão Observer. Usar eventos é uma maneira idiomática e segura de implementar comunicação assíncrona entre objetos em C#.

