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
            public static readonly string ForegroundWhite = string.Format(FORMAT, 97);
            public static readonly string ForegroundYellow = string.Format(FORMAT, 33);
            public static readonly string ForegroundLightGray = string.Format(FORMAT, 37);
            public static readonly string ForegroundDarkGray = string.Format(FORMAT, 90);
            public static readonly string BackgroundMagenta = string.Format(FORMAT, 45);
            public static readonly string BackgroundRed = string.Format(FORMAT, 41);

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
            // For the colors, mostly match the colors used by Cake, see https://github.com/cake-build/cake/blob/ed612029b92f5da2b6cbdfe295c62e6b99a2963d/src/Cake.Core/Diagnostics/Console/ConsolePalette.cs#L34C17-L34C17
            // Not however, that the GitLab Web UI seems to render some colors the same (e.g. white and dark gray
            switch (level)
            {
                case LogLevel.Fatal:
                    _console.WriteErrorLine($"{AnsiEscapeCodes.BackgroundMagenta}{AnsiEscapeCodes.ForegroundWhite}{level}: {string.Format(format, args)}{AnsiEscapeCodes.Reset}");
                    break;
                case LogLevel.Error:
                    _console.WriteErrorLine($"{AnsiEscapeCodes.BackgroundRed}{AnsiEscapeCodes.ForegroundWhite}{level}: {string.Format(format, args)}{AnsiEscapeCodes.Reset}");
                    break;
                case LogLevel.Warning:
                    _console.WriteLine($"{AnsiEscapeCodes.ForegroundYellow}{level}: {string.Format(format, args)}{AnsiEscapeCodes.Reset}");
                    break;
                case LogLevel.Information:
                    _console.WriteLine($"{level}: {string.Format(format, args)}");
                    break;
                case LogLevel.Verbose:
                    _console.WriteLine($"{AnsiEscapeCodes.ForegroundLightGray}{level}: {string.Format(format, args)}{AnsiEscapeCodes.Reset}");
                    break;
                case LogLevel.Debug:
                    _console.WriteLine($"{AnsiEscapeCodes.ForegroundDarkGray}{level}: {string.Format(format, args)}{AnsiEscapeCodes.Reset}");
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
