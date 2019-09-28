using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steeltoe.CircuitBreaker.Hystrix;

namespace Company.WebApplication1
{
    public class MyCircuitBreakerCommand : HystrixCommand<string>
    {
        private string _name;
        public MyCircuitBreakerCommand(string name): base(HystrixCommandGroupKeyDefault.AsKey("MyCircuitBreakerGroup"))
        {
            _name = name;
            IsFallbackUserDefined = true;
        }
        protected override async Task<string> RunAsync()
        {
            return await Task.FromResult("Hello " + _name);
        }
        protected override async Task<string> RunFallbackAsync()
        {
            return await Task.FromResult("Hello " + _name + " via fallback");
        }
    }
}
