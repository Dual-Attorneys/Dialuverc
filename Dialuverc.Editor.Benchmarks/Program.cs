using BenchmarkDotNet.Running;

namespace Dialuverc.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                .Run();
        }
    }
}