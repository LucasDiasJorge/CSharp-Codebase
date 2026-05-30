using System.Globalization;
using RedisCacheKeyParams.Contracts;

namespace RedisCacheKeyParams.CacheKeys;

public abstract class RedisCacheKeys : IRedisCacheKeys
{
    private readonly string separator;

    protected RedisCacheKeys(string separator = "_")
    {
        if (string.IsNullOrWhiteSpace(separator))
        {
            throw new ArgumentException("Separator cannot be null or whitespace.", nameof(separator));
        }

        this.separator = separator;
    }

    public abstract string Namespace { get; }

    public string Join(params object[] parts)
    {
        ValidateParts(parts);
        string[] normalizedParts = NormalizeParts(parts);
        return string.Join(this.separator, normalizedParts);
    }

    public string JoinWith(string customSeparator, params object[] parts)
    {
        if (string.IsNullOrWhiteSpace(customSeparator))
        {
            throw new ArgumentException("Custom separator cannot be null or whitespace.", nameof(customSeparator));
        }

        ValidateParts(parts);
        string[] normalizedParts = NormalizeParts(parts);
        return string.Join(customSeparator, normalizedParts);
    }

    private static void ValidateParts(object[] parts)
    {
        if (parts is null || parts.Length == 0)
        {
            throw new ArgumentException("At least one part is required.", nameof(parts));
        }
    }

    private static string[] NormalizeParts(object[] parts)
    {
        List<string> values = new List<string>(parts.Length);

        foreach (object part in parts)
        {
            if (part is null)
            {
                throw new ArgumentException("Parts cannot contain null values.", nameof(parts));
            }

            string? formattedValue = Convert.ToString(part, CultureInfo.InvariantCulture);

            if (string.IsNullOrWhiteSpace(formattedValue))
            {
                throw new ArgumentException("Parts cannot contain empty values.", nameof(parts));
            }

            values.Add(formattedValue);
        }

        return values.ToArray();
    }
}
