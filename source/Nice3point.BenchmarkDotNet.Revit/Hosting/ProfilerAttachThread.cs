namespace Nice3point.BenchmarkDotNet.Revit.Hosting;

/// <summary>
///     Starts the profiler attach thread of the .NET Framework runtime in every benchmark process.
/// </summary>
/// <remarks>
///     A .NET Framework benchmark process with Revit answers a profiler attach request only on the attach thread.
///     The runtime reads the setting when it starts.
///     A benchmark process inherits the setting from the BenchmarkDotNet host process.
/// </remarks>
internal static class ProfilerAttachThread
{
    /// <summary>
    ///     Enables the profiler attach thread for the benchmark processes the host process starts.
    /// </summary>
    /// <remarks>
    ///     The call has no effect outside the BenchmarkDotNet host process and on .NET.
    /// </remarks>
    public static void Enable()
    {
#if NETFRAMEWORK
        if (BenchmarkHostProcess.IsCurrent)
        {
            Environment.SetEnvironmentVariable("COMPlus_AttachThreadAlwaysOn", "1");
        }
#endif
    }
}
