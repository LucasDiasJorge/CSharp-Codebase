# 🎨 Decorator Pattern

## 📋 Descrição

O **Decorator Pattern** é um padrão de design estrutural que permite adicionar novos comportamentos a objetos dinamicamente, colocando-os dentro de objetos especiais chamados "wrappers" (embrulhos) que contêm esses comportamentos.

## 🎯 Objetivo

- Adicionar responsabilidades a objetos individuais de forma dinâmica e transparente
- Fornecer uma alternativa flexível à herança para estender funcionalidades
- Permitir combinações de comportamentos sem criar explosão de subclasses

## 🏗️ Estrutura

```
INotifier (Interface)
    ↑
    |
BaseNotifier (Componente Concreto)
    ↑
    |
NotifierDecorator (Decorator Abstrato)
    ↑
    |
    ├── EmailDecorator (Decorator Concreto)
    ├── SmsDecorator (Decorator Concreto)
    ├── SlackDecorator (Decorator Concreto)
    └── PriorityDecorator (Decorator Concreto)
```

## 📦 Componentes

### 1. **INotifier** (Component Interface)
Interface que define as operações que podem ser alteradas por decoradores.

### 2. **BaseNotifier** (Concrete Component)
Implementação básica que pode ser decorada.

### 3. **NotifierDecorator** (Base Decorator)
Classe abstrata que mantém referência ao componente decorado e delega operações para ele.

### 4. **Decoradores Concretos**
- **EmailDecorator**: Adiciona envio por email
- **SmsDecorator**: Adiciona envio por SMS
- **SlackDecorator**: Adiciona envio por Slack
- **PriorityDecorator**: Adiciona marcação de prioridade

## 💡 Exemplo de Uso

```csharp
// Notificação simples
INotifier notifier = new BaseNotifier();

// Adicionar Email
notifier = new EmailDecorator(notifier, "user@example.com");

// Adicionar SMS
notifier = new SmsDecorator(notifier, "+55 11 98765-4321");

// Adicionar Slack
notifier = new SlackDecorator(notifier, "alerts");

// Adicionar Prioridade
notifier = new PriorityDecorator(notifier, "HIGH");

// Enviar notificação com todos os decoradores
notifier.Send("Mensagem importante!");
```

## ✅ Vantagens

1. **Flexibilidade**: Adiciona funcionalidades em tempo de execução
2. **Composição**: Combina múltiplos comportamentos
3. **Single Responsibility**: Cada decorador tem uma responsabilidade específica
4. **Open/Closed**: Estende funcionalidades sem modificar código existente
5. **Sem explosão de classes**: Evita criar dezenas de subclasses para cada combinação

## ❌ Desvantagens

1. Pode resultar em muitas classes pequenas no sistema
2. Dificulta a remoção de um decorador específico da pilha
3. Pode ser difícil configurar decoradores que dependem da ordem
4. Código inicial pode parecer complexo para iniciantes

## 🔄 Diferença: Decorator vs Herança

### Herança
```csharp
// Precisa criar uma classe para cada combinação
class EmailNotifier : BaseNotifier { }
class SmsNotifier : BaseNotifier { }
class EmailAndSmsNotifier : BaseNotifier { }
class EmailSmsSlackNotifier : BaseNotifier { }
// ... explosão de classes!
```

### Decorator
```csharp
// Composição flexível
var notifier = new EmailDecorator(
    new SmsDecorator(
        new SlackDecorator(
            new BaseNotifier()
        )
    )
);
```

## 🎭 Cenários de Uso Real

1. **Sistema de Logging**: Adicionar timestamps, níveis de log, formatação
2. **Streams I/O**: BufferedStream, GZipStream, CryptoStream
3. **UI Components**: Adicionar scrollbars, bordas, sombras
4. **Middleware HTTP**: Pipeline de processamento de requisições
5. **Notificações**: Múltiplos canais de comunicação (como neste exemplo)
6. **Cache**: Adicionar camadas de cache a repositórios
7. **Segurança**: Adicionar criptografia, compressão, validação

## 🏃 Como Executar

```bash
cd DesignPattern/Structural/Decorator
dotnet run
```

## 📊 Saída Esperada

```
=== DECORATOR PATTERN DEMO ===

--- Cenário 1: Notificação Básica ---
📧 [Base Notification] Sistema iniciado com sucesso!
Descrição: Base Notification
Custo: $0.00

--- Cenário 2: Notificação + Email ---
📧 [Base Notification] Novo usuário registrado!
📧 [EMAIL] Sending to admin@example.com: Novo usuário registrado!
Descrição: Base Notification + Email
Custo: $0.10

--- Cenário 3: Notificação + Email + SMS ---
📧 [Base Notification] Pagamento aprovado!
📧 [EMAIL] Sending to admin@example.com: Pagamento aprovado!
📱 [SMS] Sending to +55 11 98765-4321: Pagamento aprovado!
Descrição: Base Notification + Email + SMS
Custo: $0.35
```

## 🔗 Relação com Outros Padrões

- **Adapter**: Muda a interface, Decorator adiciona funcionalidades
- **Composite**: Decorator pode ser visto como Composite com um único filho
- **Strategy**: Decorator muda a "pele", Strategy muda as "entranhas"
- **Proxy**: Controla acesso, Decorator adiciona funcionalidades
- **Chain of Responsibility**: Similar na estrutura, mas com propósitos diferentes

## 📚 Referências

- Design Patterns: Elements of Reusable Object-Oriented Software (GoF)
- [Refactoring Guru - Decorator](https://refactoring.guru/design-patterns/decorator)
- [Microsoft Docs - Decorator Pattern](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)

## 🎯 Princípios SOLID Aplicados

- ✅ **Single Responsibility**: Cada decorador tem uma responsabilidade
- ✅ **Open/Closed**: Aberto para extensão (novos decoradores), fechado para modificação
- ✅ **Liskov Substitution**: Decoradores são substituíveis pelo componente base
- ✅ **Interface Segregation**: Interface INotifier é específica e coesa
- ✅ **Dependency Inversion**: Depende de abstrações (INotifier), não de implementações
