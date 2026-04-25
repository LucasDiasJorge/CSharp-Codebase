// filepath: c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\MySimpleSdk\MySimpleSdk\src\MySimpleSdk\MySimpleSdkExtensions.cs
using System;

namespace MySimpleSdk
{
    public static class MySimpleSdkExtensions
    {
        public static string ToFormattedString(this SdkModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return $"Id: {model.Id}, Name: {model.Name}, Description: {model.Description}";
        }

        public static void Log(this SdkException exception)
        {
            // Here you can implement logging logic, e.g., log to a file or monitoring system
            Console.WriteLine($"Exception: {exception.Message}");
        }
    }
}