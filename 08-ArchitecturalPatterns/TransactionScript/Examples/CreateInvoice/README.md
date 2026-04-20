# Create Invoice Transaction Script

## Visão geral

Projeto didático do CSharp-101 dedicado a Create Invoice Transaction Script, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

- Exemplo didático sobre Create Invoice Transaction Script no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Create Invoice Transaction Script se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
CreateInvoice/
+-- DTOs/
|   +-- CreateInvoiceInput.cs
|   +-- CreateInvoiceOutput.cs
|   \-- InvoiceItemInput.cs
\-- Scripts/
    \-- CreateInvoiceScript.cs
```

## Como executar

Consulte o código desta pasta e os projetos relacionados antes de executar comandos específicos.

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Descrição

Script de transação para criação de notas fiscais com cálculo de impostos.

##### Fluxo

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

##### Cálculos

- **Subtotal Item**: Quantidade × Preço Unitário
- **Subtotal Geral**: Σ Subtotais dos Itens
- **Imposto**: Subtotal × Alíquota (padrão 18%)
- **Total**: Subtotal + Imposto
