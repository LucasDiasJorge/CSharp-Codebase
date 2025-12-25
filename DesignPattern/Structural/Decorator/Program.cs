using Decorator;
using Decorator.Decorators;

Console.WriteLine("=== DECORATOR PATTERN DEMO ===\n");
Console.WriteLine("Demonstração do padrão Decorator com Sistema de Notificações\n");

// Cenário 1: Notificação básica
Console.WriteLine("--- Cenário 1: Notificação Básica ---");
INotifier basicNotifier = new BaseNotifier();
basicNotifier.Send("Sistema iniciado com sucesso!");
Console.WriteLine($"Descrição: {basicNotifier.GetDescription()}");
Console.WriteLine($"Custo: ${basicNotifier.GetCost():F2}\n");

// Cenário 2: Notificação com Email
Console.WriteLine("--- Cenário 2: Notificação + Email ---");
INotifier emailNotifier = new EmailDecorator(
    new BaseNotifier(),
    "admin@example.com"
);
emailNotifier.Send("Novo usuário registrado!");
Console.WriteLine($"Descrição: {emailNotifier.GetDescription()}");
Console.WriteLine($"Custo: ${emailNotifier.GetCost():F2}\n");

// Cenário 3: Notificação com Email e SMS (múltiplos decoradores)
Console.WriteLine("--- Cenário 3: Notificação + Email + SMS ---");
INotifier multiChannelNotifier = new SmsDecorator(
    new EmailDecorator(
        new BaseNotifier(),
        "admin@example.com"
    ),
    "+55 11 98765-4321"
);
multiChannelNotifier.Send("Pagamento aprovado!");
Console.WriteLine($"Descrição: {multiChannelNotifier.GetDescription()}");
Console.WriteLine($"Custo: ${multiChannelNotifier.GetCost():F2}\n");

// Cenário 4: Notificação completa (Email + SMS + Slack + Priority)
Console.WriteLine("--- Cenário 4: Notificação Completa (Email + SMS + Slack + Priority) ---");
INotifier completeNotifier = new PriorityDecorator(
    new SlackDecorator(
        new SmsDecorator(
            new EmailDecorator(
                new BaseNotifier(),
                "security@example.com"
            ),
            "+55 11 91234-5678"
        ),
        "alerts"
    ),
    "HIGH"
);
completeNotifier.Send("ALERTA: Tentativa de acesso não autorizado detectada!");
Console.WriteLine($"Descrição: {completeNotifier.GetDescription()}");
Console.WriteLine($"Custo: ${completeNotifier.GetCost():F2}\n");

// Cenário 5: Apenas SMS e Slack (sem Email)
Console.WriteLine("--- Cenário 5: Apenas SMS + Slack ---");
INotifier customNotifier = new SlackDecorator(
    new SmsDecorator(
        new BaseNotifier(),
        "+55 21 99999-8888"
    ),
    "general"
);
customNotifier.Send("Manutenção programada para hoje à noite");
Console.WriteLine($"Descrição: {customNotifier.GetDescription()}");
Console.WriteLine($"Custo: ${customNotifier.GetCost():F2}\n");

// Cenário 6: Demonstração da flexibilidade - Construindo dinamicamente
Console.WriteLine("--- Cenário 6: Construção Dinâmica de Notificador ---");
INotifier dynamicNotifier = new BaseNotifier();

Console.WriteLine("Escolha os canais de notificação:");
Console.WriteLine("Adicionando Email...");
dynamicNotifier = new EmailDecorator(dynamicNotifier, "dynamic@example.com");

Console.WriteLine("Adicionando Slack...");
dynamicNotifier = new SlackDecorator(dynamicNotifier, "notifications");

Console.WriteLine("Adicionando Prioridade...");
dynamicNotifier = new PriorityDecorator(dynamicNotifier, "MEDIUM");

Console.WriteLine("\nEnviando notificação customizada:");
dynamicNotifier.Send("Notificação configurada dinamicamente!");
Console.WriteLine($"Descrição: {dynamicNotifier.GetDescription()}");
Console.WriteLine($"Custo: ${dynamicNotifier.GetCost():F2}\n");

Console.WriteLine("=== FIM DA DEMONSTRAÇÃO ===");
