using System;

namespace GenericConstraintsDemo.Models;

public readonly record struct StockSnapshot(int Available, int Reserved) : IFormattable
{
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        string selectedFormat = string.IsNullOrWhiteSpace(format) ? "G" : format.ToUpperInvariant();

        return selectedFormat switch
        {
            "R" => $"{Reserved} reservados de {Available + Reserved} unidades totais",
            _ => $"{Available} disponiveis, {Reserved} reservados"
        };
    }
}
