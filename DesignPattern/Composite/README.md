```markdown
# Padrão Composite (Composto)

## 🎯 Objetivo
O **Composite** permite tratar objetos individuais (folhas) e composições de objetos (compostos) de forma uniforme. É útil para representar estruturas hierárquicas como árvores de arquivos, menus, ou elementos de UI.

> Benefício principal: Simplifica o código cliente ao permitir que componentes simples e compostos compartilhem a mesma interface.

## 🧠 Quando Usar
Use este padrão quando:
- Você precisa representar uma hierarquia de objetos (árvores)
- Clientes devem tratar objetos individuais e composições de forma idêntica
- Operações em nós e subárvores são necessárias (ex: exibir, percorrer, calcular)

## 🏗️ Estrutura do Exemplo
Este exemplo modela um sistema de arquivos mínimo com `Folder` (composto) e `File` (folha). Ambos implementam a mesma interface `IComponent` que define a operação `Display`.

| Componente | Função |
|------------|--------|
| `IComponent` | Contrato comum (método `Display`) |
| `File` | Folha: representa um arquivo |
| `Folder` | Composto: contém filhos (arquivos e/ou pastas) |
| `Program.cs` | Demonstração da montagem da árvore e exibição |

## 🔄 Fluxo Demonstrado
1. Criar `Folder` raiz e adicionar `File`/`Folder` filhos
2. Inserir subpastas e arquivos recursivamente
3. Chamar `Display` na raiz para imprimir a árvore inteira

## ▶️ Execução
```bash
cd Composite
dotnet run
```

Saída esperada (simplificada):
```
-Root
---File A
---File B
---Folder 1
-----File C
-----File D
-----Folder 2
-------File E
```

## ✅ Benefícios Evidenciados
- Interface única para folhas e compostos
- Facilidade para adicionar operações recursivas
- Estrutura intuitiva para representar hierarquias

## ⚠️ Trade-offs
| Ponto | Observação |
|-------|------------|
| Interface inchada | Todos os implementadores precisam fornecer os métodos da interface (alguns podem ser irrelevantes) |
| Performance | Operações recursivas em grandes árvores podem ser custosas |

## 🔧 Possíveis Extensões
- Implementar remoção/ordenação de filhos
- Suporte a operações assíncronas (I/O)
- Serialização da árvore (JSON/Xml)
- Adicionar metadados (tamanhos, permissões)

## 📌 TL;DR
Use Composite quando quiser tratar objetos individuais e composições uniformemente em uma estrutura hierárquica.

---
Autor: Lucas Jorge  
Tecnologia: .NET / C#  
Categoria: Structural Pattern
```
