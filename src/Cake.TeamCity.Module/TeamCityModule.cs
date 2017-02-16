using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.TeamCity.Module;

[assembly: CakeModule(typeof(TeamCityModule))]

namespace Cake.TeamCity.Module
{
    class TeamCityModule : ICakeModule
    {
        public void Register(ICakeContainerRegistrar registrar)
        {
            if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
            {
                registrar.RegisterType<TeamCityEngine>().As<ICakeEngine>().Singleton();
                registrar.RegisterType<TeamCityLog>().As<ICakeLog>().Singleton();
                //registrar.RegisterType<TFBuildReportPrinter>().As<ICakeReportPrinter>().Singleton();
            }
        }
    }
}