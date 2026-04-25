# Padrão Adapter (Adaptador)

## Visão geral

Projeto didático do CSharp-101 dedicado a Padrão Adapter (Adaptador), com foco em design patterns, modelagem OO e código limpo.

## Conceitos abordados

- Exemplo didático sobre Padrão Adapter (Adaptador) no contexto de design patterns, modelagem OO e código limpo.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

O **Adapter** (Adaptador) permite que **interfaces incompatíveis trabalhem juntas** ao atuar como um tradutor entre o código cliente e um sistema legado ou biblioteca externa. Resolve o problema de integração sem modificar o código existente.

> Benefício principal: Integra sistemas com interfaces diferentes sem alterar suas implementações originais.

## Estrutura do projeto

```text
Adapter/
+-- Adapters/
|   \-- ClientRepositoryAdapter.cs
+-- Interfaces/
|   \-- IClientRepository.cs
+-- Legacy/
|   \-- LegacyDatabase.cs
+-- Models/
|   \-- Client.cs
+-- Adapter.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/DesignPattern/Structural/Adapter/Adapter.csproj
```

Saída esperada:

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Quando Usar

Use este padrão quando:
- Precisa integrar uma biblioteca ou sistema com interface incompatível
- Quer reutilizar código legado em uma arquitetura moderna
- Há necessidade de converter formatos de dados entre camadas
- Deseja isolar dependências externas através de uma interface limpa

##### Estrutura do Exemplo

Este exemplo simula a integração entre um **repositório moderno** e um **banco de dados legado** com formatos incompatíveis.

| Componente | Função | Localização |
|------------|--------|-------------|
| `IClientRepository` | Interface target (moderna) | `Interfaces/` |
| `Client` | Modelo de domínio | `Models/` |
| `LegacyDatabase` | Sistema legado (adaptee) | `Legacy/` |
| `ClientRepositoryAdapter` | Adapter que traduz as interfaces | `Adapters/` |
| `Program.cs` | Demonstração de uso | Raiz |

##### Organização do Projeto

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

##### Fluxo Demonstrado

1. **Criação** do adapter que encapsula o sistema legado
2. **Adição** de clientes usando interface moderna (`IClientRepository`)
3. **Conversão** automática para formato legado (dicionários)
4. **Recuperação** e conversão de volta para objetos modernos
5. **Demonstração** de funcionalidades adicionais do adapter

##### Exemplo de Código (trecho de `ClientRepositoryAdapter.cs`)

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

##### Benefícios Evidenciados

- **Reutilização**: Sistema legado continua funcionando sem modificações
- **Desacoplamento**: Interface moderna isolada de detalhes legados
- **Manutenibilidade**: Conversões centralizadas em um local
- **Testabilidade**: Adapter pode ser facilmente testado e mockado

##### Trade-offs

| Aspecto | Consideração |
|---------|--------------|
| **Complexidade** | Adiciona uma camada extra de abstração |
| **Performance** | Conversões de dados podem impactar performance |
| **Manutenção** | Mudanças no sistema legado podem quebrar o adapter |
| **Debugging** | Pode dificultar rastreamento de bugs entre sistemas |

##### Possíveis Extensões

- **Cache**: Implementar cache para reduzir chamadas ao sistema legado
- **Validação**: Adicionar validações na conversão de dados
- **Logging**: Melhorar logs para auditoria e debugging
- **Async**: Suporte a operações assíncronas
- **Múltiplos Sistemas**: Adapter para vários sistemas legados diferentes

##### Vs Outros Padrões

| Padrão | Diferença |
|--------|-----------|
| **Facade** | Adapter traduz interfaces; Facade simplifica complexidade |
| **Decorator** | Adapter muda interface; Decorator mantém interface e adiciona comportamento |
| **Proxy** | Adapter muda interface; Proxy mantém mesma interface |

##### TL;DR

Use o Adapter para **integrar sistemas incompatíveis** mantendo o código cliente limpo e o sistema legado inalterado.

**Autor:** Lucas Jorge  
**Tecnologia:** .NET / C#  
**Categoria:** Structural Pattern
