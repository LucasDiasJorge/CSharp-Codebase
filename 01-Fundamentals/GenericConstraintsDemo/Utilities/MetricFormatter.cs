using System;
using System.Globalization;

namespace GenericConstraintsDemo.Utilities;

public static class MetricFormatter
{
    public static string FormatStruct<TValue>(TValue value)
        where TValue : struct, IFormattable
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }
}
