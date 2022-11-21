using System;

using Cake.Core;
using Cake.Core.Diagnostics;

using JetBrains.Annotations;

namespace Cake.GitHubActions.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for GitHub Actions.
    /// </summary>
    [UsedImplicitly]
    public class GitHubActionsLog : ICakeLog
    {
        private readonly ICakeLog _cakeLogImplementation;
        private readonly IConsole _console;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubActionsLog"/> class.
        /// </summary>
        /// <param name="console">Implementation of <see cref="IConsole"/>.</param>
        /// <param name="verbosity">Default <see cref="Verbosity"/>.</param>
        public GitHubActionsLog(IConsole console, Verbosity verbosity = Verbosity.Normal)
        {
            _cakeLogImplementation = new CakeBuildLog(console, verbosity);
            _console = console;
        }

        /// <inheritdoc />
        public Verbosity Verbosity
        {
            get { return _cakeLogImplementation.Verbosity; }
            set { _cakeLogImplementation.Verbosity = value; }
        }

        /// <inheritdoc />
        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
            {
                _cakeLogImplementation.Write(verbosity, level, format, args);
            }

            if (verbosity > Verbosity)
            {
                return;
            }

            // see https://docs.github.com/en/actions/learn-github-actions/workflow-commands-for-github-actions
            // for available commands.
            switch (level)
            {
                case LogLevel.Fatal:
                case LogLevel.Error:
                    _console.WriteLine("::error::{0}", string.Format(format, args));
                    break;
                case LogLevel.Warning:
                    _console.WriteLine("::warning::{0}", string.Format(format, args));
                    break;
                case LogLevel.Information:
                case LogLevel.Verbose:
                case LogLevel.Debug:
                    // writing to debug in GH-Actions only shows when "runner diagnostic logging" or
                    // "step diagnostic logging" is enabled. Not sure if that's really useful, here.
                    _console.WriteLine(format, args);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}
