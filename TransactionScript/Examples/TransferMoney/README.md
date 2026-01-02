# Transfer Money Transaction Script

## Descrição

Script de transação para transferência de dinheiro entre contas bancárias.

## Fluxo

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

## Características

- **Procedural**: Lógica linear passo-a-passo
- **Transacional**: Todas operações em sequência
- **Validação**: Múltiplas validações inline
- **Simples**: Fácil de entender e debugar
