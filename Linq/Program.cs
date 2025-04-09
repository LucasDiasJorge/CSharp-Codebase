using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        // Data Source
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // LINQ Query
        var evenNumbers = numbers.Where(num => num % 2 == 0);
        
        // Execution
        foreach (var num in evenNumbers)
        {
            Console.WriteLine(num);
        }
    }
}