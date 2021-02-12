using Cake.Common.Build;
using Cake.Core;
using Cake.Module.Shared;
using JetBrains.Annotations;

namespace Cake.TravisCI.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for Travis CI.
    /// </summary>
    [UsedImplicitly]
    public class TravisCIEngine : CakeEngineBase
    {
        private readonly string _buildMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TravisCIEngine"/> class.
        /// </summary>
        /// <param name="dataService">Implementation of <see cref="ICakeDataService"/>.</param>
        /// <param name="console">Implementation of <see cref="IConsole"/>.</param>
        public TravisCIEngine(ICakeDataService dataService, IConsole console)
            : base(new CakeEngine(dataService, new RawBuildLog(console)))
        {
            _engine.Setup += OnBuildSetup;
            _engine.TaskSetup += OnTaskSetup;
            _engine.TaskTeardown += OnTaskTeardown;
            _engine.Teardown += OnBuildTeardown;

            // _buildMessage = "Cake";
            _buildMessage = $"Cake Build (running {_engine.Tasks.Count} tasks)";
        }

        private void OnBuildTeardown(object sender, TeardownEventArgs e)
        {
            var b = e.TeardownContext.BuildSystem();
            if (b.IsRunningOnTravisCI)
            {
                var tr = b.TravisCI;
                tr.WriteEndFold(_buildMessage.ToFoldMessage());
            }
        }

        private void OnTaskTeardown(object sender, TaskTeardownEventArgs e)
        {
            var b = e.TaskTeardownContext.BuildSystem();
            if (b.IsRunningOnTravisCI)
            {
                var tr = b.TravisCI;
                tr.WriteEndFold(e.TaskTeardownContext.Task.Name.ToFoldMessage());
            }
        }

        private void OnTaskSetup(object sender, TaskSetupEventArgs e)
        {
            var b = e.TaskSetupContext.BuildSystem();
            if (b.IsRunningOnTravisCI)
            {
                var tr = b.TravisCI;
                tr.WriteStartFold(e.TaskSetupContext.Task.Name.ToFoldMessage());
            }
        }

        private void OnBuildSetup(object sender, SetupEventArgs e)
        {
            var b = e.Context.BuildSystem();
            if (b.IsRunningOnTravisCI)
            {
                var tr = b.TravisCI;
                tr.WriteStartFold(_buildMessage.ToFoldMessage());
            }
        }
    }
}
