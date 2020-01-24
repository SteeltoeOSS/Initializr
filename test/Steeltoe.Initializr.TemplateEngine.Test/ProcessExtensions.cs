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

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable MethodSupportsCancellation
namespace Steeltoe.Initializr.TemplateEngine.Test
{
    public static class ProcessExtensions
    {
        public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>();

            void ProcessExited(object sender, EventArgs e)
            {
                Task.Run(() => tcs.TrySetResult(true));
            }

            process.EnableRaisingEvents = true;
            process.Exited += ProcessExited;

            try
            {
                if (process.HasExited)
                {
                    return;
                }

                using (cancellationToken.Register(() => Task.Run(() => tcs.TrySetCanceled())))
                {
                    await tcs.Task;
                }
            }
            finally
            {
                process.Exited -= ProcessExited;
            }
        }
    }
}
