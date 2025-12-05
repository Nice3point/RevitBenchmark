using System.Runtime.CompilerServices;
using Autodesk.Revit.ApplicationServices;
using Nice3point.Revit.Injector;

namespace Nice3point.BenchmarkDotNet.Revit;

/// <summary>
/// Represents an abstract base class for benchmark that require interaction with the Revit application environment.
/// Provides methods to initialize and terminate the connection to the Revit application.
/// </summary>
public abstract class RevitApplicationBenchmark
{
    private static Injector? _injector;

    /// <summary>
    /// Initializes the required components or dependencies for the execution of the module.
    /// This method is automatically invoked during the module's initialization process.
    /// </summary>
    /// <remarks>BenchmarkDotNet required a duplicated ModuleInitializer to run from another .exe</remarks>
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Initialize()
    {
        InjectorInitializer.InitializeModule();
    }

    /// <summary>
    /// Represents the database level Autodesk Revit Application, providing access to documents, options and other application wide data and settings.
    /// </summary>
    protected static Application Application { get; private set; } = null!;

    /// <summary>
    /// Initializes the connection to the Revit application.
    /// </summary>
    protected static void InitializeRevitConnection()
    {
        _injector = new Injector();
        Application = _injector.InjectApplication();
    }

    /// <summary>
    /// Terminates the connection to the Revit application.
    /// Frees associated resources and properly closes the interaction with the Revit environment.
    /// </summary>
    protected static void TerminateRevitConnection()
    {
        _injector?.EjectApplication();
    }
}