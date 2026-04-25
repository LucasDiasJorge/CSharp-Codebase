# Composite

## Visão geral

Projeto didático do CSharp-101 dedicado a Composite, com foco em design patterns, modelagem OO e código limpo.

## Conceitos abordados

- Exemplo didático sobre Composite no contexto de design patterns, modelagem OO e código limpo.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Composite se aplica em um cenário prático de design patterns, modelagem OO e código limpo.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

### Objetivo

O **Composite** permite tratar objetos individuais (folhas) e composições de objetos (compostos) de forma uniforme. É útil para representar estruturas hierárquicas como árvores de arquivos, menus, ou elementos de UI.

> Benefício principal: Simplifica o código cliente ao permitir que componentes simples e compostos compartilhem a mesma interface.

## Estrutura do projeto

```text
Composite/
+-- Composite.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/DesignPattern/Structural/Composite/Composite.csproj
```

Saída esperada (simplificada):

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Quando Usar

Use este padrão quando:
- Você precisa representar uma hierarquia de objetos (árvores)
- Clientes devem tratar objetos individuais e composições de forma idêntica
- Operações em nós e subárvores são necessárias (ex: exibir, percorrer, calcular)

##### Fluxo Demonstrado

1. Criar `Folder` raiz e adicionar `File`/`Folder` filhos
2. Inserir subpastas e arquivos recursivamente
3. Chamar `Display` na raiz para imprimir a árvore inteira

##### Benefícios Evidenciados

- Interface única para folhas e compostos
- Facilidade para adicionar operações recursivas
- Estrutura intuitiva para representar hierarquias

##### Trade-offs

| Ponto | Observação |
|-------|------------|
| Interface inchada | Todos os implementadores precisam fornecer os métodos da interface (alguns podem ser irrelevantes) |
| Performance | Operações recursivas em grandes árvores podem ser custosas |

##### Possíveis Extensões

- Implementar remoção/ordenação de filhos
- Suporte a operações assíncronas (I/O)
- Serialização da árvore (JSON/Xml)
- Adicionar metadados (tamanhos, permissões)

##### TL;DR

Use Composite quando quiser tratar objetos individuais e composições uniformemente em uma estrutura hierárquica.

Autor: Lucas Jorge  
Tecnologia: .NET / C#  
Categoria: Structural Pattern
