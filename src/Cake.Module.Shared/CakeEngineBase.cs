using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cake.Core;

namespace Cake.Module.Shared
{
    public abstract class CakeEngineBase : ICakeEngine
    {
        protected readonly ICakeEngine _engine;
        protected CakeEngineBase(ICakeEngine implementation)
        {
            _engine = implementation;
        }
        /// <summary>Registers a new task.</summary>
        /// <param name="name">The name of the task.</param>
        /// <returns>A <see cref="T:Cake.Core.CakeTaskBuilder`1" />.</returns>
        public CakeTaskBuilder<ActionTask> RegisterTask(string name)
        {
            return _engine.RegisterTask(name);
        }

        /// <summary>
        ///     Allows registration of an action that's executed before any tasks are run.
        ///     If setup fails, no tasks will be executed but teardown will be performed.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public void RegisterSetupAction(Action<ICakeContext> action)
        {
            _engine.RegisterSetupAction(action);
        }

        /// <summary>
        ///     Allows registration of an action that's executed after all other tasks have been run.
        ///     If a setup action or a task fails with or without recovery, the specified teardown action will still be executed.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public void RegisterTeardownAction(Action<ITeardownContext> action)
        {
            _engine.RegisterTeardownAction(action);
        }

        /// <summary>
        /// Runs the specified target using the specified <see cref="T:Cake.Core.IExecutionStrategy" />.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="strategy">The execution strategy.</param>
        /// <param name="target">The target to run.</param>
        /// <returns>
        /// The resulting report.
        /// </returns>
        public Task<CakeReport> RunTargetAsync(ICakeContext context, IExecutionStrategy strategy, string target)
        {
            return _engine.RunTargetAsync(context, strategy, target);
        }

        /// <summary>
        ///     Allows registration of an action that's executed before each task is run.
        ///     If the task setup fails, the task will not be executed but the task's teardown will be performed.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public void RegisterTaskSetupAction(Action<ITaskSetupContext> action)
        {
            _engine.RegisterTaskSetupAction(action);
        }

        /// <summary>
        ///     Allows registration of an action that's executed after each task has been run.
        ///     If a task setup action or a task fails with or without recovery, the specified task teardown action will still be
        ///     executed.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public void RegisterTaskTeardownAction(Action<ITaskTeardownContext> action)
        {
            _engine.RegisterTaskTeardownAction(action);
        }

        /// <summary>Gets all registered tasks.</summary>
        /// <value>The registered tasks.</value>
        public IReadOnlyList<CakeTask> Tasks => _engine.Tasks;

        /// <summary>Raised during setup before any tasks are run.</summary>
        public event EventHandler<SetupEventArgs> Setup
        {
            add { _engine.Setup += value; }
            remove { _engine.Setup -= value; }
        }

        /// <summary>
        ///     Raised during teardown after all other tasks have been run.
        /// </summary>
        public event EventHandler<TeardownEventArgs> Teardown
        {
            add { _engine.Teardown += value; }
            remove { _engine.Teardown -= value; }
        }

        /// <summary>Raised before each task is run.</summary>
        public event EventHandler<TaskSetupEventArgs> TaskSetup
        {
            add { _engine.TaskSetup += value; }
            remove { _engine.TaskSetup -= value; }
        }

        /// <summary>Raised after each task has been run.</summary>
        public event EventHandler<TaskTeardownEventArgs> TaskTeardown
        {
            add { _engine.TaskTeardown += value; }
            remove { _engine.TaskTeardown -= value; }
        }
    }
}