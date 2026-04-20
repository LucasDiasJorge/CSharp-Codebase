# Custom Filter API - ASP.NET Core

## Visão geral

> 📚 **Projeto educacional** demonstrando o uso de **Action Filters customizados** no ASP.NET Core para capturar e logar propriedades específicas de modelos baseado em atributos personalizados.

## Conceitos abordados

- Exemplo didático sobre Custom Filter API - ASP.NET Core no contexto de ASP.NET Core, contratos HTTP e pipeline web.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Custom Filter API - ASP.NET Core se aplica em um cenário prático de ASP.NET Core, contratos HTTP e pipeline web.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
CustomFilterApi/
+-- Attributes/
|   +-- DisableLogPropertyAttribute.cs
|   \-- LogPropertyAttribute.cs
+-- Controllers/
|   +-- ProductsController.cs
|   \-- UsersController.cs
+-- Filters/
|   \-- LogPropertyFilter.cs
+-- Models/
|   +-- ProductDto.cs
|   \-- UserDto.cs
+-- Properties/
|   \-- launchSettings.json
+-- Services/
|   +-- BusinessServiceA.cs
|   +-- BusinessServiceB.cs
|   +-- IBusinessService.cs
|   \-- SelectedServiceAccessor.cs
+-- appsettings.Development.json
+-- appsettings.json
\-- ...
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/CustomFilterApi/CustomFilterApi.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Conceitos Demonstrados](#conceitos-demonstrados)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Como Funciona](#como-funciona)
- [Como Executar](#como-executar)
- [Exemplos de Uso](#exemplos-de-uso)
- [Saída de Log Esperada](#saída-de-log-esperada)

##### Sobre o Projeto

Este projeto demonstra uma implementação didática e profissional de **Action Filters** no ASP.NET Core 9, mostrando como:

- ✅ Criar atributos personalizados customizados
- ✅ Implementar filtros de ação que interceptam requisições HTTP
- ✅ Usar reflexão (Reflection) para inspecionar objetos em tempo de execução
- ✅ Capturar propriedades específicas baseado em atributos
- ✅ Logar valores de forma seletiva e segura (com mascaramento de dados sensíveis)
- ✅ Organizar código seguindo as melhores práticas .NET

##### 1. **Action Filters**

Filtros são componentes que executam código antes ou depois de etapas específicas no pipeline de processamento de requisições. Este projeto implementa `IActionFilter` com dois métodos:
- `OnActionExecuting`: Executa **ANTES** do método do controller
- `OnActionExecuted`: Executa **DEPOIS** do método do controller

##### 2. **Atributos Customizados**

Criamos `[LogProperty]`, um atributo personalizado que pode ser aplicado a propriedades para marcá-las como "loggable". Suporta:
- Nome customizado para o log
- Mascaramento de valores sensíveis

##### 3. **Reflection (Reflexão)**

O filtro usa reflexão para inspecionar objetos em tempo de execução, buscando propriedades decoradas com `[LogProperty]` e extraindo seus valores dinamicamente.

##### Estrutura do Projeto

```
CustomFilterApi/
├── Attributes/
│   └── LogPropertyAttribute.cs      # Atributo customizado [LogProperty]
├── Filters/
│   └── LogPropertyFilter.cs         # Action Filter que captura e loga propriedades
├── Models/
│   ├── UserDto.cs                   # Modelo de exemplo: usuário
│   └── ProductDto.cs                # Modelo de exemplo: produto
├── Controllers/
│   ├── UsersController.cs           # Controller com filtro aplicado globalmente
│   └── ProductsController.cs        # Controller com filtro aplicado por action
├── Program.cs                       # Configuração da aplicação
└── README.md                        # Este arquivo
```

##### Organização por Responsabilidade

- **Attributes**: Contém atributos customizados reutilizáveis
- **Filters**: Implementações de filtros do ASP.NET Core
- **Models**: DTOs e modelos de dados
- **Controllers**: Endpoints da API

##### Exemplo de Modelo Anotado

```csharp
public class UserDto
{
    [LogProperty]
    public string Username { get; set; }
    
    [LogProperty(logName: "E-mail do usuário")]
    public string Email { get; set; }
    
    [LogProperty(MaskValue = true)]
    public string Password { get; set; }
    
    // Esta propriedade NÃO será logada
    public string PhoneNumber { get; set; }
}
```

##### Aplicação do Filtro

O filtro pode ser aplicado em **3 níveis**:

##### 1. **Global** (afeta toda a aplicação)

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogPropertyFilter>();
});
```

##### 2. **Controller** (afeta todas as actions do controller)

```csharp
[ApiController]
[ServiceFilter(typeof(LogPropertyFilter))]
public class UsersController : ControllerBase
{
    // Todas as actions serão interceptadas
}
```

##### 3. **Action** (afeta apenas uma action específica)

```csharp
[HttpPost]
[ServiceFilter(typeof(LogPropertyFilter))]
public IActionResult CreateProduct([FromBody] ProductDto product)
{
    // Apenas esta action será interceptada
}
```

##### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

##### Passos

1. **Clone ou navegue até o diretório do projeto**

```bash
cd CustomFilterApi
```

2. **Restaure as dependências**

```bash
dotnet restore
```

3. **Execute o projeto**

```bash
dotnet run
```

4. **Acesse a API**

A aplicação estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

##### 1. Criar um Usuário (POST)

**Endpoint**: `POST /api/users`

**Body**:
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "senha123",
  "phoneNumber": "11999999999",
  "age": 30
}
```

**cURL**:
```bash
curl -X POST https://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "username": "johndoe",
    "email": "john@example.com",
    "password": "senha123",
    "phoneNumber": "11999999999",
    "age": 30
  }'
```

##### 2. Criar um Produto (POST)

**Endpoint**: `POST /api/products`

**Body**:
```json
{
  "name": "Notebook Dell",
  "price": 3499.99,
  "category": "Eletrônicos",
  "description": "Notebook Dell Inspiron 15",
  "stock": 10
}
```

**cURL**:
```bash
curl -X POST https://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Notebook Dell",
    "price": 3499.99,
    "category": "Eletrônicos",
    "description": "Notebook Dell Inspiron 15",
    "stock": 10
  }'
```

##### 3. Atualizar um Usuário (PUT)

**Endpoint**: `PUT /api/users/1`

**Body**:
```json
{
  "username": "johndoe_updated",
  "email": "john.updated@example.com",
  "password": "novasenha456",
  "phoneNumber": "11988888888",
  "age": 31
}
```

##### Saída de Log Esperada

Ao fazer uma requisição POST para criar um usuário, você verá logs similares a:

```
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      === Iniciando interceptação da requisição ===
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Controller: UsersController
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Action: CustomFilterApi.Controllers.UsersController.CreateUser (CustomFilterApi)
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Analisando argumento: user do tipo UserDto
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      --- Propriedades marcadas para log encontradas ---
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      [PROPRIEDADE LOGADA] Username: johndoe
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      [PROPRIEDADE LOGADA] E-mail do usuário: john@example.com
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      [PROPRIEDADE LOGADA] Password: se***23
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      [PROPRIEDADE LOGADA] Age: 30
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      === Fim da interceptação ===
info: CustomFilterApi.Controllers.UsersController[0]
      Processando criação do usuário no controller
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Action executada. Status: 200
```

##### Observações sobre o Log:

- ✅ `Username` foi logado normalmente
- ✅ `Email` foi logado com o nome customizado "E-mail do usuário"
- ✅ `Password` foi logado com máscara: `se***23`
- ✅ `Age` foi logado normalmente
- ❌ `PhoneNumber` **NÃO** foi logado (não tem o atributo `[LogProperty]`)

##### Mascaramento de Dados Sensíveis

O filtro implementa uma função de mascaramento para proteger dados sensíveis:

```csharp
"senha123" → "se***23"
"12345678" → "12***78"
```

##### Reflection em Ação

O código usa `GetProperties()` e `GetCustomAttribute()` para inspecionar tipos em runtime:

```csharp
var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
foreach (var property in properties)
{
    var logAttribute = property.GetCustomAttribute<LogPropertyAttribute>();
    if (logAttribute != null)
    {
        var value = property.GetValue(obj);
        // Processa o valor...
    }
}
```

##### Tecnologias Utilizadas

- **.NET 9** - Framework principal
- **ASP.NET Core** - Framework web
- **C# 12** - Linguagem de programação
- **Swagger/OpenAPI** - Documentação da API
- **Reflection** - Inspeção de tipos em runtime

##### Aprendizados

Este projeto demonstra:

1. **Pipeline de Filtros**: Como interceptar e modificar o fluxo de requisições
2. **Atributos Customizados**: Criar metadados reutilizáveis
3. **Reflection**: Inspecionar e manipular tipos em runtime
4. **Injeção de Dependência**: Usar services no filtro
5. **Logging**: Práticas de logging estruturado
6. **Segurança**: Mascaramento de dados sensíveis

##### Melhores Práticas Demonstradas

- ✅ Separação de responsabilidades (SoC)
- ✅ Princípio DRY (Don't Repeat Yourself)
- ✅ Código limpo e bem documentado
- ✅ Uso apropriado de namespaces
- ✅ Configuração centralizada no Program.cs
- ✅ Logging estruturado

##### Licença

Este projeto é parte do repositório educacional CSharp-101 e está disponível para fins de aprendizado.

Desenvolvido com 💙 como material educacional para a comunidade .NET

## Referências

- [LOG_EXAMPLES.md](./LOG_EXAMPLES.md) - Exemplo de Saída de Log
- [SERVICE_SELECTION_TODO.md](./SERVICE_SELECTION_TODO.md) - Plano de Ação — Seleção de Service via Filter (CustomFilterApi)
- [todo.md](./todo.md) - Plano de ação para melhorias no CustomFilterApi

## Documentação complementar

- [LOG_EXAMPLES.md](./LOG_EXAMPLES.md) - Exemplo de Saída de Log
- [SERVICE_SELECTION_TODO.md](./SERVICE_SELECTION_TODO.md) - Plano de Ação — Seleção de Service via Filter (CustomFilterApi)
- [todo.md](./todo.md) - Plano de ação para melhorias no CustomFilterApi
