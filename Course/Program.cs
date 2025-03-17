namespace Course;

// https://www.coursera.org/learn/foundations-of-coding-back-end/assignment-submission/eURIS/activity-algorithm-structures/attempt

class Program
{
    static void Main(string[] args)
    {

        Console.WriteLine("Enter your age: ");
        int age = Convert.ToInt32(Console.ReadLine());

        if (age >= 18)
        {
            Console.WriteLine("You are able to vote");
        }
        else
        {
            Console.WriteLine("You are not abel to vote");
        }

        return;
    }
}
