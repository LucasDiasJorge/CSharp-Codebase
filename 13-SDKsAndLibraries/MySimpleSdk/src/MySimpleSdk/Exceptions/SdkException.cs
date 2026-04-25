// filepath: c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\MySimpleSdk\MySimpleSdk\src\MySimpleSdk\Exceptions\SdkException.cs
using System;

namespace MySimpleSdk.Exceptions
{
    public class SdkException : Exception
    {
        public SdkException()
        {
        }

        public SdkException(string message)
            : base(message)
        {
        }

        public SdkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}