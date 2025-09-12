namespace UnitOfWork;

class Program
{
    static void Main(string[] args)
    {
        var uow = new UnitOfWork();

        var customer = new Customer { Id = 1, Name = "Lucas" };
        var order = new Order { Id = 101, Description = "Pedido de livros" };

        uow.Customers.Add(customer);
        uow.Orders.Add(order);

        // Aqui, nenhuma "gravação" ainda ocorreu — está pendente

        uow.Commit(); // Tudo é "salvo" de uma vez!
    }

    public class UnitOfWork
    {
        public CustomerRepository Customers { get; }
        public OrderRepository Orders { get; }

        public UnitOfWork()
        {
            Customers = new CustomerRepository();
            Orders = new OrderRepository();
        }

        public void Commit()
        {
            Customers.Commit();
            Orders.Commit();
            Console.WriteLine("[UnitOfWork] Transação concluída.");
        }
    }
}
