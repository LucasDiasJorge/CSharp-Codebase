namespace ChainOfResponsability;

abstract class Approver
{
    protected Approver? Next { get; private set; }

    public void SetNext(Approver next)
    {
        Next = next;
    }

    public abstract void ApproveExpense(decimal amount);
}

class Manager : Approver
{
    public override void ApproveExpense(decimal amount)
    {
        if (amount <= 1000)
            Console.WriteLine($"Manager aprovou a despesa de R${amount}");
        else
            Next?.ApproveExpense(amount);
    }
}

class Director : Approver
{
    public override void ApproveExpense(decimal amount)
    {
        if (amount <= 10000)
            Console.WriteLine($"Director aprovou a despesa de R${amount}");
        else
            Next?.ApproveExpense(amount);
    }
}

class CEO : Approver
{
    public override void ApproveExpense(decimal amount)
    {
        if (amount <= 50000)
            Console.WriteLine($"CEO aprovou a despesa de R${amount}");
        else
            Console.WriteLine($"Despesa de R${amount} não aprovada. Valor muito alto!");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Approver manager = new Manager();
        Approver director = new Director();
        Approver ceo = new CEO();

        manager.SetNext(director);
        director.SetNext(ceo);

        decimal[] expenses = { 500, 2000, 15000, 60000 };

        foreach (var expense in expenses)
        {
            manager.ApproveExpense(expense);
        }
    }
}
