# Process Refund Transaction Script

## Descrição

Script de transação para processar reembolso de produtos comprados.

## Fluxo

```
1. Validar dados de entrada
2. Buscar conta do cliente
3. Buscar produto
4. Calcular valor do reembolso
5. Creditar conta do cliente
6. Restaurar estoque do produto
7. Registrar transação de reembolso
8. Retornar resultado
```

## Regras de Negócio

- Valor de reembolso = Quantidade × Preço do Produto
- Estoque é restaurado automaticamente
- Transação é registrada como "Reembolso"
- Conta deve existir e estar ativa
