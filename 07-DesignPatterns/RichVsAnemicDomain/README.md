# Domínio Rico vs Domínio Anêmico

## Visão geral

Este projeto demonstra as diferenças práticas entre **Domínio Anêmico (Anemic Domain)** e **Domínio Rico (Rich Domain)** usando C# e exemplos do mundo real.

## Conceitos abordados

- Organização de exemplos sobre design patterns, modelagem OO e código limpo.
- Navegação entre implementações relacionadas a Domínio Rico vs Domínio Anêmico.
- Comparação prática entre abordagens presentes nesta pasta.

## Objetivos de aprendizagem

- Identificar como os exemplos desta pasta cobrem design patterns, modelagem OO e código limpo.
- Escolher o subprojeto mais adequado para aprofundar o estudo.
- Reutilizar a navegação da pasta como índice prático de consulta.

## Estrutura do projeto

```text
RichVsAnemicDomain/
+-- AnemicDomain/
|   +-- Models/
|   +-- Services/
|   +-- AnemicDomain.csproj
|   \-- Program.cs
+-- RichDomain/
|   +-- Models/
|   +-- Services/
|   +-- Program.cs
|   \-- RichDomain.csproj
+-- RichVsAnemicDomain/
|   \-- RichDomain/
+-- COMPARISON.md
\-- QUICK_GUIDE.md
```

## Como executar

Escolha um dos projetos abaixo para execução direcionada:

- `dotnet run --project 07-DesignPatterns/RichVsAnemicDomain/AnemicDomain/AnemicDomain.csproj`
- `dotnet run --project 07-DesignPatterns/RichVsAnemicDomain/RichDomain/RichDomain.csproj`

## Boas práticas e pontos de atenção

- Use os READMEs dos subprojetos como ponto de entrada antes de alterar o código.
- Compare implementações relacionadas mantendo o mesmo conceito em foco.
- Prefira build ou execução direcionada por projeto em vez de validar a solução inteira.

## Conteúdo complementar

##### Domínio Anêmico (Anemic Domain)

**Definição**: Um antipadrão onde as classes de domínio contêm apenas dados (propriedades) sem comportamento (lógica de negócio). A lógica fica em serviços externos.

**Características**:
- ❌ Classes são apenas "sacolas de dados" (DTOs disfarçados)
- ❌ Getters e Setters públicos sem proteção
- ❌ Lógica de negócio espalhada em serviços
- ❌ Viola o princípio de encapsulamento
- ❌ Dificulta manutenção e testes

**Quando usar**:
- Aplicações CRUD muito simples
- Protótipos rápidos
- Casos onde o domínio é trivial

##### Domínio Rico (Rich Domain)

**Definição**: Um padrão onde as classes de domínio encapsulam tanto dados quanto comportamento, mantendo a lógica de negócio próxima aos dados que ela manipula.

**Características**:
- ✅ Classes contêm comportamento e regras de negócio
- ✅ Encapsulamento forte (setters privados)
- ✅ Invariantes garantidas
- ✅ Auto-validação
- ✅ Código mais expressivo e testável
- ✅ Segue princípios de DDD (Domain-Driven Design)

**Quando usar**:
- Aplicações com lógica de negócio complexa
- Projetos de longo prazo
- Quando regras de negócio são críticas
- Aplicações enterprise

##### Estrutura do Projeto

```
RichVsAnemicDomain/
├── README.md (este arquivo)
├── AnemicDomain/              # Exemplo de domínio anêmico
│   ├── Models/                # Modelos anêmicos (apenas dados)
│   ├── Services/              # Toda lógica fica aqui
│   └── Program.cs
├── RichDomain/                # Exemplo de domínio rico
│   ├── Models/                # Modelos ricos (dados + comportamento)
│   ├── Services/              # Serviços de aplicação (orquestração)
│   └── Program.cs
└── COMPARISON.md              # Comparação detalhada
```

##### Exemplo Prático: Sistema de Pedidos (E-commerce)

Ambos os projetos implementam o mesmo cenário: um sistema de pedidos com as seguintes regras:

1. **Pedido** pode ter múltiplos **Itens**
2. Cada item tem produto, quantidade e preço
3. Pedido calcula total automaticamente
4. Descontos podem ser aplicados
5. Pedido só pode ser cancelado se estiver "Pendente"
6. Validações: quantidade mínima, valores positivos, etc.

##### Domínio Anêmico

```bash
cd AnemicDomain
dotnet run
```

##### Domínio Rico

```bash
cd RichDomain
dotnet run
```

##### Comparação Rápida

| Aspecto | Domínio Anêmico | Domínio Rico |
|---------|----------------|--------------|
| **Encapsulamento** | ❌ Fraco | ✅ Forte |
| **Manutenibilidade** | ❌ Baixa | ✅ Alta |
| **Testabilidade** | ⚠️ Média | ✅ Alta |
| **Complexidade** | ✅ Simples início | ⚠️ Mais complexo |
| **Acoplamento** | ❌ Alto | ✅ Baixo |
| **Validação** | Espalhada | Centralizada |
| **Regras de Negócio** | Em serviços | No domínio |

##### Do Domínio Anêmico:

- Veja como a lógica fica espalhada nos serviços
- Note a falta de proteção contra estados inválidos
- Observe a dificuldade em encontrar regras de negócio

##### Do Domínio Rico:

- Veja como as regras ficam próximas aos dados
- Note o encapsulamento e proteção de invariantes
- Observe a facilidade de entender e testar o domínio

##### O que observar no código

1. **Construtores**: Como cada abordagem inicializa objetos
2. **Setters**: Públicos (anêmico) vs Privados (rico)
3. **Validação**: Onde e como acontece
4. **Cálculos**: Quem é responsável pelo cálculo do total
5. **Regras de Negócio**: Onde estão localizadas
6. **Testes**: Qual é mais fácil de testar

**Conclusão**: Domínio Rico é preferível na maioria dos casos, especialmente em aplicações com lógica de negócio significativa. Domínio Anêmico pode ser aceitável apenas em CRUDs muito simples.

## Referências

- **Martin Fowler**: [Anemic Domain Model](https://martinfowler.com/bliki/AnemicDomainModel.html)
- **Eric Evans**: Domain-Driven Design: Tackling Complexity in the Heart of Software
- **Vernon Vaughn**: Implementing Domain-Driven Design

## Documentação complementar

- [COMPARISON.md](./COMPARISON.md) - Comparação: Domínio Anêmico vs Domínio Rico
- [QUICK_GUIDE.md](./QUICK_GUIDE.md) - Guia Rápido: Domínio Anêmico vs Rico
