using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit;

/// <summary>
///     Represents a benchmark that runs inside the Revit API context.
/// </summary>
/// <remarks>
///     The class opens the connection to Revit before the first benchmark iteration and terminates it after the last one.
/// </remarks>
public abstract class RevitApiBenchmark : RevitApplicationBenchmark
{
    /// <summary>
    ///     Opens the connection to Revit and runs <see cref="OnGlobalSetup" />.
    /// </summary>
    /// <remarks>
    ///     BenchmarkDotNet calls the method once, before the warm-up.
    ///     An exception from <see cref="OnGlobalSetup" /> terminates the connection and propagates to BenchmarkDotNet.
    /// </remarks>
    [GlobalSetup]
    public void RevitSessionSetup()
    {
        InitializeRevitConnection();

        try
        {
            OnGlobalSetup();
        }
        catch
        {
            TerminateRevitConnection();
            throw;
        }
    }

    /// <summary>
    ///     Runs <see cref="OnGlobalCleanup" /> and terminates the connection to Revit.
    /// </summary>
    /// <remarks>BenchmarkDotNet calls the method once, after the last benchmark iteration.</remarks>
    [GlobalCleanup]
    public void RevitSessionCleanup()
    {
        OnGlobalCleanup();
        TerminateRevitConnection();
    }

    /// <summary>
    ///     When overridden in a derived class, initializes the benchmark before the first iteration.
    /// </summary>
    /// <remarks>
    ///     The method runs once, before the warm-up, while the connection to Revit is open.
    ///     An exception from the method terminates the connection, and <see cref="OnGlobalCleanup" /> does not run.
    /// </remarks>
    protected virtual void OnGlobalSetup()
    {
    }

    /// <summary>
    ///     When overridden in a derived class, releases the resources of the benchmark after the last iteration.
    /// </summary>
    /// <remarks>The method runs once, after the last iteration, while the connection to Revit is open.</remarks>
    protected virtual void OnGlobalCleanup()
    {
    }
}
