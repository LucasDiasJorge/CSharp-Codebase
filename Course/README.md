# 🔄 Course - JSON Serialization e Deserialization

## 🎯 Objetivos de Aprendizado

- Dominar **serialização e desserialização JSON** em .NET
- Usar **System.Text.Json** para manipulação de dados
- Configurar **mapeamento de propriedades** com atributos
- Trabalhar com **diferentes tipos de dados** (DateTime, DateTimeOffset, etc.)
- Implementar **conversores customizados**
- Comparar **System.Text.Json** vs **Newtonsoft.Json**

## 📚 Conceitos Fundamentais

### O que é Serialização JSON?

**Serialização** é o processo de converter objetos .NET em formato JSON (texto). **Desserialização** é o processo inverso - converter JSON em objetos .NET.

### System.Text.Json vs Newtonsoft.Json

| Aspecto | System.Text.Json | Newtonsoft.Json |
|---------|------------------|-----------------|
| **Performance** | ⚡ Mais rápido | 🐌 Mais lento |
| **Allocations** | 🔋 Menos memória | 💾 Mais memória |
| **API** | 🔧 Funcional | 🛠️ Flexível |
| **Compatibilidade** | .NET Core 3.0+ | .NET Framework 2.0+ |
| **Features** | 📦 Básico, extensível | 🚀 Rico em recursos |

## 🏗️ Estrutura do Projeto

```
Course/
├── Program.cs          # Exemplo principal de serialização
├── Models/            # Modelos de dados (caso expandido)
├── Converters/        # Conversores customizados
├── README.md
└── Course.csproj
```

## 💡 Exemplos Práticos

### 1. Modelo Base - DebtConfirmation

```csharp
public class DebtConfirmation 
{
    [JsonPropertyName("batchId")]
    public long BatchId { get; set; }

    [JsonPropertyName("debtId")]
    public long DebtId { get; set; }

    [JsonPropertyName("dateAdded")]
    public DateTimeOffset DateAdded { get; set; }

    [JsonPropertyName("debtCategoryId")]
    public long DebtCategoryId { get; set; }

    [JsonPropertyName("agreementNumber")]
    public long AgreementNumber { get; set; }

    [JsonPropertyName("clientNumber")]
    public long ClientNumber { get; set; }

    [JsonPropertyName("clientName")]
    public string ClientName { get; set; }
}
```

### 2. Desserialização Básica

```csharp
string jsonData = @"[
  {
    ""batchId"": 789,
    ""debtId"": 1841,
    ""dateAdded"": ""2021-07-27T16:01:39.41"",
    ""debtCategoryId"": 2,
    ""agreementNumber"": 78262155,
    ""clientNumber"": 1068055,
    ""clientName"": ""Client Two""
  }
]";

// Desserializar array JSON para coleção
var debts = JsonSerializer.Deserialize<ICollection<DebtConfirmation>>(jsonData);

// Iterar pelos resultados
foreach (var debt in debts)
{
    Console.WriteLine($"Client: {debt.ClientName}, Date: {debt.DateAdded}");
}
```

### 3. Serialização para JSON

```csharp
var debt = new DebtConfirmation
{
    BatchId = 999,
    DebtId = 2000,
    DateAdded = DateTimeOffset.Now,
    DebtCategoryId = 3,
    AgreementNumber = 12345678,
    ClientNumber = 9876543,
    ClientName = "New Client"
};

// Serializar objeto para JSON
string json = JsonSerializer.Serialize(debt);
Console.WriteLine(json);

// Serializar com formatação (pretty-print)
var options = new JsonSerializerOptions 
{ 
    WriteIndented = true 
};
string prettyJson = JsonSerializer.Serialize(debt, options);
Console.WriteLine(prettyJson);
```

### 4. Configurações Avançadas

```csharp
var options = new JsonSerializerOptions
{
    // Formatação
    WriteIndented = true,
    
    // Propriedades
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    
    // Campos incluídos
    IncludeFields = true,
    
    // Valores padrão
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    
    // Numbers
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    
    // Dates
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

var result = JsonSerializer.Serialize(debt, options);
```

### 5. Conversores Customizados

```csharp
// Conversor para formato de data personalizado
public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private readonly string _format = "yyyy-MM-dd HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateTime.ParseExact(value!, _format, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}

// Usar conversor
var options = new JsonSerializerOptions();
options.Converters.Add(new CustomDateTimeConverter());

var debt = JsonSerializer.Deserialize<DebtConfirmation>(json, options);
```

### 6. Trabalhar com JSON Dinâmico

```csharp
// JsonDocument para JSON estruturado
using JsonDocument document = JsonDocument.Parse(jsonData);
JsonElement root = document.RootElement;

foreach (JsonElement element in root.EnumerateArray())
{
    if (element.TryGetProperty("clientName", out JsonElement clientName))
    {
        Console.WriteLine($"Client: {clientName.GetString()}");
    }
}

// JsonNode para manipulação dinâmica
JsonNode? node = JsonNode.Parse(jsonData);
JsonArray? array = node?.AsArray();

if (array != null)
{
    foreach (JsonNode? item in array)
    {
        string? name = (string?)item?["clientName"];
        Console.WriteLine($"Client: {name}");
    }
}
```

## 🚀 Configuração e Execução

### 1. Pré-requisitos

- .NET 8 SDK
- Editor de código (Visual Studio, VS Code, etc.)

### 2. Executar o Projeto

```bash
# Navegar para o diretório
cd Course

# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run
```

### 3. Exemplos de Saída

```
2021-07-27T16:01:39.41+00:00
2021-08-25T14:47:18.13+00:00
```

## 🔧 Cenários Avançados

### 1. Herança e Polimorfismo

```csharp
[JsonDerivedType(typeof(PersonalDebt), typeDiscriminator: "personal")]
[JsonDerivedType(typeof(CorporateDebt), typeDiscriminator: "corporate")]
public abstract class BaseDebt
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}

public class PersonalDebt : BaseDebt
{
    [JsonPropertyName("ssn")]
    public string SSN { get; set; }
}

public class CorporateDebt : BaseDebt
{
    [JsonPropertyName("cnpj")]
    public string CNPJ { get; set; }
}

// JSON com discriminator
string json = @"{
  ""$type"": ""personal"",
  ""id"": 1,
  ""amount"": 1000.50,
  ""ssn"": ""123-45-6789""
}";

BaseDebt debt = JsonSerializer.Deserialize<BaseDebt>(json);
```

### 2. Validação durante Desserialização

```csharp
public class ValidatedDebtConfirmation : DebtConfirmation
{
    private string _clientName = string.Empty;
    
    [JsonPropertyName("clientName")]
    public new string ClientName 
    { 
        get => _clientName;
        set => _clientName = !string.IsNullOrEmpty(value) 
            ? value 
            : throw new JsonException("Client name cannot be empty");
    }
    
    private decimal _amount;
    
    [JsonPropertyName("amount")]
    public decimal Amount 
    { 
        get => _amount;
        set => _amount = value >= 0 
            ? value 
            : throw new JsonException("Amount cannot be negative");
    }
}
```

### 3. Performance com Utf8JsonReader

```csharp
public static List<DebtConfirmation> ParseDebtsHighPerformance(ReadOnlySpan<byte> jsonUtf8)
{
    var debts = new List<DebtConfirmation>();
    var reader = new Utf8JsonReader(jsonUtf8);
    
    // Leitura manual para máxima performance
    while (reader.Read())
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var debt = ReadDebtFromReader(ref reader);
            debts.Add(debt);
        }
    }
    
    return debts;
}

private static DebtConfirmation ReadDebtFromReader(ref Utf8JsonReader reader)
{
    var debt = new DebtConfirmation();
    
    while (reader.Read())
    {
        if (reader.TokenType == JsonTokenType.EndObject)
            break;
            
        if (reader.TokenType == JsonTokenType.PropertyName)
        {
            string propertyName = reader.GetString()!;
            reader.Read();
            
            switch (propertyName)
            {
                case "batchId":
                    debt.BatchId = reader.GetInt64();
                    break;
                case "clientName":
                    debt.ClientName = reader.GetString()!;
                    break;
                // ... outros campos
            }
        }
    }
    
    return debt;
}
```

## 💯 Melhores Práticas

### ✅ Boas Práticas

1. **Use System.Text.Json** para novos projetos (.NET Core 3.0+)
2. **Configure options uma vez** e reutilize
3. **Use async methods** para I/O de arquivos grandes
4. **Valide dados** após desserialização
5. **Cache JsonSerializerOptions** para performance
6. **Use source generators** para AOT scenarios

### 📋 Configuração Recomendada

```csharp
public static class JsonConfig
{
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false, // true apenas para debug
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
}

// Uso em toda aplicação
var result = JsonSerializer.Serialize(data, JsonConfig.DefaultOptions);
```

### ❌ Evitar

1. **Desserializar sem try-catch**: Sempre trate JsonException
2. **Criar options repetidamente**: Cache configurações
3. **Usar reflection desnecessariamente**: Prefira source generators
4. **Ignorar encoding**: UTF-8 é padrão e mais eficiente
5. **Misturar bibliotecas**: Use apenas uma (System.Text.Json ou Newtonsoft)

## 🔍 Comparação de Tipos de Data

| Tipo | Formato JSON | Uso Recomendado |
|------|-------------|-----------------|
| **DateTime** | "2024-01-15T10:30:00" | Datas locais |
| **DateTimeOffset** | "2024-01-15T10:30:00+00:00" | Datas com timezone |
| **DateOnly** | "2024-01-15" | Apenas data |
| **TimeOnly** | "10:30:00" | Apenas hora |

## 📋 Exercícios Práticos

1. **Converter Newtonsoft**: Migre código existente para System.Text.Json
2. **API Integration**: Consuma API externa com JSON
3. **Configuration**: Use JSON para arquivos de configuração
4. **Streaming**: Processe arquivos JSON grandes com streaming
5. **Validation**: Implemente validação customizada durante desserialização

## 🔗 Recursos Adicionais

- [System.Text.Json Overview](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- [JSON Serialization](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to)
- [Migration from Newtonsoft.Json](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to)
- [Performance Tips](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-performance)

---

💡 **Dica**: System.Text.Json é o futuro da serialização JSON em .NET. Oferece melhor performance e menor uso de memória que Newtonsoft.Json, sendo a escolha padrão para novos projetos!
