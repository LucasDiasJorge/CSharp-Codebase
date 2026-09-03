using System;
using System.Globalization;
using SpanAndMemoryDemo.Models;

namespace SpanAndMemoryDemo.Services;

public sealed class SpanParsingDemo
{
    public TradeOrder Parse(ReadOnlySpan<char> rawLine)
    {
        int firstCommaIndex = rawLine.IndexOf(',');

        if (firstCommaIndex < 0)
        {
            throw new FormatException("Linha sem separador de simbolo.");
        }

        ReadOnlySpan<char> symbolSlice = rawLine[..firstCommaIndex];
        ReadOnlySpan<char> remainingLine = rawLine[(firstCommaIndex + 1)..];
        int secondCommaIndex = remainingLine.IndexOf(',');

        if (secondCommaIndex < 0)
        {
            throw new FormatException("Linha sem separador de quantidade.");
        }

        ReadOnlySpan<char> quantitySlice = remainingLine[..secondCommaIndex];
        ReadOnlySpan<char> priceSlice = remainingLine[(secondCommaIndex + 1)..];

        bool quantityParsed = int.TryParse(
            quantitySlice,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int quantity);

        bool priceParsed = decimal.TryParse(
            priceSlice,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out decimal price);

        if (!quantityParsed || !priceParsed)
        {
            throw new FormatException("Linha contem quantidade ou preco invalido.");
        }

        string symbol = symbolSlice.ToString();
        return new TradeOrder(symbol, quantity, price);
    }
}
