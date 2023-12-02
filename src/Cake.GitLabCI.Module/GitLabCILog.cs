using System;
using System.Collections.Generic;

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
        private readonly ICakeLog _cakeLogImplementation;
        private readonly IConsole _console;

        // Define the escape sequenes to make GitLab show colored messages
        // For reference, see https://docs.gitlab.com/ee/ci/yaml/script.html#add-color-codes-to-script-output
        // For the colors, match the colors used by Cake, see https://github.com/cake-build/cake/blob/ed612029b92f5da2b6cbdfe295c62e6b99a2963d/src/Cake.Core/Diagnostics/Console/ConsolePalette.cs#L34C17-L34C17
        private readonly Dictionary<LogLevel, string> _escapeSequences = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Fatal, $"{AnsiEscapeCodes.BackgroundMagenta}{AnsiEscapeCodes.ForegroundWhite}" },
            { LogLevel.Error, $"{AnsiEscapeCodes.BackgroundRed}{AnsiEscapeCodes.ForegroundWhite}" },
            { LogLevel.Warning, AnsiEscapeCodes.ForegroundYellow },
            { LogLevel.Verbose, AnsiEscapeCodes.ForegroundLightGray },
            { LogLevel.Debug, AnsiEscapeCodes.ForegroundDarkGray },
        };

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
        public Verbosity Verbosity
        {
            get { return _cakeLogImplementation.Verbosity; }
            set { _cakeLogImplementation.Verbosity = value; }
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

            string message;
            if (_escapeSequences.TryGetValue(level, out var sequence))
            {
                message = $"{sequence}{level}: {string.Format(format, args)}{AnsiEscapeCodes.Reset}";
            }
            else
            {
                message = $"{level}: {string.Format(format, args)}";
            }

            if (level > LogLevel.Error)
            {
                _console.WriteLine(message);
            }
            else
            {
                _console.WriteErrorLine(message);
            }
        }
    }
}
