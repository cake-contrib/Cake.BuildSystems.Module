using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Cake.Core;
using Cake.Core.Diagnostics;

namespace Cake.AzurePipelines.Module
{
    /// <summary>
    /// The AzurePipelines execution strategy.
    /// </summary>
    public class AzurePipelinesExecutionStrategy : IExecutionStrategy
    {
        private readonly ICakeLog _log;
        private readonly IExecutionStrategy _defaultStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzurePipelinesExecutionStrategy"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        public AzurePipelinesExecutionStrategy(ICakeLog log)
        {
            _log = log;
            _defaultStrategy = new DefaultExecutionStrategy(log);
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync(CakeTask task, ICakeContext context)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                await _defaultStrategy.ExecuteAsync(task, context).ConfigureAwait(false);
                return;
            }

            if (task != null)
            {
                _log.Information(string.Empty);
                _log.Information("##[group]{0}", task.Name);

                await task.Execute(context).ConfigureAwait(false);

                _log.Information("##[endgroup]");
            }
        }

        /// <inheritdoc/>
        public async Task HandleErrorsAsync(Func<Exception, ICakeContext, Task> action, Exception exception, ICakeContext context)
        {
            await _defaultStrategy.HandleErrorsAsync(action, exception, context).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task InvokeFinallyAsync(Func<Task> action)
        {
            await _defaultStrategy.InvokeFinallyAsync(action).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void PerformSetup(Action<ISetupContext> action, ISetupContext context)
        {

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                _defaultStrategy.PerformSetup(action, context);
                return;
            }

            if (action != null)
            {
                _log.Information(string.Empty);
                _log.Information("##[group]Setup");

                action(context);

                _log.Information("##[endgroup]");
            }
        }

        /// <inheritdoc/>
        public void PerformTaskSetup(Action<ITaskSetupContext> action, ITaskSetupContext taskSetupContext)
        {
            if (taskSetupContext == null)
            {
                throw new ArgumentNullException(nameof(taskSetupContext));
            }

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                _defaultStrategy.PerformTaskSetup(action, taskSetupContext);
                return;
            }

            if (action != null)
            {
                _log.Information(string.Empty);
                _log.Information("##[group]TaskSetup: {0}", taskSetupContext.Task.Name);

                action(taskSetupContext);

                _log.Information("##[endgroup]");
            }
        }

        /// <inheritdoc/>
        public void PerformTaskTeardown(Action<ITaskTeardownContext> action, ITaskTeardownContext taskTeardownContext)
        {
            if (taskTeardownContext == null)
            {
                throw new ArgumentNullException(nameof(taskTeardownContext));
            }

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                _defaultStrategy.PerformTaskTeardown(action, taskTeardownContext);
            }

            if (action != null)
            {
                _log.Information(string.Empty);
                _log.Information("##[group]TaskTeardown: {0}", taskTeardownContext.Task.Name);

                action(taskTeardownContext);

                _log.Information("##[endgroup]");
            }
        }

        /// <inheritdoc/>
        public void PerformTeardown(Action<ITeardownContext> action, ITeardownContext teardownContext)
        {
            if (teardownContext == null)
            {
                throw new ArgumentNullException(nameof(teardownContext));
            }

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                _defaultStrategy.PerformTeardown(action, teardownContext);
                return;
            }

            if (action != null)
            {
                _log.Information(string.Empty);
                _log.Information("##[group]Teardown");

                action(teardownContext);

                _log.Information("##[endgroup]");
            }
        }

        /// <inheritdoc/>
        public async Task ReportErrorsAsync(Func<Exception, Task> action, Exception exception)
        {
            await _defaultStrategy.ReportErrorsAsync(action, exception).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Skip(CakeTask task, CakeTaskCriteria criteria)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                _defaultStrategy.Skip(task, criteria);
                return;
            }

            if (task != null)
            {
                _log.Verbose(string.Empty);
                _log.Information("##[group]{0}", task.Name);

                var message = string.IsNullOrWhiteSpace(criteria.Message)
                    ? task.Name : criteria.Message;
                _log.Verbose("Skipping task: {0}", message);

                _log.Information("##[endgroup]");
            }
        }
    }
}
