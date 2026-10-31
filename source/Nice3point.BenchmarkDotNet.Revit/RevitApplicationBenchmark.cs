using System.Reflection;
using System.Runtime.CompilerServices;
using Autodesk.Revit.ApplicationServices;
using Nice3point.BenchmarkDotNet.Revit.Hosting;
using Nice3point.Revit.Injector;

namespace Nice3point.BenchmarkDotNet.Revit;

/// <summary>
///     Represents a benchmark that connects to the Revit application.
/// </summary>
/// <remarks>
///     <see cref="InitializeRevitConnection" /> opens the connection, and <see cref="TerminateRevitConnection" /> closes it.
/// </remarks>
public abstract class RevitApplicationBenchmark
{
    private static Injector? _injector;

    /// <summary>
    ///     Gets the database-level Revit application.
    /// </summary>
    /// <remarks>
    ///     The application exposes the documents, the options, and the application-wide settings of Revit.
    ///     The value is valid only between <see cref="InitializeRevitConnection" /> and <see cref="TerminateRevitConnection" />.
    /// </remarks>
    protected static Application Application { get; private set; } = null!;

    /// <summary>
    ///     Prepares the current process for the connection to Revit.
    /// </summary>
    /// <remarks>
    ///     The runtime calls the method once, when the module loads in the host process of BenchmarkDotNet and in every benchmark process.
    ///     The method sets the STA apartment state of the current thread and initializes the injector.
    ///     In the host process, the method resolves RevitAPIUI to its metadata, which the host process reads to discover the benchmarks.
    ///     Inside the Revit user interface application, which prepares its own process, the method has no effect.
    /// </remarks>
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Initialize()
    {
        // Revit.exe is a native host with no entry assembly, and it loads the module on a started thread whose apartment state is fixed.
        if (Assembly.GetEntryAssembly() is null)
        {
            return;
        }

        Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
        Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

        RevitUiMetadataResolver.Register();
        InjectorInitializer.InitializeModule();
    }

    /// <summary>
    ///     Opens the connection to the Revit application and assigns <see cref="Application" />.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">The current process has already opened a connection to Revit.</exception>
    /// <remarks>
    ///     A process opens the connection once.
    ///     Every later call throws, whether the first call succeeded, failed, or the connection was terminated.
    /// </remarks>
    protected static void InitializeRevitConnection()
    {
        _injector = new Injector();
        Application = _injector.InjectApplication();
    }

    /// <summary>
    ///     Terminates the connection to the Revit application.
    /// </summary>
    /// <remarks>
    ///     The method does nothing when <see cref="InitializeRevitConnection" /> has not run.
    ///     After the call, <see cref="Application" /> is invalid.
    /// </remarks>
    protected static void TerminateRevitConnection()
    {
        _injector?.EjectApplication();
    }
}
