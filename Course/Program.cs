using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Enter the student's score: ");
        int score = Convert.ToInt32(Console.ReadLine());

        switch (score)
        {
            case int s when s >= 90:
                Console.WriteLine("Grade A");
                break;
            case int s when s >= 80:
                Console.WriteLine("Grade B");
                break;
            case int s when s >= 70:
                Console.WriteLine("Grade C");
                break;
            default:
                Console.WriteLine("Grade F");
                break;
        }
    }
}
