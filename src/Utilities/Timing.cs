using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Steeltoe.Initializr.Utilities
{
    public class Timing : IDisposable
    {
        private static int _depth = -1;
        private readonly Action<TimeSpan, int> _result;
        private readonly Stopwatch _stopwatch;

        public static Timing Over(ILogger host, string label)
        {
            return new Timing((x, d) =>
            {
                host.LogInformation(label, x, d);
                var indent = string.Join(string.Empty, Enumerable.Repeat("  ", d));
                Console.WriteLine($"{indent} {label} {x.TotalMilliseconds}");
            });
        }

        public Timing(Action<TimeSpan, int> result)
        {
            ++_depth;
            _result = result;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _result(_stopwatch.Elapsed, _depth);
            --_depth;
        }
    }
}
