# SDKs Enterprise — Publicação e Gestão (Guia Básico)

Este README fornece orientações práticas e concisas para publicar, versionar e gerenciar SDKs em ambientes enterprise. O foco é .NET/NuGet, mas as práticas podem ser adaptadas para outras plataformas.

**Princípios**
- Qualidade: builds e testes automatizados antes da publicação.
- Compatibilidade: preservar contratos públicos sempre que possível.
- Segurança: proteger credenciais, assinar pacotes e controlar acesso.
- Automação: CI/CD para empacotar e publicar releases.
- Documentação: changelogs e guias de migração claros.

## Versionamento (SemVer)
Adote Semantic Versioning: MAJOR.MINOR.PATCH

- MAJOR: mudanças incompatíveis (breaking changes).
- MINOR: novas funcionalidades compatíveis.
- PATCH: correções de bugs.

Exemplos:
- `1.2.3` — release estável
- `1.3.0` — novas features compatíveis
- `2.0.0` — breaking changes
- Pré-release: `1.4.0-beta.1`

## Branching e tags
- `main` (ou `master`): código de produção/versões estáveis.
- `develop`: integração de features (opcional).
- `feature/*`, `release/*`, `hotfix/*` para fluxo Git Flow-like.
- Marque releases com tags semânticas: `vMAJOR.MINOR.PATCH`.

Exemplo de tag:

```bash
git tag -a v1.2.0 -m "Release v1.2.0"
git push origin v1.2.0
```

## Processo de release (checklist básico)
1. Atualizar a versão do projeto (ex.: `<Version>` no `.csproj` ou via GitVersion/NBGV).
2. Atualizar `CHANGELOG.md` e notas de release.
3. Garantir que todos os testes passam (`dotnet test`).
4. Gerar o pacote:

```bash
dotnet pack -c Release -o ./nupkg
```

5. Assinar o pacote se necessário (política enterprise).
6. Publicar no feed interno (Azure Artifacts, Artifactory, Nexus, GitHub Packages):

```bash
dotnet nuget push ./nupkg/*.nupkg --source "MyCompanyFeed" --api-key $NUGET_API_KEY
```

7. Criar tag e registrar release (GitHub Release, Artifactory release notes, etc.).
8. Atualizar exemplos e documentação de uso.
9. Comunicar time/consumers (release notes, canais internos).

## Publicação e repositórios
- Preferir feed interno para pacotes enterprise (Azure Artifacts, Artifactory, Nexus, GitHub Packages).
- Não embutir API keys no código — use secrets do CI/CD.
- Controle de acesso por times/roles e retenção de pacotes.

## CI/CD (exemplo rápido GitHub Actions)
Automatize publish ao receber uma tag `v*.*.*`:

```yaml
name: Publish NuGet
on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '7.0.x'
      - run: dotnet restore
      - run: dotnet test --no-build --verbosity normal
      - run: dotnet pack -c Release -o ./nupkg
      - run: dotnet nuget push ./nupkg/*.nupkg --source ${{ secrets.NUGET_SOURCE }} --api-key ${{ secrets.NUGET_API_KEY }}
```

## Compatibilidade e breaking changes
- Documente breaking changes e forneça guias de migração.
- Em C#, marque APIs obsoletas com `[Obsolete("mensagem, sugestão de migração")]` antes de remover.
- Quando necessário, aumente MAJOR e publique notas detalhadas.

## Hotfixes e rollback
- Hotfix: criar `hotfix/*`, corrigir e publicar patch (incrementar PATCH).
- Rollback público: preferível publicar um novo pacote com correção; evite remover versões publicadas sem coordenação.

## Changelog e notas de release
- Mantenha `CHANGELOG.md` seguindo uma convenção (ex.: "Keep a Changelog").
- Gere notas a partir de commits (Conventional Commits) quando possível.

## Checklist rápido
- Versão atualizada no projeto
- Tests verdes
- Pacote gerado
- Pacote assinado (se aplicável)
- Publicado no feed interno
- Release notes e comunicação

## Contato / Responsáveis
- Mantenedores: adicionar nomes/e-mails ou equipe responsável aqui.

---

Este é um guia básico. Posso adaptar para um fluxo específico (ex.: GitHub Packages + Azure DevOps) ou adicionar um exemplo completo de CI/CD se desejar.
