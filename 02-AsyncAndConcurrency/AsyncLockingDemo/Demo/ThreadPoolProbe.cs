using System.Diagnostics;
using AsyncLockingDemo.Models;

namespace AsyncLockingDemo.Demo;

/// <summary>
/// Instrumentação da thread pool durante um cenário. Combina duas medidas:
/// um amostrador de <see cref="ThreadPool.ThreadCount"/> e um heartbeat agendado na
/// própria pool, que só continua batendo enquanto sobrar thread para ele.
/// </summary>
public sealed class ThreadPoolProbe
{
    private static readonly TimeSpan SampleInterval = TimeSpan.FromMilliseconds(10);
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMilliseconds(25);

    private readonly CancellationTokenSource _stopSource = new CancellationTokenSource();
    private readonly Stopwatch _clock = new Stopwatch();

    private Thread? _sampler;
    private Task? _heartbeat;
    private int _baselineThreadCount;
    private int _peakThreadCount;
    private int _heartbeats;
    private double _maxHeartbeatGapMs;

    public void Start()
    {
        _baselineThreadCount = ThreadPool.ThreadCount;
        _peakThreadCount = _baselineThreadCount;
        _clock.Start();

        // Thread dedicada, e não Task: o amostrador precisa continuar medindo
        // justamente quando a pool está saturada, então ele não pode depender dela.
        _sampler = new Thread(SampleLoop)
        {
            IsBackground = true,
            Name = "threadpool-sampler"
        };
        _sampler.Start();

        _heartbeat = Task.Run(HeartbeatLoopAsync);
    }

    public async Task<ThreadPoolSnapshot> StopAsync()
    {
        await _stopSource.CancelAsync().ConfigureAwait(false);

        if (_heartbeat is not null)
        {
            await _heartbeat.ConfigureAwait(false);
        }

        _sampler?.Join();
        _clock.Stop();
        _stopSource.Dispose();

        return new ThreadPoolSnapshot(_baselineThreadCount, _peakThreadCount, _heartbeats, _maxHeartbeatGapMs);
    }

    private void SampleLoop()
    {
        while (!_stopSource.IsCancellationRequested)
        {
            int current = ThreadPool.ThreadCount;
            if (current > _peakThreadCount)
            {
                _peakThreadCount = current;
            }

            Thread.Sleep(SampleInterval);
        }
    }

    private async Task HeartbeatLoopAsync()
    {
        double lastTickMs = _clock.Elapsed.TotalMilliseconds;

        while (!_stopSource.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(HeartbeatInterval, _stopSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            double nowMs = _clock.Elapsed.TotalMilliseconds;
            double gapMs = nowMs - lastTickMs;
            lastTickMs = nowMs;
            _heartbeats++;

            if (gapMs > _maxHeartbeatGapMs)
            {
                _maxHeartbeatGapMs = gapMs;
            }
        }
    }
}
