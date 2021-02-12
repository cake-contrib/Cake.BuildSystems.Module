using System;
using Cake.Core;
using Cake.Core.Diagnostics;
using JetBrains.Annotations;

namespace Cake.AzurePipelines.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for TF.
    /// </summary>
    [UsedImplicitly]
    public class AzurePipelinesLog : ICakeLog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzurePipelinesLog"/> class.
        /// </summary>
        /// <param name="console">Implementation of <see cref="IConsole"/>.</param>
        /// <param name="verbosity">Default <see cref="Verbosity"/>.</param>
        public AzurePipelinesLog(IConsole console, Verbosity verbosity = Verbosity.Normal)
        {
            _cakeLogImplementation = new CakeBuildLog(console, verbosity);
        }

        private readonly ICakeLog _cakeLogImplementation;

        /// <inheritdoc />
        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                switch (level)
                {
                    case LogLevel.Fatal:
                    case LogLevel.Error:
                        _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information,
                            "##vso[task.logissue type=error;]{0}", string.Format(format, args));
                        break;
                    case LogLevel.Warning:
                        _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information,
                            "##vso[task.logissue type=warning;]{0}", string.Format(format, args));
                        break;
                    case LogLevel.Information:
                    case LogLevel.Verbose:
                    case LogLevel.Debug:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            }

            _cakeLogImplementation.Write(verbosity, level, format, args);
        }

        /// <inheritdoc />
        public Verbosity Verbosity
        {
            get { return _cakeLogImplementation.Verbosity; }
            set { _cakeLogImplementation.Verbosity = value; }
        }
    }
}
