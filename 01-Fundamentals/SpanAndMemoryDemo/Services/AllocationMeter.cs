using System;

namespace SpanAndMemoryDemo.Services;

public static class AllocationMeter
{
    public static long Measure(Action action)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long beforeBytes = GC.GetAllocatedBytesForCurrentThread();
        action();
        long afterBytes = GC.GetAllocatedBytesForCurrentThread();

        return afterBytes - beforeBytes;
    }
}
