using System;

// Publisher
public class Button
{
    public event EventHandler? Clicked;

    public void Click()
    {
        Console.WriteLine("[Button] Clicked!");
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}

// Subscriber
public class Logger
{
    public void OnButtonClicked(object? sender, EventArgs e)
    {
        Console.WriteLine("[Logger] Button was clicked!");
    }
}

class Program
{
    static void Main()
    {
        var button = new Button();
        var logger = new Logger();

        button.Clicked += logger.OnButtonClicked;
        button.Click();

        // Unsubscribe
        button.Clicked -= logger.OnButtonClicked;
        button.Click();  // No output (no subscribers)
    }
}