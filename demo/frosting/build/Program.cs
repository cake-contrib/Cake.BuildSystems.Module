using Cake.AzurePipelines.Module;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Frosting;
using Cake.MyGet.Module;
using Cake.TeamCity.Module;
using Cake.TravisCI.Module;
using JetBrains.Annotations;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            // Register all modules from Cake.Buildsystems.Module
            .UseModule<AzurePipelinesModule>()
            .UseModule<MyGetModule>()
            .UseModule<TravisCIModule>()
            .UseModule<TeamCityModule>()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public BuildContext(ICakeContext context)
        : base(context)
    {
    }
}

[TaskName("Default")]
[UsedImplicitly]
public class DefaultTask : FrostingTask
{
    public override void Run(ICakeContext context)
    {
        context.Information("Hello Cake!");
        context.Verbose("This is really verbose.");
        context.Warning("This is a warning.");
        context.Debug("This is a debug-message.");
        context.Information("Next comes the error:");
        context.Error("This is an error.");
    }
}
