# EncryptDecrypt Console App

## Visão geral

Projeto didático do CSharp-101 dedicado a EncryptDecrypt Console App, com foco em autenticação, autorização e segurança.

## Conceitos abordados

- Exemplo didático sobre EncryptDecrypt Console App no contexto de autenticação, autorização e segurança.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

Aplicativo de console em C# para encriptar e decriptar dados digitados pelo usuário usando o algoritmo AES (Advanced Encryption Standard) com senha definida pelo usuário.

## Estrutura do projeto

```text
EncryptDecrypt/
+-- .vscode/
|   \-- tasks.json
+-- EncryptDecrypt.csproj
+-- EncryptDecrypt.sln
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 04-Authentication/Security/EncryptDecrypt/EncryptDecrypt.csproj
```

## Boas práticas e pontos de atenção

- Use senhas fortes e únicas.
- Nunca compartilhe a senha.
- O IV é gerado aleatoriamente e armazenado junto ao texto encriptado (padrão seguro).
- Não reutilize o mesmo IV para diferentes mensagens.

## Conteúdo complementar

##### Criptografia Simétrica

- **Chave única**: A mesma senha é usada para encriptar e decriptar.
- **AES**: Algoritmo moderno, seguro e eficiente para dados em repouso e em trânsito.
- **IV (Vetor de Inicialização)**: Garante que a mesma mensagem encriptada com a mesma senha produza resultados diferentes.

##### Fluxo do Programa

1. Usuário escolhe: encriptar ou decriptar.
2. Usuário digita a mensagem e a senha.
3. O resultado (texto encriptado ou decriptado) é exibido no console.

##### Exemplo de Uso

```
Escolha uma opção:
1 - Encriptar
2 - Decriptar
Opção: 1
Digite o texto: Olá, mundo!
Digite a senha: minhaSenhaForte
Texto encriptado: 3v1J8... (base64)

Opção: 2
Digite o texto: 3v1J8... (base64)
Digite a senha: minhaSenhaForte
Texto decriptado: Olá, mundo!
```

##### Exercícios Sugeridos

- Adapte para ler/escrever arquivos encriptados.
- Implemente suporte a outros algoritmos (ex: ChaCha20).
- Adicione testes automatizados para garantir a integridade.

## Referências

- [Documentação AES .NET](https://learn.microsoft.com/dotnet/api/system.security.cryptography.aes)
- [Criptografia Simétrica - Microsoft Docs](https://learn.microsoft.com/dotnet/standard/security/symmetric-encryption)

💡 **Dica:** Sempre valide a senha e trate exceções de decriptação para evitar vazamento de informações!
