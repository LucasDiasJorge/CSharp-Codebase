# Acme.TextKit

Utilitários de texto para slug, truncamento e contagem de palavras.

```csharp
using Acme.TextKit;

string slug = TextKit.Slugify("Olá, Mundo Cruel!");   // "ola-mundo-cruel"
string cut  = TextKit.Truncate("texto bem longo", 8); // "texto..."
int words   = TextKit.CountWords("um dois  tres");    // 3
```

Pacote didático do repositório CSharp-Codebase — publicado apenas em feed local.
