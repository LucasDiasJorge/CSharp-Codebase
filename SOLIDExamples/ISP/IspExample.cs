using System;

namespace SOLIDExamples.ISP
{
    public static class IspExample
    {
        public static void Run()
        {
            Console.WriteLine("Exemplo INCORRETO:");
            IMachine oldPrinter = new OldPrinter();
            oldPrinter.Print();
            try { oldPrinter.Scan(); } catch (Exception ex) { Console.WriteLine($"Erro: {ex.Message}"); }

            Console.WriteLine("\nExemplo CORRETO:");
            IPrinter printer = new SimplePrinter();
            printer.Print();
        }
    }

    // Violando o ISP
    public interface IMachine
    {
        void Print();
        void Scan();
        void Fax();
    }
    public class OldPrinter : IMachine
    {
        public void Print() => Console.WriteLine("Imprimindo...");
        public void Scan() => throw new NotImplementedException("Não suporta scan!");
        public void Fax() => throw new NotImplementedException("Não suporta fax!");
    }

    // Correto: interfaces segregadas
    public interface IPrinter { void Print(); }
    public interface IScanner { void Scan(); }
    public class SimplePrinter : IPrinter
    {
        public void Print() => Console.WriteLine("Imprimindo...");
    }
}
