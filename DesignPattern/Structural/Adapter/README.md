# Padrão Adapter (Adaptador)

## 🎯 Objetivo
O **Adapter** (Adaptador) permite que **interfaces incompatíveis trabalhem juntas** ao atuar como um tradutor entre o código cliente e um sistema legado ou biblioteca externa. Resolve o problema de integração sem modificar o código existente.

> Benefício principal: Integra sistemas com interfaces diferentes sem alterar suas implementações originais.

## 🧠 Quando Usar
Use este padrão quando:
- Precisa integrar uma biblioteca ou sistema com interface incompatível
- Quer reutilizar código legado em uma arquitetura moderna
- Há necessidade de converter formatos de dados entre camadas
- Deseja isolar dependências externas através de uma interface limpa

## 🏗️ Estrutura do Exemplo
Este exemplo simula a integração entre um **repositório moderno** e um **banco de dados legado** com formatos incompatíveis.

| Componente | Função | Localização |
|------------|--------|-------------|
| `IClientRepository` | Interface target (moderna) | `Interfaces/` |
| `Client` | Modelo de domínio | `Models/` |
| `LegacyDatabase` | Sistema legado (adaptee) | `Legacy/` |
| `ClientRepositoryAdapter` | Adapter que traduz as interfaces | `Adapters/` |
| `Program.cs` | Demonstração de uso | Raiz |

## 📁 Organização do Projeto
```
Adapter/
├── Adapters/
│   └── ClientRepositoryAdapter.cs    # Classe adapter principal
├── Interfaces/
│   └── IClientRepository.cs          # Interface target moderna
├── Legacy/
│   └── LegacyDatabase.cs            # Sistema legado (adaptee)
├── Models/
│   └── Client.cs                    # Modelo de domínio
├── Program.cs                       # Demonstração e execução
└── README.md                        # Este arquivo
```

## 🔄 Fluxo Demonstrado
1. **Criação** do adapter que encapsula o sistema legado
2. **Adição** de clientes usando interface moderna (`IClientRepository`)
3. **Conversão** automática para formato legado (dicionários)
4. **Recuperação** e conversão de volta para objetos modernos
5. **Demonstração** de funcionalidades adicionais do adapter

## ▶️ Execução
```bash
cd Adapter
dotnet run
```

Saída esperada:
```
=== Demonstração do Padrão Adapter ===
Integrando sistema legado com interface moderna

1. Criando repositório com adapter:
[Adapter] Initialized with legacy database

2. Adicionando clientes através da interface moderna:
[Legacy DB] Inserted client: Lucas
[Legacy DB] Inserted client: Maria
[Legacy DB] Inserted client: João

3. Recuperando clientes através da interface moderna:
[Legacy DB] Fetching 3 records
[Adapter] Converted 3 legacy records to Client objects

4. Listando clientes:
  • Lucas, 22 years old
  • Maria, 30 years old
  • João, 25 years old

5. Demonstrando funcionalidade adicional do adapter:
  Total de clientes: 3

=== Benefícios do Adapter ===
✅ Sistema legado integrado sem modificações
✅ Interface moderna mantida consistente
✅ Conversões de dados centralizadas no adapter
✅ Facilita testes e manutenção
```

## 🧪 Exemplo de Código (trecho de `ClientRepositoryAdapter.cs`)
```csharp
public void AddClient(Client client)
{
    // Converte Client para formato esperado pelo sistema legado
    var record = new Dictionary<string, object>
    {
        { "Name", client.Name },
        { "Age", client.Age },
        { "CreatedAt", DateTime.Now }
    };

    _legacyDb.Insert(record); // Chama sistema legado
}

public List<Client> GetAllClients()
{
    var legacyRecords = _legacyDb.FetchAll();
    var clients = new List<Client>();

    foreach (var record in legacyRecords)
    {
        // Converte formato legado de volta para Client
        var client = new Client
        {
            Name = record["Name"]?.ToString() ?? "Unknown",
            Age = Convert.ToInt32(record["Age"])
        };
        clients.Add(client);
    }
    return clients;
}
```

## ✅ Benefícios Evidenciados
- **Reutilização**: Sistema legado continua funcionando sem modificações
- **Desacoplamento**: Interface moderna isolada de detalhes legados
- **Manutenibilidade**: Conversões centralizadas em um local
- **Testabilidade**: Adapter pode ser facilmente testado e mockado

## ⚠️ Trade-offs
| Aspecto | Consideração |
|---------|--------------|
| **Complexidade** | Adiciona uma camada extra de abstração |
| **Performance** | Conversões de dados podem impactar performance |
| **Manutenção** | Mudanças no sistema legado podem quebrar o adapter |
| **Debugging** | Pode dificultar rastreamento de bugs entre sistemas |

## 🔧 Possíveis Extensões
- **Cache**: Implementar cache para reduzir chamadas ao sistema legado
- **Validação**: Adicionar validações na conversão de dados
- **Logging**: Melhorar logs para auditoria e debugging
- **Async**: Suporte a operações assíncronas
- **Múltiplos Sistemas**: Adapter para vários sistemas legados diferentes

## 🆚 Vs Outros Padrões
| Padrão | Diferença |
|--------|-----------|
| **Facade** | Adapter traduz interfaces; Facade simplifica complexidade |
| **Decorator** | Adapter muda interface; Decorator mantém interface e adiciona comportamento |
| **Proxy** | Adapter muda interface; Proxy mantém mesma interface |

## 📌 TL;DR
Use o Adapter para **integrar sistemas incompatíveis** mantendo o código cliente limpo e o sistema legado inalterado.

---
**Autor:** Lucas Jorge  
**Tecnologia:** .NET / C#  
**Categoria:** Structural Pattern
