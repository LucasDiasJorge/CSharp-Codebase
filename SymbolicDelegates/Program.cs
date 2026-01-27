using System;
using System.Collections.Generic;

class MainClass
{
    public static void Main(string[] args)
    {
        // The "programming language"
        Dictionary<string, Action> keywords = new Dictionary<string, Action>
        {
            // The "hello" keyword
            ["hello"] = () => {
                Console.WriteLine("hello was invoked");
            },
            // The "world" keyword
            ["world"] = () => {
                Console.WriteLine("world was invoked");
            }
        };
        
        // The "code"
        string code = "hello world";
        
        // "Tokenising" the code
        string[] tokens = code.Split(' ');
        
        // Evaluating the tokens
        foreach (string token in tokens)
        {
            if (keywords.ContainsKey(token))
            {
                keywords[token]();
            }
        }
    }
}
