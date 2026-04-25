using System;

namespace SOLIDExamples.OCP
{
    public static class OcpExample
    {
        public static void Run()
        {
            Console.WriteLine("Exemplo INCORRETO:");
            var badCalc = new BadDiscountCalculator();
            Console.WriteLine($"Desconto Regular: {badCalc.CalculateDiscount("Regular")}");
            Console.WriteLine($"Desconto Premium: {badCalc.CalculateDiscount("Premium")}");

            Console.WriteLine("\nExemplo CORRETO:");
            var calc = new DiscountCalculator();
            Console.WriteLine($"Desconto Regular: {calc.CalculateDiscount(new RegularCustomer())}");
            Console.WriteLine($"Desconto Premium: {calc.CalculateDiscount(new PremiumCustomer())}");
        }
    }

    // Violando o OCP
    public class BadDiscountCalculator
    {
        public double CalculateDiscount(string customerType)
        {
            if (customerType == "Regular") return 0.1;
            if (customerType == "Premium") return 0.2;
            return 0;
        }
    }

    // Correto: aberto para extensão, fechado para modificação
    public interface ICustomer
    {
        double GetDiscount();
    }
    public class RegularCustomer : ICustomer
    {
        public double GetDiscount() => 0.1;
    }
    public class PremiumCustomer : ICustomer
    {
        public double GetDiscount() => 0.2;
    }
    public class DiscountCalculator
    {
        public double CalculateDiscount(ICustomer customer) => customer.GetDiscount();
    }
}
