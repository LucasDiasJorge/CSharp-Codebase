namespace MediatoR;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Demonstração do Padrão Mediator ===");
        Console.WriteLine("Simulando uma sala de chat onde usuários se comunicam através de um mediador\n");

        // Criar o mediador (sala de chat)
        ChatRoomMediator chatRoom = new ChatRoomMediator();

        // Criar usuários
        User alice = new User("Alice");
        User bob = new User("Bob");
        User charlie = new User("Charlie");

        Console.WriteLine("1. Adicionando usuários à sala de chat:");
        chatRoom.AddUser(alice);
        chatRoom.AddUser(bob);
        chatRoom.AddUser(charlie);

        Console.WriteLine("\n2. Usuários enviando mensagens:");
        alice.SendMessage("Olá pessoal! Como estão?");
        
        Console.WriteLine();
        bob.SendMessage("Oi Alice! Tudo bem por aqui!");
        
        Console.WriteLine();
        charlie.SendMessage("Opa! Chegando agora na conversa!");

        Console.WriteLine("\n3. Removendo um usuário da sala:");
        chatRoom.RemoveUser(bob);

        Console.WriteLine("\n4. Mensagem após remoção do Bob:");
        alice.SendMessage("Bob saiu da conversa?");

        Console.WriteLine("\n5. Tentativa de envio sem mediador:");
        User diana = new User("Diana");
        diana.SendMessage("Tentando enviar sem estar na sala");

        Console.WriteLine("\n=== Fim da Demonstração ===");
        Console.WriteLine("\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }
}
