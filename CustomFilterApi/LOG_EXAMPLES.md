# 📊 Exemplo de Saída de Log

Este arquivo demonstra a saída de log esperada ao fazer requisições para a API.

## Requisição: POST /api/users

### Payload enviado:
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "senha123",
  "phoneNumber": "11999999999",
  "age": 30
}
```

### Log gerado pelo filtro:

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

### Análise do Log:

✅ **Username**: Logado normalmente com o valor "johndoe"

✅ **Email**: Logado com o nome customizado "E-mail do usuário" definido no atributo

✅ **Password**: Logado com mascaramento por segurança - "senha123" virou "se***23"

✅ **Age**: Logado normalmente com o valor numérico 30

❌ **PhoneNumber**: NÃO foi logado (não possui o atributo [LogProperty])

---

## Requisição: POST /api/products

### Payload enviado:
```json
{
  "name": "Notebook Dell",
  "price": 3499.99,
  "category": "Eletrônicos",
  "description": "Notebook Dell Inspiron 15",
  "stock": 10
}
```

### Log gerado pelo filtro:

```
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      === Iniciando interceptação da requisição ===

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Controller: ProductsController

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Action: CustomFilterApi.Controllers.ProductsController.CreateProduct (CustomFilterApi)

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Analisando argumento: product do tipo ProductDto

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      --- Propriedades marcadas para log encontradas ---

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      [PROPRIEDADE LOGADA] Name: Notebook Dell

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      [PROPRIEDADE LOGADA] Price: 3499.99

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      [PROPRIEDADE LOGADA] Categoria do Produto: Eletrônicos

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      === Fim da interceptação ===

info: CustomFilterApi.Controllers.ProductsController[0]
      Criando produto: Notebook Dell

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Action executada. Status: 201
```

### Análise do Log:

✅ **Name**: Logado normalmente

✅ **Price**: Logado com o valor decimal

✅ **Category**: Logado com nome customizado "Categoria do Produto"

❌ **Description**: NÃO foi logado (não possui o atributo [LogProperty])

❌ **Stock**: NÃO foi logado (não possui o atributo [LogProperty])

---

## Requisição: GET /api/users/1

### Log gerado:

```
info: CustomFilterApi.Filters.LogPropertyFilter[0]
      === Iniciando interceptação da requisição ===

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Controller: UsersController

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Action: CustomFilterApi.Controllers.UsersController.GetUser (CustomFilterApi)

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Analisando argumento: id do tipo Int32

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Nenhuma propriedade marcada com [LogProperty] encontrada

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      === Fim da interceptação ===

info: CustomFilterApi.Filters.LogPropertyFilter[0]
      Action executada. Status: 200
```

### Análise do Log:

Como a requisição GET não possui body (apenas o parâmetro `id` na rota), o filtro não encontra propriedades marcadas para logar, mas ainda executa e registra sua passagem.

---

## Observações Importantes:

1. **Mascaramento de Dados Sensíveis**: Propriedades marcadas com `MaskValue = true` têm seus valores parcialmente ocultados no log

2. **Nomes Customizados**: O atributo permite definir um nome customizado para o log através do parâmetro `logName`

3. **Seletividade**: Apenas propriedades explicitamente marcadas com `[LogProperty]` são logadas

4. **Execução Transparente**: O filtro executa antes e depois da action sem interferir no fluxo normal da aplicação

5. **Status HTTP**: O filtro registra o status HTTP da resposta após a execução da action
