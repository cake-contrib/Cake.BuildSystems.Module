using System;
using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using JetBrains.Annotations;

namespace Cake.MyGet.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for MyGet.
    /// </summary>
    [UsedImplicitly]
    public class MyGetEngine : CakeEngineBase
    {
        private System.Diagnostics.Stopwatch _stopwatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyGetEngine"/> class.
        /// </summary>
        /// <param name="dataService">Implementation of <see cref="ICakeDataService"/>.</param>
        /// <param name="log">Implementation of <see cref="ICakeLog"/>.</param>
        public MyGetEngine(ICakeDataService dataService, ICakeLog log)
            : base(new CakeEngine(dataService, log))
        {
            _engine.BeforeTaskSetup += OnTaskSetup;
            _engine.BeforeTaskTeardown += OnTaskTeardown;
        }

        private void OnTaskTeardown(object sender, BeforeTaskTeardownEventArgs e)
        {
            var b = e.TaskTeardownContext.BuildSystem();
            if (b.IsRunningOnMyGet)
            {
                _stopwatch.Stop();
                var messageText = e.TaskTeardownContext.Skipped
                    ? $"Skipped Task {e.TaskTeardownContext.Task.Name}"
                    : $"Completed Task {e.TaskTeardownContext.Task.Name} in {_stopwatch.Elapsed.ToString("c", System.Globalization.CultureInfo.InvariantCulture)}";

                Console.WriteLine("##myget[message text='{0}' status='NORMAL']", messageText);
            }
        }

        private void OnTaskSetup(object sender, BeforeTaskSetupEventArgs e)
        {
            var b = e.TaskSetupContext.BuildSystem();
            if (b.IsRunningOnMyGet)
            {
                var messageText = $"Starting Task {e.TaskSetupContext.Task.Name}{(string.IsNullOrWhiteSpace(e.TaskSetupContext.Task.Description) ? string.Empty : $" ({e.TaskSetupContext.Task.Description})")}";
                Console.WriteLine("##myget[message text='{0}' status='NORMAL']", messageText);
                _stopwatch = _stopwatch.EnsureStarted();
            }
        }
    }
}
