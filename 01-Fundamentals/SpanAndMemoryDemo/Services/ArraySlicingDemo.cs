using System;
using SpanAndMemoryDemo.Models;

namespace SpanAndMemoryDemo.Services;

public sealed class ArraySlicingDemo
{
    public SliceReport CreateArraySlice(string rawLine)
    {
        char[] buffer = rawLine.ToCharArray();
        int firstCommaIndex = Array.IndexOf(buffer, ',');

        if (firstCommaIndex < 0)
        {
            throw new FormatException("Linha sem separador de simbolo.");
        }

        char[] symbolCopy = buffer[..firstCommaIndex];
        string symbol = new(symbolCopy);

        return new SliceReport(
            rawLine,
            symbol,
            "Range em array cria uma nova copia para o slice.");
    }
}
