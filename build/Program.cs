using Build.Modules;
using Build.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularPipelines.Extensions;
using ModularPipelines.Host;

await PipelineHostBuilder.Create()
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, collection) =>
    {
        collection.AddOptions<BuildOptions>().Bind(context.Configuration.GetSection("Build")).ValidateDataAnnotations();

        if (args.Contains("delete-nuget"))
        {
            collection.AddOptions<NuGetOptions>().Bind(context.Configuration.GetSection("NuGet")).ValidateDataAnnotations();
            collection.AddModule<DeleteNugetModule>();
            return;
        }

        collection.AddModule<ResolveConfigurationsModule>();
        collection.AddModule<UpdateNugetSourceModule>();
        collection.AddModule<CompileProjectModule>();

        if (args.Contains("pack"))
        {
            collection.AddModule<CleanProjectsModule>();
            collection.AddModule<RepackInjectorModule>();
            collection.AddModule<PackNugetModule>();
        }

        if (args.Contains("publish"))
        {
            collection.AddOptions<PublishOptions>().Bind(context.Configuration.GetSection("Publish")).ValidateDataAnnotations();
            collection.AddOptions<NuGetOptions>().Bind(context.Configuration.GetSection("NuGet")).ValidateDataAnnotations();

            collection.AddModule<PublishNugetModule>();
        }
    })
    .ExecutePipelineAsync();