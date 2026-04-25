using System;

namespace SOLIDExamples.DIP
{
    public static class DipExample
    {
        public static void Run()
        {
            Console.WriteLine("Exemplo INCORRETO:");
            var badService = new BadOrderService();
            badService.ProcessOrder();

            Console.WriteLine("\nExemplo CORRETO:");
            ILogger logger = new ConsoleLogger();
            var service = new OrderService(logger);
            service.ProcessOrder();
        }
    }

    // Violando o DIP
    public class BadOrderService
    {
        private BadLogger logger = new BadLogger();
        public void ProcessOrder()
        {
            logger.Log("Pedido processado!");
        }
    }
    public class BadLogger
    {
        public void Log(string msg) => Console.WriteLine($"[BAD] {msg}");
    }

    // Correto: depende de abstração
    public interface ILogger
    {
        void Log(string msg);
    }
    public class ConsoleLogger : ILogger
    {
        public void Log(string msg) => Console.WriteLine($"[OK] {msg}");
    }
    public class OrderService
    {
        private readonly ILogger logger;
        public OrderService(ILogger logger)
        {
            this.logger = logger;
        }
        public void ProcessOrder()
        {
            logger.Log("Pedido processado!");
        }
    }
}
