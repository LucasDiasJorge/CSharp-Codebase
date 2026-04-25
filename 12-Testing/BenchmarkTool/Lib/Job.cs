using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace BenchmarkTool.Lib
{
    public class Job
    {
        // Job 1: Encontrar números primos em um intervalo grande (CPU intensivo)
        public static List<long> FindPrimes(long start, long end)
        {
            List<long> primes = new List<long>();
            
            for (long num = start; num <= end; num++)
            {
                if (IsPrime(num))
                {
                    primes.Add(num);
                }
            }
            
            return primes;
        }

        private static bool IsPrime(long number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            long boundary = (long)Math.Floor(Math.Sqrt(number));

            for (long i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0) return false;
            }

            return true;
        }

        // Job 2: Multiplicação de matrizes grandes (CPU e Memória intensivo)
        public static double[,] MatrixMultiplication(int size)
        {
            double[,] matrixA = GenerateRandomMatrix(size);
            double[,] matrixB = GenerateRandomMatrix(size);
            double[,] result = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < size; k++)
                    {
                        result[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }

            return result;
        }

        private static double[,] GenerateRandomMatrix(int size)
        {
            double[,] matrix = new double[size, size];
            Random random = new Random();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] = random.NextDouble() * 100;
                }
            }

            return matrix;
        }

        // Job 3: Hash computacional intensivo (simula criptografia pesada)
        public static List<string> ComputeHashes(int iterations)
        {
            List<string> hashes = new List<string>();
            string data = "Heavy computation benchmark test data";

            using (SHA256 sha256 = SHA256.Create())
            {
                for (int i = 0; i < iterations; i++)
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(data + i);
                    byte[] hashBytes = sha256.ComputeHash(inputBytes);
                    
                    // Faz múltiplos rounds de hash para aumentar a carga
                    for (int round = 0; round < 100; round++)
                    {
                        hashBytes = sha256.ComputeHash(hashBytes);
                    }
                    
                    string hash = BitConverter.ToString(hashBytes).Replace("-", "");
                    hashes.Add(hash);
                }
            }

            return hashes;
        }

        // Job 4: Cálculo de Fibonacci com BigInteger (memória e CPU intensivo)
        public static BigInteger FibonacciBig(int n)
        {
            if (n <= 1) return n;

            BigInteger a = 0;
            BigInteger b = 1;

            for (int i = 2; i <= n; i++)
            {
                BigInteger temp = a + b;
                a = b;
                b = temp;
            }

            return b;
        }

        // Job 5: Ordenação de lista gigante (memória e CPU intensivo)
        public static List<int> SortLargeList(int size)
        {
            Random random = new Random();
            List<int> list = new List<int>(size);

            // Gera lista aleatória
            for (int i = 0; i < size; i++)
            {
                list.Add(random.Next(1, 1000000));
            }

            // Ordena usando bubble sort (O(n²) - propositalmente ineficiente)
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = 0; j < list.Count - i - 1; j++)
                {
                    if (list[j] > list[j + 1])
                    {
                        int temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;
                    }
                }
            }

            return list;
        }

        // Job 6: Processamento de strings intensivo
        public static string StringManipulation(int iterations)
        {
            StringBuilder result = new StringBuilder();
            string baseString = "BenchmarkTest";

            for (int i = 0; i < iterations; i++)
            {
                string temp = baseString + i;
                
                // Operações pesadas de string
                temp = temp.ToUpper().ToLower().ToUpper();
                temp = string.Concat(temp.Reverse());
                temp = string.Concat(temp.Reverse());
                
                if (i % 1000 == 0)
                {
                    result.Append(temp);
                }
            }

            return result.ToString();
        }
    }
}
