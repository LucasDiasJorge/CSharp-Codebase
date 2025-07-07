using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Xml.Serialization;
using MessagePack;

namespace Serialization;

class Program
{
    static void Main(string[] args)
    {
        TestsRethrow testRethrow = new TestsRethrow();
        testRethrow.CatchException();
    }
}

class TestsRethrow
{

    public bool CatchException()
    {
        try
        {
            return method();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Method Exception called");
            throw new Exception(ex.Message);
        }
    }

    public bool method()
    {
        try
        {
            int i = 0;
            int j = 1;

            if (j > i)
            {
                throw new ArithmeticException("illegal expression baby");
            }

            Console.WriteLine("CUDI CUDI ?");
            return false;
        }
        catch (ArithmeticException ex)
        {
            Console.WriteLine("Arithmetic Exception");
            throw new ArithmeticException(ex.Message);
        }
    }

}
