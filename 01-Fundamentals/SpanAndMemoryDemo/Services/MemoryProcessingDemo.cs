using System;
using System.Threading;
using System.Threading.Tasks;
using SpanAndMemoryDemo.Models;

namespace SpanAndMemoryDemo.Services;

public sealed class MemoryProcessingDemo
{
    private readonly SpanParsingDemo parser;

    public MemoryProcessingDemo(SpanParsingDemo parser)
    {
        this.parser = parser;
    }

    public async Task<TradeOrder> ParseAfterAsync(
        ReadOnlyMemory<char> rawLine,
        CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        return ParseMemory(rawLine);
    }

    private TradeOrder ParseMemory(ReadOnlyMemory<char> rawLine)
    {
        return parser.Parse(rawLine.Span);
    }
}
