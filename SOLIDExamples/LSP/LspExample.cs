using System;

namespace SOLIDExamples.LSP
{
    public static class LspExample
    {
        public static void Run()
        {
            Console.WriteLine("Exemplo INCORRETO:");
            Bird badBird = new OstrichBad();
            try { badBird.Fly(); } catch (Exception ex) { Console.WriteLine($"Erro: {ex.Message}"); }

            Console.WriteLine("\nExemplo CORRETO:");
            IFlyingBird sparrow = new Sparrow();
            sparrow.Fly();
            Bird ostrich = new Ostrich();
            ostrich.Move();
        }
    }

    // Violando o LSP
    public class Bird
    {
        public virtual void Fly() => Console.WriteLine("Voando...");
    }
    public class OstrichBad : Bird
    {
        public override void Fly() => throw new NotImplementedException("Avestruz não voa!");
    }

    // Correto: subclasses substituíveis
    public abstract class BirdBase
    {
        public abstract void Move();
    }
    public interface IFlyingBird
    {
        void Fly();
    }
    public class Sparrow : BirdBase, IFlyingBird
    {
        public override void Move() => Fly();
        public void Fly() => Console.WriteLine("Pardal voando!");
    }
    public class Ostrich : BirdBase
    {
        public override void Move() => Console.WriteLine("Avestruz correndo!");
    }
}
