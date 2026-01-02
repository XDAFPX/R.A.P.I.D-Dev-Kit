namespace DAFP.TOOLS.Common.Utill
{
    using System;
    using System.Diagnostics;

    public static class BenchmarkUtility
    {
        /// <summary>
        /// Measures total elapsed time for calling <paramref name="action"/> <paramref name="iterations"/> times.
        /// </summary>
        public static TimeSpan Measure(Action action, int iterations = 1000000)
        {
            // Warm up JIT and any caches
            action();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++) action();

            sw.Stop();
            return sw.Elapsed;
        }
    }
}