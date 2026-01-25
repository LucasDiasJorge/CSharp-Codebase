using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkTool.Lib
{
    public interface IBenchmark
    {
        void Run(Func<Task> func);
    }
}
