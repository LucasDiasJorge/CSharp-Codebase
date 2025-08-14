# Padrão Mediator (Mediador)

## 🎯 Objetivo
O **Mediator** (Mediador) tem como propósito **centralizar a comunicação** entre múltiplos objetos (componentes) para **reduzir o acoplamento direto** entre eles. Em vez de cada objeto conhecer e chamar os outros diretamente, eles interagem por meio de um mediador.

> Benefício principal: Facilita evolução e manutenção ao evitar uma malha de dependências (N x N) entre objetos.

## 🧠 Quando Usar
Use este padrão quando:
- Muitos objetos se comunicam de forma complexa entre si
- Há regras de interação que mudam com frequência
- Você deseja isolar comportamentos de orquestração em um único ponto
- Quer evitar código "espaguete" com múltiplas referências cruzadas

## 🏗️ Estrutura do Exemplo
Este exemplo simula uma **sala de chat** onde vários usuários enviam mensagens para os demais através de um mediador.

| Componente | Função |
|------------|--------|
| `IMediator` | Contrato do mediador |
| `IUser` | Contrato dos participantes |
| `ChatRoomMediator` | Mediador concreto que gerencia os usuários e repassa mensagens |
| `User` | Participante concreto (envia e recebe mensagens) |
| `Program.cs` | Demonstra o fluxo completo |

## 🔄 Fluxo Demonstrado
1. Criação do mediador (`ChatRoomMediator`)
2. Criação e registro de usuários (`AddUser`)
3. Envio de mensagens (o mediador repassa para os demais)
4. Remoção de um usuário (`RemoveUser`)
5. Tentativa de envio sem estar associado a um mediador

## ▶️ Execução
```bash
dotnet run
```

Saída (resumida):
```
=== Demonstração do Padrão Mediator ===
Simulando uma sala de chat...
[Sistema] Alice entrou na sala de chat
[Sistema] Bob entrou na sala de chat
[Sistema] Charlie entrou na sala de chat
[Chat] Alice: Olá pessoal! Como estão?
  > Bob recebeu: 'Olá pessoal! Como estão?' de Alice
  > Charlie recebeu: 'Olá pessoal! Como estão?' de Alice
...
[Sistema] Bob saiu da sala de chat
[Erro] Diana não está conectado a nenhuma sala de chat
```

## 🧪 Exemplo de Código (trecho de `Program.cs`)
```csharp
ChatRoomMediator chatRoom = new ChatRoomMediator();

User alice = new User("Alice");
User bob = new User("Bob");
User charlie = new User("Charlie");

chatRoom.AddUser(alice);
chatRoom.AddUser(bob);
chatRoom.AddUser(charlie);

alice.SendMessage("Olá pessoal! Como estão?");
bob.SendMessage("Oi Alice! Tudo bem por aqui!");
chatRoom.RemoveUser(bob);
alice.SendMessage("Bob saiu da conversa?");

User diana = new User("Diana");
diana.SendMessage("Tentando enviar sem estar na sala");
```

## ✅ Benefícios Evidenciados
- Redução de acoplamento entre usuários
- Facilidade para adicionar novas regras (ex: moderação, logs, filtros)
- Clareza da orquestração centralizada

## ⚠️ Trade-offs
| Ponto | Observação |
|-------|------------|
| Mediador "obeso" | Pode concentrar lógica demais se não modularizado |
| Testes | Mediador precisa ser bem coberto para evitar regressões |
| Escalabilidade | Regras complexas podem exigir divisão em múltiplos mediadores |

## 🔧 Possíveis Extensões
- Registrar histórico de mensagens
- Restringir envio (ex: usuários silenciados)
- Suporte a salas múltiplas
- Envio privado (direct message)

## 📌 TL;DR
Use o Mediator para **organizar interações** e **diminuir acoplamento** quando muitos objetos precisam conversar entre si.