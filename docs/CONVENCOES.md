# Convenções de Código e Estrutura

Estas convenções garantem consistência, legibilidade e propósito didático.

## 1. Linguagem e Versões
- TargetFramework único: `net9.0` (centralizado em `Directory.Build.props`).
- `Nullable` habilitado para incentivar código seguro.
- `ImplicitUsings` habilitado exceto quando o exemplo exige mostrar namespaces explicitamente.

## 2. Estilo de Código
- Não usar `var` (exceto em LINQ anônimos inevitáveis). Declare sempre o tipo explícito: `int total = ...;`.
- Nome de classes: PascalCase. Métodos: PascalCase. Parâmetros e variáveis locais: camelCase.
- Uma classe por arquivo salvo quando o exemplo exige contraste rápido.
- Usar `readonly` para dependências injetadas via construtor.

## 3. Estrutura de Projetos
Cada projeto deve conter (quando aplicável):
```
/Projeto
  Program.cs
  Controllers/ (ou Endpoints)
  Services/
  Repositories/
  Models/
  Configuration/ (extensions, DI, options)
  README.md
```

## 4. Injeção de Dependência
- Toda dependência deve ser registrada em um método de extensão separado quando a configuração for não trivial.
- Evitar lógica dentro de `Program.cs`; delegar para métodos de extensão.

## 5. Tratamento de Erros
- Preferir middleware global de tratamento de exceções para APIs.
- Em exemplos de console, capturar exceções no ponto mais alto para demonstrar fallback claro.

## 6. Logging
- Sempre logar eventos importantes (start, stop, erros, integrações externas).
- Evitar log de dados sensíveis (tokens, segredos, PII).
- Exemplo preferencial: `ILogger<T>`.

## 7. Organização de Códigos de Exemplo
- Cada exemplo deve ser autoexplicativo com comentários apenas onde exista intenção pedagógica.
- Comentários devem responder "por que" mais do que "o que".

## 8. Testes (quando houver)
- Nome de métodos de teste: `Metodo_Cenario_ResultadoEsperado`.
- Agrupar testes por contexto de negócio ou componente.

## 9. Nomenclatura de Pastas Didáticas
Categorias principais (solution folders):
- AutenticacaoESeguranca
- Cache
- APIs
- BancoDeDados
- Concorrencia
- PadroesDeProjeto
- Algoritmos
- SDKs

## 10. Boas Práticas Ilustradas
- SRP: cada classe com uma responsabilidade clara.
- DRY minimizado sem esconder conceitos pedagógicos.
- Uso explícito de tipos para reforçar leitura.

## 11. Pacotes e Dependências
- Preferir versões estáveis (evitar pre-release salvo propósito didático).
- Centralizar atualizações e validar build full antes de commitar.

## 12. Segurança Básica
- Nunca hardcode de segredos (usar `appsettings.Development.json` ou variáveis de ambiente).
- Se JWT: validar emissor, audiência e tempo de expiração.

## 13. Serialização e JSON
- System.Text.Json padrão.
- Configurações customizadas devem estar em um método de extensão nomeado claramente (`AddJsonOptionsConfigured`).

## 14. Performance / Concorrência
- Usar `async`/`await` corretamente (sem `.Result` ou `.Wait()` exceto em exemplos de antipadrões claramente marcados).
- Evitar lock desnecessário; quando usar, explicar motivo no comentário.

## 15. Checklist por Projeto
- [ ] README preenchido
- [ ] Sem `var` indevido
- [ ] Build sem warnings críticos
- [ ] Comentários pedagógicos presentes
- [ ] Fluxo principal descrito

---
Essas convenções podem evoluir. Sugira melhorias via Pull Request.
