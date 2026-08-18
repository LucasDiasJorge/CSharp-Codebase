# CLAUDE.md — Course

Console de serialização/desserialização JSON com `System.Text.Json`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/Course/Course.csproj
dotnet run --project 01-Fundamentals/Course/Course.csproj
```

## Estrutura interna

Projeto de arquivo único: todo o conteúdo está em `Program.cs`, executado como uma sequência de blocos demonstrativos (serializar, desserializar, opções de nomenclatura, tratamento de nulos). Não há camadas nem injeção de dependência — a leitura linear do arquivo é a intenção.

## Pontos de atenção

- TFM `net9.0`. Sem pacotes externos: usa `System.Text.Json` do runtime, não Newtonsoft.
- O nome `Course` é histórico e não descreve o conteúdo (o assunto real é JSON). Não renomeie sem atualizar o `.sln`, o README local e o índice do README raiz.
