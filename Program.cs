class Program
{
    public static void Main(string[] args)
    {
        var depthFinder = new DepthFinder();
        for (int i = 0; i < 1000; i++)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            depthFinder.Update();
            List<Result> results = depthFinder.GetVisibleObjects();
            if (results.Count > 0) {
                Console.WriteLine("-> {0}", results[0].ClassName);
            }
            watch.Stop();
            Console.WriteLine("Time elapsed this frame: {0}ms", watch.ElapsedMilliseconds);
        }
        depthFinder.Terminate();
    }
}
