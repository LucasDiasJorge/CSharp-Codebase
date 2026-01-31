# 📋 Template Padrão para READMEs de Projetos

Este documento define o template padrão para READMEs dos projetos deste repositório.

---

## 📝 Estrutura Recomendada

```markdown
# 📦 Nome do Projeto

Breve descrição do projeto em uma ou duas linhas.

---

## 📚 Conceitos Abordados

Lista dos principais conceitos demonstrados:

- **Conceito 1**: Breve descrição
- **Conceito 2**: Breve descrição
- **Conceito 3**: Breve descrição

---

## 🎯 Objetivos de Aprendizado

- Objetivo 1
- Objetivo 2
- Objetivo 3

---

## 📂 Estrutura do Projeto

```
NomeDoProjeto/
├── Pasta1/
│   └── Arquivo1.cs
├── Pasta2/
│   └── Arquivo2.cs
├── Program.cs
└── README.md
```

---

## 🚀 Como Executar

### Pré-requisitos

- .NET 9.0 SDK
- (Outros pré-requisitos se necessário)

### Execução

```bash
cd NomeDoProjeto
dotnet run
```

---

## 💡 Exemplos de Código

### Exemplo Básico

```csharp
// Código demonstrativo
public void ExemploMetodo()
{
    // implementação
}
```

---

## 📋 Endpoints / Comandos (se aplicável)

| Endpoint/Comando | Descrição |
|------------------|-----------|
| `GET /api/exemplo` | Descrição |
| `POST /api/exemplo` | Descrição |

---

## ✅ Boas Práticas

- Prática 1
- Prática 2

---

## ⚠️ Pontos de Atenção

- Ponto 1
- Ponto 2

---

## 🔗 Referências

- [Link 1](url)
- [Link 2](url)
```

---

## 📐 Diretrizes de Formatação

### Emojis Padrão

| Seção | Emoji |
|-------|-------|
| Conceitos | 📚 |
| Objetivos | 🎯 |
| Estrutura | 📂 |
| Execução | 🚀 |
| Exemplos | 💡 |
| Endpoints | 📋 |
| Boas Práticas | ✅ |
| Pontos de Atenção | ⚠️ |
| Referências | 🔗 |
| Pré-requisitos | ⚙️ |
| Troubleshooting | 🔧 |
| Próximos Passos | 🔜 |

### Idioma

- **Português Brasileiro** como idioma principal
- Termos técnicos em inglês quando apropriado

### Código

- Sempre usar language hints nos code blocks (```csharp, ```bash, etc.)
- Preferir exemplos concisos e comentados
- Evitar `var` em exemplos didáticos (tipos explícitos)

### Tabelas

- Usar tabelas para endpoints, comandos e comparações
- Manter tabelas concisas (máximo 5-6 colunas)

### Separadores

- Usar `---` entre seções principais
- Facilita navegação visual
