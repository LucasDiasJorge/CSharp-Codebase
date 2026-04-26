// https://youtu.be/b7Vy-uIQUrE?si=voL4fKMkjS4phtd4

namespace Two_Sum;

class Program
{
    static void Main(string[] args)
    {
        int[] arr = { 2, 3, 11, 15, 7 };
        Solution solution = new Solution();
        int[] solutionResponse = solution.TwoSum(arr, 9);

        Console.WriteLine($"Índices encontrados: [{solutionResponse[0]}, {solutionResponse[1]}]");
        Console.WriteLine($"Valores: {arr[solutionResponse[0]]} + {arr[solutionResponse[1]]} = 9");
    }
}

class Solution
{
    public int[] TwoSum(int[] nums, int target)
    {
        Dictionary<int, int> numToIndex = new Dictionary<int, int>();

        for (int i = 0; i < nums.Length; i++)
        {
            int complement = target - nums[i];
            if (numToIndex.ContainsKey(complement))
            {
                return new int[] { numToIndex[complement], i };
            }
            numToIndex[nums[i]] = i;
        }

        throw new ArgumentException("No two sum solution");
    }
}
