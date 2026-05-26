

string date = "2024-01-01";
DateTime dateTime = date.ToDateTime();
Console.WriteLine(dateTime.Year);

public static class StringExtensions
{
    public static DateTime ToDateTime(this string str)
    {
        return DateTime.Parse(str);
    }
}