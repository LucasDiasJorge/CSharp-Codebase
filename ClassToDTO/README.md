## 🔁 1. **Lazy Loading** (Carregamento preguiçoso)

### ✅ Requisitos:

* Propriedades `virtual`.
* Habilitar o proxy do EF.

### 📌 Exemplo:

```csharp
var pedido = context.Pedidos.Find(1);  // apenas o Pedido é carregado

var itens = pedido.Itens;  // agora o EF faz uma nova consulta para carregar os itens
```

🔧 O EF só busca os `Itens` **quando você acessa** a propriedade.

---

## ⚡ 2. **Eager Loading** (Carregamento ansioso)

### ✅ Usando `.Include()`:

```csharp
var pedido = context.Pedidos
    .Include(p => p.Itens)   // carrega os Itens junto com o Pedido
    .FirstOrDefault(p => p.Id == 1);
```

📝 Aqui, o EF gera um JOIN ou uma consulta que já traz tudo junto — **Pedido e Itens** ao mesmo tempo.

---

## 🛠 3. **Explicit Loading** (Carregamento manual)

### ✅ Exemplo:

```csharp
var pedido = context.Pedidos.Find(1);  // só o Pedido é carregado

context.Entry(pedido)
    .Collection(p => p.Itens)
    .Load();  // carrega os Itens agora, manualmente
```

📌 Isso é útil quando você quer **controlar quando exatamente** os dados são carregados, por exemplo, para otimizar a performance.

---

## 🔍 Comparação rápida em C\#

| Tipo         | Código exemplo                                           | Quando carrega?                  |
| ------------ | -------------------------------------------------------- | -------------------------------- |
| **Lazy**     | `var itens = pedido.Itens;`                              | Só ao acessar a propriedade      |
| **Eager**    | `.Include(p => p.Itens)`                                 | Junto com o `Pedido` na consulta |
| **Explicit** | `context.Entry(pedido).Collection(p => p.Itens).Load();` | Manualmente, sob demanda         |

---
