// https://www.youtube.com/watch?v=dOonV4byDEg

using System;

namespace SlidingWindows
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("Sliding Window Example");

            // Example input array and window size
            int[] nums = { 8,3,-2,4,5,-1,0,5,3,9,-6 };
            int windowSize = 5;
            int maxSum = MaxSumSlidingWindow(nums, windowSize);

            Console.WriteLine($"Max sum of subarray of length {windowSize} is: {maxSum}");
        }

        // Calculates the maximum sum of any subarray of the given fixed size.
        static int MaxSumSlidingWindow(int[] nums, int k)
        {
            if (nums == null || nums.Length < k)
                throw new ArgumentException("Invalid input");

            int maxSum = 0;
            for (int i = 0; i < k; i++)
                maxSum += nums[i];

            int windowSum = maxSum;
            for (int i = k; i < nums.Length; i++)
            {
                windowSum += nums[i] - nums[i - k];
                maxSum = Math.Max(maxSum, windowSum);
            }
            return maxSum;
        }
    }
}
