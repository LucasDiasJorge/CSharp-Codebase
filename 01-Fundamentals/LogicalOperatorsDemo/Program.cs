using System;
using System.Collections.Generic;
using System.Linq;

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
        // Examples of evaluating expressions from strings
        var vars = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["A"] = true,
            ["B"] = false,
            ["C"] = false
        };

        string[] examples = new[]
        {
            "true && false",
            "A || (B && !C)",
            "!(A && B) || C",
            "A ^ B",
            "(A || B) && !C"
        };

        Console.WriteLine();
        Console.WriteLine("String expression evaluation examples:");
        foreach (var ex in examples)
        {
            try
            {
                bool value = EvaluateExpression(ex, vars);
                Console.WriteLine($"{ex} => {value}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{ex} => Error: {e.Message}");
            }
        }

        Console.ReadLine();
    }

    // Evaluates a boolean expression written as a string.
    // Supports: identifiers (variables), literals true/false, parentheses, operators: ! (NOT), && (AND), || (OR), ^ (XOR).
    // variables can be provided via the 'vars' dictionary (case-insensitive); unknown variables throw an exception.
    public static bool EvaluateExpression(string expr, IDictionary<string, bool>? vars = null)
    {
        if (expr == null) throw new ArgumentNullException(nameof(expr));

        var tokens = Tokenize(expr);
        var rpn = ToRpn(tokens);
        return EvalRpn(rpn, vars ?? new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> Tokenize(string s)
    {
        int i = 0;
        while (i < s.Length)
        {
            if (char.IsWhiteSpace(s[i]))
            {
                i++; continue;
            }

            // operators with two chars
            if (i + 1 < s.Length)
            {
                var two = s.Substring(i, 2);
                if (two == "&&" || two == "||")
                {
                    yield return two;
                    i += 2; continue;
                }
            }

            char c = s[i];
            if (c == '!' || c == '^' || c == '(' || c == ')')
            {
                yield return c.ToString();
                i++; continue;
            }

            if (char.IsLetterOrDigit(c) || c == '_' )
            {
                int start = i;
                while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i++;
                yield return s.Substring(start, i - start);
                continue;
            }

            throw new ArgumentException($"Unexpected character '{c}' in expression");
        }
    }

    private static List<string> ToRpn(IEnumerable<string> tokens)
    {
        var output = new List<string>();
        var ops = new Stack<string>();

        var prec = new Dictionary<string, int>
        {
            ["!"] = 4,
            ["&&"] = 3,
            ["^"] = 2,
            ["||"] = 1
        };

        foreach (var token in tokens)
        {
            var tl = token.ToLowerInvariant();
            if (tl == "true" || tl == "false" || IsIdentifier(token))
            {
                output.Add(token);
            }
            else if (token == "(")
            {
                ops.Push(token);
            }
            else if (token == ")")
            {
                while (ops.Count > 0 && ops.Peek() != "(") output.Add(ops.Pop());
                if (ops.Count == 0 || ops.Pop() != "(") throw new ArgumentException("Mismatched parentheses");
            }
            else // operator
            {
                while (ops.Count > 0 && ops.Peek() != "(" &&
                       ((IsRightAssociative(token) == false && prec[ops.Peek()] >= prec[token]) ||
                        (IsRightAssociative(token) && prec[ops.Peek()] > prec[token])))
                {
                    output.Add(ops.Pop());
                }
                ops.Push(token);
            }
        }

        while (ops.Count > 0)
        {
            var op = ops.Pop();
            if (op == "(" || op == ")") throw new ArgumentException("Mismatched parentheses");
            output.Add(op);
        }

        return output;
    }

    private static bool IsIdentifier(string token)
    {
        // not a literal 'true'/'false' and not an operator/paren -> treat as variable identifier
        var low = token.ToLowerInvariant();
        if (low == "true" || low == "false") return false;
        if (token == "&&" || token == "||" || token == "!" || token == "^" || token == "(" || token == ")") return false;
        return true;
    }

    private static bool IsRightAssociative(string op) => op == "!";

    private static bool EvalRpn(List<string> rpn, IDictionary<string, bool> vars)
    {
        var st = new Stack<bool>();
        foreach (var token in rpn)
        {
            var low = token.ToLowerInvariant();
            if (low == "true") { st.Push(true); continue; }
            if (low == "false") { st.Push(false); continue; }

            if (IsIdentifier(token))
            {
                if (!vars.TryGetValue(token, out var val))
                {
                    // try case-insensitive lookup
                    var key = vars.Keys.FirstOrDefault(k => string.Equals(k, token, StringComparison.OrdinalIgnoreCase));
                    if (key != null) val = vars[key];
                    else throw new KeyNotFoundException($"Unknown variable '{token}'");
                }
                st.Push(val);
                continue;
            }

            // operator
            if (token == "!")
            {
                if (st.Count < 1) throw new ArgumentException("Not enough operands for '!'");
                var a = st.Pop();
                st.Push(!a);
                continue;
            }

            if (st.Count < 2) throw new ArgumentException($"Not enough operands for '{token}'");
            var right = st.Pop();
            var left = st.Pop();

            switch (token)
            {
                case "&&": st.Push(left && right); break;
                case "||": st.Push(left || right); break;
                case "^": st.Push(left ^ right); break;
                default: throw new ArgumentException($"Unknown operator '{token}'");
            }
        }

        if (st.Count != 1) throw new ArgumentException("Invalid expression");
        return st.Pop();
    }
}
