using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cake.Core;

namespace Cake.Module.Shared
{
    /// <summary>
    /// Base-implementation for a wrapped <see cref="ICakeEngine"/> - the base Shared Engine used by CI modules.
    /// </summary>
    public abstract class CakeEngineBase : ICakeEngine
    {
        /// <summary>
        /// Gets the wrapped <see cref="ICakeEngine"/>.
        /// </summary>
        // ReSharper disable once SA1401
        protected readonly ICakeEngine _engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeEngineBase"/> class.
        /// </summary>
        /// <param name="implementation">The wrapped <see cref="ICakeEngine"/>.</param>
        protected CakeEngineBase(ICakeEngine implementation)
        {
            _engine = implementation;
        }

        /// <summary>Registers a new task.</summary>
        /// <param name="name">The name of the task.</param>
        /// <returns>A <see cref="T:Cake.Core.CakeTaskBuilder`1" />.</returns>
        public CakeTaskBuilder RegisterTask(string name)
        {
            return _engine.RegisterTask(name);
        }

        /// <inheritdoc />
        public void RegisterSetupAction(Action<ISetupContext> action)
        {
            _engine.RegisterSetupAction(action);
        }

        /// <inheritdoc />
        public void RegisterSetupAction<TData>(Func<ISetupContext, TData> action)
            where TData : class
        {
            _engine.RegisterSetupAction(action);
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

        /// <inheritdoc />
        public void RegisterTeardownAction<TData>(Action<ITeardownContext, TData> action)
            where TData : class
        {
            _engine.RegisterTeardownAction(action);
        }

        /// <summary>
        /// Runs the specified target using the specified <see cref="T:Cake.Core.IExecutionStrategy" />.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="strategy">The execution strategy.</param>
        /// <param name="settings">The execution settings.</param>
        /// <returns>
        /// The resulting report.
        /// </returns>
        public Task<CakeReport> RunTargetAsync(ICakeContext context, IExecutionStrategy strategy, ExecutionSettings settings)
        {
            return _engine.RunTargetAsync(context, strategy, settings);
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

        /// <inheritdoc />
        public void RegisterTaskSetupAction<TData>(Action<ITaskSetupContext, TData> action)
            where TData : class
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

        /// <inheritdoc />
        public void RegisterTaskTeardownAction<TData>(Action<ITaskTeardownContext, TData> action)
            where TData : class
        {
            _engine.RegisterTaskTeardownAction(action);
        }

        /// <inheritdoc />
        IReadOnlyList<ICakeTaskInfo> ICakeEngine.Tasks => _engine.Tasks;

        /// <summary>Gets all registered tasks.</summary>
        /// <value>The registered tasks.</value>
        public IReadOnlyList<ICakeTaskInfo> Tasks => _engine.Tasks;

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
