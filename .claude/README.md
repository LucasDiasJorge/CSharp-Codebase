# .claude/

Configuração do Claude Code para o CSharp-Codebase.

```text
.claude/
|-- agents/          6 subagentes especializados
|-- skills/          7 skills invocáveis por /nome
|-- reference/       dados do catálogo, compartilhados por skills e agentes
`-- settings.json    permissões dos comandos rotineiros
```

Regras de conduta continuam em `CLAUDE.md` (raiz) e nos `CLAUDE.md` de cada projeto. Este
diretório traz **procedimentos** e **dados**, não substitui aquelas regras.

## Skills

Invocáveis com `/nome`, ou acionadas sozinhas quando o pedido se encaixa na descrição.

| Skill | Para quê |
|---|---|
| `/novo-sample` | Criar um sample novo na trilha certa, com `.sln`, READMEs e build validado |
| `/revisar-sample` | Auditar **um** projeto contra as convenções (`var`, `readonly`, TFM, docs, build) |
| `/consertar-build` | Diagnosticar e corrigir falha de build por código de erro |
| `/padronizar-readme` | Escrever ou normalizar o README local nas 9 seções do template |
| `/atualizar-claude-md` | Escrever ou atualizar o `CLAUDE.md` de um projeto |
| `/auditar-catalogo` | Varrer os 125 projetos e reportar divergências de catálogo |
| `/subir-servicos` | Subir e verificar Redis, Kafka, RabbitMQ, MySQL, PostgreSQL, MongoDB |

`/auditar-catalogo` traz um script determinístico:

```bash
powershell -ExecutionPolicy Bypass -File .\.claude\skills\auditar-catalogo\scripts\Audit-Catalog.ps1
```

## Agentes

Rodam isolados, com contexto próprio — úteis para trabalho pesado ou paralelo. Peça pelo nome
("use o `sample-reviewer` em ...") ou deixe que sejam escolhidos.

| Agente | Escreve? | Para quê |
|---|---|---|
| `catalog-navigator` | não | "Onde estudo X?", comparar samples, dar o comando certo |
| `sample-reviewer` | não | Revisão de conformidade de um projeto |
| `catalog-auditor` | não | Auditoria do catálogo com delta contra o baseline |
| `build-doctor` | sim | Consertar o build de um projeto e validar |
| `docs-curator` | sim | README local + `CLAUDE.md` + índice do README raiz |
| `sample-author` | sim | Criar um sample completo, do `dotnet new` ao build |

Os agentes seguem as mesmas skills — a diferença é onde o trabalho acontece. Use a skill para
trabalho no thread principal; use o agente quando quiser em segundo plano, em worktree isolado,
ou vários projetos em paralelo.

## reference/catalogo.md

Fatos do checkout que skills e agentes consultam em vez de redescobrir: taxonomia das 13 trilhas,
distribuição real de TFMs, os 8 projetos com layout `src/`, os 11 com build quebrado no baseline,
os 10 fora da `.sln`, a matriz projeto → serviço externo e os comandos de verificação.

**Isso envelhece.** Quando um número divergir do checkout, o checkout vence — os comandos da
seção final do arquivo reconferem tudo. Depois de uma limpeza grande (consertar os `NETSDK1013`,
registrar projetos na `.sln`), atualize também o baseline em
`skills/auditar-catalogo/SKILL.md`, senão a auditoria passa a apontar dívida já paga.

## Convenções internas

- Português-BR, o mesmo do repositório.
- Uma fonte de verdade por assunto: as skills **linkam** `docs/CONVENCOES.md`,
  `docs/README_TEMPLATE.md` e `.github/skills/create-csharp-project-by-task/SKILL.md` em vez de
  copiar o conteúdo. Ao mudar uma regra, mude no original.
- `Audit-Catalog.ps1` é UTF-8 **com BOM**: o Windows PowerShell 5.1 lê `.ps1` sem BOM como ANSI e
  quebra os regex com acento. Preserve o BOM ao editar.
