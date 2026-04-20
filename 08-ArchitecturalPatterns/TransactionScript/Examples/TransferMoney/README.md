# Transfer Money Transaction Script

## Visão geral

Projeto didático do CSharp-101 dedicado a Transfer Money Transaction Script, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

- Exemplo didático sobre Transfer Money Transaction Script no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Transfer Money Transaction Script se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
TransferMoney/
+-- DTOs/
|   +-- TransferMoneyInput.cs
|   \-- TransferMoneyOutput.cs
\-- Scripts/
    \-- TransferMoneyScript.cs
```

## Como executar

Consulte o código desta pasta e os projetos relacionados antes de executar comandos específicos.

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Descrição

Script de transação para transferência de dinheiro entre contas bancárias.

##### Fluxo

```
1. Validar entrada (valores positivos, contas diferentes)
2. Buscar conta origem
3. Buscar conta destino
4. Validar contas (existem, ativas)
5. Validar saldo suficiente
6. Debitar conta origem
7. Creditar conta destino
8. Registrar transação
9. Retornar resultado
```

##### Características

- **Procedural**: Lógica linear passo-a-passo
- **Transacional**: Todas operações em sequência
- **Validação**: Múltiplas validações inline
- **Simples**: Fácil de entender e debugar
