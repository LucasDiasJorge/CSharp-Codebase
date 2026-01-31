# 🔌 Circuit Breaker - C# Didático

> **Implementação simples e intuitiva do padrão Circuit Breaker**  
> Aprenda o conceito essencial em menos de 150 linhas de código!

---

## 📚 O que é Circuit Breaker?

Circuit Breaker é um padrão de design que **previne falhas em cascata** em sistemas distribuídos.

**Analogia:** Funciona como um **disjuntor elétrico** na sua casa:
- ⚡ Quando tudo está normal, a eletricidade flui
- 🔥 Se detecta sobrecarga (muitos erros), **desliga** automaticamente
- ⏰ Após um tempo, **tenta religar** para ver se voltou ao normal

---

## 🎯 Por que usar?

### Problema sem Circuit Breaker:
```
Seu App → Serviço Lento/Fora
   ↓           ↓
 Espera      Timeout
   ↓           ↓
Repete      Timeout
   ↓           ↓
Repete      Timeout
   ↓
❌ Sistema trava esperando algo que não funciona
```

### Solução com Circuit Breaker:
```
Seu App → Circuit Breaker → Serviço Lento/Fora
   ↓            ↓
 Falha      Detecta 3 erros
   ↓            ↓
Falha       Abre circuito
   ↓            ↓
  ❌ ←────── Rejeita imediatamente
   ↓
✅ Sistema continua responsivo (fast-fail)
```

**Benefícios:**
- ⚡ **Fast-fail** - Falha rápido em vez de esperar timeout
- 🛡️ **Proteção** - Não sobrecarrega serviços já problemáticos
- 🔄 **Auto-recuperação** - Testa automaticamente quando voltar
- 📊 **Feedback** - Sabe quando algo está errado

---

## 🚦 Estados do Circuit Breaker

```
        Normal                    3+ Erros              Timeout
       ┌─────────┐              ┌─────────┐           ┌─────────┐
   ┌──>│ FECHADO │──── Erro ───>│ ABERTO  │─── 60s ──>│  MEIO-  │
   │   │ (Closed)│              │ (Open)  │           │ ABERTO  │
   │   └─────────┘              └─────────┘           └─────────┘
   │        ✅                       🚫                     ⚠️
   │   Permite tudo            Bloqueia tudo         Testa 1 vez
   │                                                        │
   └─────────────────────────── Sucesso ────────────────────┘
```

### 1️⃣ **FECHADO** (Closed)
- ✅ Estado normal
- Permite todas as chamadas
- Conta erros consecutivos

### 2️⃣ **ABERTO** (Open)
- 🚫 Modo proteção
- Rejeita todas as chamadas imediatamente
- Aguarda tempo de timeout (ex: 60 segundos)

### 3️⃣ **MEIO-ABERTO** (Half-Open)
- ⚠️ Testando recuperação
- Permite **1 chamada** de teste
- Se funcionar → volta FECHADO ✅
- Se falhar → volta ABERTO 🚫

---

## 🚀 Como usar?

### Uso básico:

```csharp
// 1. Criar o Circuit Breaker
var cb = new CircuitBreaker(
    limiteErros: 3,       // Abre após 3 erros
    segundosEspera: 60    // Fica aberto por 60s
);

// 2. Usar para proteger operações
try
{
    var resultado = cb.Executar(() => {
        // Sua operação arriscada aqui
        return ChamarAPIExterna();
    });
    
    Console.WriteLine($"Sucesso: {resultado}");
}
catch (Exception ex)
{
    Console.WriteLine($"Falhou: {ex.Message}");
}
```

### Exemplo real:

```csharp
// Proteger chamada HTTP
var circuitBreaker = new CircuitBreaker(limiteErros: 5, segundosEspera: 30);

string BuscarUsuario(int id)
{
    return circuitBreaker.Executar(() => {
        using var client = new HttpClient();
        var response = client.GetStringAsync($"https://api.exemplo.com/users/{id}").Result;
        return response;
    }, nome: "API Usuários");
}
```

---

## ⚙️ Configuração

| Parâmetro | Descrição | Valor Recomendado |
|-----------|-----------|-------------------|
| `limiteErros` | Quantos erros antes de abrir | **3-5** |
| `segundosEspera` | Tempo que fica aberto | **30-60** segundos |

**Dicas de configuração:**

- **APIs críticas:** `limiteErros: 2, segundosEspera: 30`
- **APIs estáveis:** `limiteErros: 5, segundosEspera: 60`
- **Serviços lentos:** `limiteErros: 3, segundosEspera: 120`

---

## 🎮 Executar demonstração

```bash
dotnet run
```

**O que você verá:**
1. Serviço falha 3 vezes → Circuito **ABRE** 🔴
2. Próximas chamadas são **bloqueadas** imediatamente 🚫
3. Após 5 segundos → Entra em **MEIO-ABERTO** ⚠️
4. Serviço funciona → Circuito **FECHA** 🟢
5. Volta ao normal ✅

---

## 📖 Conceitos importantes

### Fast-Fail
> Falhar rápido em vez de esperar timeout

**Sem Circuit Breaker:**
```
Timeout: 30s × 10 tentativas = 5 minutos esperando ❌
```

**Com Circuit Breaker:**
```
3 timeouts (30s cada) = 1.5 min
Depois: rejeição imediata (0.001s) ✅
```

### Self-Healing
> Recuperação automática sem intervenção manual

O Circuit Breaker **testa automaticamente** se o serviço voltou:
- Não precisa reiniciar nada
- Não precisa intervenção humana
- Recupera sozinho quando possível

### Graceful Degradation
> Degradação elegante do serviço

```csharp
try
{
    return circuitBreaker.Executar(() => BuscarDoBancoPrincipal());
}
catch
{
    // Fallback: usa cache ou banco secundário
    return BuscarDoCache();
}
```

---

## 🔬 Quando usar?

### ✅ Use Circuit Breaker quando:
- Chamar **APIs externas** (podem estar fora)
- Acessar **banco de dados** (pode ficar lento)
- Integrar com **microsserviços** (podem falhar)
- Operações com **timeout** (rede, I/O)
- Sistemas **distribuídos** (falhas parciais)

### ❌ Não use para:
- Validação de entrada do usuário
- Lógica de negócio local
- Operações sempre síncronas e rápidas
- Erros de programação (bugs)

---

## 🏗️ Estrutura do código

```
CircuitBreakerDemo/
├── Program.cs                 # Código completo (~150 linhas)
│   ├── CircuitBreaker        # Implementação do padrão
│   ├── Program               # Demonstração
│   └── ServicoInstavel       # Mock para testes
├── CircuitBreakerDemo.csproj # Projeto .NET 8.0
└── README.md                 # Este arquivo
```

**Simples assim!** Apenas 1 arquivo de código. 🎯

---

## 📚 Referências para estudo aprofundado

### 📄 Artigos Fundamentais
- [**Martin Fowler - Circuit Breaker**](https://martinfowler.com/bliki/CircuitBreaker.html)  
  ⭐ O artigo original que define o padrão

- [**Microsoft - Circuit Breaker Pattern**](https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)  
  Implementação em ambientes cloud

### 📦 Bibliotecas Profissionais

#### Polly (.NET)
```bash
dotnet add package Polly
```
- ⭐ Biblioteca mais popular para resiliência em .NET
- Circuit Breaker + Retry + Timeout + Bulkhead
- [Documentação Polly](https://github.com/App-vNext/Polly)

**Exemplo com Polly:**
```csharp
var policy = Policy
    .Handle<Exception>()
    .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));

await policy.ExecuteAsync(() => MinhaOperacao());
```

#### Resilience4j (Java)
- Circuit Breaker para ecosistema Java/Spring
- [GitHub Resilience4j](https://github.com/resilience4j/resilience4j)

#### Hystrix (Java - Netflix)
- Pioneiro, agora em manutenção
- Inspirou Polly e Resilience4j
- [GitHub Netflix/Hystrix](https://github.com/Netflix/Hystrix)

### 📖 Livros

**Release It! (Michael Nygard)**
- Capítulo sobre Circuit Breaker e padrões de estabilidade
- [Link Amazon](https://pragprog.com/titles/mnee2/release-it-second-edition/)

**Building Microservices (Sam Newman)**
- Circuit Breaker no contexto de microsserviços
- [Link O'Reilly](https://www.oreilly.com/library/view/building-microservices-2nd/9781492034018/)

### 🎥 Vídeos

- [Circuit Breaker Pattern - Gaurav Sen](https://www.youtube.com/watch?v=ADHcBxEXvFA) ⭐
- [Microservices Resilience - IBM Technology](https://www.youtube.com/watch?v=zKdRmKB1_lA)

### 🔧 Padrões Relacionados

| Padrão | O que faz | Quando usar |
|--------|-----------|-------------|
| **Retry** | Tenta novamente após falha | Erros transitórios |
| **Timeout** | Limita tempo de espera | Operações lentas |
| **Bulkhead** | Isola recursos | Prevenir esgotamento |
| **Rate Limiter** | Limita taxa de chamadas | Controle de carga |
| **Fallback** | Alternativa quando falha | Degradação elegante |

**Exemplo combinado:**
```csharp
// 1. Timeout de 5s
// 2. Retry até 3 vezes
// 3. Circuit Breaker se falhar muito
// 4. Fallback se tudo falhar

var policy = Policy
    .Handle<Exception>()
    .FallbackAsync(() => UsarCache())  // 4. Fallback
    .WrapAsync(
        Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1))  // 3. Circuit Breaker
            .WrapAsync(
                Policy
                    .Handle<Exception>()
                    .RetryAsync(3)  // 2. Retry
                    .WrapAsync(
                        Policy
                            .TimeoutAsync(TimeSpan.FromSeconds(5))  // 1. Timeout
                    )
            )
    );
```

### 🌐 Recursos Online

- [Awesome Resilience](https://github.com/Badel2/awesome-resilience) - Lista curada de recursos
- [Azure Architecture Patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/) - Padrões cloud
- [AWS Well-Architected](https://aws.amazon.com/architecture/well-architected/) - Best practices

---

## ❓ FAQ

**P: Circuit Breaker substitui tratamento de exceções?**  
R: Não, ele **complementa**. Você ainda precisa de try-catch.

**P: Posso ter múltiplos Circuit Breakers?**  
R: Sim! Um por serviço externo (API A, API B, Banco, etc).

**P: Como monitorar em produção?**  
R: Use logs, métricas (Prometheus) ou APM (Application Insights).

**P: E se o serviço demorar muito para voltar?**  
R: O Circuit Breaker vai continuar testando periodicamente.

**P: Funciona com async/await?**  
R: Esta versão não, mas basta trocar `Func<T>` por `Func<Task<T>>`.

---
