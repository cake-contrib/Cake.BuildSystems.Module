// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Diagnostics.Formatting;

namespace Cake.Module.Shared
{
    /// <summary>
    /// Writes the log, as raw.
    /// </summary>
    public sealed class RawBuildLog : ICakeLog
    {
        private readonly IConsole _console;
        private readonly object _lock;

        /// <inheritdoc />
        public Verbosity Verbosity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawBuildLog"/> class.
        /// </summary>
        /// <param name="console">The <see cref="IConsole"/> implementation to log to.</param>
        /// <param name="verbosity">Initial verbosity.</param>
        public RawBuildLog(IConsole console, Verbosity verbosity = Verbosity.Normal)
        {
            _console = console;
            _lock = new object();
            Verbosity = verbosity;
        }

        /// <inheritdoc />
        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (verbosity > Verbosity)
            {
                return;
            }

            lock (_lock)
            {
                try
                {
                    var tokens = FormatParser.Parse(format);
                    foreach (var token in tokens)
                    {
                        if (level > LogLevel.Error)
                        {
                            _console.Write("{0}", token.Render(args));
                        }
                        else
                        {
                            _console.WriteError("{0}", token.Render(args));
                        }
                    }
                }
                finally
                {
                    _console.ResetColor();
                    if (level > LogLevel.Error)
                    {
                        _console.WriteLine();
                    }
                    else
                    {
                        _console.WriteErrorLine();
                    }
                }
            }
        }
    }
}
