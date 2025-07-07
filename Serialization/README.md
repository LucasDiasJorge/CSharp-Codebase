# 🔄 Serialization - Técnicas de Serialização e Exception Handling

## 🎯 Objetivos de Aprendizado

- Dominar **diferentes tipos de serialização** em .NET
- Comparar **Binary**, **JSON**, **XML** e **MessagePack**
- Implementar **exception handling** adequado
- Entender **propagação de exceções** e **re-throw patterns**
- Aplicar **melhores práticas** de tratamento de erros
- Avaliar **performance** entre formatos de serialização

## 📚 Conceitos Fundamentais

### Tipos de Serialização

| Formato | Uso | Vantagens | Desvantagens |
|---------|-----|-----------|--------------|
| **Binary** | Sistemas internos | 🚀 Rápido, compacto | ❌ Não portável, .NET específico |
| **JSON** | APIs, web | 🌐 Universal, legível | 📄 Maior tamanho |
| **XML** | Sistemas legados | 📋 Estruturado, validável | 🐌 Verboso, lento |
| **MessagePack** | Performance crítica | ⚡ Muito rápido, compacto | 🔧 Menos suporte |

### Exception Handling Patterns

- **Catch and Handle**: Trata a exceção localmente
- **Catch and Rethrow**: Adiciona contexto e repropaga
- **Catch and Wrap**: Encapsula em exceção mais específica
- **Let it Bubble**: Deixa exceção subir para camadas superiores

## 🏗️ Estrutura do Projeto

```
Serialization/
├── Program.cs              # Exemplo principal
├── Models/                 # Modelos para serialização
├── Serializers/           # Implementações de serialização
├── ExceptionHandling/     # Padrões de tratamento de exceções
├── Performance/           # Testes de performance
└── README.md
```

## 💡 Exemplos Práticos

### 1. Modelos para Serialização

```csharp
[Serializable]
[MessagePackObject]
public class Person
{
    [Key(0)]
    public int Id { get; set; }
    
    [Key(1)]
    public string Name { get; set; } = string.Empty;
    
    [Key(2)]
    public DateTime BirthDate { get; set; }
    
    [Key(3)]
    public List<string> Hobbies { get; set; } = new();
    
    [IgnoreDataMember] // Para Binary
    [JsonIgnore]       // Para JSON
    [XmlIgnore]        // Para XML
    [IgnoreMember]     // Para MessagePack
    public string Password { get; set; } = string.Empty;
}
```

### 2. Serialização JSON (System.Text.Json)

```csharp
public class JsonSerializationService
{
    private readonly JsonSerializerOptions _options;

    public JsonSerializationService()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public string Serialize<T>(T obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj, _options);
        }
        catch (JsonException ex)
        {
            throw new SerializationException($"Failed to serialize {typeof(T).Name}", ex);
        }
    }

    public T Deserialize<T>(string json)
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(json, _options);
            return result ?? throw new SerializationException($"Deserialization returned null for {typeof(T).Name}");
        }
        catch (JsonException ex)
        {
            throw new SerializationException($"Failed to deserialize to {typeof(T).Name}", ex);
        }
    }

    public async Task<T> DeserializeFromFileAsync<T>(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var result = await JsonSerializer.DeserializeAsync<T>(stream, _options);
            return result ?? throw new SerializationException($"File {filePath} contained null data");
        }
        catch (FileNotFoundException ex)
        {
            throw new SerializationException($"Serialization file not found: {filePath}", ex);
        }
        catch (JsonException ex)
        {
            throw new SerializationException($"Invalid JSON in file: {filePath}", ex);
        }
    }
}
```

### 3. Serialização XML

```csharp
public class XmlSerializationService
{
    public string Serialize<T>(T obj)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings 
            { 
                Indent = true,
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8
            });
            
            serializer.Serialize(xmlWriter, obj);
            return stringWriter.ToString();
        }
        catch (InvalidOperationException ex)
        {
            throw new SerializationException($"XML serialization failed for {typeof(T).Name}", ex);
        }
    }

    public T Deserialize<T>(string xml)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(xml);
            
            var result = (T?)serializer.Deserialize(stringReader);
            return result ?? throw new SerializationException($"XML deserialization returned null");
        }
        catch (InvalidOperationException ex)
        {
            throw new SerializationException($"XML deserialization failed for {typeof(T).Name}", ex);
        }
    }
}
```

### 4. Serialização MessagePack

```csharp
public class MessagePackSerializationService
{
    private readonly MessagePackSerializerOptions _options;

    public MessagePackSerializationService()
    {
        _options = ContractlessStandardResolver.Options;
    }

    public byte[] Serialize<T>(T obj)
    {
        try
        {
            return MessagePackSerializer.Serialize(obj, _options);
        }
        catch (MessagePackSerializationException ex)
        {
            throw new SerializationException($"MessagePack serialization failed for {typeof(T).Name}", ex);
        }
    }

    public T Deserialize<T>(byte[] data)
    {
        try
        {
            return MessagePackSerializer.Deserialize<T>(data, _options);
        }
        catch (MessagePackSerializationException ex)
        {
            throw new SerializationException($"MessagePack deserialization failed for {typeof(T).Name}", ex);
        }
    }

    public async Task<byte[]> SerializeAsync<T>(T obj)
    {
        try
        {
            using var stream = new MemoryStream();
            await MessagePackSerializer.SerializeAsync(stream, obj, _options);
            return stream.ToArray();
        }
        catch (MessagePackSerializationException ex)
        {
            throw new SerializationException($"MessagePack async serialization failed for {typeof(T).Name}", ex);
        }
    }
}
```

### 5. Exception Handling Patterns

```csharp
public class ExceptionHandlingExamples
{
    // ❌ RUIM - Engole exceção
    public bool BadExceptionHandling()
    {
        try
        {
            return RiskyOperation();
        }
        catch (Exception)
        {
            return false; // Informação de erro perdida
        }
    }

    // ❌ RUIM - Perde stack trace
    public bool BadRethrow()
    {
        try
        {
            return RiskyOperation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw ex; // RUIM - 'throw ex' perde stack trace
        }
    }

    // ✅ BOM - Preserva stack trace
    public bool GoodRethrow()
    {
        try
        {
            return RiskyOperation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GoodRethrow: {ex.Message}");
            throw; // BOM - 'throw' preserva stack trace
        }
    }

    // ✅ BOM - Adiciona contexto
    public bool BestPracticeWrapper()
    {
        try
        {
            return RiskyOperation();
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException("Business logic error in RiskyOperation", ex);
        }
        catch (Exception ex)
        {
            // Log exceção inesperada
            Console.WriteLine($"Unexpected error: {ex}");
            throw; // Repropaga exceção inesperada
        }
    }

    // ✅ MELHOR - Try pattern
    public bool TryRiskyOperation(out string errorMessage)
    {
        errorMessage = string.Empty;
        
        try
        {
            return RiskyOperation();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private bool RiskyOperation()
    {
        // Simula operação que pode falhar
        if (Random.Shared.Next(2) == 0)
            throw new ArgumentException("Simulated error");
        return true;
    }
}
```

### 6. Exception Handling com Logging

```csharp
public class LoggingExceptionHandler
{
    private readonly ILogger<LoggingExceptionHandler> _logger;

    public LoggingExceptionHandler(ILogger<LoggingExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation, int maxRetries = 3)
    {
        var attempt = 0;
        
        while (true)
        {
            try
            {
                attempt++;
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries && IsRetriableException(ex))
            {
                _logger.LogWarning(ex, "Operation failed on attempt {Attempt}/{MaxRetries}. Retrying...", 
                    attempt, maxRetries);
                
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Operation failed after {Attempts} attempts", attempt);
                throw;
            }
        }
    }

    private static bool IsRetriableException(Exception ex)
    {
        return ex is HttpRequestException or TaskCanceledException or TimeoutException;
    }
}
```

## 🚀 Configuração e Execução

### 1. Dependências necessárias

```xml
<PackageReference Include="MessagePack" Version="2.5.108" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />
```

### 2. Executar o Projeto

```bash
# Navegar para o diretório
cd Serialization

# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run
```

### 3. Teste de Performance

```csharp
public class SerializationBenchmark
{
    private readonly Person _testData;
    private readonly JsonSerializationService _jsonService;
    private readonly XmlSerializationService _xmlService;
    private readonly MessagePackSerializationService _msgPackService;

    public SerializationBenchmark()
    {
        _testData = new Person
        {
            Id = 1,
            Name = "John Doe",
            BirthDate = DateTime.Now.AddYears(-25),
            Hobbies = new List<string> { "Reading", "Gaming", "Coding" }
        };

        _jsonService = new JsonSerializationService();
        _xmlService = new XmlSerializationService();
        _msgPackService = new MessagePackSerializationService();
    }

    public void RunBenchmarks()
    {
        // JSON
        var jsonStopwatch = Stopwatch.StartNew();
        var jsonSerialized = _jsonService.Serialize(_testData);
        var jsonDeserialized = _jsonService.Deserialize<Person>(jsonSerialized);
        jsonStopwatch.Stop();

        // XML
        var xmlStopwatch = Stopwatch.StartNew();
        var xmlSerialized = _xmlService.Serialize(_testData);
        var xmlDeserialized = _xmlService.Deserialize<Person>(xmlSerialized);
        xmlStopwatch.Stop();

        // MessagePack
        var msgPackStopwatch = Stopwatch.StartNew();
        var msgPackSerialized = _msgPackService.Serialize(_testData);
        var msgPackDeserialized = _msgPackService.Deserialize<Person>(msgPackSerialized);
        msgPackStopwatch.Stop();

        // Results
        Console.WriteLine($"JSON: {jsonStopwatch.ElapsedMilliseconds}ms, Size: {jsonSerialized.Length} chars");
        Console.WriteLine($"XML: {xmlStopwatch.ElapsedMilliseconds}ms, Size: {xmlSerialized.Length} chars");
        Console.WriteLine($"MessagePack: {msgPackStopwatch.ElapsedMilliseconds}ms, Size: {msgPackSerialized.Length} bytes");
    }
}
```

## 💯 Melhores Práticas

### ✅ Exception Handling

1. **Use 'throw' não 'throw ex'** para preservar stack trace
2. **Catch exceções específicas** primeiro, genéricas por último
3. **Log antes de rethrow** para debugging
4. **Use try patterns** quando apropriado
5. **Implemente retry** para operações transientes

### ✅ Serialização

1. **Escolha formato apropriado** para o caso de uso
2. **Configure opções uma vez** e reutilize
3. **Valide dados** após desserialização
4. **Use async** para I/O operations
5. **Trate encoding** adequadamente (UTF-8 padrão)

### ❌ Evitar

1. **Catch Exception** genérico sem motivo específico
2. **Engolir exceções** sem logging
3. **Usar BinaryFormatter** (obsoleto e inseguro)
4. **Misturar formatos** sem necessidade
5. **Serializar dados sensíveis** sem criptografia

## 🔍 Comparação de Performance

| Métrica | JSON | XML | MessagePack | Binary* |
|---------|------|-----|------------|---------|
| **Velocidade** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Tamanho** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Legibilidade** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐ | ⭐ |
| **Portabilidade** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐ |
| **Suporte** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |

*Binary = BinaryFormatter (obsoleto)

## 📋 Exercícios Práticos

1. **Benchmark Comparison**: Compare performance de todos os formatos
2. **Custom Converters**: Implemente conversores customizados
3. **Streaming**: Serialização de dados grandes com streaming
4. **Encryption**: Combine serialização com criptografia
5. **Compression**: Adicione compressão aos dados serializados

## 🔗 Recursos Adicionais

- [System.Text.Json Documentation](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp)
- [Exception Handling Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Serialization Security](https://docs.microsoft.com/en-us/dotnet/standard/serialization/security)

---

💡 **Dica**: A escolha do formato de serialização depende dos requisitos: JSON para APIs web, MessagePack para performance crítica, XML para sistemas legados que requerem validação de schema!
