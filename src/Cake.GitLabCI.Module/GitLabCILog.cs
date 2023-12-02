using System;
using Cake.Core;
using Cake.Core.Diagnostics;
using JetBrains.Annotations;

namespace Cake.AzurePipelines.Module
{
    /// <summary>
    /// <see cref="ICakeLog"/> implementation for GitLab CI.
    /// </summary>
    [UsedImplicitly]
    public class GitLabCILog : ICakeLog
    {
        private static class AnsiEscapeCodes
        {
            public static readonly string Reset = string.Format(FORMAT, 0);
            public static readonly string ForegroundRed = string.Format(FORMAT, 31);
            public static readonly string ForegroundYellow = string.Format(FORMAT, 33);

            private const string FORMAT = "\u001B[{0}m";
        }

        private readonly ICakeLog _cakeLogImplementation;
        private readonly IConsole _console;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabCILog"/> class.
        /// </summary>
        /// <param name="console">Implementation of <see cref="IConsole"/>.</param>
        /// <param name="verbosity">Default <see cref="Verbosity"/>.</param>
        public GitLabCILog(IConsole console, Verbosity verbosity = Verbosity.Normal)
        {
            _cakeLogImplementation = new CakeBuildLog(console, verbosity);
            _console = console;
        }

        /// <inheritdoc />
        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(Environment.GetEnvironmentVariable("CI_SERVER"), "yes"))
            {
                _cakeLogImplementation.Write(verbosity, level, format, args);
            }

            if (verbosity > Verbosity)
            {
                return;
            }

            // Use colored output for log messages on GitLab CI
            // For reference, see https://docs.gitlab.com/ee/ci/yaml/script.html#add-color-codes-to-script-output
            switch (level)
            {
                case LogLevel.Fatal:
                case LogLevel.Error:
                    _console.WriteLine($"{AnsiEscapeCodes.ForegroundRed}{level}: {string.Format(format, args)}{AnsiEscapeCodes.Reset}");
                    break;
                case LogLevel.Warning:
                    _console.WriteLine($"{AnsiEscapeCodes.ForegroundYellow}{level}: {string.Format(format, args)}{AnsiEscapeCodes.Reset}");
                    break;
                case LogLevel.Information:
                case LogLevel.Verbose:
                case LogLevel.Debug:
                    _console.WriteLine($"{level}: {string.Format(format, args)}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        /// <inheritdoc />
        public Verbosity Verbosity
        {
            get { return _cakeLogImplementation.Verbosity; }
            set { _cakeLogImplementation.Verbosity = value; }
        }
    }
}
