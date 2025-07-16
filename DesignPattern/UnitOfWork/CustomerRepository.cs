using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork
{
    public class CustomerRepository
    {
        private readonly List<Customer> _pending = new();

        public void Add(Customer customer)
        {
            _pending.Add(customer);
            Console.WriteLine($"[CustomerRepository] Registrado: {customer.Name}");
        }

        public void Commit()
        {
            foreach (var c in _pending)
                Console.WriteLine($"[CustomerRepository] SALVO: {c.Name}");

            _pending.Clear();
        }
    }

    public class OrderRepository
    {
        private readonly List<Order> _pending = new();

        public void Add(Order order)
        {
            _pending.Add(order);
            Console.WriteLine($"[OrderRepository] Registrado: {order.Description}");
        }

        public void Commit()
        {
            foreach (var o in _pending)
                Console.WriteLine($"[OrderRepository] SALVO: {o.Description}");

            _pending.Clear();
        }
    }

}
