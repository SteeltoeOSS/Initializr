// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

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

                // Console.WriteLine($"{indent} {label} {x.TotalMilliseconds}");
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
