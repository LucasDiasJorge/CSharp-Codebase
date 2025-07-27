using System;
using SOLIDExamples.SRP;
using SOLIDExamples.OCP;
using SOLIDExamples.LSP;
using SOLIDExamples.ISP;
using SOLIDExamples.DIP;

namespace SOLIDExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- SRP (Single Responsibility Principle) ---");
            SrpExample.Run();
            Console.WriteLine();

            Console.WriteLine("--- OCP (Open/Closed Principle) ---");
            OcpExample.Run();
            Console.WriteLine();

            Console.WriteLine("--- LSP (Liskov Substitution Principle) ---");
            LspExample.Run();
            Console.WriteLine();

            Console.WriteLine("--- ISP (Interface Segregation Principle) ---");
            IspExample.Run();
            Console.WriteLine();

            Console.WriteLine("--- DIP (Dependency Inversion Principle) ---");
            DipExample.Run();
            Console.WriteLine();
        }
    }
}
