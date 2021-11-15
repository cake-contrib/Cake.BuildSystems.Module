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
#pragma warning disable SA1401
        // ReSharper disable once InconsistentNaming
        protected readonly ICakeEngine _engine;
#pragma warning restore SA1401

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeEngineBase"/> class.
        /// </summary>
        /// <param name="implementation">The wrapped <see cref="ICakeEngine"/>.</param>
        protected CakeEngineBase(ICakeEngine implementation)
        {
            _engine = implementation;
        }

#pragma warning disable CS0618
        /// <inheritdoc cref="ICakeEngine.Setup"/>
        [Obsolete]
        public event EventHandler<SetupEventArgs> Setup
        {
            add { _engine.Setup += value; }
            remove { _engine.Setup -= value; }
        }
#pragma warning restore CS0618

        /// <inheritdoc cref="ICakeEngine.BeforeSetup"/>
        public event EventHandler<BeforeSetupEventArgs> BeforeSetup
        {
            add { _engine.BeforeSetup += value; }
            remove { _engine.BeforeSetup -= value; }
        }

        /// <inheritdoc cref="ICakeEngine.AfterSetup"/>
        public event EventHandler<AfterSetupEventArgs> AfterSetup
        {
            add { _engine.AfterSetup += value; }
            remove { _engine.AfterSetup -= value; }
        }

#pragma warning disable CS0618
        /// <inheritdoc cref="ICakeEngine.Teardown"/>
        [Obsolete]
        public event EventHandler<TeardownEventArgs> Teardown
        {
            add { _engine.Teardown += value; }
            remove { _engine.Teardown -= value; }
        }
#pragma warning restore CS0618

        /// <inheritdoc cref="ICakeEngine.BeforeTeardown"/>
        public event EventHandler<BeforeTeardownEventArgs> BeforeTeardown
        {
            add { _engine.BeforeTeardown += value; }
            remove { _engine.BeforeTeardown -= value; }
        }

        /// <inheritdoc cref="ICakeEngine.AfterTeardown"/>
        public event EventHandler<AfterTeardownEventArgs> AfterTeardown
        {
            add { _engine.AfterTeardown += value; }
            remove { _engine.AfterTeardown -= value; }
        }

#pragma warning disable CS0618
        /// <inheritdoc cref="ICakeEngine.TaskSetup"/>
        [Obsolete]
        public event EventHandler<TaskSetupEventArgs> TaskSetup
        {
            add { _engine.TaskSetup += value; }
            remove { _engine.TaskSetup -= value; }
        }
#pragma warning restore CS0618

        /// <inheritdoc cref="ICakeEngine.BeforeTaskSetup"/>
        public event EventHandler<BeforeTaskSetupEventArgs> BeforeTaskSetup
        {
            add { _engine.BeforeTaskSetup += value; }
            remove { _engine.BeforeTaskSetup -= value; }
        }

        /// <inheritdoc cref="ICakeEngine.AfterTaskSetup"/>
        public event EventHandler<AfterTaskSetupEventArgs> AfterTaskSetup
        {
            add { _engine.AfterTaskSetup += value; }
            remove { _engine.AfterTaskSetup -= value; }
        }

#pragma warning disable CS0618
        /// <inheritdoc cref="ICakeEngine.TaskTeardown"/>
        [Obsolete]
        public event EventHandler<TaskTeardownEventArgs> TaskTeardown
        {
            add { _engine.TaskTeardown += value; }
            remove { _engine.TaskTeardown -= value; }
        }
#pragma warning restore CS0618

        /// <inheritdoc cref="ICakeEngine.BeforeTaskTeardown"/>
        public event EventHandler<BeforeTaskTeardownEventArgs> BeforeTaskTeardown
        {
            add { _engine.BeforeTaskTeardown += value; }
            remove { _engine.BeforeTaskTeardown -= value; }
        }

        /// <inheritdoc cref="ICakeEngine.AfterTaskTeardown"/>
        public event EventHandler<AfterTaskTeardownEventArgs> AfterTaskTeardown
        {
            add { _engine.AfterTaskTeardown += value; }
            remove { _engine.AfterTaskTeardown -= value; }
        }

        /// <inheritdoc />
        IReadOnlyList<ICakeTaskInfo> ICakeEngine.Tasks => _engine.Tasks;

        /// <summary>Gets all registered tasks.</summary>
        /// <value>The registered tasks.</value>
        public IReadOnlyList<ICakeTaskInfo> Tasks => _engine.Tasks;

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
    }
}
