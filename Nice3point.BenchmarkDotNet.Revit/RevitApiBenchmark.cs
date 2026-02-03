using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit;

/// <summary>
/// Represents a base class for executing benchmark within the Revit environment.
/// This class provides setup and cleanup methods for initializing and terminating
/// the connection to the Revit API before and after the benchmark session.
/// </summary>
public abstract class RevitApiBenchmark : RevitApplicationBenchmark
{
    /// <summary>
    /// Sets up the Revit session by initializing the connection to the Revit API.
    /// This method is executed before the benchmark session begins, ensuring that the
    /// necessary prerequisites for the tests interacting with the Revit environment are satisfied.
    /// </summary>
    [GlobalSetup]
    public void RevitSessionSetup()
    {
        InitializeRevitConnection();
        OnGlobalSetup();
    }

    /// <summary>
    /// Cleans up the Revit session by terminating the connection to the Revit API.
    /// This method is executed after the benchmark session concludes, ensuring that
    /// resources and connections related to the Revit environment are properly released.
    /// </summary>
    [GlobalCleanup]
    public void RevitSessionCleanup()
    {
        OnGlobalCleanup();
        TerminateRevitConnection();
    }
    
    /// <summary>
    /// Override this method to implement custom initialization logic before all benchmark iterations.
    /// </summary>
    /// <remarks>It's going to be executed only once, just before warm up.</remarks>
    protected virtual void OnGlobalSetup()
    {
    }

    /// <summary>
    /// Override this method to implement custom cleanup logic after all benchmark iterations.
    /// </summary>
    /// <remarks>It's going to be executed only once, after all benchmark runs.</remarks>
    protected virtual void OnGlobalCleanup()
    {
    }
}