# Create Invoice Transaction Script

## Descrição

Script de transação para criação de notas fiscais com cálculo de impostos.

## Fluxo

```
1. Validar cliente
2. Validar itens (não vazio)
3. Gerar número da nota
4. Calcular subtotal de cada item
5. Calcular subtotal geral
6. Calcular impostos (alíquota configurável)
7. Calcular total
8. Criar e salvar invoice
9. Retornar resultado
```

## Cálculos

- **Subtotal Item**: Quantidade × Preço Unitário
- **Subtotal Geral**: Σ Subtotais dos Itens
- **Imposto**: Subtotal × Alíquota (padrão 18%)
- **Total**: Subtotal + Imposto
