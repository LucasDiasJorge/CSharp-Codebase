using System;

namespace LogicalOperatorsDemo;

public static class Program
{
    // Demonstrates logical operators: AND (&&), OR (||), NOT (!), XOR (^)
    public static bool And(bool left, bool right)
    {
        bool result = left && right;
        return result;
    }

    public static bool Or(bool left, bool right)
    {
        bool result = left || right;
        return result;
    }

    public static bool Not(bool operand)
    {
        bool result = !operand;
        return result;
    }

    public static bool Xor(bool left, bool right)
    {
        // In C# ^ on bool is logical exclusive OR
        bool result = left ^ right;
        return result;
    }

    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }

    public static void PrintTruthTableTwoInputs(Func<bool, bool, bool> op, string opSymbol)
    {
        Console.WriteLine("A\tB\tResult");

        bool a0 = false;
        bool a1 = true;

        bool result00 = op(a0, a0);
        Console.WriteLine($"{a0}\t{a0}\t{result00}");

        bool result01 = op(a0, a1);
        Console.WriteLine($"{a0}\t{a1}\t{result01}");

        bool result10 = op(a1, a0);
        Console.WriteLine($"{a1}\t{a0}\t{result10}");

        bool result11 = op(a1, a1);
        Console.WriteLine($"{a1}\t{a1}\t{result11}");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Logical Operators Demo (explicit types, no 'var')");

        // AND
        PrintHeader("AND (&&)");
        PrintTruthTableTwoInputs(And, "&&");

        // OR
        PrintHeader("OR (||)");
        PrintTruthTableTwoInputs(Or, "||");

        // XOR
        PrintHeader("XOR (^ - exclusive)");
        PrintTruthTableTwoInputs(Xor, "^");

        // NOT
        PrintHeader("NOT (!)");
        bool inputFalse = false;
        bool inputTrue = true;
        bool notFalse = Not(inputFalse);
        bool notTrue = Not(inputTrue);
        Console.WriteLine($"!{inputFalse} => {notFalse}");
        Console.WriteLine($"!{inputTrue} => {notTrue}");

        // Additional demonstration: combination and short-circuiting
        PrintHeader("Short-circuiting and combination examples");

        bool left = true;
        bool right = false;

        // AND short-circuit: right side not evaluated if left is false
        bool combinedAnd = left && right && Not(false) ;
        Console.WriteLine($"(true && false && !false) => {combinedAnd}");

        // OR short-circuit: right side not evaluated if left is true
        bool combinedOr = left || right || Not(true);
        Console.WriteLine($"(true || false || !true) => {combinedOr}");

        // XOR with booleans
        bool xorResult = Xor(true, false);
        Console.WriteLine($"(true ^ false) => {xorResult}");

        // Bitwise operators on integers (demonstrating bit-level AND/OR/XOR/NOT)
        PrintHeader("Bitwise operators on integers (for comparison)");
        int valueA = 6; // 0110
        int valueB = 3; // 0011
        int andBits = valueA & valueB; // 0010 => 2
        int orBits = valueA | valueB;  // 0111 => 7
        int xorBits = valueA ^ valueB; // 0101 => 5
        int notA = ~valueA;            // bitwise complement

        Console.WriteLine($"{valueA} & {valueB} = {andBits}");
        Console.WriteLine($"{valueA} | {valueB} = {orBits}");
        Console.WriteLine($"{valueA} ^ {valueB} = {xorBits}");
        Console.WriteLine($"~{valueA} = {notA}");

        Console.WriteLine();
        Console.WriteLine("Demo finished. Press Enter to exit.");
        Console.ReadLine();
    }
}
