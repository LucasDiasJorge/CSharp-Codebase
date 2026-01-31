using System.ComponentModel.DataAnnotations;

namespace MinimalApiDemo.Annotations
{
    public class ProductPriceAttribute : ValidationAttribute
    {
 
        public override bool IsValid(object value)
        {
            if (value is decimal price)
            {
                Console.WriteLine("Ok doke");
                return price >= 0;
            }
            Console.WriteLine("Corre");
            return false;
        }

    }
}
