using System;
using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;

namespace Cake.MyGet.Module
{
    public class MyGetEngine : CakeEngineBase
    {
        private ICakeLog _log;
        private System.Diagnostics.Stopwatch _stopwatch;

        public MyGetEngine(ICakeLog log) : base(new CakeEngine(log))
        {
            _log = log;
            //_engine.Setup += BuildSetup;
            _engine.TaskSetup += OnTaskSetup;
            _engine.TaskTeardown += OnTaskTeardown;
            //_engine.Teardown += OnBuildTeardown;
        }

        private void OnTaskTeardown(object sender, TaskTeardownEventArgs e)
        {
            var b = e.TaskTeardownContext.BuildSystem();
            if (b.IsRunningOnMyGet) {
                _stopwatch.Stop();
                var messageText = e.TaskTeardownContext.Skipped
                    ? $"Skipped Task '{e.TaskTeardownContext.Task.Name}'"
                    : $"Completed Task '{e.TaskTeardownContext.Task.Name}' in {_stopwatch.Elapsed.ToString("c", System.Globalization.CultureInfo.InvariantCulture)}";
                    _log.Write(Verbosity.Quiet, LogLevel.Information,
                        "##myget[message text='{0}' status='NORMAL']", messageText);
            }
        }

        private void OnTaskSetup(object sender, TaskSetupEventArgs e)
        {
            var b = e.TaskSetupContext.BuildSystem();
            if (b.IsRunningOnMyGet) {
                var messageText = $"Starting Task '{e.TaskSetupContext.Task.Name}'{(string.IsNullOrWhiteSpace(e.TaskSetupContext.Task.Description) ? string.Empty : $" ({e.TaskSetupContext.Task.Description})")}";
                _log.Write(Verbosity.Quiet, LogLevel.Information,
                            "##myget[message text='{0}' status='NORMAL']", messageText);
                _stopwatch = _stopwatch.EnsureStarted();
            }
        }
    }
}