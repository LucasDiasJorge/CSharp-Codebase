# Plano de Refatoração Física de Pastas

Objetivo: reorganizar os diretórios físicos do repositório conforme a taxonomia aprovada, mantendo a solução compilável em cada etapa e evitando perdas de histórico git (via `git mv`).

## 1. Princípios
- Atomicidade: mover grupos pequenos por commit (ex: uma categoria por vez).
- Reprodutibilidade: script opcional de apoio listando todos os `git mv`.
- Preservar histórico: sempre usar `git mv` em vez de criar/copiar/apagar.
- Build verde após cada categoria movida.
- Nenhuma mudança de código funcional durante a fase de movimentação.

## 2. Taxonomia (Versão 1 - Padronizada em Inglês)
```
/Apis
/Authentication
/Data
/Caching
/Concurrency
/Messaging
/Patterns
/Security
/Serialization
/Utilities
```
(Possíveis futuras: `Observability`, `Testing`, `Benchmarks`).

## 3. Mapeamento Inicial (Exemplos)
| Projeto | Nova Categoria | Caminho Destino Proposto |
|---------|----------------|--------------------------|
| Auth | Authentication | Authentication/Auth |
| OAuthApplication | Authentication | Authentication/OAuthApplication |
| SecurityAndAuthentication | Security | Security/SecurityAndAuthentication |
| EncryptDecrypt | Security | Security/EncryptDecrypt |
| SafeVault | Security | Security/SafeVault |
| FluentValidationUserApi | Apis | Apis/FluentValidationUserApi |
| MongoUserApi | Data | Data/MongoUserApi |
| Postgres | Data | Data/Postgres |
| MysqlExample | Data | Data/MysqlExample |
| CacheAside | Caching | Caching/CacheAside |
| CacheIncrement | Caching | Caching/CacheIncrement |
| RedisConsoleApp | Caching | Caching/RedisConsoleApp |
| RedisMySQLIntegration | Caching | Caching/RedisMySQLIntegration |
| RabbitMQ | Messaging | Messaging/RabbitMQ |
| Kafka | Messaging | Messaging/Kafka |
| GrpcSample | Messaging | Messaging/GrpcSample |
| BackgroudWorker | Concurrency | Concurrency/BackgroudWorker |
| AsyncTasksDemo | Concurrency | Concurrency/AsyncTasksDemo |
| Threads | Concurrency | Concurrency/Threads |
| AtomicOperationsDemo | Concurrency | Concurrency/AtomicOperationsDemo |
| RealWorldBubbleSort | Patterns | Patterns/RealWorldBubbleSort |
| SOLIDExamples | Patterns | Patterns/SOLIDExamples |
| DesignPattern | Patterns | Patterns/DesignPattern |
| StrategyIntegration | Patterns | Patterns/StrategyIntegration |
| ClassToXml | Serialization | Serialization/ClassToXml |
| Serialization | Serialization | Serialization/Serialization |
| XmlBasics | Serialization | Serialization/XmlBasics |
| (Outros utilitários) | Utilities | Utilities/<Projeto> |
```
(Pendente: revisar todos os demais antes da execução.)
```

## 4. Estratégia de Execução
1. Criar branch: `refactor/layout-taxonomia`.
2. Para cada categoria:
   - Criar pasta alvo se não existir.
   - Executar `git mv` para cada projeto listado.
   - Atualizar referências no `.sln` (ajustar caminhos relativos) usando IDE ou edição manual.
   - `dotnet build` na solução.
   - Commit: `refactor(category): mover projetos de <Categoria>`.
3. Concluir todas as categorias e executar build final + testes (quando existirem).
4. Revisar diffs para garantir ausência de mudanças de código acidentais.
5. Merge via PR (permitindo revisão).

## 5. Ajustes no .sln
- Após cada movimento, remover entradas antigas quebradas (se a IDE não fizer automaticamente).
- Manter a mesma estrutura lógica de Solution Folders (podem ou não refletir 1:1 com a física, mas recomendável manter alinhado inicialmente para reduzir confusão).

## 6. Riscos e Mitigações
| Risco | Mitigação |
|-------|-----------|
| Quebra de paths relativos em scripts/arquivos | Buscar por `../` ou caminhos diretos antes de mover; ajustar após cada commit. |
| Conflitos de merge com desenvolvimentos paralelos | Comunicar janela de congelamento ou fazer rebase frequente enquanto PR aberto. |
| Perda de histórico git (autores, blame) | Uso estrito de `git mv`. |
| Falha de build por referência antiga | Build imediato após cada categoria. |
| Confusão de namespaces vs. paths | Não alterar namespaces nesta etapa (apenas físico). Opcionalmente adicionar TODO para eventual alinhamento. |

## 7. Itens Fora de Escopo Nesta Fase
- Renomear namespaces para refletir novas pastas.
- Introduzir soluções parciais / split solutions.
- Consolidar pacotes NuGet ou refatorar código interno.

## 8. Pós-Migração (Próximos Passos)
- Eventual ajuste de namespaces (busca e substituição controlada + build + testes).
- Atualização de READMEs raiz refletindo nova estrutura.
- Script de auditoria para garantir que todo projeto possui README e segue `Directory.Build.props`.

## 9. Checklist de Execução (Marcar Durante a Refatoração)
- [ ] Branch criada
- [ ] Categoria Autenticacao movida e build ok
- [ ] Categoria Seguranca movida e build ok
- [ ] Categoria BancoDeDados movida e build ok
- [ ] Categoria Cache movida e build ok
- [ ] Categoria InfraestruturaMensageria movida e build ok
- [ ] Categoria Concorrencia movida e build ok
- [ ] Categoria Padroes movida e build ok
- [ ] Categoria Serializacao movida e build ok
- [ ] Ajustes finais no .sln
- [ ] Build final sem warnings críticos

## 10. Comandos Exemplos (PowerShell)
```powershell
# Exemplo para uma categoria
mkdir Cache
git mv CacheAside Cache/CacheAside
git mv CacheIncrement Cache/CacheIncrement
# ... demais projetos Cache
# Atualizar .sln (abrir na IDE e salvar) depois

dotnet build .\CSharp-101.sln
```

## 11. Aprovação
Somente iniciar execução após validação deste documento.

---
Documento gerado automaticamente (primeira versão). Ajuste livremente antes da execução.
