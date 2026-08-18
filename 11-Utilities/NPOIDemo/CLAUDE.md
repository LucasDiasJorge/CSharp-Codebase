# CLAUDE.md — NPOIDemo

Console: geração de arquivos Excel (.xlsx) e Word (.docx) com NPOI. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/NPOIDemo/NPOIDemo.csproj
dotnet run --project 11-Utilities/NPOIDemo/NPOIDemo.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): cria planilha com células, tipos e formatação, e um documento Word com parágrafos. **Sem Microsoft Office instalado e sem Interop** — a biblioteca escreve o formato OpenXML diretamente, que é a razão de ela ser usada em servidor.

## Pontos de atenção

- **Escreve arquivos no diretório de saída** ao executar; não comite os artefatos gerados.
- TFM **`net10.0`**. `NPOI` 2.7.5.
- Na API do NPOI, `XSSF*` é para `.xlsx` e `HSSF*` para o `.xls` legado; misturar os dois é o erro mais comum ao editar.
